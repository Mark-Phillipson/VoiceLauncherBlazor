using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using CursorlessVisualStudio.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Threading;

namespace CursorlessVisualStudio
{
    internal sealed class HatViewManager : IDisposable
    {
        internal const double HatSpace = 8;
        private readonly IWpfTextView view;
        private readonly HatSettings settings;
        private readonly IAdornmentLayer layer;
        private readonly DispatcherTimer timer;
        private readonly JoinableTaskCollection tasks;
        private readonly JoinableTaskFactory taskFactory;
        private CancellationTokenSource? pending;
        private SyntaxTree? syntaxTree;
        private ITextSnapshot? syntaxSnapshot;
        private TokenHatMap? map;
        private ITextSnapshot? mapSnapshot;
        private IReadOnlyList<HatTarget> previousTargets = Array.Empty<HatTarget>();
        private ITextSnapshot? previousSnapshot;
        private long revision;
        private long refreshRequestedAt;
        private bool disposed;
        private string? lastWarning;

        public string Status { get; private set; } = "Cursorless: waiting for editor layout.";

        public HatViewManager(IWpfTextView view, HatSettings settings)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            this.view = view;
            this.settings = settings;
            tasks = ThreadHelper.JoinableTaskContext.CreateCollection();
            taskFactory = ThreadHelper.JoinableTaskContext.CreateFactory(tasks);
            layer = view.GetAdornmentLayer(HatViewProvider.LayerName);
            timer = new DispatcherTimer(DispatcherPriority.Background, view.VisualElement.Dispatcher)
            {
                Interval = TimeSpan.FromMilliseconds(35)
            };
            timer.Tick += OnTick;
            view.LayoutChanged += OnLayoutChanged;
            view.TextBuffer.Changed += OnBufferChanged;
            view.Closed += OnClosed;
            view.GotAggregateFocus += OnFocus;
            view.VisualElement.IsVisibleChanged += OnVisibilityChanged;
            settings.Changed += OnSettingsChanged;
            VSColorTheme.ThemeChanged += OnThemeChanged;
            Schedule();
        }

        public void Select(HatMark mark, long expectedRevision)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (disposed || !settings.Enabled || map == null || mapSnapshot != view.TextSnapshot
                || mapSnapshot != view.TextBuffer.CurrentSnapshot || revision != expectedRevision)
                throw new InvalidOperationException("The visible hats changed. Close the dialog and try again.");
            var target = map.GetTarget(mark);
            var span = new SnapshotSpan(mapSnapshot, target.TokenSpan.Start, target.TokenSpan.Length);
            view.VisualElement.Focus();
            view.Caret.MoveTo(span.Start);
            view.Selection.Select(span, isReversed: false);
        }

        public long CaptureRevision()
        {
            if (map == null) throw new InvalidOperationException("No current hat map is available. Wait for the editor to finish drawing.");
            return revision;
        }

        private void OnLayoutChanged(object sender, TextViewLayoutChangedEventArgs args)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Schedule();
        }
        private void OnBufferChanged(object sender, TextContentChangedEventArgs args) => QueueRefresh();
        private void OnThemeChanged(ThemeChangedEventArgs args) => QueueRefresh();
        private void OnVisibilityChanged(object sender, DependencyPropertyChangedEventArgs args) => QueueRefresh();
        private void OnFocus(object sender, EventArgs args)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            ReportStatus();
        }
        private void OnClosed(object sender, EventArgs args)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Dispose();
        }

        private void OnSettingsChanged(object sender, EventArgs args)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Schedule();
            if (!view.IsClosed && !view.InLayout && view.TextViewLines.Count > 0)
            {
                var first = view.TextViewLines.FirstVisibleLine;
                view.DisplayTextLineContainingBufferPosition(first.Start, first.Top - view.ViewportTop, ViewRelativePosition.Top);
            }
        }

        private void QueueRefresh()
        {
            taskFactory.RunAsync(ScheduleAsync).FileAndForget("CursorlessVisualStudio/Schedule");
        }

        private async Task ScheduleAsync()
        {
            await taskFactory.SwitchToMainThreadAsync();
            Schedule();
        }

        private void Schedule()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (disposed) return;
            revision++;
            refreshRequestedAt = Stopwatch.GetTimestamp();
            pending?.Cancel();
            timer.Stop();
            map = null;
            layer.RemoveAllAdornments();
            if (settings.Enabled) timer.Start();
            else
            {
                Status = "Cursorless hats are disabled.";
                ReportStatus();
            }
        }

        private void OnTick(object? sender, EventArgs args)
        {
            timer.Stop();
            taskFactory.RunAsync(RefreshAsync).FileAndForget("CursorlessVisualStudio/Refresh");
        }

        private async Task RefreshAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            if (disposed || !settings.Enabled || view.InLayout || !view.VisualElement.IsVisible) return;
            var currentRevision = revision;
            var requestedAt = refreshRequestedAt;
            var snapshot = view.TextSnapshot;
            var visible = GetVisibleSpans(snapshot);
            var retained = TranslatePreviousTargets(snapshot);
            var oldTree = syntaxTree;
            var oldSnapshot = syntaxSnapshot;
            using (var cancellation = new CancellationTokenSource())
            {
                pending = cancellation;
                try
                {
                    var result = await Task.Run(() =>
                    {
                        var tree = oldTree;
                        if (tree == null || snapshot != oldSnapshot)
                        {
                            var source = SourceText.From(snapshot.GetText());
                            tree = tree == null
                                ? CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview), cancellationToken: cancellation.Token)
                                : tree.WithChangedText(source);
                        }
                        var hats = TokenHatMap.Create(tree, visible, retained, cancellation.Token);
                        return (Tree: tree, Hats: hats);
                    }, cancellation.Token);
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellation.Token);
                    if (disposed || revision != currentRevision || snapshot != view.TextSnapshot) return;
                    syntaxTree = result.Tree;
                    syntaxSnapshot = snapshot;
                    map = result.Hats;
                    mapSnapshot = snapshot;
                    previousSnapshot = snapshot;
                    previousTargets = map.Targets;
                    DrawHats();
                    var elapsed = (Stopwatch.GetTimestamp() - requestedAt) * 1000 / Stopwatch.Frequency;
                    Status = $"Cursorless: {map.Targets.Count} visible hats ({elapsed} ms including debounce).";
                    if (map.OmittedTokenCount > 0)
                    {
                        Status += $" WARNING: {map.OmittedTokenCount} tokens have no available visible mark; longer tokens take priority.";
                        if (Status.Substring(Status.IndexOf("WARNING", StringComparison.Ordinal)) != lastWarning)
                        {
                            lastWarning = Status.Substring(Status.IndexOf("WARNING", StringComparison.Ordinal));
                            ActivityLog.LogWarning(nameof(CursorlessVisualStudio), lastWarning);
                        }
                    }
                    else lastWarning = null;
                    ReportStatus();
                }
                catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
                {
                    // A newer layout or edit superseded this snapshot.
                }
                catch (ArgumentException exception)
                {
                    if (!disposed && revision == currentRevision) ReportFailure(exception);
                }
                catch (InvalidOperationException exception)
                {
                    if (!disposed && revision == currentRevision) ReportFailure(exception);
                }
                finally
                {
                    if (ReferenceEquals(pending, cancellation)) pending = null;
                }
            }
        }

        private List<TextSpan> GetVisibleSpans(ITextSnapshot snapshot)
        {
            var spans = new List<TextSpan>();
            foreach (var line in view.TextViewLines)
            {
                if (line.TextTop - 6 < view.ViewportTop || line.TextTop >= view.ViewportBottom) continue;
                var left = line.GetBufferPositionFromXCoordinate(view.ViewportLeft);
                var right = line.GetBufferPositionFromXCoordinate(view.ViewportRight);
                var start = left?.Position ?? (view.ViewportLeft <= line.Left ? line.Start.Position : line.End.Position);
                var end = right?.Position + 1 ?? line.End.Position;
                for (var position = start; position < Math.Min(end, line.End.Position);)
                {
                    var element = line.GetTextElementSpan(new SnapshotPoint(snapshot, position));
                    var bounds = line.GetCharacterBounds(element.Start);
                    var mapped = view.BufferGraph.MapUpToSnapshot(element, SpanTrackingMode.EdgeExclusive, view.VisualSnapshot);
                    if (mapped.Count > 0 && bounds.Right > view.ViewportLeft && bounds.Left < view.ViewportRight)
                    {
                        var span = new TextSpan(element.Start.Position, element.Length);
                        if (spans.Count > 0 && spans[spans.Count - 1].End == span.Start)
                            spans[spans.Count - 1] = TextSpan.FromBounds(spans[spans.Count - 1].Start, span.End);
                        else spans.Add(span);
                    }
                    position = Math.Max(position + 1, element.End.Position);
                }
            }
            return spans;
        }

        private IReadOnlyList<HatTarget> TranslatePreviousTargets(ITextSnapshot snapshot)
        {
            if (previousSnapshot == null) return Array.Empty<HatTarget>();
            var retained = new List<HatTarget>();
            foreach (var target in previousTargets)
            {
                var oldSpan = new SnapshotSpan(previousSnapshot, target.TokenSpan.Start, target.TokenSpan.Length);
                var newSpan = oldSpan.TranslateTo(snapshot, SpanTrackingMode.EdgeExclusive);
                if (newSpan.Length == oldSpan.Length && newSpan.GetText() == oldSpan.GetText())
                    retained.Add(new HatTarget(target.Mark, new TextSpan(newSpan.Start.Position, newSpan.Length),
                        newSpan.Start.Position + target.CharacterPosition - target.TokenSpan.Start));
            }
            return retained;
        }

        private void DrawHats()
        {
            if (map == null || mapSnapshot == null) return;
            layer.RemoveAllAdornments();
            var background = view.Background as SolidColorBrush;
            var light = background != null && background.Color.R * 0.299 + background.Color.G * 0.587 + background.Color.B * 0.114 > 140;
            foreach (var target in map.Targets)
            {
                var point = new SnapshotPoint(mapSnapshot, target.CharacterPosition);
                var line = view.TextViewLines.GetTextViewLineContainingBufferPosition(point);
                var bounds = line.GetCharacterBounds(point);
                var visual = HatVisual.Create(target.Mark, light);
                Canvas.SetLeft(visual, bounds.Left + (bounds.Width - visual.Width) / 2);
                Canvas.SetTop(visual, line.TextTop - 6);
                var span = new SnapshotSpan(point, target.Mark.Symbol.Length);
                layer.AddAdornment(AdornmentPositioningBehavior.TextRelative, span, target, visual, null);
            }
        }

        private void ReportFailure(Exception exception)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            map = null;
            layer.RemoveAllAdornments();
            Status = "Cursorless could not update hats: " + exception.Message;
            ActivityLog.LogError(nameof(CursorlessVisualStudio), exception.ToString());
            ReportStatus();
        }

        private void ReportStatus()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (view.HasAggregateFocus && ServiceProvider.GlobalProvider.GetService(typeof(SVsStatusbar)) is IVsStatusbar statusBar)
                statusBar.SetText(Status);
        }

        public void Dispose()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (disposed) return;
            disposed = true;
            pending?.Cancel();
            timer.Stop();
            timer.Tick -= OnTick;
            view.LayoutChanged -= OnLayoutChanged;
            view.TextBuffer.Changed -= OnBufferChanged;
            view.Closed -= OnClosed;
            view.GotAggregateFocus -= OnFocus;
            view.VisualElement.IsVisibleChanged -= OnVisibilityChanged;
            settings.Changed -= OnSettingsChanged;
            VSColorTheme.ThemeChanged -= OnThemeChanged;
            ThreadHelper.JoinableTaskFactory.Run(tasks.JoinTillEmptyAsync);
            layer.RemoveAllAdornments();
            map = null;
            mapSnapshot = null;
            previousSnapshot = null;
            previousTargets = Array.Empty<HatTarget>();
            syntaxTree = null;
            syntaxSnapshot = null;
        }
    }

    internal static class HatVisual
    {
        private static readonly Brush[] Dark = MakeBrushes("#BBBBC5", "#29A8DC", "#91CE3F", "#F02466", "#EC63C1", "#F9ED42", "#1FE8D5", "#FFA33C");
        private static readonly Brush[] Light = MakeBrushes("#595963", "#00699E", "#3C6F13", "#C31A43", "#A62888", "#7B6900", "#006D65", "#995100");

        public static FrameworkElement Create(HatMark mark, bool light)
        {
            var brush = (light ? Light : Dark)[(int)mark.Color];
            Shape visual;
            switch (mark.Shape)
            {
                case HatShape.Default:
                    visual = new Ellipse { Fill = brush };
                    break;
                case HatShape.Wing:
                    visual = new Polyline { Points = new PointCollection { new Point(0, 4), new Point(3, 0), new Point(6, 4) }, Stroke = brush, StrokeThickness = 1.4 };
                    break;
                case HatShape.Curve:
                    visual = new Path { Data = Geometry.Parse("M0,4 C0,0 6,0 6,4"), Stroke = brush, StrokeThickness = 1.4 };
                    break;
                case HatShape.Bolt:
                    visual = new Polyline { Points = new PointCollection { new Point(0, 4), new Point(2, 0), new Point(4, 4), new Point(6, 0) }, Stroke = brush, StrokeThickness = 1.4 };
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mark));
            }
            visual.Width = 6;
            visual.Height = 4;
            visual.IsHitTestVisible = false;
            visual.Focusable = false;
            AutomationProperties.SetName(visual, "Cursorless hat " + mark);
            return visual;
        }

        private static Brush[] MakeBrushes(params string[] colors) => colors.Select(color =>
        {
            var brush = (Brush)new BrushConverter().ConvertFromInvariantString(color);
            brush.Freeze();
            return brush;
        }).ToArray();
    }
}

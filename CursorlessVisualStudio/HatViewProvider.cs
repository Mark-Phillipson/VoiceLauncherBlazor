using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.Utilities;

namespace CursorlessVisualStudio
{
    [Export(typeof(HatSettings))]
    internal sealed class HatSettings
    {
        public bool Enabled { get; private set; } = true;
        public event EventHandler? Changed;
        public void Toggle()
        {
            Enabled = !Enabled;
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }

    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal sealed class HatViewProvider : IWpfTextViewCreationListener
    {
        internal const string LayerName = "CursorlessVisualStudio.Hats";

        [Export(typeof(AdornmentLayerDefinition))]
        [Name(LayerName)]
        [Order(After = PredefinedAdornmentLayers.Text, Before = PredefinedAdornmentLayers.Caret)]
        public AdornmentLayerDefinition? Layer = null;

        [Import]
        public HatSettings Settings { get; set; } = null!;

        public void TextViewCreated(IWpfTextView textView)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            textView.Properties.GetOrCreateSingletonProperty(() => new HatViewManager(textView, Settings));
        }
    }

    [Export(typeof(ILineTransformSourceProvider))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal sealed class HatLineTransformProvider : ILineTransformSourceProvider
    {
        [Import]
        public HatSettings Settings { get; set; } = null!;

        public ILineTransformSource Create(IWpfTextView textView) => new HatLineTransform(Settings);

        private sealed class HatLineTransform : ILineTransformSource
        {
            private readonly HatSettings settings;
            public HatLineTransform(HatSettings settings) => this.settings = settings;
            public LineTransform GetLineTransform(ITextViewLine line, double yPosition, ViewRelativePosition placement)
                => new LineTransform(settings.Enabled ? HatViewManager.HatSpace : 0, 0, 1);
        }
    }
}

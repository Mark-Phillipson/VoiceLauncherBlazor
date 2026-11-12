using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace CursorlessVisualStudio.Core
{
    public sealed class TokenHatMap
    {
        private readonly Dictionary<HatMark, HatTarget> byMark;

        private TokenHatMap(List<HatTarget> targets, int omittedTokenCount)
        {
            Targets = targets.OrderBy(target => target.TokenSpan.Start).ToList().AsReadOnly();
            OmittedTokenCount = omittedTokenCount;
            byMark = targets.ToDictionary(target => target.Mark);
        }

        public IReadOnlyList<HatTarget> Targets { get; }
        public int OmittedTokenCount { get; }

        public HatTarget GetTarget(HatMark mark)
        {
            if (!byMark.TryGetValue(mark, out var target))
                throw new KeyNotFoundException($"No visible token has the hat '{mark}'. Character case matters.");
            return target;
        }

        public static TokenHatMap Create(SyntaxTree tree, IReadOnlyList<TextSpan> visibleSpans,
            IReadOnlyList<HatTarget>? previousTargets = null, CancellationToken cancellationToken = default)
        {
            if (tree == null) throw new ArgumentNullException(nameof(tree));
            if (visibleSpans == null) throw new ArgumentNullException(nameof(visibleSpans));
            cancellationToken.ThrowIfCancellationRequested();
            var source = tree.GetText(cancellationToken);
            var root = tree.GetRoot(cancellationToken);
            if (visibleSpans.Any(span => span.End > source.Length))
                throw new ArgumentException("Visible spans must be inside the document.", nameof(visibleSpans));
            var spans = visibleSpans.Where(span => span.Length > 0).OrderBy(span => span.Start).ToArray();
            for (var index = 0; index < spans.Length; index++)
            {
                if (index > 0 && spans[index - 1].End > spans[index].Start)
                    throw new ArgumentException("Visible spans must be non-overlapping and inside the document.", nameof(visibleSpans));
            }

            var candidates = new Dictionary<TextSpan, Dictionary<string, int>>();
            foreach (var visible in spans)
            {
                cancellationToken.ThrowIfCancellationRequested();
                foreach (var token in root.DescendantTokens(visible, descendIntoTrivia: false))
                {
                    if (token.IsMissing || token.Span.Length == 0 || !token.Span.OverlapsWith(visible)) continue;
                    if (!candidates.TryGetValue(token.Span, out var characters))
                    {
                        characters = new Dictionary<string, int>(StringComparer.Ordinal);
                        candidates.Add(token.Span, characters);
                    }
                    var end = Math.Min(token.Span.End, visible.End);
                    for (var position = Math.Max(token.SpanStart, visible.Start); position < end; position++)
                    {
                        var character = source[position];
                        if (char.IsWhiteSpace(character) || char.IsControl(character) || char.IsLowSurrogate(character)) continue;
                        var length = char.IsHighSurrogate(character) ? 2 : 1;
                        if (position + length > end || (length == 2 && !char.IsLowSurrogate(source[position + 1]))) continue;
                        var symbol = source.ToString(new TextSpan(position, length));
                        if (!characters.ContainsKey(symbol)) characters.Add(symbol, position);
                    }
                }
            }

            var previous = (previousTargets ?? Array.Empty<HatTarget>()).ToDictionary(target => target.TokenSpan);
            var usedMarks = new HashSet<HatMark>();
            var targets = new List<HatTarget>();
            // Preserve longer tokens before reserving scarce punctuation/one-character marks.
            foreach (var group in candidates.OrderByDescending(pair => pair.Key.Length > 1)
                .ThenBy(pair => pair.Key.Start).GroupBy(pair => pair.Key.Length > 1))
            {
                var remaining = new List<KeyValuePair<TextSpan, Dictionary<string, int>>>();
                foreach (var candidate in group)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (previous.TryGetValue(candidate.Key, out var old)
                        && candidate.Value.TryGetValue(old.Mark.Symbol, out var position) && usedMarks.Add(old.Mark))
                        targets.Add(new HatTarget(old.Mark, candidate.Key, position));
                    else
                        remaining.Add(candidate);
                }
                foreach (var candidate in remaining)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var target = Allocate(candidate.Key, candidate.Value, usedMarks);
                    if (target != null) targets.Add(target);
                }
            }
            return new TokenHatMap(targets, candidates.Count - targets.Count);
        }

        private static HatTarget? Allocate(TextSpan token, Dictionary<string, int> characters, HashSet<HatMark> used)
        {
            var symbols = characters.OrderByDescending(pair => char.IsLetterOrDigit(pair.Key, 0))
                .ThenBy(pair => pair.Value).ToArray();
            foreach (HatShape shape in Enum.GetValues(typeof(HatShape)))
            foreach (HatColor color in Enum.GetValues(typeof(HatColor)))
            foreach (var symbol in symbols)
            {
                var mark = new HatMark(color, shape, symbol.Key);
                if (used.Add(mark)) return new HatTarget(mark, token, symbol.Value);
            }
            return null;
        }
    }
}

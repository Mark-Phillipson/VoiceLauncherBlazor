using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace CursorlessVisualStudio.Core
{
    internal readonly struct TargetSpan
    {
        public TargetSpan(TextSpan selection, TextSpan characters)
        {
            Selection = selection;
            Characters = characters;
        }

        public TextSpan Selection { get; }
        public TextSpan Characters { get; }
    }

    internal static class TextTargetReader
    {
        public static IEnumerable<TargetSpan> ReadCSharp(SyntaxNode root, SourceText source,
            IReadOnlyList<TextSpan> visible, CancellationToken cancellationToken)
        {
            var seen = new HashSet<TextSpan>();
            foreach (var viewport in visible)
            foreach (var token in root.DescendantTokens(viewport, descendIntoTrivia: false))
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (token.IsMissing || token.Span.Length == 0 || !token.Span.OverlapsWith(viewport) || !seen.Add(token.Span)) continue;
                if (token.Parent is LiteralExpressionSyntax literal &&
                    (literal.IsKind(SyntaxKind.StringLiteralExpression) || literal.IsKind(SyntaxKind.Utf8StringLiteralExpression)))
                {
                    var text = token.Text;
                    var quote = text.IndexOf('"');
                    var quoteCount = CountQuotes(text, quote);
                    var verbatim = text.StartsWith("@", StringComparison.Ordinal);
                    var raw = !verbatim && quoteCount >= 3;
                    var delimiterLength = raw ? quoteCount : 1;
                    var start = token.SpanStart + quote + delimiterLength;
                    var end = token.Span.End - (literal.IsKind(SyntaxKind.Utf8StringLiteralExpression) ? 2 : 0);
                    var hasClosingDelimiter = end - delimiterLength >= start;
                    for (var index = 0; hasClosingDelimiter && index < delimiterLength; index++)
                        hasClosingDelimiter = source[end - index - 1] == '"';
                    if (hasClosingDelimiter) end -= delimiterLength;
                    yield return new TargetSpan(token.Span, new TextSpan(token.SpanStart + quote, 1));
                    foreach (var word in ReadStringWords(source, TextSpan.FromBounds(start, end), visible,
                        skipEscapes: !raw && !verbatim, cancellationToken))
                        yield return new TargetSpan(word, word);
                }
                else if (token.Parent is InterpolatedStringExpressionSyntax interpolated)
                {
                    if (token == interpolated.StringStartToken)
                    {
                        var quote = token.Text.IndexOf('"');
                        yield return new TargetSpan(interpolated.Span, new TextSpan(token.SpanStart + quote, 1));
                    }
                }
                else if (token.IsKind(SyntaxKind.InterpolatedStringTextToken))
                {
                    var owner = token.Parent!.AncestorsAndSelf().OfType<InterpolatedStringExpressionSyntax>().First();
                    var prefix = owner.StringStartToken.Text;
                    var escaped = !prefix.Contains("@") && CountQuotes(prefix, prefix.IndexOf('"')) < 3;
                    foreach (var word in ReadStringWords(source, token.Span, visible, escaped, cancellationToken))
                        yield return new TargetSpan(word, word);
                }
                else yield return new TargetSpan(token.Span, token.Span);
            }
        }

        public static IEnumerable<TargetSpan> ReadText(SourceText source, IReadOnlyList<TextSpan> visible,
            CancellationToken cancellationToken)
        {
            var seen = new HashSet<TextSpan>();
            foreach (var viewport in visible)
            {
                var position = viewport.Start;
                while (position < viewport.End)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (char.IsLowSurrogate(source[position]) && position > 0 && char.IsHighSurrogate(source[position - 1])) position--;
                    var start = position;
                    if (IsWordCharacter(source, position))
                    {
                        while (start > 0)
                        {
                            var previous = PreviousCharacter(source, start);
                            if (!IsWordCharacter(source, previous)) break;
                            start = previous;
                        }
                        do { position += CharacterLength(source, position); }
                        while (position < source.Length && IsWordCharacter(source, position));
                    }
                    else
                    {
                        position += CharacterLength(source, position);
                        if (char.IsWhiteSpace(source[start]) || char.IsControl(source[start])) continue;
                    }
                    var span = TextSpan.FromBounds(start, position);
                    if (seen.Add(span)) yield return new TargetSpan(span, span);
                }
            }
        }

        private static IEnumerable<TextSpan> ReadStringWords(SourceText source, TextSpan content, IReadOnlyList<TextSpan> visible,
            bool skipEscapes, CancellationToken cancellationToken)
        {
            var ranges = visible.Where(span => span.OverlapsWith(content)).Select(span =>
                TextSpan.FromBounds(Math.Max(span.Start, content.Start), Math.Min(span.End, content.End))).ToArray();
            if (ranges.Length == 0) yield break;
            if (!skipEscapes)
            {
                foreach (var target in ReadText(source, ranges, cancellationToken))
                    if (IsWordCharacter(source, target.Selection.Start)) yield return target.Selection;
                yield break;
            }
            var position = content.Start;
            var visibleEnd = ranges[ranges.Length - 1].End;
            while (position < visibleEnd)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (skipEscapes && source[position] == '\\')
                {
                    position = SkipEscape(source, position, content.End);
                    continue;
                }
                if (!IsWordCharacter(source, position))
                {
                    position += CharacterLength(source, position);
                    continue;
                }
                var start = position;
                do { position += CharacterLength(source, position); }
                while (position < content.End && IsWordCharacter(source, position));
                var word = TextSpan.FromBounds(start, position);
                if (ranges.Any(range => range.OverlapsWith(word))) yield return word;
            }
        }

        private static int SkipEscape(SourceText source, int position, int end)
        {
            if (++position >= end) return end;
            var kind = source[position++];
            var digits = kind == 'u' ? 4 : kind == 'U' ? 8 : kind == 'x' ? 4 : 0;
            while (digits-- > 0 && position < end && Uri.IsHexDigit(source[position])) position++;
            return position;
        }

        private static int CountQuotes(string text, int start)
        {
            if (start < 0) throw new ArgumentException("A string token must contain an opening quote.", nameof(text));
            var end = start;
            while (end < text.Length && text[end] == '"') end++;
            return end - start;
        }

        private static bool IsWordCharacter(SourceText source, int position)
        {
            var character = source[position];
            if (character == '_' || char.IsLetterOrDigit(character)) return true;
            var category = char.IsHighSurrogate(character) && CharacterLength(source, position) == 2
                ? CharUnicodeInfo.GetUnicodeCategory(source.ToString(new TextSpan(position, 2)), 0)
                : char.GetUnicodeCategory(character);
            return category == UnicodeCategory.UppercaseLetter || category == UnicodeCategory.LowercaseLetter
                || category == UnicodeCategory.TitlecaseLetter || category == UnicodeCategory.ModifierLetter
                || category == UnicodeCategory.OtherLetter || category == UnicodeCategory.DecimalDigitNumber
                || category == UnicodeCategory.NonSpacingMark || category == UnicodeCategory.SpacingCombiningMark
                || category == UnicodeCategory.ConnectorPunctuation;
        }

        private static int CharacterLength(SourceText source, int position) => char.IsHighSurrogate(source[position])
            && position + 1 < source.Length && char.IsLowSurrogate(source[position + 1]) ? 2 : 1;

        private static int PreviousCharacter(SourceText source, int position) => position >= 2
            && char.IsLowSurrogate(source[position - 1]) && char.IsHighSurrogate(source[position - 2]) ? position - 2 : position - 1;
    }
}

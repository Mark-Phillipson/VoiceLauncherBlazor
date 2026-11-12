using CursorlessVisualStudio.Core;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Xunit;

namespace TestProjectxUnit
{
    public class CursorlessHatTests
    {
        [Fact]
        public void CaseSensitiveMarks_SelectDifferentLexicalSpans()
        {
            const string text = "A + a";
            var map = Create(text);
            var upper = map.GetTarget(new HatMark(HatColor.Default, HatShape.Default, "A"));
            var lower = map.GetTarget(new HatMark(HatColor.Default, HatShape.Default, "a"));
            Assert.Equal(new TextSpan(0, 1), upper.TokenSpan);
            Assert.Equal(new TextSpan(4, 1), lower.TokenSpan);
        }

        [Fact]
        public void LexicalSpans_IncludeNumbersStringsAndOperatorsButNotTrivia()
        {
            const string text = "  var total = A + 42; // ignored\nvar text = \"hello world\";";
            var map = Create(text);
            var selected = map.Targets.Select(target => text.Substring(target.TokenSpan.Start, target.TokenSpan.Length));
            Assert.Equal(new[] { "var", "total", "=", "A", "+", "42", ";", "var", "text", "=", "\"hello world\"", "hello", "world", ";" }, selected);
            Assert.All(map.Targets, target => Assert.Equal(target.Mark.Symbol,
                text.Substring(target.CharacterPosition, target.Mark.Symbol.Length)));
        }

        [Fact]
        public void DisjointVisibleSpans_OnlyMarkVisibleStringWordsNotTheHiddenOpeningQuote()
        {
            const string text = "var message = @\"first\nsecond\nthird\"; var hidden = 0;";
            var first = text.IndexOf("second", StringComparison.Ordinal);
            var last = text.IndexOf("third", StringComparison.Ordinal);
            var tree = CSharpSyntaxTree.ParseText(text);
            var map = TokenHatMap.Create(tree, new[] { new TextSpan(first, 6), new TextSpan(last, 5) });
            Assert.Equal(new[] { new TextSpan(first, 6), new TextSpan(last, 5) }, map.Targets.Select(target => target.TokenSpan));
            Assert.DoesNotContain(map.Targets, target => target.Mark.Symbol == "\"");
        }

        [Fact]
        public void Scrolling_PreservesMarksForTokensStillVisible()
        {
            const string text = "alpha + alpha + alpha";
            var tree = CSharpSyntaxTree.ParseText(text);
            var original = TokenHatMap.Create(tree, new[] { new TextSpan(0, text.Length) });
            var next = TokenHatMap.Create(tree, new[] { TextSpan.FromBounds(8, text.Length) }, original.Targets);
            foreach (var target in next.Targets)
                Assert.Equal(original.Targets.Single(old => old.TokenSpan == target.TokenSpan).Mark, target.Mark);
        }

        [Fact]
        public void Exhaustion_PrioritizesLongerTokensEvenOverPreviouslyAssignedSingleCharacters()
        {
            var text = string.Join(";", Enumerable.Repeat("a", 40)) + ";" + string.Join(";", Enumerable.Repeat("aa", 10));
            var tree = CSharpSyntaxTree.ParseText(text);
            var boundary = text.IndexOf("aa", StringComparison.Ordinal);
            var previous = TokenHatMap.Create(tree, new[] { new TextSpan(0, boundary) });
            var map = TokenHatMap.Create(tree, new[] { new TextSpan(0, text.Length) }, previous.Targets);
            Assert.Equal(10, map.Targets.Count(target => target.TokenSpan.Length == 2));
            Assert.Contains(map.Targets, target => target.TokenSpan.Length == 1);
            Assert.True(map.OmittedTokenCount > 0);
            Assert.Equal(map.Targets.Count, map.Targets.Select(target => target.Mark).Distinct().Count());
        }

        [Theory]
        [InlineData("")]
        [InlineData("  \r\n\t")]
        [InlineData("/* only a comment */")]
        public void EmptyAndTriviaOnlyText_HasNoHats(string text)
        {
            var map = Create(text);
            Assert.Empty(map.Targets);
            Assert.Equal(0, map.OmittedTokenCount);
        }

        [Fact]
        public void IncompleteCodeAndUnicodeIdentifiers_KeepSourceSpans()
        {
            const string text = "var \u0394value = (";
            var map = Create(text);
            var identifier = map.GetTarget(new HatMark(HatColor.Default, HatShape.Default, "\u0394"));
            Assert.Equal(new TextSpan(4, 6), identifier.TokenSpan);
            Assert.Equal(4, map.Targets.Count);
            Assert.Throws<KeyNotFoundException>(() => map.GetTarget(new HatMark(HatColor.Default, HatShape.Default, "Z")));
        }

        [Fact]
        public void InvalidViewportAndCanceledWork_AreNotSuccessfulEmptyMaps()
        {
            var tree = CSharpSyntaxTree.ParseText("A + a");
            Assert.Throws<ArgumentException>(() => TokenHatMap.Create(tree, new[] { new TextSpan(0, 10) }));
            Assert.Throws<ArgumentException>(() => TokenHatMap.Create(tree, new[] { new TextSpan(10, 0) }));
            Assert.Throws<ArgumentException>(() => TokenHatMap.Create(tree, new[] { new TextSpan(0, 3), new TextSpan(2, 2) }));
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            Assert.ThrowsAny<OperationCanceledException>(() => TokenHatMap.Create(tree, new[] { new TextSpan(0, 5) }, cancellationToken: cancellation.Token));
            Assert.ThrowsAny<OperationCanceledException>(() => TokenHatMap.Create(tree, Array.Empty<TextSpan>(), cancellationToken: cancellation.Token));
        }

        [Theory]
        [InlineData(".card { color: red; }", ".|card|{|color|:|red|;|}")]
        [InlineData("<Project>net10.0</Project>", "<|Project|>|net10|.|0|<|/|Project|>")]
        [InlineData("# Alpha Act A a", "#|Alpha|Act|A|a")]
        [InlineData("print('hello world')", "print|(|'|hello|world|'|)")]
        public void TextFallback_HandlesCssXmlMarkdownAndOtherLanguages(string text, string expected)
        {
            var map = TokenHatMap.CreateText(SourceText.From(text), new[] { new TextSpan(0, text.Length) });
            Assert.Equal(expected.Split('|'), SelectedText(text, map));
            Assert.All(map.Targets, target => Assert.Equal(target.Mark.Symbol,
                text.Substring(target.CharacterPosition, target.Mark.Symbol.Length)));
        }

        [Fact]
        public void TextFallback_ClippedWordsKeepWholeSpansAndDistinctCase()
        {
            const string text = "prefixMixedCase A a hidden";
            var map = TokenHatMap.CreateText(SourceText.From(text), new[] { new TextSpan(6, 3), new TextSpan(16, 3) });
            Assert.Equal(new[] { "prefixMixedCase", "A", "a" }, SelectedText(text, map));
            Assert.InRange(map.Targets[0].CharacterPosition, 6, 8);
            Assert.Equal("A", text.Substring(map.GetTarget(new HatMark(HatColor.Default, HatShape.Default, "A")).TokenSpan.Start, 1));
            Assert.Equal("a", text.Substring(map.GetTarget(new HatMark(HatColor.Default, HatShape.Default, "a")).TokenSpan.Start, 1));
        }

        [Theory]
        [InlineData("\"hello world\"", "hello|world")]
        [InlineData("@\"hello \"\"quoted\"\" world\"", "hello|quoted|world")]
        [InlineData("\"\"\"hello \"quoted\" world\"\"\"", "hello|quoted|world")]
        [InlineData("\"\"\"\nhello\nworld\n\"\"\"", "hello|world")]
        [InlineData("\"hello world\"u8", "hello|world")]
        [InlineData("\"unfinished u8", "unfinished|u8")]
        public void Strings_HaveWordTargetsAndAnOpeningQuoteForTheWholeLiteral(string text, string expectedWords)
        {
            var map = Create(text);
            var whole = map.GetTarget(new HatMark(HatColor.Default, HatShape.Default, "\""));
            Assert.Equal(new TextSpan(0, text.Length), whole.TokenSpan);
            Assert.Equal(text.IndexOf('"'), whole.CharacterPosition);
            Assert.Equal(expectedWords.Split('|'), SelectedText(text, map).Skip(1));
            Assert.Equal(map.Targets.Count, map.Targets.Select(target => target.CharacterPosition).Distinct().Count());
        }

        [Theory]
        [InlineData("\"\"")]
        [InlineData("@\"\"")]
        public void EmptyStrings_StillHaveAWholeLiteralTarget(string text)
        {
            var target = Assert.Single(Create(text).Targets);
            Assert.Equal("\"", target.Mark.Symbol);
            Assert.Equal(new TextSpan(0, text.Length), target.TokenSpan);
        }

        [Fact]
        public void StringEscapeSequences_AreNotMistakenForWords()
        {
            const string text = "\"one\\ntwo \\\"quoted\\\" \\u0041 three\"";
            var map = Create(text);
            Assert.Equal(new[] { text, "one", "two", "quoted", "three" }, SelectedText(text, map));
        }

        [Theory]
        [InlineData("$\"hello {user.Name} world\"")]
        [InlineData("$@\"hello {user.Name} world\"")]
        [InlineData("$\"\"\"hello {user.Name} world\"\"\"")]
        public void InterpolatedStrings_PreserveExpressionTokensAlongsideWordsAndWholeString(string text)
        {
            var map = Create(text);
            Assert.Equal(new[] { text, "hello", "{", "user", ".", "Name", "}", "world" }, SelectedText(text, map));
            Assert.Equal(text.IndexOf('"'), map.GetTarget(new HatMark(HatColor.Default, HatShape.Default, "\"")).CharacterPosition);
        }

        [Fact]
        public void NestedStringsAndWrappedWords_KeepDistinctSelectionSpans()
        {
            const string text = "$\"outer {$\"inner {x}\"} end\"";
            Assert.Equal(new[] { text, "outer", "{", "$\"inner {x}\"", "inner", "{", "x", "}", "}", "end" }, SelectedText(text, Create(text)));
            const string wrapped = "\"longword\"";
            var map = TokenHatMap.Create(CSharpSyntaxTree.ParseText(wrapped), new[] { new TextSpan(1, 3), new TextSpan(5, 4) });
            Assert.Equal(new TextSpan(1, 8), Assert.Single(map.Targets).TokenSpan);
        }

        private static IEnumerable<string> SelectedText(string text, TokenHatMap map) =>
            map.Targets.Select(target => text.Substring(target.TokenSpan.Start, target.TokenSpan.Length));

        private static TokenHatMap Create(string text) => TokenHatMap.Create(
            CSharpSyntaxTree.ParseText(text), new[] { new TextSpan(0, text.Length) });
    }
}

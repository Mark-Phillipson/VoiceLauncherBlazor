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
            Assert.Equal(new[] { "var", "total", "=", "A", "+", "42", ";", "var", "text", "=", "\"hello world\"", ";" }, selected);
            Assert.All(map.Targets, target => Assert.Equal(target.Mark.Symbol,
                text.Substring(target.CharacterPosition, target.Mark.Symbol.Length)));
        }

        [Fact]
        public void DisjointVisibleSpans_ExcludeHiddenTextAndDeduplicateMultilineTokens()
        {
            const string text = "var message = @\"first\nsecond\nthird\"; var hidden = 0;";
            var first = text.IndexOf("second", StringComparison.Ordinal);
            var last = text.IndexOf("third", StringComparison.Ordinal);
            var tree = CSharpSyntaxTree.ParseText(text);
            var map = TokenHatMap.Create(tree, new[] { new TextSpan(first, 6), new TextSpan(last, 5) });
            var target = Assert.Single(map.Targets);
            Assert.Equal("@\"first\nsecond\nthird\"", text.Substring(target.TokenSpan.Start, target.TokenSpan.Length));
            Assert.InRange(target.CharacterPosition, first, first + 5);
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

        private static TokenHatMap Create(string text) => TokenHatMap.Create(
            CSharpSyntaxTree.ParseText(text), new[] { new TextSpan(0, text.Length) });
    }
}

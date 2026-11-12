using System;
using Microsoft.CodeAnalysis.Text;

namespace CursorlessVisualStudio.Core
{
    public enum HatColor { Default, Blue, Green, Red, Pink, Yellow, UserColor1, UserColor2 }
    public enum HatShape { Default, Wing, Curve, Bolt }

    public readonly struct HatMark : IEquatable<HatMark>
    {
        public HatMark(HatColor color, HatShape shape, string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
                throw new ArgumentException("A hat must identify a source character.", nameof(symbol));
            Color = color;
            Shape = shape;
            Symbol = symbol;
        }

        public HatColor Color { get; }
        public HatShape Shape { get; }
        public string Symbol { get; }

        public bool Equals(HatMark other) => Color == other.Color && Shape == other.Shape
            && string.Equals(Symbol, other.Symbol, StringComparison.Ordinal);
        public override bool Equals(object? obj) => obj is HatMark other && Equals(other);
        public override int GetHashCode() => ((int)Color * 397 ^ (int)Shape) * 397
            ^ (Symbol == null ? 0 : StringComparer.Ordinal.GetHashCode(Symbol));
        public override string ToString() => $"{Color} {Shape} {Symbol}";
    }

    public sealed class HatTarget
    {
        public HatTarget(HatMark mark, TextSpan tokenSpan, int characterPosition)
        {
            if (!tokenSpan.Contains(new TextSpan(characterPosition, mark.Symbol.Length)))
                throw new ArgumentException("The marked character must be inside its token.", nameof(characterPosition));
            Mark = mark;
            TokenSpan = tokenSpan;
            CharacterPosition = characterPosition;
        }

        public HatMark Mark { get; }
        public TextSpan TokenSpan { get; }
        public int CharacterPosition { get; }
    }
}

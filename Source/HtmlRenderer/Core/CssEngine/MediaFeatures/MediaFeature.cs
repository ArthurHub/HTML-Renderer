using System.Linq;
using System;
using System.IO;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal abstract class MediaFeature : StylesheetNode, IMediaFeature
    {
        private TokenValue _tokenValue;
        private TokenType _constraintDelimiter;

        internal MediaFeature(string name)
        {
            Name = name;
            IsMinimum = name.StartsWith("min-");
            IsMaximum = name.StartsWith("max-");
        }

        internal abstract IValueConverter Converter { get; }

        public bool IsMinimum { get; }

        public bool IsMaximum { get; }

        public string Name { get; }

        // How the feature value is compared to the environment: min-* maps to
        // GreaterThanOrEqual, max-* to LessThanOrEqual, and range syntax
        // ("width >= 600px") maps from its operator. ":" and "=" are Equal.
        public MediaFeatureComparison Comparison
        {
            get
            {
                if (IsMinimum) return MediaFeatureComparison.GreaterThanOrEqual;
                if (IsMaximum) return MediaFeatureComparison.LessThanOrEqual;

                if (_constraintDelimiter == TokenType.GreaterThan) return MediaFeatureComparison.GreaterThan;
                if (_constraintDelimiter == TokenType.GreaterThanOrEqual) return MediaFeatureComparison.GreaterThanOrEqual;
                if (_constraintDelimiter == TokenType.LessThan) return MediaFeatureComparison.LessThan;
                if (_constraintDelimiter == TokenType.LessThanOrEqual) return MediaFeatureComparison.LessThanOrEqual;

                return MediaFeatureComparison.Equal;
            }
        }

        public string Value => HasValue ? _tokenValue.Text : string.Empty;

        public bool HasValue => _tokenValue is {Count: > 0};

        /// <summary>The stored value parsed as a <see cref="Length"/>, or <c>null</c> if absent/not a length.</summary>
        internal Length? AsLength() => HasValue ? _tokenValue.ToLength() : null;

        /// <summary>The stored value parsed as a <see cref="Resolution"/>, or <c>null</c> if absent/not a resolution.</summary>
        internal Resolution? AsResolution() => HasValue ? _tokenValue.ToResolution() : null;

        /// <summary>
        /// The stored value parsed as a numeric ratio (<c>&lt;number&gt; [ / &lt;number&gt; ]?</c>), e.g.
        /// <c>16/9</c>, or <c>null</c> if absent/unparseable.
        /// </summary>
        internal double? AsRatio()
        {
            if (!HasValue) return null;

            var numbers = _tokenValue.OfType<NumberToken>().Select(token => (double)token.Value).ToList();

            if (numbers.Count >= 2) return numbers[1] != 0 ? numbers[0] / numbers[1] : (double?)null;
            if (numbers.Count == 1) return numbers[0] > 0 ? numbers[0] : (double?)null;
            return null;
        }

        public override void ToCss(TextWriter writer, IStyleFormatter formatter)
        {
            var constraintDelimiter = GetConstraintDelimiter();
            var value = HasValue ? Value : null;
            writer.Write(formatter.Constraint(Name, value, GetConstraintDelimiter()));
        }

        private string GetConstraintDelimiter()
        {
            if (_constraintDelimiter == TokenType.Colon)
                return ": ";
            if (_constraintDelimiter == TokenType.GreaterThan)
                return " > ";
            if (_constraintDelimiter == TokenType.LessThan)
                return " < ";
            if (_constraintDelimiter == TokenType.Equal)
                return " = ";
            if (_constraintDelimiter == TokenType.GreaterThanOrEqual)
                return " >= ";
            if (_constraintDelimiter == TokenType.LessThanOrEqual)
                return " <= ";
            return ": ";
        }

        internal bool TrySetValue(TokenValue tokenValue, TokenType constraintDelimiter)
        {
            bool result;

            if (tokenValue == null)
                result = !IsMinimum && !IsMaximum && Converter.ConvertDefault() != null;
            else
                result = Converter.Convert(tokenValue) != null;

            if (result) _tokenValue = tokenValue;

            _constraintDelimiter = constraintDelimiter;

            return result;
        }
    }
}
using System.IO;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// How a media feature's stored value constrains the device value, unifying the <c>min-</c>/<c>max-</c>
    /// name-prefix forms with the Media Queries 4 range operators (<c>&gt;=</c>/<c>&lt;=</c>/<c>&gt;</c>/<c>&lt;</c>).
    /// </summary>
    internal enum MediaFeatureComparison
    {
        /// <summary>Exact match (the <c>feature: value</c> form, or a boolean feature).</summary>
        Exact,
        /// <summary>Device value must be ≥ the feature value (<c>min-*</c> or <c>&gt;=</c>).</summary>
        Minimum,
        /// <summary>Device value must be ≤ the feature value (<c>max-*</c> or <c>&lt;=</c>).</summary>
        Maximum,
        /// <summary>Device value must be strictly &gt; the feature value.</summary>
        GreaterThan,
        /// <summary>Device value must be strictly &lt; the feature value.</summary>
        LessThan
    }

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

        public string Value => HasValue ? _tokenValue.Text : string.Empty;

        public bool HasValue => _tokenValue != null && _tokenValue.Count > 0;

        /// <summary>
        /// The comparison the stored value imposes on the device value, derived from the <c>min-</c>/
        /// <c>max-</c> name prefix or, for the range syntax (<c>width &gt;= 48rem</c>), the parsed operator.
        /// </summary>
        internal MediaFeatureComparison Comparison
        {
            get
            {
                if (IsMinimum) return MediaFeatureComparison.Minimum;
                if (IsMaximum) return MediaFeatureComparison.Maximum;

                switch (_constraintDelimiter)
                {
                    case TokenType.GreaterThanOrEqual: return MediaFeatureComparison.Minimum;
                    case TokenType.GreaterThan: return MediaFeatureComparison.GreaterThan;
                    case TokenType.LessThanOrEqual: return MediaFeatureComparison.Maximum;
                    case TokenType.LessThan: return MediaFeatureComparison.LessThan;
                    default: return MediaFeatureComparison.Exact;
                }
            }
        }

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
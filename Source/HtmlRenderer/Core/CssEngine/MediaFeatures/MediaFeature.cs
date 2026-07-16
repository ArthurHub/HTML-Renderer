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

        public string Value => HasValue ? _tokenValue.Text : string.Empty;

        public bool HasValue => _tokenValue != null && _tokenValue.Count > 0;

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
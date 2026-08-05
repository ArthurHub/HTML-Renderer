using System.Globalization;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class NumberToken : Token
    {
        private static readonly char[] FloatIndicators = { '.', 'e', 'E' };

        public NumberToken(string number, TextPosition position)
            : base(TokenType.Number, number, position)
        {
        }

        public bool IsInteger => Data.IndexOfAny(FloatIndicators) == -1;

        public int IntegerValue
        {
            get
            {
                var parsed = int.TryParse(Data, out var result);

                if (parsed)
                {
                    return result;
                }

                if (Data.All(char.IsDigit))
                {
                    return int.MaxValue;
                }

                throw new ParseException($"Unrecognized integer value '{Data}.'");
            }
        }

        public float Value => float.Parse(Data, CultureInfo.InvariantCulture);
    }
}
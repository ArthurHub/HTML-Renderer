using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Grammar of <c>image-orientation</c> (CSS Images 4 §5.2): <c>from-image | none | [ &lt;angle&gt; || flip ]</c>.
    /// </summary>
    internal static class ImageOrientationGrammar
    {
        private const string FromImage = "from-image";
        private const string Flip = "flip";

        private static readonly IValueConverter AngleCalc = new CalcValueConverter(CalcCategory.Angle);

        internal static bool IsValid(IEnumerable<Token> value)
        {
            var tokens = value.Where(t => t.Type != TokenType.Whitespace).ToArray();

            if (tokens.Length < 1 || tokens.Length > 2) return false;

            if (tokens.Length == 1 && tokens[0].Type == TokenType.Ident &&
                (tokens[0].Data.Isi(FromImage) || tokens[0].Data.Isi(Keywords.None)))
                return true;

            var hasAngle = false;
            var hasFlip = false;

            foreach (var token in tokens)
            {
                if (token.Type == TokenType.Ident && token.Data.Isi(Flip) && !hasFlip)
                {
                    hasFlip = true;
                }
                else if (!hasAngle && IsAngle(token))
                {
                    hasAngle = true;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        // A bare 0 is not accepted: image-orientation does not take the unitless-zero legacy form.
        private static bool IsAngle(Token token)
        {
            if (token is UnitToken unit && token.Type == TokenType.Dimension)
                return Angle.GetUnit(unit.Unit.ToLowerInvariant()) != Angle.Unit.None;

            return token is FunctionToken && AngleCalc.Convert(new[] { token }) != null;
        }
    }
}

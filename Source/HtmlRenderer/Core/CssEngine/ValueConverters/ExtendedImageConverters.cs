using System;
using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    using static Converters;

    /// <summary>
    /// The value of an <c>image-set()</c>/<c>cross-fade()</c>/<c>element()</c> function (CSS Images 4 §2/§3).
    /// These are validated as syntactically-valid <c>&lt;image&gt;</c> values; the original tokens are kept
    /// for serialization.
    /// </summary>
    internal sealed class ImageFunctionValue : IPropertyValue
    {
        public ImageFunctionValue(IEnumerable<Token> tokens)
        {
            Original = new TokenValue(tokens);
        }

        public string CssText => Original.Text;

        public TokenValue Original { get; }

        public TokenValue ExtractFor(string name) => Original;
    }

    // The inner <image> of an image-set()/cross-fade() option - a url() or a gradient. Referencing the two
    // base converters directly (rather than ImageSourceConverter) avoids a static-init cycle.
    internal static class ExtendedImage
    {
        // Qualify GradientConverter - the bare name also resolves to the abstract GradientConverter type.
        public static readonly IValueConverter InnerImage = UrlConverter.Or(Converters.GradientConverter);
    }

    // element( <id-selector> ) - a single '#id' hash argument.
    internal sealed class ElementImageConverter : IValueConverter
    {
        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var tokens = value.Where(t => t.Type != TokenType.Whitespace).ToArray();

            // A '#id' is a single <hash-token>: an all-hex id lexes as a Color token, any other as a Hash.
            var isSingleHash = tokens.Length == 1 &&
                (tokens[0].Type == TokenType.Hash || tokens[0].Type == TokenType.Color);

            return isSingleHash ? new ImageFunctionValue(value) : null;
        }

        public IPropertyValue Construct(Property[] properties) => properties.Guard<ImageFunctionValue>();
    }

    // image-set( <image-set-option># ),
    // option = [ <image> | <string> ] [ <resolution> || type(<string>) ]?
    internal sealed class ImageSetConverter : IValueConverter
    {
        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var options = value.ToList();
            if (options.Count == 0 || options.Any(o => !IsOption(o))) return null;
            return new ImageFunctionValue(value);
        }

        private static bool IsOption(List<Token> option)
        {
            var items = option.ToItems();
            if (items.Count == 0) return false;

            // The source: a <string> or a url()/gradient <image>.
            if (StringConverter.Convert(items[0]) == null && ExtendedImage.InnerImage.Convert(items[0]) == null)
                return false;

            // Any remaining items are a <resolution> (including the `x` dppx alias) and/or type(<string>).
            for (var i = 1; i < items.Count; i++)
            {
                if (ResolutionConverter.Convert(items[i]) == null && !IsTypeFunction(items[i]))
                    return false;
            }

            return true;
        }

        private static bool IsTypeFunction(List<Token> item)
        {
            if (item.Count != 1 || item[0] is not FunctionToken function ||
                !function.Data.Equals("type", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var inner = function.Where(t =>
                t.Type != TokenType.Whitespace && t.Type != TokenType.RoundBracketClose).ToArray();

            return inner.Length == 1 && inner[0].Type == TokenType.String;
        }

        public IPropertyValue Construct(Property[] properties) => properties.Guard<ImageFunctionValue>();
    }

    // cross-fade( <cf-image># ), <cf-image> = <percentage>? && [ <image> | <color> ]; plus the legacy
    // cross-fade( <image>, <image>, <percentage> ) form.
    internal sealed class CrossFadeConverter : IValueConverter
    {
        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var args = value.ToList();
            if (args.Count == 0) return null;

            var isModern = args.All(IsCrossFadeImage);
            var isLegacy = args.Count == 3 && IsCrossFadeImage(args[0]) && IsCrossFadeImage(args[1]) &&
                PercentConverter.Convert(args[2]) != null;

            return isModern || isLegacy ? new ImageFunctionValue(value) : null;
        }

        private static bool IsCrossFadeImage(List<Token> arg)
        {
            var hasImageOrColor = false;

            foreach (var item in arg.ToItems())
            {
                if (ExtendedImage.InnerImage.Convert(item) != null || ColorConverter.Convert(item) != null)
                {
                    hasImageOrColor = true;
                }
                else if (PercentConverter.Convert(item) == null)
                {
                    return false;
                }
            }

            return hasImageOrColor;
        }

        public IPropertyValue Construct(Property[] properties) => properties.Guard<ImageFunctionValue>();
    }
}

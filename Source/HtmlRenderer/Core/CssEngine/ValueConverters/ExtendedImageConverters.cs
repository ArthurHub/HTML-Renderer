using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    using static Converters;

    /// <summary>
    /// An <c>image-set()</c>, <c>cross-fade()</c> or <c>element()</c> value, CSS Images 4 §2.4, §2.6 and §2.7.
    /// Only validated and serialized, the painter doesn't handle them.
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

    /// <summary>
    /// The <c>&lt;image&gt;</c> inside <c>image-set()</c> and <c>cross-fade()</c>, only a url() or a gradient.
    /// Using <c>ImageSourceConverter</c> here would be a construction cycle.
    /// </summary>
    internal static class ExtendedImage
    {
        // Qualified, the bare name is the GradientConverter type
        public static readonly IValueConverter InnerImage = UrlConverter.Or(Converters.GradientConverter);
    }

    /// <summary><c>element( &lt;id-selector&gt; )</c> — a single <c>#id</c> hash argument.</summary>
    internal sealed class ElementImageConverter : IValueConverter
    {
        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            // ToList() on tokens is the comma group splitter
            var tokens = value.Where(t => t.Type != TokenType.Whitespace).ToArray();

            // An id that looks like a hex color, like #f00, is tokenized as a color
            return tokens.Length == 1 && (tokens[0].Type == TokenType.Hash || tokens[0].Type == TokenType.Color)
                ? new ImageFunctionValue(value)
                : null;
        }

        public IPropertyValue Construct(Property[] properties) => properties.Guard<ImageFunctionValue>();
    }

    /// <summary><c>image-set( &lt;image-set-option&gt;# )</c>, option = <c>[ &lt;image&gt; | &lt;string&gt; ] [ &lt;resolution&gt; || type(&lt;string&gt;) ]?</c>.</summary>
    internal sealed class ImageSetConverter : IValueConverter
    {
        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var options = value.ToList(); // comma-separated groups
            if (options.Count == 0 || options.Any(o => !IsOption(o))) return null;
            return new ImageFunctionValue(value);
        }

        private static bool IsOption(List<Token> option)
        {
            var items = option.ToItems(); // whitespace/function-boundary split
            if (items.Count == 0) return false;

            // The source: a <string> or a url()/gradient <image>.
            if (StringConverter.Convert(items[0]) == null && ExtendedImage.InnerImage.Convert(items[0]) == null)
                return false;

            for (var i = 1; i < items.Count; i++)
                if (ResolutionConverter.Convert(items[i]) == null && !IsTypeFunction(items[i]))
                    return false;

            return true;
        }

        private static bool IsTypeFunction(List<Token> item)
        {
            var fn = item.Count == 1 ? item[0] as FunctionToken : null;
            if (fn == null || !fn.Data.Equals("type", System.StringComparison.OrdinalIgnoreCase))
                return false;
            var inner = fn.Where(t => t.Type != TokenType.Whitespace && t.Type != TokenType.RoundBracketClose).ToArray();
            return inner.Length == 1 && inner[0].Type == TokenType.String;
        }

        public IPropertyValue Construct(Property[] properties) => properties.Guard<ImageFunctionValue>();
    }

    /// <summary><c>cross-fade( &lt;cf-image&gt;# )</c>, <c>&lt;cf-image&gt; = &lt;percentage&gt;? &amp;&amp; [ &lt;image&gt; | &lt;color&gt; ]</c>;
    /// plus the legacy <c>cross-fade( &lt;image&gt;, &lt;image&gt;, &lt;percentage&gt; )</c> form.</summary>
    internal sealed class CrossFadeConverter : IValueConverter
    {
        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var args = value.ToList(); // comma-separated groups
            if (args.Count == 0) return null;

            if (args.All(IsCrossFadeImage) ||
                (args.Count == 3 && IsCrossFadeImage(args[0]) && IsCrossFadeImage(args[1]) && PercentConverter.Convert(args[2]) != null))
                return new ImageFunctionValue(value);

            return null;
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

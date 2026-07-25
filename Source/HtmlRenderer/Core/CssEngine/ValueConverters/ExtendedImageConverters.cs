using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    using static Converters;

    /// <summary>
    /// A validated-but-not-rendered image function (<c>image-set()</c>/<c>cross-fade()</c>/<c>element()</c>,
    /// CSS Images 4 §2/§3). PeachPDF accepts these as syntactically-valid <c>&lt;image&gt;</c> values (so a
    /// property or <c>@property</c> registration using one is valid, per spec) but paints nothing for them —
    /// the render path (<c>CssValueParser.ParseImage</c> → <c>CssImagePainter</c>) only handles url()/gradients.
    /// This value carries the original tokens for serialization only.
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

    /// <summary>The inner <c>&lt;image&gt;</c> of an <c>image-set()</c>/<c>cross-fade()</c> option — a url() or a
    /// gradient. Nested <c>image-set()</c> isn't modeled (not a real authoring pattern); referencing the two
    /// base converters directly also avoids a construction cycle with <c>ImageSourceConverter</c>.</summary>
    internal static class ExtendedImage
    {
        // Qualify GradientConverter — the bare name also resolves to the abstract GradientConverter *type*.
        public static readonly IValueConverter InnerImage = UrlConverter.Or(Converters.GradientConverter);
    }

    /// <summary><c>element( &lt;id-selector&gt; )</c> — a single <c>#id</c> hash argument.</summary>
    internal sealed class ElementImageConverter : IValueConverter
    {
        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            // ToArray (LINQ) — a bare ToList() on IEnumerable<Token> resolves to the CSS comma-group splitter.
            var tokens = value.Where(t => t.Type != TokenType.Whitespace).ToArray();

            // An '#id' selector is a single <hash-token> (CSS Syntax §4). A '#f00'-shaped id lexes as a Color
            // token, any other id as a Hash token; both are the single '#'-prefixed form.
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

            // Any remaining items are a <resolution> (incl. the `x` dppx alias, modeled by Resolution.GetUnit)
            // and/or a type(<string>), in either order.
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

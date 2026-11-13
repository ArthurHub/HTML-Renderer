using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    using static Converters;

    /// <summary><c>block-step-size</c> (CSS Rhythmic Sizing 1 4.1): <c>none | &lt;length [0,inf]&gt;</c>.</summary>
    internal sealed class BlockStepSizeProperty : Property
    {
        internal static readonly IValueConverter GrammarConverter = new GrammarValueConverter(RhythmGrammar.IsNoneOrLength);

        private static readonly IValueConverter StyleConverter = GrammarConverter.OrDefault();

        internal BlockStepSizeProperty()
            : base(PropertyNames.BlockStepSize)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }

    /// <summary><c>block-step-insert</c> (CSS Rhythmic Sizing 1 4.2): <c>margin-box | padding-box | content-box</c>.</summary>
    internal sealed class BlockStepInsertProperty : Property
    {
        internal static readonly IValueConverter GrammarConverter =
            GrammarValueConverter.OneOf("margin-box", "padding-box", "content-box");

        private static readonly IValueConverter StyleConverter = GrammarConverter.OrDefault();

        internal BlockStepInsertProperty()
            : base(PropertyNames.BlockStepInsert)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }

    /// <summary><c>block-step-align</c> (CSS Rhythmic Sizing 1 4.3): <c>auto | center | start | end</c>.</summary>
    internal sealed class BlockStepAlignProperty : Property
    {
        internal static readonly IValueConverter GrammarConverter =
            GrammarValueConverter.OneOf(Keywords.Auto, "center", "start", "end");

        private static readonly IValueConverter StyleConverter = GrammarConverter.OrDefault();

        internal BlockStepAlignProperty()
            : base(PropertyNames.BlockStepAlign)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }

    /// <summary><c>block-step-round</c> (CSS Rhythmic Sizing 1 4.4): <c>up | down | nearest</c>.</summary>
    internal sealed class BlockStepRoundProperty : Property
    {
        internal static readonly IValueConverter GrammarConverter = GrammarValueConverter.OneOf("up", "down", "nearest");

        private static readonly IValueConverter StyleConverter = GrammarConverter.OrDefault();

        internal BlockStepRoundProperty()
            : base(PropertyNames.BlockStepRound)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }

    /// <summary>
    /// <c>block-step</c> (CSS Rhythmic Sizing 1 4):
    /// <c>&lt;'block-step-size'&gt; || &lt;'block-step-insert'&gt; || &lt;'block-step-align'&gt; || &lt;'block-step-round'&gt;</c>.
    /// </summary>
    internal sealed class BlockStepProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter = WithAny(
            BlockStepSizeProperty.GrammarConverter.Option().For(PropertyNames.BlockStepSize),
            BlockStepInsertProperty.GrammarConverter.Option().For(PropertyNames.BlockStepInsert),
            BlockStepAlignProperty.GrammarConverter.Option().For(PropertyNames.BlockStepAlign),
            BlockStepRoundProperty.GrammarConverter.Option().For(PropertyNames.BlockStepRound)).OrDefault();

        internal BlockStepProperty()
            : base(PropertyNames.BlockStep, PropertyFlags.None)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }

    /// <summary><c>text-box-trim</c> (CSS Inline 3 5.1): <c>none | trim-start | trim-end | trim-both</c>.</summary>
    internal sealed class TextBoxTrimProperty : Property
    {
        internal static readonly IValueConverter GrammarConverter =
            GrammarValueConverter.OneOf(Keywords.None, "trim-start", "trim-end", "trim-both");

        private static readonly IValueConverter StyleConverter = GrammarConverter.OrDefault();

        internal TextBoxTrimProperty()
            : base(PropertyNames.TextBoxTrim)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }

    /// <summary>
    /// <c>text-box-edge</c> (CSS Inline 3 5.2):
    /// <c>auto | [ text | cap | ex | ideographic | ideographic-ink ] [ text | alphabetic | ideographic | ideographic-ink ]?</c>.
    /// </summary>
    internal sealed class TextBoxEdgeProperty : Property
    {
        internal static readonly IValueConverter GrammarConverter = new GrammarValueConverter(RhythmGrammar.IsTextBoxEdge);

        private static readonly IValueConverter StyleConverter = GrammarConverter.OrDefault();

        internal TextBoxEdgeProperty()
            : base(PropertyNames.TextBoxEdge, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;
    }

    /// <summary>
    /// <c>text-box</c> (CSS Inline 3 5.3): <c>normal | [ &lt;'text-box-trim'&gt; || &lt;'text-box-edge'&gt; ]</c>;
    /// <c>normal</c> is <c>none auto</c>.
    /// </summary>
    internal sealed class TextBoxProperty : ShorthandProperty
    {
        private static readonly IValueConverter StyleConverter = new TextBoxNormalConverter().Or(WithAny(
            TextBoxTrimProperty.GrammarConverter.Option().For(PropertyNames.TextBoxTrim),
            TextBoxEdgeProperty.GrammarConverter.Option().For(PropertyNames.TextBoxEdge)).OrDefault());

        internal TextBoxProperty()
            : base(PropertyNames.TextBox, PropertyFlags.Inherited)
        {
        }

        internal override IValueConverter Converter => StyleConverter;

        /// <summary><c>normal</c>: both longhands at their initial values.</summary>
        private sealed class TextBoxNormalConverter : IValueConverter
        {
            public IPropertyValue Convert(IEnumerable<Token> value)
            {
                var tokens = value.ToArray();
                var significant = tokens.Where(t => t.Type != TokenType.Whitespace).ToArray();

                return significant.Length == 1 && significant[0].Type == TokenType.Ident &&
                       significant[0].Data.Isi(Keywords.Normal)
                    ? new NormalValue(tokens)
                    : null;
            }

            public IPropertyValue Construct(Property[] properties)
            {
                return properties.Guard<NormalValue>();
            }

            private sealed class NormalValue : IPropertyValue
            {
                public NormalValue(IEnumerable<Token> tokens)
                {
                    Original = new TokenValue(tokens);
                }

                public string CssText => Original.Text;

                public TokenValue Original { get; }

                public TokenValue ExtractFor(string name)
                {
                    if (name.Is(PropertyNames.TextBoxTrim)) return TokenValue.FromString(Keywords.None);
                    if (name.Is(PropertyNames.TextBoxEdge)) return TokenValue.FromString(Keywords.Auto);

                    return null;
                }
            }
        }
    }
}

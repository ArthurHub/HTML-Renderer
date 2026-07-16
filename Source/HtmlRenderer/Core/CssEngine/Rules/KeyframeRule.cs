using System.IO;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class KeyframeRule : Rule, IKeyframeRule
    {
        internal KeyframeRule(StylesheetParser parser)
            : base(RuleType.Keyframe, parser)
        {
            AppendChild(new StyleDeclaration(this));
        }

        public override void ToCss(TextWriter writer, IStyleFormatter formatter)
        {
            writer.Write(formatter.Style(KeyText, Style));
        }

        public string KeyText
        {
            get => Key.Text;
            set
            {
                var selector = Parser.ParseKeyframeSelector(value);
                Key = selector ?? throw new ParseException("Unable to parse keyframe selector");
            }
        }

        public KeyframeSelector Key
        {
            get => Children.OfType<KeyframeSelector>().FirstOrDefault();
            set => ReplaceSingle(Key, value);
        }

        public StyleDeclaration Style => Children.OfType<StyleDeclaration>().FirstOrDefault();
    }
}
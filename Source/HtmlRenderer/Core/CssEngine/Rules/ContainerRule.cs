using System.IO;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class ContainerRule : ConditionRule, IContainerRule
    {
        internal ContainerRule(StylesheetParser parser) : base(RuleType.Container, parser)
        {
            AppendChild(new MediaList(parser));
        }

        public override void ToCss(TextWriter writer, IStyleFormatter formatter)
        {
            var rules = formatter.Block(Rules);
            var name = "@container";
            if (!string.IsNullOrEmpty(Name))
                name = $"{name} {Name}";
            writer.Write(formatter.Rule(name, ConditionText, rules));
        }

        public MediaList Media => Children.OfType<MediaList>().FirstOrDefault();

        public string Name { get; set; }

        public string ConditionText
        {
            get => Media.MediaText;
            set => Media.MediaText = value;
        }
    }
}

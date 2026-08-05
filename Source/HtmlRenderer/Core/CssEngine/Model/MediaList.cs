using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class MediaList : StylesheetNode
    {
        private readonly StylesheetParser _parser;

        internal MediaList(StylesheetParser parser)
        {
            _parser = parser;
        }

        public string this[int index] => Media.GetItemByIndex(index).ToCss();
        public IEnumerable<Medium> Media => Children.OfType<Medium>();
        public int Length => Media.Count();

        public string MediaText
        {
            get => this.ToCss();
            set
            {
                Clear();

                foreach (var medium in _parser.ParseMediaList(value))
                {
                    if (medium == null) throw new ParseException("Unable to parse media list element");
                    AppendChild(medium);
                }
            }
        }

        public override void ToCss(TextWriter writer, IStyleFormatter formatter)
        {
            var parts = Media.ToArray();
            if (parts.Length <= 0) return;
            parts[0].ToCss(writer, formatter);

            for (var i = 1; i < parts.Length; i++)
            {
                writer.Write(", ");
                parts[i].ToCss(writer, formatter);
            }
        }

        public void Add(string newMedium)
        {
            var medium = _parser.ParseMedium(newMedium);
            if (medium == null) throw new ParseException("Unable to parse medium");
            AppendChild(medium);
        }

        public void Remove(string oldMedium)
        {
            var medium = _parser.ParseMedium(oldMedium);
            if (medium == null) throw new ParseException("Unable to parse medium");

            foreach (var item in Media)
            {
                if (!item.Equals(medium)) continue;

                RemoveChild(item);
                return;
            }

            throw new ParseException("Media list element not found");
        }

        public IEnumerator<Medium> GetEnumerator()
        {
            return Media.GetEnumerator();
        }
    }
}
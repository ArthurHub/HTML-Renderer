using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The <c>string-set</c> property (GCPM §7.3):
    /// <c>[ &lt;custom-ident&gt; &lt;content-list&gt; ]# | none</c>, where a content-list item is a
    /// <c>&lt;string&gt;</c>, <c>counter()</c>/<c>counters()</c>, <c>content()</c>, <c>string()</c>, or
    /// <c>attr()</c>.
    /// </summary>
    internal sealed class StringSetValueConverter : IValueConverter
    {
        private readonly IValueConverter _contentListItem;

        public StringSetValueConverter()
        {
            _contentListItem = Converters.ContentListItemConverter;
        }

        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            if (value.OnlyOrDefault() is KeywordToken keyword && keyword.Data.Isi(Keywords.None))
                return new StringSetValue(null, value);

            var items = value.ToList(); // comma-separated groups
            var pairs = new List<StringSetPair>();

            foreach (var item in items)
            {
                var pair = ParsePair(item);
                if (pair == null) return null;
                pairs.Add(pair);
            }

            return pairs.Count == 0 ? null : new StringSetValue(pairs, value);
        }

        private StringSetPair ParsePair(List<Token> tokens)
        {
            if (tokens.Count < 2) return null;

            if (tokens[0] is not KeywordToken nameToken || nameToken.Type != TokenType.Ident) return null;

            var contentItems = tokens.Skip(1).ToItems();
            var values = new List<IPropertyValue>();

            foreach (var contentItem in contentItems)
            {
                var converted = _contentListItem.Convert(contentItem);
                if (converted == null) return null;
                values.Add(converted);
            }

            return values.Count == 0 ? null : new StringSetPair(nameToken.Data, values);
        }

        public IPropertyValue Construct(Property[] properties)
        {
            return properties.Guard<StringSetValue>();
        }

        private sealed class StringSetPair
        {
            public StringSetPair(string name, List<IPropertyValue> contentList)
            {
                Name = name;
                ContentList = contentList;
            }

            public string Name { get; }
            public List<IPropertyValue> ContentList { get; }
        }

        private sealed class StringSetValue : IPropertyValue
        {
            private readonly List<StringSetPair> _pairs;

            public StringSetValue(List<StringSetPair> pairs, IEnumerable<Token> tokens)
            {
                _pairs = pairs;
                Original = new TokenValue(tokens);
            }

            public string CssText
            {
                get
                {
                    if (_pairs == null || _pairs.Count == 0) return Keywords.None;

                    return string.Join(", ", _pairs.Select(pair =>
                        pair.Name + " " + string.Join(" ", pair.ContentList.Select(c => c.CssText))));
                }
            }

            public TokenValue Original { get; }

            public TokenValue ExtractFor(string name) => Original;
        }
    }
}

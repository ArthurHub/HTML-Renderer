using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// <c>string-set</c> (CSS Generated Content 3 §2.7.1). Each name takes the GCPM content list
    /// (counters, <c>content()</c>, <c>attr()</c>, <c>string()</c>), not just the strings of the /TR grammar.
    /// </summary>
    internal sealed class StringSetValueConverter : IValueConverter
    {
        private readonly IValueConverter _contentListItemConverter;

        public StringSetValueConverter()
        {
            _contentListItemConverter = Converters.StringConverter
                .Or(Converters.CounterConverter)
                .Or(new ContentFunctionConverter())
                .Or(new StringFunctionConverter())
                .Or(Converters.AttrConverter);
        }

        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var first = value.OnlyOrDefault();

            if (first is KeywordToken keyword && keyword.Data.Isi(Keywords.None))
            {
                return new StringSetValue(null, value);
            }

            // ValueExtensions.ToList, not LINQ's: one token list per comma-separated pair
            var items = value.ToList();
            var pairs = new List<StringSetPair>();

            foreach (var item in items)
            {
                var pair = ParsePair(item);
                if (pair == null)
                    return null;

                pairs.Add(pair);
            }

            if (pairs.Count == 0)
                return null;

            return new StringSetValue(pairs, value);
        }

        private StringSetPair ParsePair(List<Token> tokens)
        {
            if (tokens.Count < 2)
                return null;

            if (!(tokens[0] is KeywordToken nameToken) || nameToken.Type != TokenType.Ident)
                return null;

            var name = nameToken.Data;

            var contentItems = tokens.Skip(1).ToItems();
            var values = new List<IPropertyValue>();

            foreach (var item in contentItems)
            {
                var converted = _contentListItemConverter.Convert(item);
                if (converted == null)
                    return null;

                values.Add(converted);
            }

            if (values.Count == 0)
                return null;

            return new StringSetPair(name, values);
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
                    if (_pairs == null || _pairs.Count == 0)
                        return Keywords.None;

                    var pairStrings = _pairs.Select(pair =>
                         {
                             var contentStr = string.Join(" ", pair.ContentList.Select(c => c.CssText));
                             return $"{pair.Name} {contentStr}";
                         });

                    return string.Join(", ", pairStrings);
                }
            }

            public TokenValue Original { get; }

            public TokenValue ExtractFor(string name)
            {
                return Original;
            }
        }
    }
}

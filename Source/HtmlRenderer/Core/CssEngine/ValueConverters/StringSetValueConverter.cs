using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Converter for the string-set property.
    /// Syntax: string-set: [ &lt;custom-ident&gt; &lt;content-list&gt; ]# | none
    /// where &lt;content-list&gt; = [ &lt;string&gt; | &lt;counter()&gt; | &lt;counters()&gt; | &lt;content()&gt; | &lt;attr()&gt; ]+
    /// </summary>
    internal sealed class StringSetValueConverter : IValueConverter
    {
        private readonly IValueConverter _contentListItemConverter;

        public StringSetValueConverter()
        {
            // Content list can contain: strings, counter(), counters(), content(), attr(), string()
            _contentListItemConverter = Converters.StringConverter
                .Or(Converters.CounterConverter)
                .Or(new ContentFunctionConverter())
                .Or(new StringFunctionConverter())
                .Or(Converters.AttrConverter);
        }

        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var first = value.OnlyOrDefault();

            // Check for "none" keyword
            if (first is KeywordToken keyword && keyword.Data.Isi(Keywords.None))
            {
                return new StringSetValue(null, value);
            }

            // Parse comma-separated list of name/content-list pairs using ToList()
            var items = value.ToList(); // This splits by commas, returns List<List<Token>>
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

            // First token should be an identifier (the name)
            if (!(tokens[0] is KeywordToken nameToken) || nameToken.Type != TokenType.Ident)
                return null;

            var name = nameToken.Data;

            // Remaining tokens form the content-list - split by whitespace
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

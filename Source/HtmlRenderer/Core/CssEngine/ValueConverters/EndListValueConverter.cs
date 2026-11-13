using System;
using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class EndListValueConverter : IValueConverter
    {
        private readonly IValueConverter _endConverter;
        private readonly IValueConverter _listConverter;

        private readonly IReadOnlyDictionary<string, List<Token>> _layerDefaults;
        private readonly IReadOnlyDictionary<string, string> _layerFallbacks;

        /// <summary>
        /// A list of layers, every one but the last converted by <paramref name="listConverter"/> and the last by
        /// <paramref name="endConverter"/> (the <c>background</c> shorthand: a final layer that alone may carry a colour).
        /// </summary>
        /// <param name="listConverter">The converter for every layer but the last.</param>
        /// <param name="endConverter">The converter for the last layer.</param>
        /// <param name="layerDefaults">
        /// For the longhands that hold one value per layer: the value a layer takes when its text leaves that longhand out
        /// (CSS Backgrounds 3 section 2.10). A longhand that is not listed (the colour, which only the last layer sets) is
        /// taken from the layers that do set it.
        /// </param>
        /// <param name="layerFallbacks">
        /// For a longhand whose omitted value comes from another longhand of the same layer rather than from its own initial
        /// value: <c>background-clip</c> takes what <c>background-origin</c> was given. Tried before <paramref name="layerDefaults"/>.
        /// </param>
        public EndListValueConverter(IValueConverter listConverter, IValueConverter endConverter,
            IReadOnlyDictionary<string, string> layerDefaults = null, IReadOnlyDictionary<string, string> layerFallbacks = null)
        {
            _listConverter = listConverter;
            _endConverter = endConverter;
            _layerFallbacks = layerFallbacks;

            // Tokenized once: ExtractFor runs for every longhand of every declaration, and the defaults never change.
            if (layerDefaults != null)
            {
                var defaults = new Dictionary<string, List<Token>>();
                foreach (var pair in layerDefaults) defaults[pair.Key] = Tokenize(pair.Value);
                _layerDefaults = defaults;
            }
        }

        private static List<Token> Tokenize(string text)
        {
            using var lexer = new Lexer(new TextSource(text)) { IsInValue = true };
            var tokens = new List<Token>();
            Token token;

            do
            {
                token = lexer.Get();
                if (token.Type != TokenType.EndOfFile) tokens.Add(token);
            } while (token.Type != TokenType.EndOfFile);

            tokens.Trim();
            return tokens;
        }

        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var items = value.ToList();
            var n = items.Count - 1;

            // A comma with nothing on one side of it is not a layer: the declaration is invalid. The layer converters accept an
            // empty layer, and the hole this left in the longhands' comma lists used to hide that.
            if (n > 0 && items.Exists(item => item.Count == 0)) return null;

            var values = new IPropertyValue[n + 1];

            for (var i = 0; i < n; i++)
            {
                values[i] = _listConverter.Convert(items[i]);

                if (values[i] == null) return null;
            }

            values[n] = _endConverter.Convert(items[n]);
            return values[n] != null ? new ListValue(values, value, _layerDefaults, _layerFallbacks) : null;
        }

        public IPropertyValue Construct(Property[] properties)
        {
            var valueList = new List<List<Token>>[properties.Length];
            var dummies = new Property[properties.Length];
            var max = 0;

            for (var i = 0; i < properties.Length; i++)
            {
                var value = properties[i].DeclaredValue;
                valueList[i] = value != null ? value.Original.ToList() : new List<List<Token>>();

                dummies[i] = PropertyFactory.Instance.CreateLonghand(properties[i].Name);
                max = Math.Max(max, valueList[i].Count);
            }

            var values = new IPropertyValue[max];

            for (var i = 0; i < max; i++)
            {
                for (var j = 0; j < dummies.Length; j++)
                {
                    var list = valueList[j];
                    var tokens = list.Count > i ? list[i] : Enumerable.Empty<Token>();
                    dummies[j].TrySetValue(new TokenValue(tokens));
                }

                var converter = i < max - 1 ? _listConverter : _endConverter;
                values[i] = converter.Construct(dummies);
            }

            return new ListValue(values, Enumerable.Empty<Token>(), _layerDefaults, _layerFallbacks);
        }

        private sealed class ListValue : IPropertyValue
        {
            private readonly IPropertyValue[] _values;
            private readonly IReadOnlyDictionary<string, List<Token>> _layerDefaults;
            private readonly IReadOnlyDictionary<string, string> _layerFallbacks;

            public ListValue(IPropertyValue[] values, IEnumerable<Token> tokens,
                IReadOnlyDictionary<string, List<Token>> layerDefaults, IReadOnlyDictionary<string, string> layerFallbacks)
            {
                _values = values;
                _layerDefaults = layerDefaults;
                _layerFallbacks = layerFallbacks;
                Original = new TokenValue(tokens);
            }

            public string CssText
            {
                get { return string.Join(", ", _values.Select(m => m.CssText)); }
            }

            public TokenValue Original { get; }

            private static List<Token> ToTokens(TokenValue value) => value == null ? null : new List<Token>(value);

            public TokenValue ExtractFor(string name)
            {
                var layers = new List<List<Token>>(_values.Length);

                foreach (var value in _values)
                {
                    var extracted = ToTokens(value.ExtractFor(name));

                    if ((extracted == null || extracted.Count == 0) && _layerFallbacks != null &&
                        _layerFallbacks.TryGetValue(name, out var fallbackName))
                    {
                        extracted = ToTokens(value.ExtractFor(fallbackName));
                    }

                    layers.Add(extracted is { Count: > 0 } ? extracted : null);
                }

                // No layer names this longhand: it is left to its initial value, which an empty value stands for.
                if (layers.TrueForAll(layer => layer == null)) return new TokenValue(Enumerable.Empty<Token>());

                List<Token> layerDefault = null;
                _layerDefaults?.TryGetValue(name, out layerDefault);

                var tokens = new List<Token>();

                // Layers stay comma-separated when exported to a longhand, matching CssText above. A layer that leaves a
                // per-layer longhand out takes that longhand's initial value so the list keeps one entry per layer; any other
                // longhand (the colour of the last layer) is only taken from the layers that set it. Without this the join left
                // a hole in the comma list, which no longhand accepts, and the whole longhand was lost.
                foreach (var layer in layers)
                {
                    var entry = layer ?? layerDefault;

                    if (entry == null) continue;

                    if (tokens.Count > 0) tokens.Add(Token.Comma);

                    tokens.AddRange(entry);
                }

                return new TokenValue(tokens);
            }
        }
    }
}

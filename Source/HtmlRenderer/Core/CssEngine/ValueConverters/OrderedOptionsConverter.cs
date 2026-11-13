using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class OrderedOptionsConverter : IValueConverter
    {
        private readonly IValueConverter[] _converters;
        private readonly bool _laterRequireFirst;

        public OrderedOptionsConverter(params IValueConverter[] converters)
        {
            _converters = converters;
        }

        /// <param name="laterRequireFirst">when true the converters after the first only take part if the first one consumed
        /// tokens, so an omitted first slot leaves the rest at their defaults and anything that was meant for them (the
        /// <c>/ &lt;size&gt;</c> of <c>&lt;position&gt; [ / &lt;size&gt; ]</c>) is left over and fails the match</param>
        /// <param name="converters">the options, in the order they are tried</param>
        public OrderedOptionsConverter(bool laterRequireFirst, params IValueConverter[] converters)
            : this(converters)
        {
            _laterRequireFirst = laterRequireFirst;
        }

        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var list = new List<Token>(value);
            var options = new IPropertyValue[_converters.Length];

            var firstConsumed = true;

            for (var i = 0; i < _converters.Length; i++)
            {
                var before = list.Count;
                options[i] = _laterRequireFirst && i > 0 && !firstConsumed
                    ? _converters[i].ConvertDefault()
                    : _converters[i].VaryStart(list);

                if (i == 0) firstConsumed = list.Count != before;

                if (options[i] == null) return null;
            }

            return list.Count == 0 ? new OptionsValue(options, value) : null;
        }

        public IPropertyValue Construct(Property[] properties)
        {
            var result = properties.Guard<OptionsValue>();

            if (result == null)
            {
                var values = new IPropertyValue[_converters.Length];

                for (var i = 0; i < _converters.Length; i++)
                {
                    var value = _converters[i].Construct(properties);

                    if (value == null) return null;

                    values[i] = value;
                }

                result = new OptionsValue(values, Enumerable.Empty<Token>());
            }

            return result;
        }

        private sealed class OptionsValue : IPropertyValue, IEnumerable<IPropertyValue>
        {
            private readonly IPropertyValue[] _options;

            public OptionsValue(IPropertyValue[] options, IEnumerable<Token> tokens)
            {
                _options = options;
                Original = new TokenValue(tokens);
            }

            IEnumerator<IPropertyValue> IEnumerable<IPropertyValue>.GetEnumerator()
            {
                return ((IEnumerable<IPropertyValue>) _options).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _options.GetEnumerator();
            }

            public string CssText
            {
                get
                {
                    return string.Join(" ",
                        _options.Where(m => !string.IsNullOrEmpty(m.CssText)).Select(m => m.CssText));
                }
            }

            public TokenValue Original { get; }

            public TokenValue ExtractFor(string name)
            {
                var tokens = new List<Token>();

                foreach (var option in _options)
                {
                    var extracted = option.ExtractFor(name);

                    if (extracted != null && extracted.Count > 0)
                    {
                        if (tokens.Count > 0) tokens.Add(Token.Whitespace);

                        tokens.AddRange(extracted);
                    }
                }

                return new TokenValue(tokens);
            }
        }
    }
}
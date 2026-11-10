using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// The 'animation' and 'transition' shorthands, where the first time is the duration and the second the
    /// delay (CSS Animations 1 §3.10, CSS Transitions 1 §2.5).
    /// </summary>
    internal sealed class TimeBasedShorthandConverter : IValueConverter
    {
        private readonly IValueConverter[] _converters;
        private readonly int _durationIndex;
        private readonly int _delayIndex;

        public TimeBasedShorthandConverter(int durationIndex, int delayIndex, params IValueConverter[] converters)
        {
            _converters = converters;
            _durationIndex = durationIndex;
            _delayIndex = delayIndex;
        }

        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var list = new List<Token>(value);
            if (list.Count == 0)
            {
                return null;
            }

            var options = new IPropertyValue[_converters.Length];
            var timeValuesFound = 0;

            var matched = new bool[_converters.Length];

            while (list.Count > 0)
            {
                var foundMatch = false;

                for (var i = 0; i < _converters.Length; i++)
                {
                    if (matched[i])
                    {
                        continue;
                    }

                    var isTimeConverter = i == _durationIndex || i == _delayIndex;

                    if (isTimeConverter)
                    {
                        var testList = new List<Token>(list);
                        if (TryMatchTime(testList, out var timeValue))
                        {
                            var targetIndex = timeValuesFound == 0 ? _durationIndex : _delayIndex;

                            if (targetIndex == i)
                            {
                                // This is the right slot for this time value - actually consume it,
                                // through the slot's converter so the value only extracts for its longhand
                                options[i] = _converters[i].VaryStart(list);
                                matched[i] = true;
                                timeValuesFound++;
                                foundMatch = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        var beforeCount = list.Count;
                        var testList = new List<Token>(list);
                        var result = _converters[i].VaryStart(testList);

                        if (result != null)
                        {
                            // A match that consumed nothing is only the converter's default
                            var consumed = beforeCount - testList.Count;
                            if (consumed > 0)
                            {
                                options[i] = result;
                                matched[i] = true;
                                list = testList;
                                foundMatch = true;
                                break;
                            }
                        }
                    }
                }

                if (!foundMatch)
                {
                    break;
                }
            }

            for (var i = 0; i < _converters.Length; i++)
            {
                if (options[i] == null)
                {
                    options[i] = _converters[i].ConvertDefault();
                }
            }

            if (list.Count == 0)
            {
                return new OptionsValue(options, value);
            }

            return null;
        }
        private bool TryMatchTime(List<Token> list, out IPropertyValue timeValue)
        {
            var testList = new List<Token>(list);
            var beforeCount = testList.Count;
            var result = Converters.TimeConverter.VaryStart(testList);

            if (result != null && testList.Count < beforeCount)
            {
                timeValue = result;
                list.Clear();
                list.AddRange(testList);
                return true;
            }

            timeValue = null;
            return false;
        }

        public IPropertyValue Construct(Property[] properties)
        {
            var result = properties.Guard<OptionsValue>();

            if (result != null) return result;

            var values = new IPropertyValue[_converters.Length];

            for (var i = 0; i < _converters.Length; i++)
            {
                var value = _converters[i].Construct(properties);

                if (value == null) return null;

                values[i] = value;
            }

            result = new OptionsValue(values, new Token[0]);

            return result;
        }

        private sealed class OptionsValue : IPropertyValue
        {
            private readonly IPropertyValue[] options;

            public OptionsValue(IPropertyValue[] options, IEnumerable<Token> tokens)
            {
                this.options = options;
                Original = new TokenValue(tokens);
            }

            public string CssText
            {
                get
                {
                    return string.Join(" ",
                     options.Where(m => !string.IsNullOrEmpty(m == null ? null : m.CssText)).Select(m => m.CssText));
                }
            }

            public TokenValue Original { get; }

            public TokenValue ExtractFor(string name)
            {
                var tokens = new List<Token>();

                foreach (var option in options)
                {
                    if (option == null) continue;

                    var extracted = option.ExtractFor(name);

                    if (extracted == null || extracted.Count == 0) continue;
                    if (tokens.Count > 0) tokens.Add(Token.Whitespace);
                    tokens.AddRange(extracted);
                }

                return new TokenValue(tokens);
            }
        }
    }
}

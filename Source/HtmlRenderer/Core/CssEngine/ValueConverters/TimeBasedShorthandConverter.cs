using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    /// Specialized converter for CSS properties with time-based disambiguation rules.
    /// Handles animation and transition shorthands where two time values must be
    /// interpreted as duration (first) and delay (second) per CSS spec.
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

            // Track which converters have been matched
            var matched = new bool[_converters.Length];

            // Parse tokens, applying special logic for time values
            while (list.Count > 0)
            {
                var foundMatch = false;

                // Try each unmatched converter
                for (var i = 0; i < _converters.Length; i++)
                {
                    if (matched[i])
                    {
                        continue;
                    }

                    // Special handling for time converters
                    var isTimeConverter = i == _durationIndex || i == _delayIndex;

                    if (isTimeConverter)
                    {
                        // Try to match a time value (without consuming)
                        var testList = new List<Token>(list);
                        if (TryMatchTime(testList, out var timeValue))
                        {
                            // We found a time value
                            // Assign to duration if it's the first time value, delay if it's the second
                            var targetIndex = timeValuesFound == 0 ? _durationIndex : _delayIndex;

                            if (targetIndex == i)
                            {
                                // This is the right slot for this time value - actually consume it
                                TryMatchTime(list, out timeValue);
                                options[i] = timeValue;
                                matched[i] = true;
                                timeValuesFound++;
                                foundMatch = true;
                                break;
                            }
                            // else: This time value belongs to a different slot, skip this converter for now
                        }
                        // else: Not a time value at all, continue to try other converters
                    }
                    else
                    {
                        // Non-time converter: try normal matching
                        var beforeCount = list.Count;
                        var testList = new List<Token>(list);
                        var result = _converters[i].VaryStart(testList);

                        if (result != null)
                        {
                            // Check if it actually consumed tokens (not just a default value)
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
                    // No converter matched the current position
                    break;
                }
            }

            // Fill in defaults for unmatched optional converters
            for (var i = 0; i < _converters.Length; i++)
            {
                if (options[i] == null)
                {
                    options[i] = _converters[i].ConvertDefault();
                }
            }

            // Success if all tokens were consumed
            if (list.Count == 0)
            {
                return new OptionsValue(options, value);
            }

            return null;
        }
        private bool TryMatchTime(List<Token> list, out IPropertyValue timeValue)
        {
            // Use the TimeConverter to try to match a time value
            var testList = new List<Token>(list);
            var beforeCount = testList.Count;
            var result = Converters.TimeConverter.VaryStart(testList);

            if (result != null && testList.Count < beforeCount)
            {
                // Successfully matched and consumed tokens
                timeValue = result;
                // Update the original list
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
                    // Output in canonical converter order
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

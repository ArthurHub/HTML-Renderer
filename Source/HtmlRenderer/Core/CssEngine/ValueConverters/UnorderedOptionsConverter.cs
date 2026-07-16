using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class UnorderedOptionsConverter : IValueConverter
    {
        private readonly IValueConverter[] converters;

        public UnorderedOptionsConverter(params IValueConverter[] converters)
        {
            this.converters = converters;
        }

        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var indices = Enumerable.Range(0, converters.Length).ToArray();

            // Try all permutations to find one that successfully consumes all tokens
            var perms = GetPermutations(indices).ToList();

            foreach (var perm in perms)
            {
                var list = new List<Token>(value);
                var options = new IPropertyValue[perm.Length];
                var success = TryOrder(perm, converters, list, options);
                if (success && list.Count == 0)
                {
                    // Map values to converter positions for ExtractFor functionality
                    var reorderedOptions = new IPropertyValue[converters.Length];

                    for (var i = 0; i < perm.Length; i++)
                    {
                        var converterIndex = perm[i];
                        reorderedOptions[converterIndex] = options[i];
                    }

                    return new OptionsValue(reorderedOptions, value);
                }
            }

            return null;
        }

        private static bool TryOrder(int[] indices, IValueConverter[] converters, List<Token> list, IPropertyValue[] options)
        {
            for (var i = 0; i < indices.Length; i++)
            {
                options[i] = converters[indices[i]].VaryStart(list);
                if (options[i] == null) return false;
            }
            return true;
        }

        private static IEnumerable<int[]> GetPermutations(int[] indices)
        {
            var list = new List<int>(indices);
            return Permute(list, 0).Select(l => l.ToArray());
        }

        private static IEnumerable<List<int>> Permute(List<int> list, int start)
        {
            if (start == list.Count - 1)
            {
                yield return new List<int>(list);
            }
            else
            {
                for (var i = start; i < list.Count; i++)
                {
                    Swap(list, start, i);
                    foreach (var perm in Permute(list, start + 1))
                    {
                        yield return perm;
                    }
                    Swap(list, start, i); // backtrack
                }
            }
        }

        private static void Swap<T>(List<T> list, int i, int j)
        {
            var temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }

        public IPropertyValue Construct(Property[] properties)
        {
            var result = properties.Guard<OptionsValue>();

            if (result != null) return result;

            var values = new IPropertyValue[converters.Length];

            for (var i = 0; i < converters.Length; i++)
            {
                var value = converters[i].Construct(properties);

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

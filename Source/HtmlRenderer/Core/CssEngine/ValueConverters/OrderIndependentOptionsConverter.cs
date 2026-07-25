using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    // Like UnorderedOptionsConverter (the WithAny combinator), but matches the operands in any order rather
    // than only in declaration order. WithAny matches greedily left-to-right, which fails whenever an
    // earlier converter claims a token a later one also accepts - e.g. "list-style: none square", where
    // list-style-type takes "none" before list-style-image can.
    //
    // Use this ONLY for shorthands whose operands are order-independent with no positional tie-break. Some
    // "||" (any-order) shorthands still assign longhands by position and so must NOT be reordered: animation's
    // first <time> is animation-duration and the second is animation-delay (CSS Animations 1 3), background's
    // first <visual-box> is background-origin and the second is background-clip (CSS Backgrounds 3 2.10), and
    // transform-origin keeps its length form axis-ordered. The declaration-order matcher (WithAny) already
    // satisfies those positional rules, so those shorthands stay on it; list-style (CSS Lists 3) and columns
    // (CSS Multi-column 1) have no such rule and genuinely need order-independent matching.
    internal sealed class OrderIndependentOptionsConverter : IValueConverter
    {
        // The reorder search is O(n!); every shorthand that uses this converter has only a few operands, so
        // cap it well below the large "||" shorthands (background has 7-8 operands) as a safety net.
        private const int MaxReorderableConverters = 6;

        private readonly IValueConverter[] _converters;

        public OrderIndependentOptionsConverter(params IValueConverter[] converters)
        {
            _converters = converters;
        }

        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            // Fast path: match the operands in declaration order, identical to WithAny. This covers every
            // already-ordered value and keeps its serialization byte-for-byte the same.
            var list = new List<Token>(value);
            var options = new IPropertyValue[_converters.Length];
            var matched = true;

            for (var i = 0; i < _converters.Length; i++)
            {
                options[i] = _converters[i].VaryAll(list);

                if (options[i] == null)
                {
                    matched = false;
                    break;
                }
            }

            if (matched && list.Count == 0) return new OptionsValue(options, value);

            // Fallback: search converter orderings until one consumes every token, then keep the results in
            // canonical converter order so ExtractFor still resolves each longhand.
            if (_converters.Length > MaxReorderableConverters) return null;

            var reordered = new IPropertyValue[_converters.Length];
            var used = new bool[_converters.Length];
            var remaining = new List<Token>(value);

            return TryReorder(remaining, used, reordered)
                ? new OptionsValue(reordered, value)
                : null;
        }

        // Depth-first search over converter orderings: each step lets an unused converter consume a leading
        // run of tokens and recurses on the rest; once every token is consumed, any converter left unused is
        // assigned its empty default (non-null only for optional operands).
        private bool TryReorder(List<Token> remaining, bool[] used, IPropertyValue[] options)
        {
            if (remaining.Count == 0)
            {
                for (var i = 0; i < _converters.Length; i++)
                {
                    if (used[i]) continue;

                    var empty = _converters[i].ConvertDefault();

                    if (empty == null) return false;

                    options[i] = empty;
                }

                return true;
            }

            for (var i = 0; i < _converters.Length; i++)
            {
                if (used[i]) continue;

                var snapshot = new List<Token>(remaining);
                var value = _converters[i].VaryStart(remaining);

                if (value != null && remaining.Count < snapshot.Count)
                {
                    used[i] = true;
                    options[i] = value;

                    if (TryReorder(remaining, used, options)) return true;

                    used[i] = false;
                }

                remaining.Clear();
                remaining.AddRange(snapshot);
            }

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

            result = new OptionsValue(values, Enumerable.Empty<Token>());

            return result;
        }

        private sealed class OptionsValue : IPropertyValue
        {
            private readonly IPropertyValue[] _options;

            public OptionsValue(IPropertyValue[] options, IEnumerable<Token> tokens)
            {
                _options = options;
                Original = new TokenValue(tokens);
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

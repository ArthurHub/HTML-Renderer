using System;
using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class FunctionValueConverter : IValueConverter
    {
        private readonly IValueConverter _arguments;
        private readonly string _name;

        public FunctionValueConverter(string name, IValueConverter arguments)
        {
            _name = name;
            _arguments = arguments;
        }

        public IPropertyValue Convert(IEnumerable<Token> value)
        {
            var function = value.OnlyOrDefault() as FunctionToken;

            if (!Check(function)) return null;

            var args = _arguments.Convert(function.ArgumentTokens);
            return args != null ? new FunctionValue(_name, args, value) : null;
        }

        public IPropertyValue Construct(Property[] properties)
        {
            return properties.Guard<FunctionValue>();
        }

        private bool Check(FunctionToken function)
        {
            return function != null && function.Data.Equals(_name, StringComparison.OrdinalIgnoreCase);
        }

        private sealed class FunctionValue : IPropertyValue
        {
            private readonly IPropertyValue _arguments;
            private readonly string _name;

            public FunctionValue(string name, IPropertyValue arguments, IEnumerable<Token> tokens)
            {
                _name = name;
                _arguments = arguments;
                Original = new TokenValue(tokens);
            }

            public string CssText => _name.StylesheetFunction(_arguments.CssText);

            public TokenValue Original { get; }

            public TokenValue ExtractFor(string name)
            {
                return Original;
            }
        }
    }
}
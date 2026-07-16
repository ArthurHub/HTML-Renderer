using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal abstract class DeclarationRule : Rule, IProperties
    {
        private readonly string _name;

        internal DeclarationRule(RuleType type, string name, StylesheetParser parser)
            : base(type, parser)
        {
            _name = name;
        }

        internal void SetProperty(Property property)
        {
            foreach (var declaration in Declarations)
            {
                if (!declaration.Name.Is(property.Name)) continue;

                ReplaceChild(declaration, property);
                return;
            }

            AppendChild(property);
        }

        public override void ToCss(TextWriter writer, IStyleFormatter formatter)
        {
            var rules = new FormatTransporter(Declarations);
            var content = formatter.Style("@" + _name, rules);
            writer.Write(content);
        }

        public string this[string propertyName] => GetValue(propertyName);
        public IEnumerable<Property> Declarations => Children.OfType<Property>();
        public int Length => Declarations.Count();

        public string GetPropertyValue(string propertyName)
        {
            return GetValue(propertyName);
        }

        public string GetPropertyPriority(string propertyName)
        {
            return null;
        }

        public void SetProperty(string propertyName, string propertyValue, string priority = null)
        {
            SetValue(propertyName, propertyValue);
        }

        public string RemoveProperty(string propertyName)
        {
            foreach (var declaration in Declarations)
            {
                if (!declaration.HasValue || !declaration.Name.Is(propertyName)) continue;

                var value = declaration.Value;
                RemoveChild(declaration);
                return value;
            }

            return null;
        }

        public IEnumerator<IProperty> GetEnumerator()
        {
            return Declarations.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private struct FormatTransporter : IStyleFormattable
        {
            private readonly IEnumerable<Property> _properties;

            public FormatTransporter(IEnumerable<Property> properties)
            {
                _properties = properties.Where(m => m.HasValue);
            }

            public void ToCss(TextWriter writer, IStyleFormatter formatter)
            {
                var properties = _properties.Select(m => m.ToCss(formatter));
                var content = formatter.Declarations(properties);
                writer.Write(content);
            }
        }

        protected abstract Property CreateNewProperty(string name);

        protected string GetValue(string propertyName)
        {
            foreach (var declaration in Declarations)
                if (declaration.HasValue && declaration.Name.Is(propertyName))
                    return declaration.Value;

            return string.Empty;
        }

        protected void SetValue(string propertyName, string valueText)
        {
            foreach (var declaration in Declarations)
            {
                if (!declaration.Name.Is(propertyName)) continue;

                var value = Parser.ParseValue(valueText);
                declaration.TrySetValue(value);
                return;
            }

            var property = CreateNewProperty(propertyName);

            if (property == null) return;
            {
                var value = Parser.ParseValue(valueText);
                property.TrySetValue(value);
                AppendChild(property);
            }
        }
    }
}
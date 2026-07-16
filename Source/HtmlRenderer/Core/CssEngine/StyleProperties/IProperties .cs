using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal interface IProperties : IEnumerable<IProperty>
    {
        string this[string propertyName] { get; }
        int Length { get; }
        string GetPropertyValue(string propertyName);
        string GetPropertyPriority(string propertyName);
        void SetProperty(string propertyName, string propertyValue, string priority = null);
        string RemoveProperty(string propertyName);
    }
}
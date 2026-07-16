using System;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal class ParseException : Exception
    {
        public ParseException(string message) : base(message)
        {
        }
    }
}
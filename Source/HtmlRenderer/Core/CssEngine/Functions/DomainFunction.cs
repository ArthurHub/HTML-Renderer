using System;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class DomainFunction : DocumentFunction
    {
        private readonly string _subdomain;

        public DomainFunction(string url) : base(FunctionNames.Domain, url)
        {
            _subdomain = "." + url;
        }

        public override bool Matches(Url url)
        {
            var domain = url.HostName;
            return domain.Isi(Data) || domain.EndsWith(_subdomain, StringComparison.OrdinalIgnoreCase);
        }
    }
}
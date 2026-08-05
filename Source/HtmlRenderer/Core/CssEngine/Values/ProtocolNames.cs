namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal static class ProtocolNames
    {
        public static readonly string Http = "http";
        public static readonly string Https = "https";
        public static readonly string Ftp = "ftp";
        public static readonly string JavaScript = "javascript";
        public static readonly string Data = "data";
        public static readonly string Mailto = "mailto";
        public static readonly string File = "file";
        public static readonly string Ws = "ws";
        public static readonly string Wss = "wss";
        public static readonly string Telnet = "telnet";
        public static readonly string Ssh = "ssh";
        public static readonly string Gopher = "gopher";
        public static readonly string Blob = "blob";

        private static readonly string[] RelativeProtocols =
        {
            Http,
            Https,
            Ftp,
            File,
            Ws,
            Wss,
            Gopher
        };

        private static readonly string[] OriginalableProtocols =
        {
            Http,
            Https,
            Ftp,
            Ws,
            Wss,
            Gopher
        };

        public static bool IsRelative(string protocol)
        {
            return RelativeProtocols.Contains(protocol);
        }

        public static bool IsOriginable(string protocol)
        {
            return OriginalableProtocols.Contains(protocol);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    /// <summary>
    ///     Url class according to RFC3986. 
    /// </summary>
    internal sealed class Url : IEquatable<Url>
    {
        private const string CurrentDirectory = ".";
        private const string CurrentDirectoryAlternative = "%2e";
        private const string UpperDirectory = "..";
        private static readonly string[] UpperDirectoryAlternatives = { "%2e%2e", ".%2e", "%2e." };
        private static readonly Url DefaultBase = new Url(string.Empty, string.Empty, string.Empty);

        private string _fragment;
        private string _query;
        private string _path;
        private string _scheme;
        private string _port;
        private string _host;
        private bool _relative;

        public static implicit operator Uri(Url value)
        {
            return new Uri(value.Serialize(), value.IsRelative ? UriKind.Relative : UriKind.Absolute);
        }

        private Url(string scheme, string host, string port)
        {
            Data = string.Empty;
            _path = string.Empty;
            _scheme = scheme;
            _host = host;
            _port = port;
            _relative = ProtocolNames.IsRelative(_scheme);
        }

        public Url(string address)
        {
            IsInvalid = ParseUrl(address);
        }

        public Url(Url baseAddress, string relativeAddress)
        {
            IsInvalid = ParseUrl(relativeAddress, baseAddress);
        }

        public Url(Url address)
        {
            _fragment = address._fragment;
            _query = address._query;
            _path = address._path;
            _scheme = address._scheme;
            _port = address._port;
            _host = address._host;
            UserName = address.UserName;
            Password = address.Password;
            _relative = address._relative;
            Data = address.Data;
        }

        public static Url Create(string address)
        {
            return new Url(address);
        }

        public static Url Convert(Uri uri)
        {
            return new Url(uri.OriginalString);
        }

        public string Origin
        {
            get
            {
                if (_scheme.Is(ProtocolNames.Blob))
                {
                    var url = new Url(Data);

                    if (!url.IsInvalid) return url.Origin;
                }
                else if (ProtocolNames.IsOriginable(_scheme))
                {
                    var output = Pool.NewStringBuilder();

                    if (string.IsNullOrEmpty(_host)) return output.ToPool();

                    if (!string.IsNullOrEmpty(_scheme)) output.Append(_scheme).Append(Symbols.Colon);

                    output.Append(Symbols.Solidus).Append(Symbols.Solidus).Append(_host);

                    if (!string.IsNullOrEmpty(_port)) output.Append(Symbols.Colon).Append(_port);

                    return output.ToPool();
                }

                return null;
            }
        }

        public bool IsInvalid { get; private set; }

        public bool IsRelative => _relative && string.IsNullOrEmpty(_scheme);

        public string UserName { get; set; }
        public string Password { get; set; }
        public string Data { get; private set; }

        public string Fragment
        {
            get => _fragment;
            set
            {
                if (value == null)
                    _fragment = null;
                else
                    ParseFragment(value, 0);
            }
        }

        public string Host
        {
            get => HostName + (string.IsNullOrEmpty(_port) ? string.Empty : ":" + _port);
            set => ParseHostName(value ?? string.Empty, 0, false, true);
        }

        public string HostName
        {
            get => _host;
            set => ParseHostName(value ?? string.Empty, 0, true);
        }

        public string Href
        {
            get => Serialize();
            set => IsInvalid = ParseUrl(value ?? string.Empty);
        }

        public string Path
        {
            get => _path;
            set => ParsePath(value ?? string.Empty, 0, true);
        }

        public string Port
        {
            get => _port;
            set => ParsePort(value ?? string.Empty, 0, true);
        }

        public string Scheme
        {
            get => _scheme;
            set => ParseScheme(value ?? string.Empty, true);
        }

        public string Query
        {
            get => _query;
            set => ParseQuery(value ?? string.Empty, 0, true);
        }

        public override int GetHashCode() => new
        {
            _fragment,
            _query,
            _path,
            _scheme,
            _port,
            _host,
            UserName,
            Password,
            _relative,
            Data
        }.GetHashCode();

        public override bool Equals(object obj)
        {
            return obj is Url url && Equals(url);
        }

        public bool Equals(Url other)
        {
            return _fragment.Is(other._fragment) && _query.Is(other._query) &&
                   _path.Is(other._path) && _scheme.Isi(other._scheme) &&
                   _port.Is(other._port) && _host.Isi(other._host) &&
                   UserName.Is(other.UserName) && Password.Is(other.Password) &&
                   Data.Is(other.Data);
        }

        public override string ToString()
        {
            return Serialize();
        }

        private string Serialize()
        {
            var output = Pool.NewStringBuilder();

            if (!string.IsNullOrEmpty(_scheme)) output.Append(_scheme).Append(Symbols.Colon);

            if (_relative)
            {
                if (!string.IsNullOrEmpty(_host) || !string.IsNullOrEmpty(_scheme))
                {
                    output.Append(Symbols.Solidus).Append(Symbols.Solidus);

                    if (!string.IsNullOrEmpty(UserName) || Password != null)
                    {
                        output.Append(UserName);

                        if (Password != null) output.Append(Symbols.Colon).Append(Password);

                        output.Append(Symbols.At);
                    }

                    output.Append(_host);

                    if (!string.IsNullOrEmpty(_port)) output.Append(Symbols.Colon).Append(_port);

                    output.Append(Symbols.Solidus);
                }

                output.Append(_path);
            }
            else
            {
                output.Append(Data);
            }

            if (_query != null) output.Append(Symbols.QuestionMark).Append(_query);

            if (_fragment != null) output.Append(Symbols.Num).Append(_fragment);

            return output.ToPool();
        }

        private bool ParseUrl(string input, Url baseUrl = null)
        {
            Reset(baseUrl ?? DefaultBase);
            return !ParseScheme(input.Trim());
        }

        private void Reset(Url baseUrl)
        {
            Data = string.Empty;
            _scheme = baseUrl._scheme;
            _host = baseUrl._host;
            _path = baseUrl._path;
            _port = baseUrl._port;
            _relative = ProtocolNames.IsRelative(_scheme);
        }

        private bool ParseScheme(string input, bool onlyScheme = false)
        {
            if (input.Length <= 0 || !input[0].IsLetter()) return !onlyScheme && RelativeState(input, 0);

            var index = 1;

            while (index < input.Length)
            {
                var c = input[index];

                if (c.IsAlphanumericAscii() || c == Symbols.Plus || c == Symbols.Minus || c == Symbols.Dot)
                {
                    index++;
                }
                else if (c == Symbols.Colon)
                {
                    var originalScheme = _scheme;
                    _scheme = input.Substring(0, index).ToLowerInvariant();

                    if (!onlyScheme)
                    {
                        _relative = ProtocolNames.IsRelative(_scheme);

                        if (_scheme.Is(ProtocolNames.File))
                        {
                            _host = string.Empty;
                            _port = string.Empty;
                            return RelativeState(input, index + 1);
                        }

                        if (!_relative)
                        {
                            _host = string.Empty;
                            _port = string.Empty;
                            _path = string.Empty;
                            return ParseSchemeData(input, index + 1);
                        }

                        if (_scheme.Is(originalScheme))
                        {
                            c = input[++index];

                            if (c == Symbols.Solidus && index + 2 < input.Length &&
                                input[index + 1] == Symbols.Solidus)
                                return IgnoreSlashesState(input, index + 2);

                            return RelativeState(input, index);
                        }

                        if (index < input.Length - 1 && input[++index] == Symbols.Solidus &&
                            ++index < input.Length && input[index] == Symbols.Solidus)
                            index++;

                        return IgnoreSlashesState(input, index);
                    }

                    return true;
                }
                else
                {
                    break;
                }
            }

            return !onlyScheme && RelativeState(input, 0);
        }

        private bool ParseSchemeData(string input, int index)
        {
            var buffer = Pool.NewStringBuilder();

            while (index < input.Length)
            {
                var c = input[index];

                if (c == Symbols.QuestionMark)
                {
                    Data = buffer.ToPool();
                    return ParseQuery(input, index + 1);
                }

                if (c == Symbols.Num)
                {
                    Data = buffer.ToPool();
                    return ParseFragment(input, index + 1);
                }

                if (c == Symbols.Percent && index + 2 < input.Length && input[index + 1].IsHex() &&
                    input[index + 2].IsHex())
                {
                    buffer.Append(input[index++]);
                    buffer.Append(input[index++]);
                    buffer.Append(input[index]);
                }
                else if (c.IsInRange(Symbols.Space, Symbols.Tilde))
                {
                    buffer.Append(c);
                }
                else if (c != Symbols.Tab && c != Symbols.LineFeed && c != Symbols.CarriageReturn)
                {
                    index += Utf8PercentEncode(buffer, input, index);
                }

                index++;
            }

            Data = buffer.ToPool();
            return true;
        }

        private bool RelativeState(string input, int index)
        {
            _relative = true;

            if (index != input.Length)
            {
                switch (input[index])
                {
                    case Symbols.QuestionMark:
                        return ParseQuery(input, index + 1);

                    case Symbols.Num:
                        return ParseFragment(input, index + 1);

                    case Symbols.Solidus:
                    case Symbols.ReverseSolidus:
                        if (index == input.Length - 1) return ParsePath(input, index);

                        var c = input[++index];

                        if (c.IsOneOf(Symbols.Solidus, Symbols.ReverseSolidus))
                            return _scheme.Is(ProtocolNames.File)
                                ? ParseFileHost(input, index + 1)
                                : IgnoreSlashesState(input, index + 1);

                        if (!_scheme.Is(ProtocolNames.File)) return ParsePath(input, index - 1);

                        _host = string.Empty;
                        _port = string.Empty;

                        return ParsePath(input, index - 1);
                }

                if (input[index].IsLetter() &&
                    _scheme.Is(ProtocolNames.File) &&
                    index + 1 < input.Length &&
                    input[index + 1].IsOneOf(Symbols.Colon, Symbols.Solidus) &&
                    (index + 2 == input.Length || input[index + 2].IsOneOf(Symbols.Solidus, Symbols.ReverseSolidus,
                        Symbols.Num, Symbols.QuestionMark)))
                {
                    _host = string.Empty;
                    _path = string.Empty;
                    _port = string.Empty;
                }

                return ParsePath(input, index);
            }

            return true;
        }

        private bool IgnoreSlashesState(string input, int index)
        {
            while (index < input.Length)
            {
                if (!input[index].IsOneOf(Symbols.ReverseSolidus, Symbols.Solidus)) return ParseAuthority(input, index);

                index++;
            }

            return false;
        }

        private bool ParseAuthority(string input, int index)
        {
            var start = index;
            var buffer = Pool.NewStringBuilder();
            var user = default(string);
            var pass = default(string);

            while (index < input.Length)
            {
                var c = input[index];

                if (c == Symbols.At)
                {
                    if (user == null)
                        user = buffer.ToString();
                    else
                        pass = buffer.ToString();

                    UserName = user;
                    Password = pass;
                    buffer.Append("%40");
                    start = index + 1;
                }
                else if (c == Symbols.Colon && user == null)
                {
                    user = buffer.ToString();
                    pass = string.Empty;
                    buffer.Clear();
                }
                else if (c == Symbols.Percent && index + 2 < input.Length && input[index + 1].IsHex() &&
                         input[index + 2].IsHex())
                {
                    buffer.Append(input[index++]).Append(input[index++]).Append(input[index]);
                }
                else if (c.IsOneOf(Symbols.Tab, Symbols.LineFeed, Symbols.CarriageReturn))
                {
                    // Parse Error
                }
                else if (c.IsOneOf(Symbols.Solidus, Symbols.ReverseSolidus, Symbols.Num, Symbols.QuestionMark))
                {
                    break;
                }
                else if (c != Symbols.Colon &&
                         (c == Symbols.Num || c == Symbols.QuestionMark || c.IsNormalPathCharacter()))
                {
                    buffer.Append(c);
                }
                else
                {
                    index += Utf8PercentEncode(buffer, input, index);
                }

                index++;
            }

            buffer.ToPool();
            return ParseHostName(input, start);
        }

        private bool ParseFileHost(string input, int index)
        {
            var start = index;
            _path = string.Empty;

            while (index < input.Length)
            {
                var c = input[index];

                if (c == Symbols.Solidus ||
                    c == Symbols.ReverseSolidus ||
                    c == Symbols.Num ||
                    c == Symbols.QuestionMark)
                    break;

                index++;
            }

            var length = index - start;

            if (length == 2 &&
                input[index - 2].IsLetter() &&
                (input[index - 1] == Symbols.Pipe || input[index - 1] == Symbols.Colon))
                return ParsePath(input, index - 2);
            if (length != 0) _host = SanatizeHost(input, start, length);

            return ParsePath(input, index);
        }

        private bool ParseHostName(string input, int index, bool onlyHost = false, bool onlyPort = false)
        {
            var inBracket = false;
            var start = index;

            while (index < input.Length)
            {
                var c = input[index];

                switch (c)
                {
                    case Symbols.SquareBracketClose:
                        inBracket = false;
                        break;

                    case Symbols.SquareBracketOpen:
                        inBracket = true;
                        break;

                    case Symbols.Colon:
                        if (inBracket)
                            break;

                        _host = SanatizeHost(input, start, index - start);

                        if (!onlyHost) return ParsePort(input, index + 1, onlyPort);

                        return true;

                    case Symbols.Solidus:
                    case Symbols.ReverseSolidus:
                    case Symbols.Num:
                    case Symbols.QuestionMark:
                        _host = SanatizeHost(input, start, index - start);
                        var error = string.IsNullOrEmpty(_host);

                        if (!onlyHost) return ParsePath(input, index) && !error;

                        return !error;
                }

                index++;
            }

            _host = SanatizeHost(input, start, index - start);

            if (!onlyHost)
            {
                _path = string.Empty;
                _query = null;
                _fragment = null;
            }

            return true;
        }

        private bool ParsePort(string input, int index, bool onlyPort = false)
        {
            var start = index;

            while (index < input.Length)
            {
                var c = input[index];

                if (c == Symbols.QuestionMark || c == Symbols.Solidus || c == Symbols.ReverseSolidus ||
                    c == Symbols.Num)
                    break;
                if (c.IsDigit() || c == Symbols.Tab || c == Symbols.LineFeed || c == Symbols.CarriageReturn)
                    index++;
                else
                    return false;
            }

            _port = SanatizePort(input, start, index - start);

            if (PortNumbers.GetDefaultPort(_scheme) == _port) _port = string.Empty;

            if (!onlyPort)
            {
                _path = string.Empty;
                return ParsePath(input, index);
            }

            return true;
        }

        private bool ParsePath(string input, int index, bool onlyPath = false)
        {
            var init = index;

            if (index < input.Length &&
                (input[index] == Symbols.Solidus || input[index] == Symbols.ReverseSolidus))
                index++;

            var paths = new List<string>();

            if (!onlyPath && !string.IsNullOrEmpty(_path) && index - init == 0)
            {
                var split = _path.Split(Symbols.Solidus);

                if (split.Length > 1)
                {
                    paths.AddRange(split);
                    paths.RemoveAt(split.Length - 1);
                }
            }

            var originalCount = paths.Count;
            var buffer = Pool.NewStringBuilder();

            while (index <= input.Length)
            {
                var c = index == input.Length ? Symbols.EndOfFile : input[index];
                var breakNow = !onlyPath && (c == Symbols.Num || c == Symbols.QuestionMark);

                if (c == Symbols.EndOfFile || c == Symbols.Solidus || c == Symbols.ReverseSolidus || breakNow)
                {
                    var path = buffer.ToString();
                    var close = false;
                    buffer.Clear();

                    if (path.Isi(CurrentDirectoryAlternative))
                        path = CurrentDirectory;
                    else if (path.Isi(UpperDirectoryAlternatives[0]) ||
                             path.Isi(UpperDirectoryAlternatives[1]) ||
                             path.Isi(UpperDirectoryAlternatives[2]))
                        path = UpperDirectory;

                    if (path.Is(UpperDirectory))
                    {
                        if (paths.Count > 0) paths.RemoveAt(paths.Count - 1);

                        close = true;
                    }
                    else if (!path.Is(CurrentDirectory))
                    {
                        if (_scheme.Is(ProtocolNames.File) &&
                            paths.Count == originalCount &&
                            path.Length == 2 &&
                            path[0].IsLetter() &&
                            path[1] == Symbols.Pipe)
                        {
                            path = path.Replace(Symbols.Pipe, Symbols.Colon);
                            paths.Clear();
                        }

                        paths.Add(path);
                    }
                    else
                    {
                        close = true;
                    }

                    if (close && c != Symbols.Solidus && c != Symbols.ReverseSolidus) paths.Add(string.Empty);

                    if (breakNow) break;
                }
                else if (c == Symbols.Percent &&
                         index + 2 < input.Length &&
                         input[index + 1].IsHex() &&
                         input[index + 2].IsHex())
                {
                    buffer.Append(input[index++]);
                    buffer.Append(input[index++]);
                    buffer.Append(input[index]);
                }
                else if (c == Symbols.Tab || c == Symbols.LineFeed || c == Symbols.CarriageReturn)
                {
                    // Parse Error
                }
                else if (c.IsNormalPathCharacter())
                {
                    buffer.Append(c);
                }
                else
                {
                    index += Utf8PercentEncode(buffer, input, index);
                }

                index++;
            }

            buffer.ToPool();
            _path = string.Join("/", paths);

            if (index < input.Length)
            {
                if (input[index] == Symbols.QuestionMark) return ParseQuery(input, index + 1);

                return ParseFragment(input, index + 1);
            }

            return true;
        }

        private bool ParseQuery(string input, int index, bool onlyQuery = false)
        {
            var buffer = Pool.NewStringBuilder();
            var fragment = false;

            while (index < input.Length)
            {
                var c = input[index];
                fragment = !onlyQuery && input[index] == Symbols.Num;

                if (fragment) break;

                if (c.IsNormalQueryCharacter())
                    buffer.Append(c);
                else
                    index += Utf8PercentEncode(buffer, input, index);

                index++;
            }

            _query = buffer.ToPool();
            return !fragment || ParseFragment(input, index + 1);
        }

        private bool ParseFragment(string input, int index)
        {
            var buffer = Pool.NewStringBuilder();

            while (index < input.Length)
            {
                var c = input[index];

                switch (c)
                {
                    case Symbols.EndOfFile:
                    case Symbols.Null:
                    case Symbols.Tab:
                    case Symbols.LineFeed:
                    case Symbols.CarriageReturn:
                        break;
                    default:
                        buffer.Append(c);
                        break;
                }

                index++;
            }

            _fragment = buffer.ToPool();
            return true;
        }

        private static int Utf8PercentEncode(StringBuilder buffer, string source, int index)
        {
            var length = char.IsSurrogatePair(source, index) ? 2 : 1;
            var bytes = TextEncoding.Utf8.GetBytes(source.Substring(index, length));

            foreach (var byteVal in bytes) buffer.Append(Symbols.Percent).Append(byteVal.ToString("X2"));

            return length - 1;
        }

        private static string SanatizeHost(string hostName, int start, int length)
        {
            if (length > 1 && hostName[start] == Symbols.SquareBracketOpen &&
                hostName[start + length - 1] == Symbols.SquareBracketClose)
                return hostName.Substring(start, length);

            var chars = new byte[4 * length];
            var count = 0;
            var n = start + length;

            for (var i = start; i < n; i++)
                switch (hostName[i])
                {
                    // U+0000, U+0009, U+000A, U+000D, U+0020, "#", "%", "/", ":", "?", "@", "[", "\", and "]"
                    case Symbols.Null:
                    case Symbols.Tab:
                    case Symbols.Space:
                    case Symbols.LineFeed:
                    case Symbols.CarriageReturn:
                    case Symbols.Num:
                    case Symbols.Solidus:
                    case Symbols.Colon:
                    case Symbols.QuestionMark:
                    case Symbols.At:
                    case Symbols.SquareBracketOpen:
                    case Symbols.SquareBracketClose:
                    case Symbols.ReverseSolidus:
                        break;
                    case Symbols.Dot:
                        chars[count++] = (byte)hostName[i];
                        break;
                    case Symbols.Percent:
                        if (i + 2 < n && hostName[i + 1].IsHex() && hostName[i + 2].IsHex())
                        {
                            var weight = hostName[i + 1].FromHex() * 16 + hostName[i + 2].FromHex();
                            chars[count++] = (byte)weight;
                            i += 2;
                        }
                        else
                        {
                            chars[count++] = (byte)Symbols.Percent;
                        }

                        break;
                    default:

                        if (Symbols.Punycode.TryGetValue(hostName[i], out var chr))
                        {
                            chars[count++] = (byte)chr;
                        }
                        else if (hostName[i].IsAlphanumericAscii() == false)
                        {
                            var l = i + 1 < n && char.IsSurrogatePair(hostName, i) ? 2 : 1;

                            if (l == 1 && hostName[i] != Symbols.Minus && !char.IsLetterOrDigit(hostName[i])) break;

                            var bytes = TextEncoding.Utf8.GetBytes(hostName.Substring(i, l));

                            foreach (var byteVal in bytes) chars[count++] = byteVal;

                            i += l - 1;
                        }
                        else
                        {
                            chars[count++] = (byte)char.ToLowerInvariant(hostName[i]);
                        }

                        break;
                }

            return TextEncoding.Utf8.GetString(chars, 0, count);
        }

        private static string SanatizePort(string port, int start, int length)
        {
            var chars = new char[length];
            var count = 0;
            var n = start + length;

            for (var i = start; i < n; i++)
                switch (port[i])
                {
                    case Symbols.Tab:
                    case Symbols.LineFeed:
                    case Symbols.CarriageReturn:
                        break;
                    default:
                        if (count == 1 && chars[0] == '0')
                            chars[0] = port[i];
                        else
                            chars[count++] = port[i];

                        break;
                }

            return new string(chars, 0, count);
        }
    }
}
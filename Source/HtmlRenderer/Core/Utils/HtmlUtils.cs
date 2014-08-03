// "Therefore those skilled at the unorthodox
// are infinite as heaven and earth,
// inexhaustible as the great rivers.
// When they come to an end,
// they begin again,
// like the days and months;
// they die and are reborn,
// like the four seasons."
// 
// - Sun Tsu,
// "The Art of War"

using System;
using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.Utils
{
    internal static class HtmlUtils
    {
        #region Fields and Consts

        /// <summary>
        /// List of html tags that don't have content
        /// </summary>
        private static readonly List<string> _list = new List<string>(
            new[]
            {
                "area", "base", "basefont", "br", "col",
                "frame", "hr", "img", "input", "isindex",
                "link", "meta", "param"
            }
            );

        /// <summary>
        /// the html encode\decode pairs
        /// </summary>
        private static readonly KeyValuePair<string, string>[] _encodeDecode = new[]
        {
            new KeyValuePair<string, string>("&lt;", "<"),
            new KeyValuePair<string, string>("&gt;", ">"),
            new KeyValuePair<string, string>("&quot;", "\""),
            new KeyValuePair<string, string>("&amp;", "&"),
        };

        /// <summary>
        /// the html decode only pairs
        /// </summary>
        private static readonly Dictionary<string, char> _decodeOnly = new Dictionary<string, char>(StringComparer.InvariantCultureIgnoreCase);

        #endregion


        /// <summary>
        /// Init.
        /// </summary>
        static HtmlUtils()
        {
            _decodeOnly["nbsp"] = ' ';
            _decodeOnly["rdquo"] = '"';
            _decodeOnly["lsquo"] = '\'';
            _decodeOnly["apos"] = '\'';

            // ISO 8859-1 Symbols
            _decodeOnly["iexcl"] = Convert.ToChar(161);
            _decodeOnly["cent"] = Convert.ToChar(162);
            _decodeOnly["pound"] = Convert.ToChar(163);
            _decodeOnly["curren"] = Convert.ToChar(164);
            _decodeOnly["yen"] = Convert.ToChar(165);
            _decodeOnly["brvbar"] = Convert.ToChar(166);
            _decodeOnly["sect"] = Convert.ToChar(167);
            _decodeOnly["uml"] = Convert.ToChar(168);
            _decodeOnly["copy"] = Convert.ToChar(169);
            _decodeOnly["ordf"] = Convert.ToChar(170);
            _decodeOnly["laquo"] = Convert.ToChar(171);
            _decodeOnly["not"] = Convert.ToChar(172);
            _decodeOnly["shy"] = Convert.ToChar(173);
            _decodeOnly["reg"] = Convert.ToChar(174);
            _decodeOnly["macr"] = Convert.ToChar(175);
            _decodeOnly["deg"] = Convert.ToChar(176);
            _decodeOnly["plusmn"] = Convert.ToChar(177);
            _decodeOnly["sup2"] = Convert.ToChar(178);
            _decodeOnly["sup3"] = Convert.ToChar(179);
            _decodeOnly["acute"] = Convert.ToChar(180);
            _decodeOnly["micro"] = Convert.ToChar(181);
            _decodeOnly["para"] = Convert.ToChar(182);
            _decodeOnly["middot"] = Convert.ToChar(183);
            _decodeOnly["cedil"] = Convert.ToChar(184);
            _decodeOnly["sup1"] = Convert.ToChar(185);
            _decodeOnly["ordm"] = Convert.ToChar(186);
            _decodeOnly["raquo"] = Convert.ToChar(187);
            _decodeOnly["frac14"] = Convert.ToChar(188);
            _decodeOnly["frac12"] = Convert.ToChar(189);
            _decodeOnly["frac34"] = Convert.ToChar(190);
            _decodeOnly["iquest"] = Convert.ToChar(191);
            _decodeOnly["times"] = Convert.ToChar(215);
            _decodeOnly["divide"] = Convert.ToChar(247);

            // ISO 8859-1 Characters
            _decodeOnly["Agrave"] = Convert.ToChar(192);
            _decodeOnly["Aacute"] = Convert.ToChar(193);
            _decodeOnly["Acirc"] = Convert.ToChar(194);
            _decodeOnly["Atilde"] = Convert.ToChar(195);
            _decodeOnly["Auml"] = Convert.ToChar(196);
            _decodeOnly["Aring"] = Convert.ToChar(197);
            _decodeOnly["AElig"] = Convert.ToChar(198);
            _decodeOnly["Ccedil"] = Convert.ToChar(199);
            _decodeOnly["Egrave"] = Convert.ToChar(200);
            _decodeOnly["Eacute"] = Convert.ToChar(201);
            _decodeOnly["Ecirc"] = Convert.ToChar(202);
            _decodeOnly["Euml"] = Convert.ToChar(203);
            _decodeOnly["Igrave"] = Convert.ToChar(204);
            _decodeOnly["Iacute"] = Convert.ToChar(205);
            _decodeOnly["Icirc"] = Convert.ToChar(206);
            _decodeOnly["Iuml"] = Convert.ToChar(207);
            _decodeOnly["ETH"] = Convert.ToChar(208);
            _decodeOnly["Ntilde"] = Convert.ToChar(209);
            _decodeOnly["Ograve"] = Convert.ToChar(210);
            _decodeOnly["Oacute"] = Convert.ToChar(211);
            _decodeOnly["Ocirc"] = Convert.ToChar(212);
            _decodeOnly["Otilde"] = Convert.ToChar(213);
            _decodeOnly["Ouml"] = Convert.ToChar(214);
            _decodeOnly["Oslash"] = Convert.ToChar(216);
            _decodeOnly["Ugrave"] = Convert.ToChar(217);
            _decodeOnly["Uacute"] = Convert.ToChar(218);
            _decodeOnly["Ucirc"] = Convert.ToChar(219);
            _decodeOnly["Uuml"] = Convert.ToChar(220);
            _decodeOnly["Yacute"] = Convert.ToChar(221);
            _decodeOnly["THORN"] = Convert.ToChar(222);
            _decodeOnly["szlig"] = Convert.ToChar(223);
            _decodeOnly["agrave"] = Convert.ToChar(224);
            _decodeOnly["aacute"] = Convert.ToChar(225);
            _decodeOnly["acirc"] = Convert.ToChar(226);
            _decodeOnly["atilde"] = Convert.ToChar(227);
            _decodeOnly["auml"] = Convert.ToChar(228);
            _decodeOnly["aring"] = Convert.ToChar(229);
            _decodeOnly["aelig"] = Convert.ToChar(230);
            _decodeOnly["ccedil"] = Convert.ToChar(231);
            _decodeOnly["egrave"] = Convert.ToChar(232);
            _decodeOnly["eacute"] = Convert.ToChar(233);
            _decodeOnly["ecirc"] = Convert.ToChar(234);
            _decodeOnly["euml"] = Convert.ToChar(235);
            _decodeOnly["igrave"] = Convert.ToChar(236);
            _decodeOnly["iacute"] = Convert.ToChar(237);
            _decodeOnly["icirc"] = Convert.ToChar(238);
            _decodeOnly["iuml"] = Convert.ToChar(239);
            _decodeOnly["eth"] = Convert.ToChar(240);
            _decodeOnly["ntilde"] = Convert.ToChar(241);
            _decodeOnly["ograve"] = Convert.ToChar(242);
            _decodeOnly["oacute"] = Convert.ToChar(243);
            _decodeOnly["ocirc"] = Convert.ToChar(244);
            _decodeOnly["otilde"] = Convert.ToChar(245);
            _decodeOnly["ouml"] = Convert.ToChar(246);
            _decodeOnly["oslash"] = Convert.ToChar(248);
            _decodeOnly["ugrave"] = Convert.ToChar(249);
            _decodeOnly["uacute"] = Convert.ToChar(250);
            _decodeOnly["ucirc"] = Convert.ToChar(251);
            _decodeOnly["uuml"] = Convert.ToChar(252);
            _decodeOnly["yacute"] = Convert.ToChar(253);
            _decodeOnly["thorn"] = Convert.ToChar(254);
            _decodeOnly["yuml"] = Convert.ToChar(255);

            // Math Symbols Supported by HTML
            _decodeOnly["forall"] = Convert.ToChar(8704);
            _decodeOnly["part"] = Convert.ToChar(8706);
            _decodeOnly["exist"] = Convert.ToChar(8707);
            _decodeOnly["empty"] = Convert.ToChar(8709);
            _decodeOnly["nabla"] = Convert.ToChar(8711);
            _decodeOnly["isin"] = Convert.ToChar(8712);
            _decodeOnly["notin"] = Convert.ToChar(8713);
            _decodeOnly["ni"] = Convert.ToChar(8715);
            _decodeOnly["prod"] = Convert.ToChar(8719);
            _decodeOnly["sum"] = Convert.ToChar(8721);
            _decodeOnly["minus"] = Convert.ToChar(8722);
            _decodeOnly["lowast"] = Convert.ToChar(8727);
            _decodeOnly["radic"] = Convert.ToChar(8730);
            _decodeOnly["prop"] = Convert.ToChar(8733);
            _decodeOnly["infin"] = Convert.ToChar(8734);
            _decodeOnly["ang"] = Convert.ToChar(8736);
            _decodeOnly["and"] = Convert.ToChar(8743);
            _decodeOnly["or"] = Convert.ToChar(8744);
            _decodeOnly["cap"] = Convert.ToChar(8745);
            _decodeOnly["cup"] = Convert.ToChar(8746);
            _decodeOnly["int"] = Convert.ToChar(8747);
            _decodeOnly["there4"] = Convert.ToChar(8756);
            _decodeOnly["sim"] = Convert.ToChar(8764);
            _decodeOnly["cong"] = Convert.ToChar(8773);
            _decodeOnly["asymp"] = Convert.ToChar(8776);
            _decodeOnly["ne"] = Convert.ToChar(8800);
            _decodeOnly["equiv"] = Convert.ToChar(8801);
            _decodeOnly["le"] = Convert.ToChar(8804);
            _decodeOnly["ge"] = Convert.ToChar(8805);
            _decodeOnly["sub"] = Convert.ToChar(8834);
            _decodeOnly["sup"] = Convert.ToChar(8835);
            _decodeOnly["nsub"] = Convert.ToChar(8836);
            _decodeOnly["sube"] = Convert.ToChar(8838);
            _decodeOnly["supe"] = Convert.ToChar(8839);
            _decodeOnly["oplus"] = Convert.ToChar(8853);
            _decodeOnly["otimes"] = Convert.ToChar(8855);
            _decodeOnly["perp"] = Convert.ToChar(8869);
            _decodeOnly["sdot"] = Convert.ToChar(8901);

            // Greek Letters Supported by HTML
            _decodeOnly["Alpha"] = Convert.ToChar(913);
            _decodeOnly["Beta"] = Convert.ToChar(914);
            _decodeOnly["Gamma"] = Convert.ToChar(915);
            _decodeOnly["Delta"] = Convert.ToChar(916);
            _decodeOnly["Epsilon"] = Convert.ToChar(917);
            _decodeOnly["Zeta"] = Convert.ToChar(918);
            _decodeOnly["Eta"] = Convert.ToChar(919);
            _decodeOnly["Theta"] = Convert.ToChar(920);
            _decodeOnly["Iota"] = Convert.ToChar(921);
            _decodeOnly["Kappa"] = Convert.ToChar(922);
            _decodeOnly["Lambda"] = Convert.ToChar(923);
            _decodeOnly["Mu"] = Convert.ToChar(924);
            _decodeOnly["Nu"] = Convert.ToChar(925);
            _decodeOnly["Xi"] = Convert.ToChar(926);
            _decodeOnly["Omicron"] = Convert.ToChar(927);
            _decodeOnly["Pi"] = Convert.ToChar(928);
            _decodeOnly["Rho"] = Convert.ToChar(929);
            _decodeOnly["Sigma"] = Convert.ToChar(931);
            _decodeOnly["Tau"] = Convert.ToChar(932);
            _decodeOnly["Upsilon"] = Convert.ToChar(933);
            _decodeOnly["Phi"] = Convert.ToChar(934);
            _decodeOnly["Chi"] = Convert.ToChar(935);
            _decodeOnly["Psi"] = Convert.ToChar(936);
            _decodeOnly["Omega"] = Convert.ToChar(937);
            _decodeOnly["alpha"] = Convert.ToChar(945);
            _decodeOnly["beta"] = Convert.ToChar(946);
            _decodeOnly["gamma"] = Convert.ToChar(947);
            _decodeOnly["delta"] = Convert.ToChar(948);
            _decodeOnly["epsilon"] = Convert.ToChar(949);
            _decodeOnly["zeta"] = Convert.ToChar(950);
            _decodeOnly["eta"] = Convert.ToChar(951);
            _decodeOnly["theta"] = Convert.ToChar(952);
            _decodeOnly["iota"] = Convert.ToChar(953);
            _decodeOnly["kappa"] = Convert.ToChar(954);
            _decodeOnly["lambda"] = Convert.ToChar(955);
            _decodeOnly["mu"] = Convert.ToChar(956);
            _decodeOnly["nu"] = Convert.ToChar(957);
            _decodeOnly["xi"] = Convert.ToChar(958);
            _decodeOnly["omicron"] = Convert.ToChar(959);
            _decodeOnly["pi"] = Convert.ToChar(960);
            _decodeOnly["rho"] = Convert.ToChar(961);
            _decodeOnly["sigmaf"] = Convert.ToChar(962);
            _decodeOnly["sigma"] = Convert.ToChar(963);
            _decodeOnly["tau"] = Convert.ToChar(964);
            _decodeOnly["upsilon"] = Convert.ToChar(965);
            _decodeOnly["phi"] = Convert.ToChar(966);
            _decodeOnly["chi"] = Convert.ToChar(967);
            _decodeOnly["psi"] = Convert.ToChar(968);
            _decodeOnly["omega"] = Convert.ToChar(969);
            _decodeOnly["thetasym"] = Convert.ToChar(977);
            _decodeOnly["upsih"] = Convert.ToChar(978);
            _decodeOnly["piv"] = Convert.ToChar(982);

            // Other Entities Supported by HTML
            _decodeOnly["OElig"] = Convert.ToChar(338);
            _decodeOnly["oelig"] = Convert.ToChar(339);
            _decodeOnly["Scaron"] = Convert.ToChar(352);
            _decodeOnly["scaron"] = Convert.ToChar(353);
            _decodeOnly["Yuml"] = Convert.ToChar(376);
            _decodeOnly["fnof"] = Convert.ToChar(402);
            _decodeOnly["circ"] = Convert.ToChar(710);
            _decodeOnly["tilde"] = Convert.ToChar(732);
            _decodeOnly["ndash"] = Convert.ToChar(8211);
            _decodeOnly["mdash"] = Convert.ToChar(8212);
            _decodeOnly["lsquo"] = Convert.ToChar(8216);
            _decodeOnly["rsquo"] = Convert.ToChar(8217);
            _decodeOnly["sbquo"] = Convert.ToChar(8218);
            _decodeOnly["ldquo"] = Convert.ToChar(8220);
            _decodeOnly["rdquo"] = Convert.ToChar(8221);
            _decodeOnly["bdquo"] = Convert.ToChar(8222);
            _decodeOnly["dagger"] = Convert.ToChar(8224);
            _decodeOnly["Dagger"] = Convert.ToChar(8225);
            _decodeOnly["bull"] = Convert.ToChar(8226);
            _decodeOnly["hellip"] = Convert.ToChar(8230);
            _decodeOnly["permil"] = Convert.ToChar(8240);
            _decodeOnly["prime"] = Convert.ToChar(8242);
            _decodeOnly["Prime"] = Convert.ToChar(8243);
            _decodeOnly["lsaquo"] = Convert.ToChar(8249);
            _decodeOnly["rsaquo"] = Convert.ToChar(8250);
            _decodeOnly["oline"] = Convert.ToChar(8254);
            _decodeOnly["euro"] = Convert.ToChar(8364);
            _decodeOnly["trade"] = Convert.ToChar(153);
            _decodeOnly["larr"] = Convert.ToChar(8592);
            _decodeOnly["uarr"] = Convert.ToChar(8593);
            _decodeOnly["rarr"] = Convert.ToChar(8594);
            _decodeOnly["darr"] = Convert.ToChar(8595);
            _decodeOnly["harr"] = Convert.ToChar(8596);
            _decodeOnly["crarr"] = Convert.ToChar(8629);
            _decodeOnly["lceil"] = Convert.ToChar(8968);
            _decodeOnly["rceil"] = Convert.ToChar(8969);
            _decodeOnly["lfloor"] = Convert.ToChar(8970);
            _decodeOnly["rfloor"] = Convert.ToChar(8971);
            _decodeOnly["loz"] = Convert.ToChar(9674);
            _decodeOnly["spades"] = Convert.ToChar(9824);
            _decodeOnly["clubs"] = Convert.ToChar(9827);
            _decodeOnly["hearts"] = Convert.ToChar(9829);
            _decodeOnly["diams"] = Convert.ToChar(9830);
        }

        /// <summary>
        /// Is the given html tag is single tag or can have content.
        /// </summary>
        /// <param name="tagName">the tag to check (must be lower case)</param>
        /// <returns>true - is single tag, false - otherwise</returns>
        public static bool IsSingleTag(string tagName)
        {
            return _list.Contains(tagName);
        }

        /// <summary>
        /// Decode html encoded string to regular string.<br/>
        /// Handles &lt;, &gt;, "&amp;.
        /// </summary>
        /// <param name="str">the string to decode</param>
        /// <returns>decoded string</returns>
        public static string DecodeHtml(string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                str = DecodeHtmlCharByCode(str);

                str = DecodeHtmlCharByName(str);

                foreach (var encPair in _encodeDecode)
                {
                    str = str.Replace(encPair.Key, encPair.Value);
                }
            }
            return str;
        }

        /// <summary>
        /// Encode regular string into html encoded string.<br/>
        /// Handles &lt;, &gt;, "&amp;.
        /// </summary>
        /// <param name="str">the string to encode</param>
        /// <returns>encoded string</returns>
        public static string EncodeHtml(string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                for (int i = _encodeDecode.Length - 1; i >= 0; i--)
                {
                    str = str.Replace(_encodeDecode[i].Value, _encodeDecode[i].Key);
                }
            }
            return str;
        }


        #region Private methods

        /// <summary>
        /// Decode html special charecters encoded using char entity code (&#8364;)
        /// </summary>
        /// <param name="str">the string to decode</param>
        /// <returns>decoded string</returns>
        private static string DecodeHtmlCharByCode(string str)
        {
            var idx = str.IndexOf("&#", StringComparison.OrdinalIgnoreCase);
            while (idx > -1)
            {
                bool hex = str.Length > idx + 3 && char.ToLower(str[idx + 2]) == 'x';
                var endIdx = idx + 2 + (hex ? 1 : 0);

                long num = 0;
                while (endIdx < str.Length && CommonUtils.IsDigit(str[endIdx], hex))
                    num = num * (hex ? 16 : 10) + CommonUtils.ToDigit(str[endIdx++], hex);
                endIdx += (endIdx < str.Length && str[endIdx] == ';') ? 1 : 0;

                str = str.Remove(idx, endIdx - idx);
                str = str.Insert(idx, Convert.ToChar(num).ToString());

                idx = str.IndexOf("&#", idx + 1);
            }
            return str;
        }

        /// <summary>
        /// Decode html special charecters encoded using char entity name (&#euro;)
        /// </summary>
        /// <param name="str">the string to decode</param>
        /// <returns>decoded string</returns>
        private static string DecodeHtmlCharByName(string str)
        {
            var idx = str.IndexOf('&');
            while (idx > -1)
            {
                var endIdx = str.IndexOf(';', idx);
                if (endIdx > -1 && endIdx - idx < 8)
                {
                    var key = str.Substring(idx + 1, endIdx - idx - 1);
                    char c;
                    if (_decodeOnly.TryGetValue(key, out c))
                    {
                        str = str.Remove(idx, endIdx - idx + 1);
                        str = str.Insert(idx, c.ToString());
                    }
                }

                idx = str.IndexOf('&', idx + 1);
            }
            return str;
        }

        #endregion
    }
}
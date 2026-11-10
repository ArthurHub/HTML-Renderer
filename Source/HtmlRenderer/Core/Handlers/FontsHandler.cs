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
using System.Linq;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.CssEngine;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.Core.Handlers
{
    /// <summary>
    /// One <c>@font-face</c> face of a family with its matching descriptors (CSS Fonts 4 §4.4). Without a
    /// <c>unicode-range</c> the ranges are null and the face is used for any character.
    /// </summary>
    internal sealed class FontFaceEntry
    {
        public FontFaceEntry(int weight, bool italic, int stretch, IReadOnlyList<CodepointRange> explicitRanges, RFontFamily fontFamily)
        {
            Weight = weight;
            Italic = italic;
            Stretch = stretch;
            ExplicitRanges = explicitRanges;
            FontFamily = fontFamily;
        }

        public int Weight { get; }
        public bool Italic { get; }
        public int Stretch { get; }
        public IReadOnlyList<CodepointRange> ExplicitRanges { get; }
        public RFontFamily FontFamily { get; }
    }

    /// <summary>
    /// One registered family: an installed or <see cref="FontsHandler.AddFontFamily"/> family
    /// (<see cref="SimpleFamily"/>), the faces registered by <c>@font-face</c> rules, or both.
    /// </summary>
    internal sealed class FontFamilyModel
    {
        public string Name { get; set; }
        public RFontFamily SimpleFamily { get; set; }
        public List<FontFaceEntry> Faces { get; } = new List<FontFaceEntry>();
    }

    /// <summary>
    /// Utilities for fonts and fonts families handling.
    /// </summary>
    internal sealed class FontsHandler
    {
        #region Fields and Consts

        /// <summary>
        ///
        /// </summary>
        private readonly RAdapter _adapter;

        /// <summary>
        /// Allow to map not installed fonts to different
        /// </summary>
        private readonly Dictionary<string, string> _fontsMapping = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// collection of all installed and added font families to check if font exists
        /// </summary>
        private readonly Dictionary<string, FontFamilyModel> _existingFontFamilies = new Dictionary<string, FontFamilyModel>(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// cache of all the font used not to create same font again and again
        /// </summary>
        private readonly Dictionary<string, Dictionary<double, Dictionary<RFontStyle, RFont>>> _fontsCache = new Dictionary<string, Dictionary<double, Dictionary<RFontStyle, RFont>>>(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// cache of fonts resolved from <c>@font-face</c> faces, keyed by everything that can change which
        /// face gets chosen (codepoint -1 when none). Two requests with the same <see cref="RFontStyle"/> can
        /// still resolve to different faces, e.g. weight 300 and 500.
        /// </summary>
        private readonly Dictionary<string, Dictionary<(double Size, int Weight, bool Italic, int Stretch, int Codepoint), RFont>> _faceFontsCache = new Dictionary<string, Dictionary<(double, int, bool, int, int), RFont>>(StringComparer.InvariantCultureIgnoreCase);

        #endregion


        /// <summary>
        /// Init.
        /// </summary>
        public FontsHandler(RAdapter adapter)
        {
            ArgChecker.AssertArgNotNull(adapter, "global");

            _adapter = adapter;
        }

        /// <summary>
        /// Check if the given font family exists by name
        /// </summary>
        /// <param name="family">the font to check</param>
        /// <returns>true - font exists by given family name, false - otherwise</returns>
        public bool IsFontExists(string family)
        {
            bool exists = _existingFontFamilies.ContainsKey(family);
            if (!exists)
            {
                string mappedFamily;
                if (_fontsMapping.TryGetValue(family, out mappedFamily))
                {
                    exists = _existingFontFamilies.ContainsKey(mappedFamily);
                }
            }
            return exists;
        }

        /// <summary>
        /// Finds a registered family by its exact name, for <c>@font-face</c> <c>src: local(...)</c>, which
        /// only needs some face to exist under that name. Mappings from <see cref="AddFontFamilyMapping"/>
        /// aren't consulted, <c>local()</c> names an actual font. Returns null if there's none.
        /// </summary>
        public RFontFamily TryGetExistingFamily(string family)
        {
            FontFamilyModel model;
            if (!_existingFontFamilies.TryGetValue(family, out model))
            {
                return null;
            }
            return model.SimpleFamily ?? (model.Faces.Count > 0 ? model.Faces[0].FontFamily : null);
        }

        /// <summary>
        /// Adds a font family to be used.
        /// </summary>
        /// <param name="fontFamily">The font family to add.</param>
        public void AddFontFamily(RFontFamily fontFamily)
        {
            ArgChecker.AssertArgNotNull(fontFamily, "family");

            FontFamilyModel model;
            if (!_existingFontFamilies.TryGetValue(fontFamily.Name, out model))
            {
                model = new FontFamilyModel { Name = fontFamily.Name };
                _existingFontFamilies[fontFamily.Name] = model;
            }
            model.SimpleFamily = fontFamily;
        }

        /// <summary>
        /// Adds a font mapping from <paramref name="fromFamily"/> to <paramref name="toFamily"/> iff the <paramref name="fromFamily"/> is not found.<br/>
        /// When the <paramref name="fromFamily"/> font is used in rendered html and is not found in existing
        /// fonts (installed or added) it will be replaced by <paramref name="toFamily"/>.<br/>
        /// </summary>
        /// <param name="fromFamily">the font family to replace</param>
        /// <param name="toFamily">the font family to replace with</param>
        public void AddFontFamilyMapping(string fromFamily, string toFamily)
        {
            ArgChecker.AssertArgNotNullOrEmpty(fromFamily, "fromFamily");
            ArgChecker.AssertArgNotNullOrEmpty(toFamily, "toFamily");

            _fontsMapping[fromFamily] = toFamily;
        }

        /// <summary>
        /// Registers one <c>@font-face</c> face under <paramref name="familyName"/>. A face with the same
        /// descriptors replaces the earlier one, the rules are registered again on every <c>SetHtml</c>.
        /// </summary>
        /// <param name="familyName">the family name of the <c>@font-face</c> rule</param>
        /// <param name="fontFamily">the loaded face</param>
        /// <param name="weight">numeric weight (1-1000)</param>
        /// <param name="isItalic">whether the face is italic or oblique</param>
        /// <param name="stretch">width on the 1-9 scale of <see cref="FontStretchResolver"/></param>
        /// <param name="ranges">the <c>unicode-range</c>, or null for any character</param>
        public void AddFontFace(string familyName, RFontFamily fontFamily, int weight, bool isItalic, int stretch, IReadOnlyList<CodepointRange> ranges)
        {
            ArgChecker.AssertArgNotNullOrEmpty(familyName, "familyName");
            ArgChecker.AssertArgNotNull(fontFamily, "fontFamily");

            FontFamilyModel model;
            if (!_existingFontFamilies.TryGetValue(familyName, out model))
            {
                model = new FontFamilyModel { Name = familyName };
                _existingFontFamilies[familyName] = model;
            }

            for (var i = model.Faces.Count - 1; i >= 0; i--)
            {
                if (IsSameFaceSlot(model.Faces[i], weight, isItalic, stretch, ranges))
                {
                    model.Faces.RemoveAt(i);
                }
            }

            model.Faces.Add(new FontFaceEntry(weight, isItalic, stretch, ranges, fontFamily));

            // A new face can change which face is nearest for a request.
            _fontsCache.Remove(familyName);
            _faceFontsCache.Remove(familyName);
        }

        /// <summary>
        /// Get cached font instance for the given font properties.<br/>
        /// Improve performance not to create same font multiple times.
        /// </summary>
        /// <returns>cached font instance</returns>
        public RFont GetCachedFont(string family, double size, RFontStyle style)
        {
            var font = TryGetFont(family, size, style);
            if (font == null)
            {
                if (!_existingFontFamilies.ContainsKey(family))
                {
                    string mappedFamily;
                    if (_fontsMapping.TryGetValue(family, out mappedFamily))
                    {
                        font = TryGetFont(mappedFamily, size, style);
                        if (font == null)
                        {
                            font = CreateFont(mappedFamily, size, style);
                            _fontsCache[mappedFamily][size][style] = font;
                        }
                    }
                }

                if (font == null)
                {
                    font = CreateFont(family, size, style);
                }

                _fontsCache[family][size][style] = font;
            }
            return font;
        }

        /// <summary>
        /// Get cached font instance for the given font properties, matching against the <c>@font-face</c>
        /// faces of <paramref name="family"/> (CSS Fonts 4 §5.2). A family without such faces goes through
        /// <see cref="GetCachedFont(string,double,RFontStyle)"/>, where the platform picks the face.
        /// </summary>
        /// <param name="family">the css font-family name</param>
        /// <param name="size">the font size</param>
        /// <param name="style">the box's style, its Bold bit is replaced by <paramref name="weight"/></param>
        /// <param name="weight">numeric weight (1-1000)</param>
        /// <param name="stretch">width on the 1-9 scale of <see cref="FontStretchResolver"/></param>
        /// <param name="codepoint">the box's first non-white-space character for <c>unicode-range</c>, or null to ignore the ranges</param>
        /// <returns>
        /// the resolved font, or null when <paramref name="codepoint"/> is given and no registered face
        /// covers it, the caller then falls back to the default font
        /// </returns>
        public RFont GetCachedFont(string family, double size, RFontStyle style, int weight, int stretch, int? codepoint)
        {
            FontFamilyModel model;
            if (_existingFontFamilies.TryGetValue(family, out model) && model.Faces.Count > 0)
            {
                var requestedItalic = (style & RFontStyle.Italic) != 0;

                FontFaceEntry face;
                if (TryFindNearestFace(model, weight, requestedItalic, stretch, codepoint, out face))
                {
                    return GetCachedFaceFont(family, face, size, style, weight, requestedItalic, stretch, codepoint ?? -1);
                }

                if (codepoint != null)
                {
                    // Don't substitute a face that doesn't cover the character.
                    return null;
                }

                // Can't happen, AddFontFace always leaves a face registered.
            }

            return GetCachedFont(family, size, style);
        }


        #region Private methods

        /// <summary>
        /// Creates the chosen <paramref name="face"/>'s <see cref="RFont"/>, synthesizing bold or italic
        /// when the face lacks what was requested.
        /// </summary>
        private RFont GetCachedFaceFont(string family, FontFaceEntry face, double size, RFontStyle requestedStyle, int requestedWeight, bool requestedItalic, int requestedStretch, int codepointKey)
        {
            Dictionary<(double, int, bool, int, int), RFont> familyCache;
            if (!_faceFontsCache.TryGetValue(family, out familyCache))
            {
                familyCache = new Dictionary<(double, int, bool, int, int), RFont>();
                _faceFontsCache[family] = familyCache;
            }

            var key = (size, requestedWeight, requestedItalic, requestedStretch, codepointKey);

            RFont font;
            if (familyCache.TryGetValue(key, out font))
            {
                return font;
            }

            // face.FontFamily is a platform family handle that groups all faces, the style passed to
            // CreateFont picks the face. So the style carries the chosen face's own bold and italic bits,
            // plus synthesized ones when the nearest face is lighter or upright. Weights from 600 count as
            // bold, like the regular/bold matching of CSS 2.1 §15.6.
            var faceIsBold = face.Weight >= 600;
            var mustSimulateBold = requestedWeight >= 600 && !faceIsBold;
            var mustSimulateItalic = requestedItalic && !face.Italic;

            var residualStyle = requestedStyle & (RFontStyle.Underline | RFontStyle.Strikeout);
            if (faceIsBold || mustSimulateBold) residualStyle |= RFontStyle.Bold;
            if (face.Italic || mustSimulateItalic) residualStyle |= RFontStyle.Italic;

            font = _adapter.CreateFont(face.FontFamily, size, residualStyle);
            familyCache[key] = font;
            return font;
        }

        /// <summary>
        /// CSS Fonts 4 §5.2 face matching, here italic first, then stretch, then weight. With a
        /// <paramref name="codepoint"/>, only faces whose <c>unicode-range</c> covers it are candidates, the
        /// glyphs of the font itself aren't checked. Returns false when no face qualifies.
        /// </summary>
        private static bool TryFindNearestFace(FontFamilyModel family, int weight, bool isItalic, int stretch, int? codepoint, out FontFaceEntry face)
        {
            face = null;

            List<FontFaceEntry> covering = codepoint is int cp
                ? family.Faces.Where(f => FaceCovers(f, cp)).ToList()
                : family.Faces;

            if (covering.Count == 0)
                return false;

            var exact = covering.Where(f => f.Weight == weight && f.Italic == isItalic && f.Stretch == stretch).ToList();
            if (exact.Count > 0)
            {
                face = exact[exact.Count - 1];
                return true;
            }

            var sameSlant = covering.Where(f => f.Italic == isItalic).ToList();
            var candidates = sameSlant.Count > 0 ? sameSlant : covering;

            var availableStretches = candidates.Select(f => f.Stretch).Distinct().ToList();
            var chosenStretch = PickNearestStretch(availableStretches, stretch);
            var stretchCandidates = candidates.Where(f => f.Stretch == chosenStretch).ToList();

            var availableWeights = stretchCandidates.Select(f => f.Weight).Distinct().ToList();
            var chosenWeight = PickNearestWeight(availableWeights, weight);

            face = stretchCandidates.Last(f => f.Weight == chosenWeight);
            return true;
        }

        /// <summary>
        /// Whether <paramref name="entry"/> is used for <paramref name="codepoint"/>: inside its declared
        /// <c>unicode-range</c> if it has one, else it covers everything.
        /// </summary>
        private static bool FaceCovers(FontFaceEntry entry, int codepoint)
        {
            return entry.ExplicitRanges == null || UnicodeRangeParser.Covers(entry.ExplicitRanges, codepoint);
        }

        /// <summary>
        /// CSS Fonts 4 §5.2 nearest width. <paramref name="availableStretches"/> must be non-empty.
        /// </summary>
        private static int PickNearestStretch(List<int> availableStretches, int target)
        {
            if (availableStretches.Contains(target))
                return target;

            IEnumerable<int> candidates = target <= FontStretchResolver.Normal
                ? availableStretches.Where(s => s < target).OrderByDescending(s => s)
                    .Concat(availableStretches.Where(s => s > target).OrderBy(s => s))
                : availableStretches.Where(s => s > target).OrderBy(s => s)
                    .Concat(availableStretches.Where(s => s < target).OrderByDescending(s => s));

            return candidates.First();
        }

        /// <summary>
        /// CSS Fonts 4 §5.2 nearest weight, the CSS 2.1 §15.6 order extended to any number.
        /// <paramref name="availableWeights"/> must be non-empty. Outside [400,500] it skips
        /// <paramref name="target"/> itself, the caller handles exact matches.
        /// </summary>
        private static int PickNearestWeight(List<int> availableWeights, int target)
        {
            IEnumerable<int> candidates;
            if (target >= 400 && target <= 500)
            {
                candidates = availableWeights.Where(w => w >= target && w <= 500).OrderBy(w => w)
                    .Concat(availableWeights.Where(w => w < target).OrderByDescending(w => w))
                    .Concat(availableWeights.Where(w => w > 500).OrderBy(w => w));
            }
            else if (target < 400)
            {
                candidates = availableWeights.Where(w => w < target).OrderByDescending(w => w)
                    .Concat(availableWeights.Where(w => w > target).OrderBy(w => w));
            }
            else
            {
                candidates = availableWeights.Where(w => w > target).OrderBy(w => w)
                    .Concat(availableWeights.Where(w => w < target).OrderByDescending(w => w));
            }

            return candidates.First();
        }

        private static bool IsSameFaceSlot(FontFaceEntry entry, int weight, bool isItalic, int stretch, IReadOnlyList<CodepointRange> ranges)
        {
            return entry.Weight == weight && entry.Italic == isItalic && entry.Stretch == stretch
                   && RangesEqual(entry.ExplicitRanges, ranges);
        }

        private static bool RangesEqual(IReadOnlyList<CodepointRange> a, IReadOnlyList<CodepointRange> b)
        {
            if (a == null || b == null)
                return a == null && b == null;
            if (a.Count != b.Count)
                return false;
            for (var i = 0; i < a.Count; i++)
            {
                if (a[i].Start != b[i].Start || a[i].End != b[i].End)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Get cached font if it exists in cache or null if it is not.
        /// </summary>
        private RFont TryGetFont(string family, double size, RFontStyle style)
        {
            RFont font = null;
            if (_fontsCache.ContainsKey(family))
            {
                var a = _fontsCache[family];
                if (a.ContainsKey(size))
                {
                    var b = a[size];
                    if (b.ContainsKey(style))
                    {
                        font = b[style];
                    }
                }
                else
                {
                    _fontsCache[family][size] = new Dictionary<RFontStyle, RFont>();
                }
            }
            else
            {
                _fontsCache[family] = new Dictionary<double, Dictionary<RFontStyle, RFont>>();
                _fontsCache[family][size] = new Dictionary<RFontStyle, RFont>();
            }
            return font;
        }

        /// <summary>
        /// Creates the font, through the registered family if there's one, so added fonts are found.
        /// </summary>
        private RFont CreateFont(string family, double size, RFontStyle style)
        {
            FontFamilyModel model;
            try
            {
                return _existingFontFamilies.TryGetValue(family, out model) && model.SimpleFamily != null
                    ? _adapter.CreateFont(model.SimpleFamily, size, style)
                    : _adapter.CreateFont(family, size, style);
            }
            catch
            {
                // handle possibility of no requested style exists for the font, use regular then
                return _existingFontFamilies.TryGetValue(family, out model) && model.SimpleFamily != null
                    ? _adapter.CreateFont(model.SimpleFamily, size, RFontStyle.Regular)
                    : _adapter.CreateFont(family, size, RFontStyle.Regular);
            }
        }

        #endregion
    }
}

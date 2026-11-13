using System;
using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal static class Map
    {
        public static readonly Dictionary<string, RotationAxis> RotationAxes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {"x", RotationAxis.X},
                {"y", RotationAxis.Y},
                {"z", RotationAxis.Z}
            };
        public static readonly Dictionary<string, Whitespace> WhitespaceModes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.Normal, Whitespace.Normal},
                {Keywords.Pre, Whitespace.Pre},
                {Keywords.Nowrap, Whitespace.NoWrap},
                {Keywords.PreWrap, Whitespace.PreWrap},
                {Keywords.PreLine, Whitespace.PreLine}
            };
        public static readonly Dictionary<string, Hyphens> HyphensModes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.None, Hyphens.None},
                {Keywords.Manual, Hyphens.Manual},
                {Keywords.Auto, Hyphens.Auto}
            };
        public static readonly Dictionary<string, TextTransform> TextTransforms =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.None, TextTransform.None},
                {Keywords.Capitalize, TextTransform.Capitalize},
                {Keywords.Uppercase, TextTransform.Uppercase},
                {Keywords.Lowercase, TextTransform.Lowercase},
                {Keywords.FullWidth, TextTransform.FullWidth}
            };
        public static readonly Dictionary<string, TextOverflow> TextOverflowModes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.Clip, TextOverflow.Clip},
                {Keywords.Ellipsis, TextOverflow.Ellipsis}
            };
        public static readonly Dictionary<string, TextAlignLast> TextAlignmentsLast =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.Auto, TextAlignLast.Auto},
                {Keywords.Start, TextAlignLast.Start},
                {Keywords.End, TextAlignLast.End},
                {Keywords.Right, TextAlignLast.Right},
                {Keywords.Left, TextAlignLast.Left},
                {Keywords.Center, TextAlignLast.Center},
                {Keywords.Justify, TextAlignLast.Justify},
                {Keywords.MatchParent, TextAlignLast.MatchParent}
            };
        public static readonly Dictionary<string, TextAnchor> TextAnchors =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.Start, TextAnchor.Start},
                {Keywords.Middle, TextAnchor.Middle},
                {Keywords.End, TextAnchor.End}
            };
        public static readonly Dictionary<string, TextJustify> TextJustifyOptions =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.Auto, TextJustify.Auto},
                {Keywords.Distribute, TextJustify.Distribute},
                {Keywords.DistributeAllLines, TextJustify.DistributeAllLines},
                {Keywords.DistributeCenterLast, TextJustify.DistributeCenterLast},
                {Keywords.InterCluster, TextJustify.InterCluster},
                {Keywords.InterIdeograph, TextJustify.InterIdeograph},
                {Keywords.InterWord, TextJustify.InterWord},
                {Keywords.Kashida, TextJustify.Kashida},
                {Keywords.Newspaper, TextJustify.Newspaper}
            };
        public static readonly Dictionary<string, JustifyContent> JustifyContentOptions =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.Start, JustifyContent.Start},
                {Keywords.Center, JustifyContent.Center},
                {Keywords.End, JustifyContent.End},
                {Keywords.FlexStart, JustifyContent.FlexStart},
                {Keywords.FlexEnd, JustifyContent.FlexEnd},
                {Keywords.Left, JustifyContent.Left},
                {Keywords.Right, JustifyContent.Right},
                {Keywords.Normal, JustifyContent.Normal },
                {Keywords.SpaceBetween, JustifyContent.SpaceBetween},
                {Keywords.SpaceAround, JustifyContent.SpaceAround},
                {Keywords.SpaceEvenly, JustifyContent.SpaceEvenly},
                {Keywords.Stretch, JustifyContent.Stretch },
            };
        public static readonly Dictionary<string, HorizontalAlignment> HorizontalAlignments =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.Left, HorizontalAlignment.Left},
                {Keywords.Right, HorizontalAlignment.Right},
                {Keywords.Center, HorizontalAlignment.Center},
                {Keywords.Justify, HorizontalAlignment.Justify},
                {Keywords.Start, HorizontalAlignment.Start},
                {Keywords.End, HorizontalAlignment.End},
                {Keywords.MatchParent, HorizontalAlignment.MatchParent}
            };
        public static readonly Dictionary<string, VerticalAlignment> VerticalAlignments =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.Baseline, VerticalAlignment.Baseline},
                {Keywords.Sub, VerticalAlignment.Sub},
                {Keywords.Super, VerticalAlignment.Super},
                {Keywords.TextTop, VerticalAlignment.TextTop},
                {Keywords.TextBottom, VerticalAlignment.TextBottom},
                {Keywords.Middle, VerticalAlignment.Middle},
                {Keywords.Top, VerticalAlignment.Top},
                {Keywords.Bottom, VerticalAlignment.Bottom}
            };
        public static readonly Dictionary<string, LineStyle> LineStyles =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.None, LineStyle.None},
                {Keywords.Solid, LineStyle.Solid},
                {Keywords.Double, LineStyle.Double},
                {Keywords.Dotted, LineStyle.Dotted},
                {Keywords.Dashed, LineStyle.Dashed},
                {Keywords.Inset, LineStyle.Inset},
                {Keywords.Outset, LineStyle.Outset},
                {Keywords.Ridge, LineStyle.Ridge},
                {Keywords.Groove, LineStyle.Groove},
                {Keywords.Hidden, LineStyle.Hidden}
            };
        // <bg-clip>: the <box> keywords plus text and border-area. Only the keywords matter, so the values are the keywords themselves.
        public static readonly Dictionary<string, string> BackgroundClips =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.BorderBox, Keywords.BorderBox},
                {Keywords.PaddingBox, Keywords.PaddingBox},
                {Keywords.ContentBox, Keywords.ContentBox},
                {Keywords.Text, Keywords.Text},
                {Keywords.BorderArea, Keywords.BorderArea}
            };

        public static readonly Dictionary<string, BoxModel> BoxModels =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.BorderBox, BoxModel.BorderBox},
                {Keywords.PaddingBox, BoxModel.PaddingBox},
                {Keywords.ContentBox, BoxModel.ContentBox}
            };
        public static readonly Dictionary<string, ITimingFunction> TimingFunctions =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.Ease, new CubicBezierTimingFunction(0.25f, 0.1f, 0.25f, 1f)},
                {Keywords.EaseIn, new CubicBezierTimingFunction(0.42f, 0f, 1f, 1f)},
                {Keywords.EaseOut, new CubicBezierTimingFunction(0f, 0f, 0.58f, 1f)},
                {Keywords.EaseInOut, new CubicBezierTimingFunction(0.42f, 0f, 0.58f, 1f)},
                {Keywords.Linear, new CubicBezierTimingFunction(0f, 0f, 1f, 1f)},
                {Keywords.StepStart, new StepsTimingFunction(1, true)},
                {Keywords.StepEnd, new StepsTimingFunction(1)}
            };
        public static readonly Dictionary<string, AnimationFillStyle> AnimationFillStyles =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.None, AnimationFillStyle.None},
                {Keywords.Forwards, AnimationFillStyle.Forwards},
                {Keywords.Backwards, AnimationFillStyle.Backwards},
                {Keywords.Both, AnimationFillStyle.Both}
            };
        public static readonly Dictionary<string, AnimationDirection> AnimationDirections =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.Normal, AnimationDirection.Normal},
                {Keywords.Reverse, AnimationDirection.Reverse},
                {Keywords.Alternate, AnimationDirection.Alternate},
                {Keywords.AlternateReverse, AnimationDirection.AlternateReverse}
            };
        public static readonly Dictionary<string, Visibility> Visibilities =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.Visible, Visibility.Visible},
                {Keywords.Hidden, Visibility.Hidden},
                {Keywords.Collapse, Visibility.Collapse}
            };
        public static readonly Dictionary<string, PlayState> PlayStates =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.Running, PlayState.Running},
                {Keywords.Paused, PlayState.Paused}
            };
        public static readonly Dictionary<string, FontVariant> FontVariants =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.Normal, FontVariant.Normal},
                {Keywords.SmallCaps, FontVariant.SmallCaps}
            };
        public static readonly Dictionary<string, DirectionMode> DirectionModes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.Ltr, DirectionMode.Ltr},
                {Keywords.Rtl, DirectionMode.Rtl}
            };
        public static readonly Dictionary<string, ListStyle> ListStyles =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.Disc, ListStyle.Disc},
                {Keywords.Circle, ListStyle.Circle},
                {Keywords.Square, ListStyle.Square},
                {Keywords.Decimal, ListStyle.Decimal},
                {Keywords.DecimalLeadingZero, ListStyle.DecimalLeadingZero},
                {Keywords.LowerRoman, ListStyle.LowerRoman},
                {Keywords.UpperRoman, ListStyle.UpperRoman},
                {Keywords.LowerGreek, ListStyle.LowerGreek},
                {Keywords.LowerLatin, ListStyle.LowerLatin},
                {Keywords.UpperLatin, ListStyle.UpperLatin},
                {Keywords.Armenian, ListStyle.Armenian},
                {Keywords.Georgian, ListStyle.Georgian},
                {Keywords.Hebrew, ListStyle.Hebrew},
                {Keywords.LowerAlpha, ListStyle.LowerLatin},
                {Keywords.UpperAlpha, ListStyle.UpperLatin},
                {Keywords.None, ListStyle.None}
            };
        public static readonly Dictionary<string, ListPosition> ListPositions =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.Inside, ListPosition.Inside},
                {Keywords.Outside, ListPosition.Outside}
            };
        public static readonly Dictionary<string, FontSize> FontSizes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.XxSmall, FontSize.Tiny},
                {Keywords.XSmall, FontSize.Little},
                {Keywords.Small, FontSize.Small},
                {Keywords.Medium, FontSize.Medium},
                {Keywords.Large, FontSize.Large},
                {Keywords.XLarge, FontSize.Big},
                {Keywords.XxLarge, FontSize.Huge},
                {Keywords.Larger, FontSize.Larger},
                {Keywords.Smaller, FontSize.Smaller}
            };
        public static readonly Dictionary<string, TextDecorationStyle> TextDecorationStyles =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.Solid, TextDecorationStyle.Solid},
                {Keywords.Double, TextDecorationStyle.Double},
                {Keywords.Dotted, TextDecorationStyle.Dotted},
                {Keywords.Dashed, TextDecorationStyle.Dashed},
                {Keywords.Wavy, TextDecorationStyle.Wavy}
            };
        public static readonly Dictionary<string, TextDecorationLine> TextDecorationLines =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.Underline, TextDecorationLine.Underline},
                {Keywords.Overline, TextDecorationLine.Overline},
                {Keywords.LineThrough, TextDecorationLine.LineThrough},
                {Keywords.Blink, TextDecorationLine.Blink}
            };
        public static readonly Dictionary<string, BorderRepeat> BorderRepeatModes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.Stretch, BorderRepeat.Stretch},
                {Keywords.Repeat, BorderRepeat.Repeat},
                {Keywords.Round, BorderRepeat.Round}
            };
        public static readonly Dictionary<string, string> DefaultFontFamilies =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.Serif, "Times New Roman"},
                {Keywords.SansSerif, "Arial"},
                {Keywords.Monospace, "Consolas"},
                {Keywords.Cursive, "Cursive"},
                {Keywords.Fantasy, "Comic Sans"}
            };
        public static readonly Dictionary<string, BackgroundAttachment> BackgroundAttachments =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.Fixed, BackgroundAttachment.Fixed},
                {Keywords.Local, BackgroundAttachment.Local},
                {Keywords.Scroll, BackgroundAttachment.Scroll}
            };
        public static readonly Dictionary<string, FontStyle> FontStyles =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.Normal, FontStyle.Normal},
                {Keywords.Italic, FontStyle.Italic},
                {Keywords.Oblique, FontStyle.Oblique}
            };
        public static readonly Dictionary<string, FontStretch> FontStretches =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.Normal, FontStretch.Normal},
                {Keywords.UltraCondensed, FontStretch.UltraCondensed},
                {Keywords.ExtraCondensed, FontStretch.ExtraCondensed},
                {Keywords.Condensed, FontStretch.Condensed},
                {Keywords.SemiCondensed, FontStretch.SemiCondensed},
                {Keywords.SemiExpanded, FontStretch.SemiExpanded},
                {Keywords.Expanded, FontStretch.Expanded},
                {Keywords.ExtraExpanded, FontStretch.ExtraExpanded},
                {Keywords.UltraExpanded, FontStretch.UltraExpanded}
            };
        public static readonly Dictionary<string, BreakMode> BreakModes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.Auto, BreakMode.Auto},
                {Keywords.Always, BreakMode.Always},
                {Keywords.Avoid, BreakMode.Avoid},
                {Keywords.Left, BreakMode.Left},
                {Keywords.Right, BreakMode.Right},
                {Keywords.Page, BreakMode.Page},
                {Keywords.Column, BreakMode.Column},
                {Keywords.AvoidPage, BreakMode.AvoidPage},
                {Keywords.AvoidColumn, BreakMode.AvoidColumn},
                {Keywords.Recto, BreakMode.Recto},
                {Keywords.Verso, BreakMode.Verso}
            };
        public static readonly Dictionary<string, BreakMode> PageBreakModes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.Auto, BreakMode.Auto},
                {Keywords.Always, BreakMode.Always},
                {Keywords.Avoid, BreakMode.Avoid},
                {Keywords.Left, BreakMode.Left},
                {Keywords.Right, BreakMode.Right}
            };
        public static readonly Dictionary<string, BreakMode> BreakInsideModes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.Auto, BreakMode.Auto},
                {Keywords.Avoid, BreakMode.Avoid},
                {Keywords.AvoidPage, BreakMode.AvoidPage},
                {Keywords.AvoidColumn, BreakMode.AvoidColumn},
                {Keywords.AvoidRegion, BreakMode.AvoidRegion}
            };
        public static readonly Dictionary<string, float> HorizontalModes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.Left, 0f},
                {Keywords.Center, 0.5f},
                {Keywords.Right, 1f}
            };
        public static readonly Dictionary<string, float> VerticalModes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.Top, 0f},
                {Keywords.Center, 0.5f},
                {Keywords.Bottom, 1f}
            };
        public static readonly Dictionary<string, UnicodeMode> UnicodeModes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.Normal, UnicodeMode.Normal},
                {Keywords.Embed, UnicodeMode.Embed},
                {Keywords.Isolate, UnicodeMode.Isolate},
                {Keywords.IsolateOverride, UnicodeMode.IsolateOverride},
                {Keywords.BidirectionalOverride, UnicodeMode.BidirectionalOverride},
                {Keywords.Plaintext, UnicodeMode.Plaintext}
            };
        public static readonly Dictionary<string, SystemCursor> Cursors =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.Auto, SystemCursor.Auto},
                {Keywords.Default, SystemCursor.Default},
                {Keywords.None, SystemCursor.None},
                {Keywords.ContextMenu, SystemCursor.ContextMenu},
                {Keywords.Help, SystemCursor.Help},
                {Keywords.Pointer, SystemCursor.Pointer},
                {Keywords.Progress, SystemCursor.Progress},
                {Keywords.Wait, SystemCursor.Wait},
                {Keywords.Cell, SystemCursor.Cell},
                {Keywords.Crosshair, SystemCursor.Crosshair},
                {Keywords.Text, SystemCursor.Text},
                {Keywords.VerticalText, SystemCursor.VerticalText},
                {Keywords.Alias, SystemCursor.Alias},
                {Keywords.Copy, SystemCursor.Copy},
                {Keywords.Move, SystemCursor.Move},
                {Keywords.NoDrop, SystemCursor.NoDrop},
                {Keywords.NotAllowed, SystemCursor.NotAllowed},
                {Keywords.EastResize, SystemCursor.EResize},
                {Keywords.NorthResize, SystemCursor.NResize},
                {Keywords.NorthEastResize, SystemCursor.NeResize},
                {Keywords.NorthWestResize, SystemCursor.NwResize},
                {Keywords.SouthResize, SystemCursor.SResize},
                {Keywords.SouthEastResize, SystemCursor.SeResize},
                {Keywords.SouthWestResize, SystemCursor.WResize},
                {Keywords.WestResize, SystemCursor.WResize},
                {Keywords.EastWestResize, SystemCursor.EwResize},
                {Keywords.NorthSouthResize, SystemCursor.NsResize},
                {Keywords.NorthEastSouthWestResize, SystemCursor.NeswResize},
                {Keywords.NorthWestSouthEastResize, SystemCursor.NwseResize},
                {Keywords.ColResize, SystemCursor.ColResize},
                {Keywords.RowResize, SystemCursor.RowResize},
                {Keywords.AllScroll, SystemCursor.AllScroll},
                {Keywords.ZoomIn, SystemCursor.ZoomIn},
                {Keywords.ZoomOut, SystemCursor.ZoomOut},
                {Keywords.Grab, SystemCursor.Grab},
                {Keywords.Grabbing, SystemCursor.Grabbing}
            };
        public static readonly Dictionary<string, PositionMode> PositionModes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.Static, PositionMode.Static},
                {Keywords.Relative, PositionMode.Relative},
                {Keywords.Absolute, PositionMode.Absolute},
                {Keywords.Sticky, PositionMode.Sticky},
                {Keywords.Fixed, PositionMode.Fixed}
            };
        public static readonly Dictionary<string, Overflow> OverflowModes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.Visible, Overflow.Visible},
                {Keywords.Hidden, Overflow.Hidden},
                {Keywords.Scroll, Overflow.Scroll},
                {Keywords.Auto, Overflow.Auto},
                {Keywords.Clip, Overflow.Clip}
            };
        public static readonly Dictionary<string, Floating> FloatingModes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.None, Floating.None},
                {Keywords.Left, Floating.Left},
                {Keywords.Right, Floating.Right},
                {Keywords.InlineStart, Floating.InlineStart},
                {Keywords.InlineEnd, Floating.InlineEnd}
            };
        public static readonly Dictionary<string, DisplayMode> DisplayModes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.None, DisplayMode.None},
                {Keywords.Inline, DisplayMode.Inline},
                {Keywords.Block, DisplayMode.Block},
                {Keywords.InlineBlock, DisplayMode.InlineBlock},
                {Keywords.ListItem, DisplayMode.ListItem},
                {Keywords.InlineTable, DisplayMode.InlineTable},
                {Keywords.Table, DisplayMode.Table},
                {Keywords.TableCaption, DisplayMode.TableCaption},
                {Keywords.TableCell, DisplayMode.TableCell},
                {Keywords.TableColumn, DisplayMode.TableColumn},
                {Keywords.TableColumnGroup, DisplayMode.TableColumnGroup},
                {Keywords.TableFooterGroup, DisplayMode.TableFooterGroup},
                {Keywords.TableHeaderGroup, DisplayMode.TableHeaderGroup},
                {Keywords.TableRow, DisplayMode.TableRow},
                {Keywords.TableRowGroup, DisplayMode.TableRowGroup},
                {Keywords.Flex, DisplayMode.Flex},
                {Keywords.InlineFlex, DisplayMode.InlineFlex},
                {Keywords.Grid, DisplayMode.Grid},
                {Keywords.InlineGrid, DisplayMode.InlineGrid},
                {Keywords.Contents, DisplayMode.Contents},
                {Keywords.FlowRoot, DisplayMode.FlowRoot}
            };
        public static readonly Dictionary<string, ClearMode> ClearModes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.None, ClearMode.None},
                {Keywords.Left, ClearMode.Left},
                {Keywords.Right, ClearMode.Right},
                {Keywords.Both, ClearMode.Both},
                {Keywords.InlineStart, ClearMode.InlineStart},
                {Keywords.InlineEnd, ClearMode.InlineEnd}
            };
        public static readonly Dictionary<string, BackgroundRepeat> BackgroundRepeats =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.NoRepeat, BackgroundRepeat.NoRepeat},
                {Keywords.Repeat, BackgroundRepeat.Repeat},
                {Keywords.Round, BackgroundRepeat.Round},
                {Keywords.Space, BackgroundRepeat.Space}
            };
        public static readonly Dictionary<string, BlendMode> BlendModes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.Color, BlendMode.Color},
                {Keywords.ColorBurn, BlendMode.ColorBurn},
                {Keywords.ColorDodge, BlendMode.ColorDodge},
                {Keywords.Darken, BlendMode.Darken},
                {Keywords.Difference, BlendMode.Difference},
                {Keywords.Exclusion, BlendMode.Exclusion},
                {Keywords.HardLight, BlendMode.HardLight},
                {Keywords.Hue, BlendMode.Hue},
                {Keywords.Lighten, BlendMode.Lighten},
                {Keywords.Luminosity, BlendMode.Luminosity},
                {Keywords.Multiply, BlendMode.Multiply},
                {Keywords.Normal, BlendMode.Normal},
                {Keywords.Overlay, BlendMode.Overlay},
                {Keywords.Saturation, BlendMode.Saturation},
                {Keywords.Screen, BlendMode.Screen},
                {Keywords.SoftLight, BlendMode.SoftLight}
            };
        public static readonly Dictionary<string, UpdateFrequency> UpdateFrequencies =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.None, UpdateFrequency.None},
                {Keywords.Slow, UpdateFrequency.Slow},
                {Keywords.Normal, UpdateFrequency.Normal}
            };
        public static readonly Dictionary<string, ScriptingState> ScriptingStates =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.None, ScriptingState.None},
                {Keywords.InitialOnly, ScriptingState.InitialOnly},
                {Keywords.Enabled, ScriptingState.Enabled}
            };
        public static readonly Dictionary<string, PointerAccuracy> PointerAccuracies =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.None, PointerAccuracy.None},
                {Keywords.Coarse, PointerAccuracy.Coarse},
                {Keywords.Fine, PointerAccuracy.Fine}
            };
        public static readonly Dictionary<string, HoverAbility> HoverAbilities =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.None, HoverAbility.None},
                {Keywords.OnDemand, HoverAbility.OnDemand},
                {Keywords.Hover, HoverAbility.Hover}
            };
        public static readonly Dictionary<string, RadialGradient.SizeMode> RadialGradientSizeModes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.ClosestSide, RadialGradient.SizeMode.ClosestSide},
                {Keywords.FarthestSide, RadialGradient.SizeMode.FarthestSide},
                {Keywords.ClosestCorner, RadialGradient.SizeMode.ClosestCorner},
                {Keywords.FarthestCorner, RadialGradient.SizeMode.FarthestCorner}
            };
        public static readonly Dictionary<string, ObjectFitting> ObjectFittings =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.None, ObjectFitting.None},
                {Keywords.Cover, ObjectFitting.Cover},
                {Keywords.Contain, ObjectFitting.Contain},
                {Keywords.Fill, ObjectFitting.Fill},
                {Keywords.ScaleDown, ObjectFitting.ScaleDown}
            };
        public static readonly Dictionary<string, FontWeight> FontWeights =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.Normal, FontWeight.Normal},
                {Keywords.Bold, FontWeight.Bold},
                {Keywords.Bolder, FontWeight.Bolder},
                {Keywords.Lighter, FontWeight.Lighter}
            };
        public static readonly Dictionary<string, SystemFont> SystemFonts =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.Caption, SystemFont.Caption},
                {Keywords.Icon, SystemFont.Icon},
                {Keywords.Menu, SystemFont.Menu},
                {Keywords.MessageBox, SystemFont.MessageBox},
                {Keywords.SmallCaption, SystemFont.SmallCaption},
                {Keywords.StatusBar, SystemFont.StatusBar}
            };
        public static readonly Dictionary<string, StrokeLinecap> StrokeLinecaps =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.Butt, StrokeLinecap.Butt},
                {Keywords.Round, StrokeLinecap.Round},
                {Keywords.Square, StrokeLinecap.Square}
            };
        public static readonly Dictionary<string, StrokeLinejoin> StrokeLinejoins =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.Miter, StrokeLinejoin.Miter},
                {Keywords.Round, StrokeLinejoin.Round},
                {Keywords.Bevel, StrokeLinejoin.Bevel}
            };
        public static readonly Dictionary<string, WordBreak> WordBreaks =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.Normal, WordBreak.Normal},
                {Keywords.BreakAll, WordBreak.BreakAll},
                {Keywords.KeepAll, WordBreak.KeepAll}
            };
        public static readonly Dictionary<string, OverflowWrap> OverflowWraps =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.Normal, OverflowWrap.Normal},
                {Keywords.BreakWord, OverflowWrap.BreakWord},
                {Keywords.Anywhere, OverflowWrap.Anywhere}
            };
        public static readonly Dictionary<string, FillRule> FillRules =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.Nonzero, FillRule.Nonzero},
                {Keywords.Evenodd, FillRule.Evenodd}
            };

        public static readonly Dictionary<string, FlexDirection> FlexDirections =
            new(StringComparer.OrdinalIgnoreCase)
            {
                { Keywords.Row, FlexDirection.Row },
                { Keywords.RowReverse, FlexDirection.RowReverse },
                { Keywords.Column, FlexDirection.Column },
                { Keywords.ColumnReverse, FlexDirection.ColumnReverse }
            };

        public static readonly Dictionary<string, FlexWrap> FlexWraps =
            new(StringComparer.OrdinalIgnoreCase)
            {
                { Keywords.Nowrap, FlexWrap.NoWrap },
                { Keywords.Wrap, FlexWrap.Wrap },
                { Keywords.WrapReverse, FlexWrap.WrapReverse }
            };

        public static readonly Dictionary<string, IntrinsicSizing> IntrinsicSizings =
            new(StringComparer.OrdinalIgnoreCase)
            {
                { Keywords.MaxContent, IntrinsicSizing.MaxContent },
                { Keywords.MinContent, IntrinsicSizing.MinContent },
                { Keywords.FitContent, IntrinsicSizing.FitContent },
                { Keywords.Content, IntrinsicSizing.Content }
            };

        public static readonly Dictionary<string, AlignContent> AlignContents =
            new(StringComparer.OrdinalIgnoreCase)
            {
                { Keywords.Center, AlignContent.Center },
                { Keywords.Start, AlignContent.Start },
                { Keywords.End, AlignContent.End },
                { Keywords.FlexStart, AlignContent.FlexStart },
                { Keywords.FlexEnd, AlignContent.FlexEnd },
                { Keywords.Normal, AlignContent.Normal },
                { Keywords.Baseline, AlignContent.Baseline },
                { Keywords.SpaceBetween, AlignContent.SpaceBetween },
                { Keywords.SpaceAround, AlignContent.SpaceAround },
                { Keywords.SpaceEvenly, AlignContent.SpaceEvenly },
                { Keywords.Stretch, AlignContent.Stretch },
            };

        public static readonly Dictionary<string, AlignItem> AlignItems =
            new(StringComparer.OrdinalIgnoreCase)
            {
                { Keywords.Normal, AlignItem.Normal },
                { Keywords.Stretch, AlignItem.Stretch },
                { Keywords.Center, AlignItem.Center },
                { Keywords.Start, AlignItem.Start },
                { Keywords.End, AlignItem.End },
                { Keywords.FlexStart, AlignItem.FlexStart },
                { Keywords.FlexEnd, AlignItem.FlexEnd },
                { Keywords.SelfStart, AlignItem.SelfStart },
                { Keywords.SelfEnd, AlignItem.SelfEnd },
                { Keywords.Baseline, AlignItem.Baseline },
            };

        public static readonly Dictionary<string, ContainerType> ContainerTypes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                {Keywords.Normal, ContainerType.Normal},
                {Keywords.Size, ContainerType.Size},
                {Keywords.InlineSize, ContainerType.InlineSize}
            };
    }
}
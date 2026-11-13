using System;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal static class Converters
    {
        public static readonly IValueConverter LineWidthConverter =
            new StructValueConverter<Length>(ValueExtensions.ToBorderWidth)
                .Or(new CalcValueConverter(CalcCategory.Length));

        public static readonly IValueConverter LengthConverter =
            new StructValueConverter<Length>(ValueExtensions.ToLength)
                .Or(new CalcValueConverter(CalcCategory.Length));

        public static readonly IValueConverter ResolutionConverter =
            new StructValueConverter<Resolution>(ValueExtensions.ToResolution);
        
        public static readonly IValueConverter TimeConverter = new StructValueConverter<Time>(ValueExtensions.ToTime);
        public static readonly IValueConverter UrlConverter = new UrlValueConverter();
        public static readonly IValueConverter StringConverter = new StringValueConverter();
        public static readonly IValueConverter EvenStringsConverter = new StringsValueConverter();

        public static readonly IValueConverter LiteralsConverter =
            new IdentifierValueConverter(ValueExtensions.ToLiterals);

        public static readonly IValueConverter IdentifierConverter =
            new IdentifierValueConverter(ValueExtensions.ToIdentifierCaseInsensitive);

        public static readonly IValueConverter AnimationNameConverter =
            new IdentifierValueConverter(ValueExtensions.ToAnimationName);

        public static readonly IValueConverter AnimatableConverter =
            new IdentifierValueConverter(ValueExtensions.ToAnimatableIdentifier);

        public static readonly IValueConverter IntegerConverter =
            new StructValueConverter<int>(ValueExtensions.ToInteger);

        public static readonly IValueConverter NaturalIntegerConverter =
            new StructValueConverter<int>(ValueExtensions.ToNaturalInteger);

        public static readonly IValueConverter WeightNumberConverter =
            new StructValueConverter<double>(ValueExtensions.ToWeightNumber)
                .Or(new CalcValueConverter(CalcCategory.Number));

        public static readonly IValueConverter PositiveIntegerConverter =
            new StructValueConverter<int>(ValueExtensions.ToPositiveInteger);

        public static readonly IValueConverter
            BinaryConverter = new StructValueConverter<int>(ValueExtensions.ToBinary);

        public static readonly IValueConverter
            AngleConverter = new StructValueConverter<Angle>(ValueExtensions.ToAngle)
                .Or(new CalcValueConverter(CalcCategory.Angle));

        /// <summary><c>[ &lt;angle&gt; | &lt;zero&gt; ]</c> for the transform functions; the general
        /// <see cref="AngleConverter"/> stays strict about a unitless zero.</summary>
        public static readonly IValueConverter TransformAngleConverter =
            new StructValueConverter<Angle>(ValueExtensions.ToAngleOrZero)
                .Or(new CalcValueConverter(CalcCategory.Angle));

        public static readonly IValueConverter NumberConverter =
            new StructValueConverter<float>(ValueExtensions.ToSingle)
                .Or(new CalcValueConverter(CalcCategory.Number));

        public static readonly IValueConverter NaturalNumberConverter =
            new StructValueConverter<float>(ValueExtensions.ToNaturalSingle);

        public static readonly IValueConverter PercentConverter =
            new StructValueConverter<Percent>(ValueExtensions.ToPercent);

        public static readonly IValueConverter RgbComponentConverter =
            new StructValueConverter<byte>(ValueExtensions.ToRgbComponent);

        public static readonly IValueConverter AlphaValueConverter =
            new StructValueConverter<float>(ValueExtensions.ToAlphaValue);

        public static readonly IValueConverter PureColorConverter =
            new StructValueConverter<Color>(ValueExtensions.ToColor);

        public static readonly IValueConverter LengthOrPercentConverter =
            new StructValueConverter<Length>(ValueExtensions.ToDistance)
                .Or(new CalcValueConverter(CalcCategory.LengthPercentage));

        public static readonly IValueConverter PercentOrFractionConverter =
            new StructValueConverter<Percent>(ValueExtensions.ToPercentOrFraction);

        public static readonly IValueConverter PercentOrNumberConverter =
            new StructValueConverter<Number>(ValueExtensions.ToPercentOrNumber);

        public static readonly IValueConverter AngleNumberConverter =
            new StructValueConverter<Angle>(ValueExtensions.ToAngleNumber)
                .Or(new CalcValueConverter(CalcCategory.Angle | CalcCategory.Number));

        public static readonly IValueConverter SideOrCornerConverter = WithAny(
            Assign(Keywords.Left, -1.0).Or(Keywords.Right, 1.0).Option(0.0),
            Assign(Keywords.Top, 1.0).Or(Keywords.Bottom, -1.0).Option(0.0)
        );

        public static readonly IValueConverter PointConverter = Construct(() =>
        {
            var hi = Assign(Keywords.Left, Length.Zero)
                .Or(Keywords.Right, new Length(100f, Length.Unit.Percent))
                .Or(Keywords.Center, new Length(50f, Length.Unit.Percent));
            var vi = Assign(Keywords.Top, Length.Zero)
                .Or(Keywords.Bottom, new Length(100f, Length.Unit.Percent))
                .Or(Keywords.Center, new Length(50f, Length.Unit.Percent));
            var h = hi.Or(LengthOrPercentConverter).Required();
            var v = vi.Or(LengthOrPercentConverter).Required();

            return LengthOrPercentConverter.Or(
                Toggle(Keywords.Left, Keywords.Right)).Or(
                Toggle(Keywords.Top, Keywords.Bottom)).Or(
                Keywords.Center, Point.Center).Or(
                WithOrder(h, v)).Or(
                WithOrder(v, h)).Or(
                WithOrder(hi, vi, LengthOrPercentConverter)).Or(
                WithOrder(hi, LengthOrPercentConverter, vi)).Or(
                WithOrder(hi, LengthOrPercentConverter, vi, LengthOrPercentConverter)).Or(
                // The 3/4-value edge-offset syntax is "&&", so the vertical component may come first too
                // (CSS Backgrounds 3 3.6): "bottom 10px right", "bottom right 20px",
                // "bottom 10px right 20px". These mirror the horizontal-first forms above.
                WithOrder(vi, hi, LengthOrPercentConverter)).Or(
                WithOrder(vi, LengthOrPercentConverter, hi)).Or(
                WithOrder(vi, LengthOrPercentConverter, hi, LengthOrPercentConverter));
        });

        public static readonly IValueConverter AttrConverter = new FunctionValueConverter(
            FunctionNames.Attr, WithArgs(StringConverter.Or(IdentifierConverter)));

        public static readonly IValueConverter StepsConverter = new FunctionValueConverter(
            FunctionNames.Steps, WithArgs(
                IntegerConverter.Required(),
                Assign(Keywords.Start, true).Or(Keywords.End, false)
                    .Or(Keywords.JumpStart, true).Or(Keywords.JumpEnd, false)
                    .Or(Keywords.JumpNone, false).Or(Keywords.JumpBoth, true)
                    .Option(false)));

        public static readonly IValueConverter LinearEasingConverter = new LinearEasingValueConverter();

        public static readonly IValueConverter CubicBezierConverter = Construct(() =>
        {
            var number = NumberConverter.Required();
            return new FunctionValueConverter(FunctionNames.CubicBezier,
                WithArgs(number, number, number, number));
        });

        public static readonly IValueConverter CounterConverter = Construct(() =>
        {
            var name = IdentifierConverter.Required();
            var kind = IdentifierConverter.Option(Keywords.Decimal);
            var def = StringConverter.Required();
            // The Or is between the two functions, not inside counter()'s arguments; otherwise a
            // top-level counters() is rejected.
            return new FunctionValueConverter(FunctionNames.Counter, WithArgs(name, kind))
                .Or(new FunctionValueConverter(FunctionNames.Counters, WithArgs(name, def, kind)));
        });

        /// <summary>
        /// The items of a CSS Generated Content <c>&lt;content-list&gt;</c>, shared by <c>content</c>,
        /// <c>string-set</c> and <c>bookmark-label</c>.
        /// </summary>
        public static readonly IValueConverter ContentListItemConverter =
            StringConverter.Or(AttrConverter).Or(CounterConverter).Or(
                new ContentFunctionConverter()).Or(
                new StringFunctionConverter()).Or(
                new TargetCounterFunctionConverter(false)).Or(
                new TargetCounterFunctionConverter(true)).Or(
                new TargetTextFunctionConverter()).Or(
                new LeaderFunctionConverter()).Or(
                new GcpmElementFunctionConverter());

        public static readonly IValueConverter ShapeConverter = Construct(() =>
        {
            // Each rect() edge is <length> | auto (CSS 2.1 11.1.2).
            var edge = LengthConverter.OrAuto().Required();
            return new FunctionValueConverter(FunctionNames.Rect, WithArgs(edge, edge, edge, edge)
                .Or(WithArgs(LengthConverter.OrAuto().Many(4, 4))));
        }).OrAuto();

        public static readonly IValueConverter LinearGradientConverter = Construct(() =>
            new FunctionValueConverter(FunctionNames.LinearGradient, new LinearGradientConverter()).Or(
                new FunctionValueConverter(FunctionNames.RepeatingLinearGradient, new LinearGradientConverter())));

        public static readonly IValueConverter RadialGradientConverter = Construct(() =>
            new FunctionValueConverter(FunctionNames.RadialGradient, new RadialGradientConverter()).Or(
                new FunctionValueConverter(FunctionNames.RepeatingRadialGradient, new RadialGradientConverter())));

        public static readonly IValueConverter ConicGradientConverter = Construct(() =>
            new FunctionValueConverter(FunctionNames.ConicGradient, new ConicGradientConverter()).Or(
                new FunctionValueConverter(FunctionNames.RepeatingConicGradient, new ConicGradientConverter())));

        public static readonly IValueConverter RgbColorConverter = Construct(() =>
        {
            var number = RgbComponentConverter.Required();
            return new FunctionValueConverter(FunctionNames.Rgb, WithArgs(number, number, number));
        });

        public static readonly IValueConverter RgbaColorConverter = Construct(() =>
        {
            var value = RgbComponentConverter.Required();
            var alpha = AlphaValueConverter.Required();
            return new FunctionValueConverter(FunctionNames.Rgba, WithArgs(value, value, value, alpha));
        });

        public static readonly IValueConverter HslColorConverter = Construct(() =>
        {
            var hue = AngleNumberConverter.Required();
            var percent = PercentConverter.Required();
            return new FunctionValueConverter(FunctionNames.Hsl, WithArgs(hue, percent, percent));
        });

        public static readonly IValueConverter HslaColorConverter = Construct(() =>
        {
            var hue = AngleNumberConverter.Required();
            var percent = PercentConverter.Required();
            var alpha = AlphaValueConverter.Required();
            return new FunctionValueConverter(FunctionNames.Hsla, WithArgs(hue, percent, percent, alpha));
        });

        public static readonly IValueConverter GrayColorConverter = Construct(() =>
        {
            var value = RgbComponentConverter.Required();
            var alpha = AlphaValueConverter.Option(1f);
            return new FunctionValueConverter(FunctionNames.Gray, WithArgs(value, alpha));
        });

        public static readonly IValueConverter HwbColorConverter = Construct(() =>
        {
            var hue = AngleNumberConverter.Required();
            var percent = PercentConverter.Required();
            var alpha = AlphaValueConverter.Option(1f);
            return new FunctionValueConverter(FunctionNames.Hwb, WithArgs(hue, percent, percent, alpha));
        });

        // CSS Color 4/5 function forms (lab/oklab/lch/oklch/color-mix). Layer A accepts them leniently
        // (preserving the specified text for serialization and shorthand expansion); the exact grammar
        // check and the sRGB resolution happen via ColorFunctionExtensions, so a
        // `background: oklch(...)` shorthand still carries its color through to the background-color
        // longhand. Reference `new AnyValueConverter()` directly rather than the shared `Any` field,
        // which is initialized later in this file (Construct here runs eagerly, so `Any` would be null).
        public static readonly IValueConverter LabColorConverter =
            new FunctionValueConverter(FunctionNames.Lab, new AnyValueConverter());
        public static readonly IValueConverter OklabColorConverter =
            new FunctionValueConverter(FunctionNames.Oklab, new AnyValueConverter());
        public static readonly IValueConverter LchColorConverter =
            new FunctionValueConverter(FunctionNames.Lch, new AnyValueConverter());
        public static readonly IValueConverter OklchColorConverter =
            new FunctionValueConverter(FunctionNames.Oklch, new AnyValueConverter());
        public static readonly IValueConverter ColorMixConverter =
            new FunctionValueConverter(FunctionNames.ColorMix, new AnyValueConverter());
        // device-cmyk(c m y k [/ alpha]); accepted leniently like the other CSS Color 5 forms, the component check is ColorFunctionExtensions'.
        public static readonly IValueConverter DeviceCmykColorConverter =
            new FunctionValueConverter(FunctionNames.DeviceCmyk, new AnyValueConverter());
        // light-dark(<color>, <color>): both arms must be colors; which one applies needs a color scheme, so it is kept as written.
        public static readonly IValueConverter LightDarkColorConverter =
            new FunctionValueConverter(FunctionNames.LightDark, new ColorPairValueConverter());
        // color(<predefined-space> c1 c2 c3 [/ alpha]); the space/component check is ColorFunctionExtensions'.
        public static readonly IValueConverter ColorFunctionConverter =
            new FunctionValueConverter(FunctionNames.ColorFunction, new AnyValueConverter());

        // Lenient fallbacks for the CSS Color 4 space/slash syntax of the legacy functions. The strict
        // comma-form converters above run first (so their canonical serialization is preserved); these
        // catch the space-separated / slash-alpha forms the strict grammars reject, so e.g.
        // `background: hsl(280 70% 55%)` or `rgb(1 2 3 / .5)` still populate the color longhand and get
        // resolved via ColorFunctionExtensions.
        public static readonly IValueConverter RgbLenientConverter =
            new FunctionValueConverter(FunctionNames.Rgb, new AnyValueConverter());
        public static readonly IValueConverter RgbaLenientConverter =
            new FunctionValueConverter(FunctionNames.Rgba, new AnyValueConverter());
        public static readonly IValueConverter HslLenientConverter =
            new FunctionValueConverter(FunctionNames.Hsl, new AnyValueConverter());
        public static readonly IValueConverter HslaLenientConverter =
            new FunctionValueConverter(FunctionNames.Hsla, new AnyValueConverter());
        public static readonly IValueConverter HwbLenientConverter =
            new FunctionValueConverter(FunctionNames.Hwb, new AnyValueConverter());

        public static readonly IValueConverter PerspectiveConverter =
            Construct(() => new FunctionValueConverter(FunctionNames.Perspective, WithArgs(LengthConverter)));

        public static readonly IValueConverter MatrixTransformConverter = Construct(() =>
            new FunctionValueConverter(FunctionNames.Matrix, WithArgs(NumberConverter, 6)).Or(
                new FunctionValueConverter(FunctionNames.Matrix3d, WithArgs(NumberConverter, 16))));

        public static readonly IValueConverter TranslateTransformConverter = Construct(() =>
        {
            var distance = LengthOrPercentConverter.Required();
            var option = LengthOrPercentConverter.Option(Length.Zero);
            return new FunctionValueConverter(FunctionNames.Translate, WithArgs(distance, option)).Or(
                new FunctionValueConverter(FunctionNames.Translate3d, WithArgs(distance, option, option))).Or(
                new FunctionValueConverter(FunctionNames.TranslateX, WithArgs(LengthOrPercentConverter))).Or(
                new FunctionValueConverter(FunctionNames.TranslateY, WithArgs(LengthOrPercentConverter))).Or(
                new FunctionValueConverter(FunctionNames.TranslateZ, WithArgs(LengthOrPercentConverter)));
        });

        public static readonly IValueConverter ScaleTransformConverter = Construct(() =>
        {
            // CSS Transforms 2: a percentage in a scale function is equivalent to a number.
            var factor = NumberConverter.Or(PercentConverter);
            var number = factor.Required();
            var option = factor.Option(float.NaN);
            return new FunctionValueConverter(FunctionNames.Scale, WithArgs(number, option)).Or(
                new FunctionValueConverter(FunctionNames.Scale3d, WithArgs(number, option, option))).Or(
                new FunctionValueConverter(FunctionNames.ScaleX, WithArgs(factor))).Or(
                new FunctionValueConverter(FunctionNames.ScaleY, WithArgs(factor))).Or(
                new FunctionValueConverter(FunctionNames.ScaleZ, WithArgs(factor)));
        });

        public static readonly IValueConverter RotateTransformConverter = Construct(() =>
        {
            var number = NumberConverter.Required();
            return new FunctionValueConverter(FunctionNames.Rotate, WithArgs(TransformAngleConverter)).Or(
                new FunctionValueConverter(FunctionNames.Rotate3d,
                    WithArgs(number, number, number, TransformAngleConverter.Required()))).Or(
                new FunctionValueConverter(FunctionNames.RotateX, WithArgs(TransformAngleConverter))).Or(
                new FunctionValueConverter(FunctionNames.RotateY, WithArgs(TransformAngleConverter))).Or(
                new FunctionValueConverter(FunctionNames.RotateZ, WithArgs(TransformAngleConverter)));
        });

        // CSS Transforms 2: translate: <length-percentage> [ <length-percentage> <length>? ]?
        public static readonly IValueConverter TranslateIndividualConverter = Construct(() =>
            WithOrder(LengthOrPercentConverter.Required(), LengthOrPercentConverter.Option(), LengthConverter.Option()));

        // scale: [ <number> | <percentage> ]{1,3}
        public static readonly IValueConverter ScaleIndividualConverter = Construct(() =>
        {
            var factor = NumberConverter.Or(PercentConverter);
            return WithOrder(factor.Required(), factor.Option(), factor.Option());
        });

        // rotate: <angle> | [ x | y | z | <number>{3} ] && <angle>
        public static readonly IValueConverter RotateIndividualConverter = Construct(() =>
        {
            var angle = TransformAngleConverter.Required();
            var number = NumberConverter.Required();
            var axisNumbers = WithOrder(number, number, number);
            return TransformAngleConverter.Or(WithAny(Map.RotationAxes.ToConverter().Required(), angle))
                .Or(WithAny(axisNumbers, angle));
        });

        public static readonly IValueConverter SkewTransformConverter = Construct(() =>
        {
            var angle = TransformAngleConverter.Required();
            var optionalAngle = TransformAngleConverter.Option(Angle.Zero);
            return new FunctionValueConverter(FunctionNames.Skew, WithArgs(angle, optionalAngle)).Or(
                new FunctionValueConverter(FunctionNames.SkewX, WithArgs(TransformAngleConverter))).Or(
                new FunctionValueConverter(FunctionNames.SkewY, WithArgs(TransformAngleConverter)));
        });

        public static readonly IValueConverter DefaultFontFamiliesConverter = Map.DefaultFontFamilies.ToConverter();
        public static readonly IValueConverter LineStyleConverter = Map.LineStyles.ToConverter();
        public static readonly IValueConverter BackgroundAttachmentConverter = Map.BackgroundAttachments.ToConverter();
        public static readonly IValueConverter BackgroundRepeatConverter = Map.BackgroundRepeats.ToConverter();
        public static readonly IValueConverter BackgroundClipConverter = Map.BackgroundClips.ToConverter();
        public static readonly IValueConverter BoxModelConverter = Map.BoxModels.ToConverter();
        public static readonly IValueConverter AnimationDirectionConverter = Map.AnimationDirections.ToConverter();
        public static readonly IValueConverter AnimationFillStyleConverter = Map.AnimationFillStyles.ToConverter();
        public static readonly IValueConverter TextDecorationStyleConverter = Map.TextDecorationStyles.ToConverter();

        public static readonly IValueConverter TextDecorationLinesConverter =
            Map.TextDecorationLines.ToConverter().Many().OrNone();

        public static readonly IValueConverter ListPositionConverter = Map.ListPositions.ToConverter();
        public static readonly IValueConverter ListStyleConverter = Map.ListStyles.ToConverter();
        public static readonly IValueConverter BreakModeConverter = Map.BreakModes.ToConverter();
        public static readonly IValueConverter BreakInsideModeConverter = Map.BreakInsideModes.ToConverter();
        public static readonly IValueConverter PageBreakModeConverter = Map.PageBreakModes.ToConverter();
        public static readonly IValueConverter UnicodeModeConverter = Map.UnicodeModes.ToConverter();
        public static readonly IValueConverter VisibilityConverter = Map.Visibilities.ToConverter();
        public static readonly IValueConverter PlayStateConverter = Map.PlayStates.ToConverter();
        public static readonly IValueConverter FontVariantConverter = Map.FontVariants.ToConverter();
        public static readonly IValueConverter DirectionModeConverter = Map.DirectionModes.ToConverter();
        public static readonly IValueConverter HorizontalAlignmentConverter = Map.HorizontalAlignments.ToConverter();
        public static readonly IValueConverter VerticalAlignmentConverter = Map.VerticalAlignments.ToConverter();
        public static readonly IValueConverter WhitespaceConverter = Map.WhitespaceModes.ToConverter();
        public static readonly IValueConverter HyphensConverter = Map.HyphensModes.ToConverter();
        public static readonly IValueConverter TextTransformConverter = Map.TextTransforms.ToConverter();
        public static readonly IValueConverter TextAlignLastConverter = Map.TextAlignmentsLast.ToConverter();
        // text-overflow: clip | ellipsis | <string> (CSS Overflow 3 / 4); the one-value form only.
        public static readonly IValueConverter TextOverflowConverter =
            Map.TextOverflowModes.ToConverter().Or(StringConverter);
        public static readonly IValueConverter TextAnchorConverter = Map.TextAnchors.ToConverter();
        public static readonly IValueConverter TextJustifyConverter = Map.TextJustifyOptions.ToConverter();
        public static readonly IValueConverter ObjectFittingConverter = Map.ObjectFittings.ToConverter();
        public static readonly IValueConverter PositionModeConverter = Map.PositionModes.ToConverter();
        public static readonly IValueConverter OverflowModeConverter = Map.OverflowModes.ToConverter();
        public static readonly IValueConverter FloatingConverter = Map.FloatingModes.ToConverter();
        public static readonly IValueConverter DisplayModeConverter = Map.DisplayModes.ToConverter();
        public static readonly IValueConverter ContainerTypeConverter = Map.ContainerTypes.ToConverter();
        public static readonly IValueConverter ClearModeConverter = Map.ClearModes.ToConverter();
        public static readonly IValueConverter FontStretchConverter = Map.FontStretches.ToConverter();
        // "oblique" alone matches via the plain keyword map (tried first); "oblique <angle>" (CSS Fonts 4
        // 2.4, e.g. "oblique 14deg") only reaches the StartsWithValueConverter branch once the plain
        // single-identifier match has failed - i.e. there are more tokens to account for. AngleConverter is
        // deliberately not Option()-wrapped here: StartsWithValueConverter treats "the wrapped converter
        // returned non-null" as its own "matched" signal, and an Option() converter never returns null, so
        // every other font-style value would then falsely reconstruct as "oblique" when the font shorthand
        // is re-serialized from its longhands.
        public static readonly IValueConverter FontStyleConverter = Map.FontStyles.ToConverter()
            .Or(new StartsWithValueConverter(TokenType.Ident, Keywords.Oblique, AngleConverter));
        public static readonly IValueConverter FontWeightConverter = Map.FontWeights.ToConverter();
        public static readonly IValueConverter SystemFontConverter = Map.SystemFonts.ToConverter();
        public static readonly IValueConverter StrokeLinecapConverter = Map.StrokeLinecaps.ToConverter();
        public static readonly IValueConverter StrokeLinejoinConverter = Map.StrokeLinejoins.ToConverter();
        public static readonly IValueConverter WordBreakConverter = Map.WordBreaks.ToConverter();
        public static readonly IValueConverter OverflowWrapConverter = Map.OverflowWraps.ToConverter();
        public static readonly IValueConverter FillRuleConverter = Map.FillRules.ToConverter();
        public static readonly IValueConverter IntrinsicSizingConverter = Map.IntrinsicSizings.ToConverter();

        public static readonly IValueConverter AlignContentConverter = Construct(() =>
        {
            var alignContentsConverter = Map.AlignContents.ToConverter();

            return alignContentsConverter.Or(alignContentsConverter.ConditionalStartsWithKeyword(Keywords.Center, Keywords.Safe, Keywords.Unsafe))
                                         .Or(alignContentsConverter.ConditionalStartsWithKeyword(Keywords.Baseline, Keywords.First, Keywords.Last))
                                         .OrGlobalValue()
                                         .OrDefault(Keywords.Normal);
        });

        public static readonly IValueConverter AlignItemsConverter = Construct(() =>
        {
            var alignItemsConverter = Map.AlignItems.ToConverter();

            return alignItemsConverter.Or(alignItemsConverter.ConditionalStartsWithKeyword(Keywords.Center, Keywords.Safe, Keywords.Unsafe))
                                      .Or(alignItemsConverter.ConditionalStartsWithKeyword(Keywords.Baseline, Keywords.First, Keywords.Last))
                                      .OrGlobalValue();
        });

        public static readonly IValueConverter JustifyContentConverter = Construct(() =>
        {
            var justifyContentConverter = Map.JustifyContentOptions.ToConverter();

            return justifyContentConverter.Or(justifyContentConverter.ConditionalStartsWithKeyword(Keywords.Center, Keywords.Safe, Keywords.Unsafe))
                                          .Or(justifyContentConverter.ConditionalStartsWithKeyword(Keywords.Baseline, Keywords.First, Keywords.Last))
                                          .OrGlobalValue();
        });

        public static readonly IValueConverter AlignSelfConverter = AlignItemsConverter.OrAuto();

        // justify-items / justify-self share align-items/align-self's value grammar for the keywords the
        // grid engine honors (start/end/center/stretch/normal; baseline falls back to start at layout).
        public static readonly IValueConverter JustifyItemsConverter = AlignItemsConverter;
        public static readonly IValueConverter JustifySelfConverter = AlignSelfConverter;

        // place-items / place-content / place-self: <align> <justify>? — one value applies to both axes.
        public static readonly IValueConverter PlaceItemsConverter =
            AlignItemsConverter.Periodic(PropertyNames.AlignItems, PropertyNames.JustifyItems);
        public static readonly IValueConverter PlaceContentConverter =
            JustifyContentConverter.Periodic(PropertyNames.AlignContent, PropertyNames.JustifyContent);
        public static readonly IValueConverter PlaceSelfConverter =
            AlignSelfConverter.Periodic(PropertyNames.AlignSelf, PropertyNames.JustifySelf);

        #region Specific

        public static readonly IValueConverter OptionalIntegerConverter = IntegerConverter.OrAuto();

        public static readonly IValueConverter PositiveOrInfiniteNumberConverter =
            NaturalNumberConverter.Or(Keywords.Infinite, float.PositiveInfinity);

        public static readonly IValueConverter OptionalNumberConverter = NumberConverter.OrNone();

        //public static readonly IValueConverter LengthOrNormalConverter =
        //    LengthConverter.Or(Keywords.Normal, new Length(1f, Length.Unit.Em));

        public static readonly IValueConverter GapConverter = LengthOrPercentConverter.Or(Keywords.Normal);

        public static readonly IValueConverter LengthOrPercentOrNumberConverter = LengthOrPercentConverter.Or(NumberConverter);

        public static readonly IValueConverter OptionalLengthConverter = LengthConverter.Or(Keywords.Normal);
        public static readonly IValueConverter AutoLengthConverter = LengthConverter.OrAuto();
        public static readonly IValueConverter OptionalLengthOrPercentConverter = LengthOrPercentConverter.OrNone();
        public static readonly IValueConverter AutoLengthOrPercentConverter = LengthOrPercentConverter.OrAuto();
        public static readonly IValueConverter OptionalPercentOrFractionConverter = PercentOrFractionConverter.OrDefault(1f);
        public static readonly IValueConverter OptionalPercentOrNumberConverter = PercentOrNumberConverter.OrDefault(1f);

        public static readonly IValueConverter FontSizeConverter =
            LengthOrPercentConverter.Or(Map.FontSizes.ToConverter());

        public static readonly IValueConverter FlexDirectionConverter = Map.FlexDirections.ToConverter()
                                                                           .OrGlobalValue()
                                                                           .OrDefault(FlexDirection.Row);

        public static readonly IValueConverter FlexWrapConverter = Map.FlexWraps.ToConverter()
                                                                      .OrGlobalValue()
                                                                      .OrDefault(FlexWrap.NoWrap);

        public static readonly IValueConverter FlexGrowShrinkConverter = NumberConverter
                                                                        .OrGlobalValue()
                                                                        .OrDefault(0);

        public static readonly IValueConverter FlexBasisConverter = AutoLengthOrPercentConverter
                                                                   .Or(IntrinsicSizingConverter)
                                                                   .OrGlobalValue()
                                                                   .OrDefault(Keywords.Auto);

        public static readonly IValueConverter FlexFlowConverter = Construct(() =>
        {
            var directionConverter = FlexDirectionConverter.For(PropertyNames.FlexDirection);
            var wrapConverter = FlexWrapConverter.For(PropertyNames.FlexWrap);

            // flex-flow is "<'flex-direction'> || <'flex-wrap'>" (CSS Flexbox 1 5.1): the double bar means
            // the two values may appear in either order, so the pair has to be WithAny, not WithOrder.
            return directionConverter
                  .Or(wrapConverter)
                  .Or(WithAny(directionConverter, wrapConverter));

        });

        public static readonly IValueConverter FlexConverter = Construct(() =>
        {
            var flexGrow = FlexGrowShrinkConverter.WithFallback(1).For(PropertyNames.FlexGrow);
            var flexShrink = FlexGrowShrinkConverter.WithFallback(1).For(PropertyNames.FlexShrink);
            var flexBasis = FlexBasisConverter.WithFallback(0).For(PropertyNames.FlexBasis);

            return WithOrder(flexGrow, flexShrink, flexBasis)
                  .OrGlobalValue()
                  .OrNone();
        });

        #endregion

        #region Composed

        public static readonly IValueConverter LineHeightConverter =
            LengthOrPercentConverter.Or(NumberConverter).Or(Keywords.Normal);

        public static readonly IValueConverter BorderSliceConverter = PercentConverter.Or(NumberConverter);

        public static readonly IValueConverter ImageBorderWidthConverter =
            LengthOrPercentConverter.Or(NumberConverter).Or(Keywords.Auto);

        public static readonly IValueConverter TransitionConverter = new DictionaryValueConverter<ITimingFunction>(
            Map.TimingFunctions)
            .Or(new EasingArgumentRangeValueConverter(StepsConverter))
            .Or(new EasingArgumentRangeValueConverter(CubicBezierConverter))
            .Or(LinearEasingConverter);

        public static readonly IValueConverter GradientConverter =
            LinearGradientConverter.Or(RadialGradientConverter).Or(ConicGradientConverter);

        public static readonly IValueConverter TransformConverter = MatrixTransformConverter
            .Or(ScaleTransformConverter)
            .Or(RotateTransformConverter)
            .Or(TranslateTransformConverter)
            .Or(SkewTransformConverter)
            .Or(PerspectiveConverter);

        public static readonly IValueConverter ColorConverter = PureColorConverter
            .Or(RgbColorConverter.Or(RgbaColorConverter))
            .Or(HslColorConverter.Or(HslaColorConverter))
            .Or(GrayColorConverter.Or(HwbColorConverter))
            .Or(LabColorConverter.Or(OklabColorConverter))
            .Or(LchColorConverter.Or(OklchColorConverter))
            .Or(ColorMixConverter.Or(ColorFunctionConverter))
            .Or(LightDarkColorConverter)
            .Or(DeviceCmykColorConverter)
            .Or(RgbLenientConverter.Or(RgbaLenientConverter))
            .Or(HslLenientConverter.Or(HslaLenientConverter))
            .Or(HwbLenientConverter);

        public static readonly IValueConverter CurrentColorConverter = ColorConverter.WithCurrentColor();
        public static readonly IValueConverter InvertedColorConverter = CurrentColorConverter.Or(Keywords.Invert);
        public static readonly IValueConverter PaintConverter = UrlConverter.Or(CurrentColorConverter.OrNone());

        public static readonly IValueConverter StrokeDasharrayConverter =
            LengthOrPercentConverter.Or(NumberConverter).Many().OrNone();

        public static readonly IValueConverter StrokeMiterlimitConverter =
            new StructValueConverter<float>(ValueExtensions.ToGreaterOrEqualOneSingle);

        public static readonly IValueConverter RatioConverter = WithOrder(
            IntegerConverter.Required(),
            IntegerConverter.StartsWithDelimiter().Required());

        // A single grid <grid-line> (auto | <integer> | span <integer>), validated by GridLineGrammar.
        public static readonly IValueConverter GridLineConverter = new GridLineValueConverter();

        // grid-column / grid-row / grid-area: slash-separated <grid-line> components with the CSS Grid
        // §8.3.1 omitted-value copy rule (a bare <custom-ident> propagates to the paired/all edges) — the
        // generic WithOrder(...).Option() DSL resets omitted slots to auto, which is wrong for named areas.
        public static readonly IValueConverter GridColumnConverter =
            new GridColumnRowShorthandValueConverter(PropertyNames.GridColumnStart, PropertyNames.GridColumnEnd);

        public static readonly IValueConverter GridRowConverter =
            new GridColumnRowShorthandValueConverter(PropertyNames.GridRowStart, PropertyNames.GridRowEnd);

        public static readonly IValueConverter GridAreaConverter = new GridAreaShorthandValueConverter();

        public static readonly IValueConverter GridTemplateConverter = new GridTemplateShorthandValueConverter();

        public static readonly IValueConverter GridConverter = new GridShorthandValueConverter();

        public static readonly IValueConverter ShadowConverter = new ShadowValueConverter(WithAny(
            Assign(Keywords.Inset, true).Option(false),
            LengthConverter.Many(2, 4).Required(),
            ColorConverter.WithCurrentColor().Option(Color.Black)), 4);

        public static readonly IValueConverter MultipleShadowConverter = ShadowConverter.FromList().OrNone();

        // CSS Text Decoration 4 §4, text-shadow takes inset and a spread distance, which can't be negative
        public static readonly IValueConverter TextShadowConverter = new ShadowValueConverter(WithAny(
            Assign(Keywords.Inset, true).Option(false),
            LengthConverter.Many(2, 4).Required(),
            ColorConverter.WithCurrentColor().Option(Color.Black)), 4, nonNegativeSpread: true).FromList().OrNone();
        // The CSS Images 4 image functions (image-set(), cross-fade(), element()). Composed into
        // ImageSourceConverter so every <image> property accepts them.
        public static readonly IValueConverter ImageSetImageConverter =
            Construct(() => new FunctionValueConverter(FunctionNames.ImageSet, new ImageSetConverter()));
        public static readonly IValueConverter CrossFadeImageConverter =
            Construct(() => new FunctionValueConverter(FunctionNames.CrossFade, new CrossFadeConverter()));
        public static readonly IValueConverter ElementImageConverter =
            Construct(() => new FunctionValueConverter(FunctionNames.Element, new ElementImageConverter()));

        public static readonly IValueConverter ImageSourceConverter = UrlConverter.Or(GradientConverter)
            .Or(ImageSetImageConverter).Or(CrossFadeImageConverter).Or(ElementImageConverter);
        public static readonly IValueConverter OptionalImageSourceConverter = ImageSourceConverter.OrNone();
        public static readonly IValueConverter MultipleImageSourceConverter = OptionalImageSourceConverter.FromList();
        public static readonly IValueConverter BorderRadiusShorthandConverter = new BorderRadiusConverter();

        public static readonly IValueConverter BorderRadiusConverter = WithOrder(
            LengthOrPercentConverter.Required(), LengthOrPercentConverter.Option());

        public static readonly IValueConverter CornerShapeConverter = new CornerShapeValueConverter();

        public static readonly IValueConverter FontFamiliesConverter =
            DefaultFontFamiliesConverter.Or(StringConverter).Or(LiteralsConverter).FromList();

        public static readonly IValueConverter BackgroundSizeConverter = AutoLengthOrPercentConverter.Or(
            Keywords.Cover).Or(Keywords.Contain).Or(
            WithOrder(AutoLengthOrPercentConverter.Required(), AutoLengthOrPercentConverter.Required()));

        public static readonly IValueConverter BackgroundRepeatsConverter = BackgroundRepeatConverter.Or(
            Keywords.RepeatX).Or(Keywords.RepeatY).Or(
            WithOrder(BackgroundRepeatConverter.Required(), BackgroundRepeatConverter.Required()));

        #endregion

        #region Toggles

        public static readonly IValueConverter TableLayoutConverter = Toggle(Keywords.Fixed, Keywords.Auto);
        public static readonly IValueConverter EmptyCellsConverter = Toggle(Keywords.Show, Keywords.Hide);
        public static readonly IValueConverter CaptionSideConverter = Toggle(Keywords.Top, Keywords.Bottom);
        public static readonly IValueConverter BackfaceVisibilityConverter = Toggle(Keywords.Visible, Keywords.Hidden);
        public static readonly IValueConverter BorderCollapseConverter = Toggle(Keywords.Separate, Keywords.Collapse);
        public static readonly IValueConverter BoxDecorationConverter = Toggle(Keywords.Clone, Keywords.Slice);
        public static readonly IValueConverter ColumnSpanConverter = Toggle(Keywords.All, Keywords.None);
        public static readonly IValueConverter ColumnFillConverter = Toggle(Keywords.Balance, Keywords.Auto);
        public static readonly IValueConverter BoxSizingConverter = Toggle(Keywords.ContentBox, Keywords.BorderBox);

        #endregion

        #region Misc

        public static IValueConverter Any = new AnyValueConverter();

        public static IValueConverter Assign<T>(string identifier, T result)
        {
            return new IdentifierValueConverter<T>(identifier, result);
        }

        public static IValueConverter Toggle(string on, string off)
        {
            return Assign(on, true).Or(off, false);
        }

        #endregion

        #region Order / Unordered

        public static IValueConverter WithOrder(params IValueConverter[] converters)
        {
            return new OrderedOptionsConverter(converters);
        }

        /// <summary>
        /// Like <see cref="WithOrder"/>, but the converters after the first only take part when the first one consumed tokens.
        /// <c>&lt;position&gt; [ / &lt;size&gt; ]?</c>: a size cannot stand without the position before its slash.
        /// </summary>
        public static IValueConverter WithOrderDependingOnFirst(params IValueConverter[] converters)
        {
            return new OrderedOptionsConverter(true, converters);
        }

        public static IValueConverter WithAny(params IValueConverter[] converters)
        {
            return new UnorderedOptionsConverter(converters);
        }

        public static IValueConverter WithAnyOrderIndependent(params IValueConverter[] converters)
        {
            return new OrderIndependentOptionsConverter(converters);
        }

        public static IValueConverter Continuous(IValueConverter converter)
        {
            return new ContinuousValueConverter(converter);
        }

        #endregion

        #region Helper

        private static IValueConverter Construct(Func<IValueConverter> f)
        {
            return f();
        }

        private static IValueConverter WithArgs(IValueConverter converter, int arguments)
        {
            var converters = Enumerable.Repeat(converter, arguments).ToArray();
            return WithArgs(converters);
        }

        private static IValueConverter WithArgs(IValueConverter converter)
        {
            return new ArgumentsValueConverter(converter);
        }

        private static IValueConverter WithArgs(params IValueConverter[] converters)
        {
            return new ArgumentsValueConverter(converters);
        }

        #endregion
    }
}
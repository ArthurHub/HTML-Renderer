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

        public static readonly IValueConverter AnimatableConverter =
            new IdentifierValueConverter(ValueExtensions.ToAnimatableIdentifier);

        public static readonly IValueConverter IntegerConverter =
            new StructValueConverter<int>(ValueExtensions.ToInteger);

        public static readonly IValueConverter NaturalIntegerConverter =
            new StructValueConverter<int>(ValueExtensions.ToNaturalInteger);

        public static readonly IValueConverter WeightIntegerConverter =
            new StructValueConverter<int>(ValueExtensions.ToWeightInteger);

        public static readonly IValueConverter PositiveIntegerConverter =
            new StructValueConverter<int>(ValueExtensions.ToPositiveInteger);

        public static readonly IValueConverter
            BinaryConverter = new StructValueConverter<int>(ValueExtensions.ToBinary);

        public static readonly IValueConverter
            AngleConverter = new StructValueConverter<Angle>(ValueExtensions.ToAngle)
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

        public static IValueConverter Any = new AnyValueConverter();

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

        // The 1/2/3/4-token background-position grammar (including the edge-relative offset syntax,
        // e.g. "right 20px bottom 10px") is implemented once in BackgroundPositionGrammar and shared
        // with the render layer's BackgroundLayerResolver, rather than re-implemented as a second,
        // independent parser of the same value string - see that class's doc comment.
        public static readonly IValueConverter PointConverter = new BackgroundPositionValueConverter();

        public static readonly IValueConverter AttrConverter = new FunctionValueConverter(
            FunctionNames.Attr, WithArgs(StringConverter.Or(IdentifierConverter)));

        public static readonly IValueConverter StepsConverter = new FunctionValueConverter(
            FunctionNames.Steps, WithArgs(
                IntegerConverter.Required(),
                Assign(Keywords.Start, true).Or(Keywords.End, false).Option(false)));

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
            return new FunctionValueConverter(FunctionNames.Counter, WithArgs(name, kind)
                .Or(new FunctionValueConverter(FunctionNames.Counters, WithArgs(name, def, kind))));
        });

        public static readonly IValueConverter ShapeConverter = Construct(() =>
        {
            var length = LengthConverter.Required();
            return new FunctionValueConverter(FunctionNames.Rect, WithArgs(length, length, length, length)
                .Or(WithArgs(LengthConverter.Many(4, 4))));
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

        // CSS Color 4/5 function forms. These validate the function name and accept its argument list
        // leniently (preserving the specified text for serialization and shorthand expansion); the
        // exact grammar check and the sRGB resolution happen in the render layer
        // (ColorFunctionExtensions), so a `background: oklch(...)` shorthand still carries its color
        // through to the background-color longhand. Construct(...) defers the `Any` reference until
        // first use, avoiding a static-init ordering dependency.
        public static readonly IValueConverter LabColorConverter =
            Construct(() => new FunctionValueConverter(FunctionNames.Lab, Any));
        public static readonly IValueConverter OklabColorConverter =
            Construct(() => new FunctionValueConverter(FunctionNames.Oklab, Any));
        public static readonly IValueConverter LchColorConverter =
            Construct(() => new FunctionValueConverter(FunctionNames.Lch, Any));
        public static readonly IValueConverter OklchColorConverter =
            Construct(() => new FunctionValueConverter(FunctionNames.Oklch, Any));
        public static readonly IValueConverter ColorMixConverter =
            Construct(() => new FunctionValueConverter(FunctionNames.ColorMix, Any));

        // Lenient fallbacks for the CSS Color 4 space/slash syntax of the legacy functions. The strict
        // comma-form converters above run first (so their canonical serialization is preserved); these
        // catch the space-separated / slash-alpha forms the strict grammars reject, so e.g.
        // `background: hsl(280 70% 55%)` or `rgb(1 2 3 / .5)` still populate the color longhand and get
        // resolved in the render layer.
        public static readonly IValueConverter RgbLenientConverter =
            Construct(() => new FunctionValueConverter(FunctionNames.Rgb, Any));
        public static readonly IValueConverter RgbaLenientConverter =
            Construct(() => new FunctionValueConverter(FunctionNames.Rgba, Any));
        public static readonly IValueConverter HslLenientConverter =
            Construct(() => new FunctionValueConverter(FunctionNames.Hsl, Any));
        public static readonly IValueConverter HslaLenientConverter =
            Construct(() => new FunctionValueConverter(FunctionNames.Hsla, Any));
        public static readonly IValueConverter HwbLenientConverter =
            Construct(() => new FunctionValueConverter(FunctionNames.Hwb, Any));

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
            var number = NumberConverter.Required();
            var option = NumberConverter.Option(float.NaN);
            return new FunctionValueConverter(FunctionNames.Scale, WithArgs(number, option)).Or(
                new FunctionValueConverter(FunctionNames.Scale3d, WithArgs(number, option, option))).Or(
                new FunctionValueConverter(FunctionNames.ScaleX, WithArgs(NumberConverter))).Or(
                new FunctionValueConverter(FunctionNames.ScaleY, WithArgs(NumberConverter))).Or(
                new FunctionValueConverter(FunctionNames.ScaleZ, WithArgs(NumberConverter)));
        });

        public static readonly IValueConverter RotateTransformConverter = Construct(() =>
        {
            var number = NumberConverter.Required();
            return new FunctionValueConverter(FunctionNames.Rotate, WithArgs(AngleConverter)).Or(
                new FunctionValueConverter(FunctionNames.Rotate3d,
                    WithArgs(number, number, number, AngleConverter.Required()))).Or(
                new FunctionValueConverter(FunctionNames.RotateX, WithArgs(AngleConverter))).Or(
                new FunctionValueConverter(FunctionNames.RotateY, WithArgs(AngleConverter))).Or(
                new FunctionValueConverter(FunctionNames.RotateZ, WithArgs(AngleConverter)));
        });

        public static readonly IValueConverter SkewTransformConverter = Construct(() =>
        {
            var angle = AngleConverter.Required();
            return new FunctionValueConverter(FunctionNames.Skew, WithArgs(angle, angle)).Or(
                new FunctionValueConverter(FunctionNames.SkewX, WithArgs(AngleConverter))).Or(
                new FunctionValueConverter(FunctionNames.SkewY, WithArgs(AngleConverter)));
        });

        public static readonly IValueConverter DefaultFontFamiliesConverter = Map.DefaultFontFamilies.ToConverter();
        public static readonly IValueConverter LineStyleConverter = Map.LineStyles.ToConverter();
        public static readonly IValueConverter BackgroundAttachmentConverter = Map.BackgroundAttachments.ToConverter();
        public static readonly IValueConverter BackgroundRepeatConverter = Map.BackgroundRepeats.ToConverter();
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
        public static readonly IValueConverter TextTransformConverter = Map.TextTransforms.ToConverter();
        public static readonly IValueConverter TextAlignLastConverter = Map.TextAlignmentsLast.ToConverter();
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
        // "oblique" alone matches via the plain keyword map (tried first); "oblique <angle>" (CSS Fonts
        // Level 4 - e.g. "oblique 10deg") only reaches the StartsWithValueConverter branch once the
        // plain single-identifier match has already failed, i.e. there are more tokens to account for.
        // AngleConverter is intentionally NOT Option()-wrapped here (unlike e.g. LineHeightConverter's
        // own StartsWithDelimiter().Option() composition elsewhere) - StartsWithValueConverter.Construct
        // trusts "the wrapped converter returned non-null" as its own "matched" signal, and an Option()
        // converter's Construct never returns null, which would make every OTHER possible font-style
        // value (plain keywords, initial, absent) falsely reconstruct as "oblique" whenever the "font"
        // shorthand needs to be re-serialized from its decomposed longhand properties.
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

            // flex-flow is "<'flex-direction'> || <'flex-wrap'>" (CSS Flexbox 1 §5.1): the double bar means
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
            Map.TimingFunctions).Or(StepsConverter).Or(CubicBezierConverter);

        public static readonly IValueConverter GradientConverter = LinearGradientConverter.Or(RadialGradientConverter).Or(ConicGradientConverter);

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
            .Or(ColorMixConverter)
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

        public static readonly IValueConverter ShadowConverter = WithAny(
            Assign(Keywords.Inset, true).Option(false),
            LengthConverter.Many(2, 4).Required(),
            ColorConverter.WithCurrentColor().Option(Color.Black));

        public static readonly IValueConverter MultipleShadowConverter = ShadowConverter.FromList().OrNone();

        // The image functions the engine validates but the renderer does not paint (CSS Images 4). Composed into
        // ImageSourceConverter so every <image> property (background-image, list-style-image, cursor,
        // content, and @property syntax:"<image>") accepts them syntactically; the render path
        // (CssValueParser.ParseImage → CssImagePainter) still handles only url()/gradients, so they paint
        // nothing (an unchanged engine-wide gap). See ExtendedImageConverters.cs.
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

        public static readonly IValueConverter FontFamiliesConverter =
            DefaultFontFamiliesConverter.Or(StringConverter).Or(LiteralsConverter).FromList();

        // Shared with the render layer's BackgroundLayerResolver via BackgroundSizeGrammar - see
        // BackgroundPositionGrammar's doc comment for why.
        public static readonly IValueConverter BackgroundSizeConverter = new BackgroundSizeValueConverter();

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
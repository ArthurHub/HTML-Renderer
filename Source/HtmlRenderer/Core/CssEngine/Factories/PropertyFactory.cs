using System;
using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class PropertyFactory
    {
        private static readonly Lazy<PropertyFactory> Lazy = new(() => new PropertyFactory());

        private readonly List<string> _animatables = new();

        private readonly Dictionary<string, LonghandCreator> _fonts = new(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, LonghandCreator> _longhands = new(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, string[]> _mappings = new();

        private readonly Dictionary<string, ShorthandCreator> _shorthands = new(StringComparer.OrdinalIgnoreCase);

        // Shorthands that parse and expand like any other, but are NOT used to reconstruct a shorthand when
        // serializing a declaration block: the grid mega-shorthands (grid, grid-template), whose multi-slash /
        // areas grammar isn't worth reconstructing. Excluded from GetShorthands (the serialization query) only;
        // CreateShorthand/GetLonghands/IsShorthand still work.
        private readonly HashSet<string> _logicalShorthands = new(StringComparer.OrdinalIgnoreCase);

        // The `all` shorthand's longhands (CSS Cascade 4 3.2): every registered longhand except direction and
        // unicode-bidi. Aliases (e.g. word-wrap, whose creator yields overflow-wrap) are skipped so no property
        // is expanded twice. Lazy so it runs after the constructor has registered everything.
        private readonly Lazy<string[]> _allLonghands;

        private PropertyFactory()
        {
            AddLonghand(PropertyNames.AccentColor, () => new AccentColorProperty(), true);
            _allLonghands = new Lazy<string[]>(GetAllLonghands);

            // `all` is registered without a static longhand mapping: its list is every registered longhand and is
            // built lazily (see GetAllLonghands), so properties added anywhere else are covered automatically.
            _shorthands.Add(PropertyNames.All, () => new AllProperty());

            AddLonghand(PropertyNames.AlignContent, () => new AlignContentProperty());
            AddLonghand(PropertyNames.AlignItems, () => new AlignItemsProperty());
            AddLonghand(PropertyNames.AlignSelf, () => new AlignSelfProperty());
            AddShorthand(PropertyNames.Animation, () => new AnimationProperty(),
                PropertyNames.AnimationName,
                PropertyNames.AnimationDuration,
                PropertyNames.AnimationTimingFunction,
                PropertyNames.AnimationDelay,
                PropertyNames.AnimationDirection,
                PropertyNames.AnimationFillMode,
                PropertyNames.AnimationIterationCount,
                PropertyNames.AnimationPlayState,
                PropertyNames.AnimationTimeline,
                PropertyNames.AnimationComposition,
                PropertyNames.AnimationRangeStart,
                PropertyNames.AnimationRangeEnd);
            AddLonghand(PropertyNames.AnimationComposition, () => new AnimationCompositionProperty());
            AddLonghand(PropertyNames.AnimationDelay, () => new AnimationDelayProperty());
            AddLonghand(PropertyNames.AnimationDirection, () => new AnimationDirectionProperty());
            AddLonghand(PropertyNames.AnimationDuration, () => new AnimationDurationProperty());
            AddLonghand(PropertyNames.AnimationFillMode, () => new AnimationFillModeProperty());
            AddLonghand(PropertyNames.AnimationIterationCount, () => new AnimationIterationCountProperty());
            AddLonghand(PropertyNames.AnimationName, () => new AnimationNameProperty());
            AddLonghand(PropertyNames.AnimationPlayState, () => new AnimationPlayStateProperty());
            AddShorthand(PropertyNames.AnimationRange, () => new AnimationRangeProperty(),
                PropertyNames.AnimationRangeStart,
                PropertyNames.AnimationRangeEnd);
            AddLonghand(PropertyNames.AnimationRangeEnd, () => new AnimationRangeEndProperty());
            AddLonghand(PropertyNames.AnimationRangeStart, () => new AnimationRangeStartProperty());
            AddLonghand(PropertyNames.AnimationTimeline, () => new AnimationTimelineProperty());
            AddLonghand(PropertyNames.AnimationTimingFunction, () => new AnimationTimingFunctionProperty());

            AddShorthand(PropertyNames.Background, () => new BackgroundProperty(),
                PropertyNames.BackgroundAttachment,
                PropertyNames.BackgroundClip,
                PropertyNames.BackgroundColor,
                PropertyNames.BackgroundImage,
                PropertyNames.BackgroundOrigin,
                PropertyNames.BackgroundPosition,
                PropertyNames.BackgroundRepeat,
                PropertyNames.BackgroundSize);
            AddLonghand(PropertyNames.BackgroundAttachment, () => new BackgroundAttachmentProperty());
            AddLonghand(PropertyNames.BackgroundBlendMode, () => new BackgroundBlendModeProperty());
            AddLonghand(PropertyNames.BackgroundColor, () => new BackgroundColorProperty(), true);
            AddLonghand(PropertyNames.BackgroundClip, () => new BackgroundClipProperty());
            AddLonghand(PropertyNames.BackgroundOrigin, () => new BackgroundOriginProperty());
            AddLonghand(PropertyNames.BackgroundSize, () => new BackgroundSizeProperty(), true);
            AddLonghand(PropertyNames.BackgroundImage, () => new BackgroundImageProperty());
            AddLonghand(PropertyNames.BackgroundPosition, () => new BackgroundPositionProperty(), true);
            AddLonghand(PropertyNames.BackgroundRepeat, () => new BackgroundRepeatProperty());
            AddLonghand(PropertyNames.MaskImage, () => new MaskImageProperty());
            AddLonghand(PropertyNames.MaskMode, () => new MaskModeProperty());
            AddLonghand(PropertyNames.MaskRepeat, () => new MaskRepeatProperty());
            AddLonghand(PropertyNames.MaskPosition, () => new MaskPositionProperty(), true);
            AddLonghand(PropertyNames.MaskSize, () => new MaskSizeProperty(), true);
            AddLonghand(PropertyNames.MaskOrigin, () => new MaskOriginProperty());
            AddLonghand(PropertyNames.MaskClip, () => new MaskClipProperty());
            AddLonghand(PropertyNames.MaskComposite, () => new MaskCompositeProperty());

            AddLonghand(PropertyNames.BorderSpacing, () => new BorderSpacingProperty());
            AddLonghand(PropertyNames.BorderCollapse, () => new BorderCollapseProperty());
            AddLonghand(PropertyNames.BoxSizing, () => new BoxSizingProperty());
            AddLonghand(PropertyNames.AspectRatio, () => new AspectRatioProperty());
            AddLonghand(PropertyNames.ImageOrientation, () => new ImageOrientationProperty());
            AddLonghand(PropertyNames.ImageResolution, () => new ImageResolutionProperty());
            AddLonghand(PropertyNames.BoxShadow, () => new BoxShadowProperty(), true);
            AddLonghand(PropertyNames.BoxDecorationBreak, () => new BoxDecorationBreak());
            AddLonghand(PropertyNames.BreakAfter, () => new BreakAfterProperty());
            AddLonghand(PropertyNames.BreakBefore, () => new BreakBeforeProperty());
            AddLonghand(PropertyNames.BreakInside, () => new BreakInsideProperty());
            AddLonghand(PropertyNames.BackfaceVisibility, () => new BackfaceVisibilityProperty());

            AddShorthand(PropertyNames.BlockStep, () => new BlockStepProperty(),
                PropertyNames.BlockStepSize,
                PropertyNames.BlockStepInsert,
                PropertyNames.BlockStepAlign,
                PropertyNames.BlockStepRound);
            AddLonghand(PropertyNames.BlockStepSize, () => new BlockStepSizeProperty());
            AddLonghand(PropertyNames.BlockStepInsert, () => new BlockStepInsertProperty());
            AddLonghand(PropertyNames.BlockStepAlign, () => new BlockStepAlignProperty());
            AddLonghand(PropertyNames.BlockStepRound, () => new BlockStepRoundProperty());
            AddLonghand(PropertyNames.BlockEllipsis, () => new BlockEllipsisProperty());

            AddShorthand(PropertyNames.BorderRadius, () => new BorderRadiusProperty(),
                PropertyNames.BorderTopLeftRadius,
                PropertyNames.BorderTopRightRadius,
                PropertyNames.BorderBottomRightRadius,
                PropertyNames.BorderBottomLeftRadius);
            AddLonghand(PropertyNames.BorderTopLeftRadius, () => new BorderTopLeftRadiusProperty(), true);
            AddLonghand(PropertyNames.BorderTopRightRadius, () => new BorderTopRightRadiusProperty(), true);
            AddLonghand(PropertyNames.BorderBottomLeftRadius, () => new BorderBottomLeftRadiusProperty(), true);
            AddLonghand(PropertyNames.BorderBottomRightRadius, () => new BorderBottomRightRadiusProperty(), true);

            // CSS Borders 4: the corner-*-shape longhands, and the corner shorthands (corner-shape, the
            // two-corner border-*-radius / corner-*-shape shorthands, and the corner-* shorthands that set a
            // corner's shape and radius together). Like the logical shorthands they are registered with
            // AddLogicalShorthand, so existing border-*-radius longhands never serialize into them.
            AddLonghand(PropertyNames.CornerTopLeftShape, () => new CornerTopLeftShapeProperty());
            AddLonghand(PropertyNames.CornerTopRightShape, () => new CornerTopRightShapeProperty());
            AddLonghand(PropertyNames.CornerBottomRightShape, () => new CornerBottomRightShapeProperty());
            AddLonghand(PropertyNames.CornerBottomLeftShape, () => new CornerBottomLeftShapeProperty());
            AddLonghand(PropertyNames.CornerStartStartShape, () => new CornerStartStartShapeProperty());
            AddLonghand(PropertyNames.CornerStartEndShape, () => new CornerStartEndShapeProperty());
            AddLonghand(PropertyNames.CornerEndStartShape, () => new CornerEndStartShapeProperty());
            AddLonghand(PropertyNames.CornerEndEndShape, () => new CornerEndEndShapeProperty());
            CornerShorthands.Register((name, converter, longhands) =>
                AddLogicalShorthand(name, () => new CornerShorthandProperty(name, converter), longhands));

            AddShorthand(PropertyNames.BorderImage, () => new BorderImageProperty(),
                PropertyNames.BorderImageOutset,
                PropertyNames.BorderImageRepeat,
                PropertyNames.BorderImageSlice,
                PropertyNames.BorderImageSource,
                PropertyNames.BorderImageWidth);
            AddLonghand(PropertyNames.BorderImageOutset, () => new BorderImageOutsetProperty());
            AddLonghand(PropertyNames.BorderImageRepeat, () => new BorderImageRepeatProperty());
            AddLonghand(PropertyNames.BorderImageSource, () => new BorderImageSourceProperty());
            AddLonghand(PropertyNames.BorderImageSlice, () => new BorderImageSliceProperty());
            AddLonghand(PropertyNames.BorderImageWidth, () => new BorderImageWidthProperty());

            AddShorthand(PropertyNames.BorderColor, () => new BorderColorProperty(),
                PropertyNames.BorderTopColor,
                PropertyNames.BorderRightColor,
                PropertyNames.BorderBottomColor,
                PropertyNames.BorderLeftColor);
            AddShorthand(PropertyNames.BorderStyle, () => new BorderStyleProperty(),
                PropertyNames.BorderTopStyle,
                PropertyNames.BorderRightStyle,
                PropertyNames.BorderBottomStyle,
                PropertyNames.BorderLeftStyle);
            AddShorthand(PropertyNames.BorderWidth, () => new BorderWidthProperty(),
                PropertyNames.BorderTopWidth,
                PropertyNames.BorderRightWidth,
                PropertyNames.BorderBottomWidth,
                PropertyNames.BorderLeftWidth);
            AddShorthand(PropertyNames.BorderTop, () => new BorderTopProperty(),
                PropertyNames.BorderTopWidth,
                PropertyNames.BorderTopStyle,
                PropertyNames.BorderTopColor);
            AddShorthand(PropertyNames.BorderRight, () => new BorderRightProperty(),
                PropertyNames.BorderRightWidth,
                PropertyNames.BorderRightStyle,
                PropertyNames.BorderRightColor);
            AddShorthand(PropertyNames.BorderBottom, () => new BorderBottomProperty(),
                PropertyNames.BorderBottomWidth,
                PropertyNames.BorderBottomStyle,
                PropertyNames.BorderBottomColor);
            AddShorthand(PropertyNames.BorderLeft, () => new BorderLeftProperty(),
                PropertyNames.BorderLeftWidth,
                PropertyNames.BorderLeftStyle,
                PropertyNames.BorderLeftColor);

            AddShorthand(PropertyNames.Border, () => new BorderProperty(),
                PropertyNames.BorderTopWidth,
                PropertyNames.BorderTopStyle,
                PropertyNames.BorderTopColor,
                PropertyNames.BorderRightWidth,
                PropertyNames.BorderRightStyle,
                PropertyNames.BorderRightColor,
                PropertyNames.BorderBottomWidth,
                PropertyNames.BorderBottomStyle,
                PropertyNames.BorderBottomColor,
                PropertyNames.BorderLeftWidth,
                PropertyNames.BorderLeftStyle,
                PropertyNames.BorderLeftColor);
            AddLonghand(PropertyNames.BorderTopColor, () => new BorderTopColorProperty(), true);
            AddLonghand(PropertyNames.BorderLeftColor, () => new BorderLeftColorProperty(), true);
            AddLonghand(PropertyNames.BorderRightColor, () => new BorderRightColorProperty(), true);
            AddLonghand(PropertyNames.BorderBottomColor, () => new BorderBottomColorProperty(), true);
            AddLonghand(PropertyNames.BorderTopStyle, () => new BorderTopStyleProperty());
            AddLonghand(PropertyNames.BorderLeftStyle, () => new BorderLeftStyleProperty());
            AddLonghand(PropertyNames.BorderRightStyle, () => new BorderRightStyleProperty());
            AddLonghand(PropertyNames.BorderBottomStyle, () => new BorderBottomStyleProperty());
            AddLonghand(PropertyNames.BorderTopWidth, () => new BorderTopWidthProperty(), true);
            AddLonghand(PropertyNames.BorderLeftWidth, () => new BorderLeftWidthProperty(), true);
            AddLonghand(PropertyNames.BorderRightWidth, () => new BorderRightWidthProperty(), true);
            AddLonghand(PropertyNames.BorderBottomWidth, () => new BorderBottomWidthProperty(), true);

            AddLonghand(PropertyNames.Bottom, () => new BottomProperty(), true);

            // CSS Logical Properties and Values Level 1. Flow-relative properties are kept as their own
            // longhands (ExCSS does no layout, so they are never resolved to a physical edge). The logical
            // shorthands expand to them, but must not make the physical longhands serialize back into a
            // logical shorthand, hence AddLogicalShorthand. `inset` is a purely physical shorthand
            // (top/right/bottom/left) that is likewise kept out of serialization.
            AddLogicalShorthand(PropertyNames.MarginBlock, () => new MarginBlockProperty(),
                PropertyNames.MarginBlockStart,
                PropertyNames.MarginBlockEnd);
            AddLonghand(PropertyNames.MarginBlockStart, () => new MarginBlockStartProperty(), true);
            AddLonghand(PropertyNames.MarginBlockEnd, () => new MarginBlockEndProperty(), true);
            AddLogicalShorthand(PropertyNames.PaddingBlock, () => new PaddingBlockProperty(),
                PropertyNames.PaddingBlockStart,
                PropertyNames.PaddingBlockEnd);
            AddLonghand(PropertyNames.PaddingBlockStart, () => new PaddingBlockStartProperty(), true);
            AddLonghand(PropertyNames.PaddingBlockEnd, () => new PaddingBlockEndProperty(), true);
            AddLogicalShorthand(PropertyNames.InsetBlock, () => new InsetBlockProperty(),
                PropertyNames.InsetBlockStart,
                PropertyNames.InsetBlockEnd);
            AddLonghand(PropertyNames.InsetBlockStart, () => new InsetBlockStartProperty(), true);
            AddLonghand(PropertyNames.InsetBlockEnd, () => new InsetBlockEndProperty(), true);
            AddLogicalShorthand(PropertyNames.MarginInline, () => new MarginInlineProperty(),
                PropertyNames.MarginInlineStart,
                PropertyNames.MarginInlineEnd);
            AddLonghand(PropertyNames.MarginInlineStart, () => new MarginInlineStartProperty(), true);
            AddLonghand(PropertyNames.MarginInlineEnd, () => new MarginInlineEndProperty(), true);
            AddLogicalShorthand(PropertyNames.PaddingInline, () => new PaddingInlineProperty(),
                PropertyNames.PaddingInlineStart,
                PropertyNames.PaddingInlineEnd);
            AddLonghand(PropertyNames.PaddingInlineStart, () => new PaddingInlineStartProperty(), true);
            AddLonghand(PropertyNames.PaddingInlineEnd, () => new PaddingInlineEndProperty(), true);
            AddLogicalShorthand(PropertyNames.InsetInline, () => new InsetInlineProperty(),
                PropertyNames.InsetInlineStart,
                PropertyNames.InsetInlineEnd);
            AddLonghand(PropertyNames.InsetInlineStart, () => new InsetInlineStartProperty(), true);
            AddLonghand(PropertyNames.InsetInlineEnd, () => new InsetInlineEndProperty(), true);
            AddLogicalShorthand(PropertyNames.Inset, () => new InsetProperty(),
                PropertyNames.Top,
                PropertyNames.Right,
                PropertyNames.Bottom,
                PropertyNames.Left);
            AddLogicalShorthand(PropertyNames.BorderBlockStart, () => new BorderBlockStartProperty(),
                PropertyNames.BorderBlockStartWidth,
                PropertyNames.BorderBlockStartStyle,
                PropertyNames.BorderBlockStartColor);
            AddLogicalShorthand(PropertyNames.BorderBlockEnd, () => new BorderBlockEndProperty(),
                PropertyNames.BorderBlockEndWidth,
                PropertyNames.BorderBlockEndStyle,
                PropertyNames.BorderBlockEndColor);
            AddLogicalShorthand(PropertyNames.BorderBlock, () => new BorderBlockProperty(),
                PropertyNames.BorderBlockStartWidth,
                PropertyNames.BorderBlockStartStyle,
                PropertyNames.BorderBlockStartColor,
                PropertyNames.BorderBlockEndWidth,
                PropertyNames.BorderBlockEndStyle,
                PropertyNames.BorderBlockEndColor);
            AddLogicalShorthand(PropertyNames.BorderBlockWidth, () => new BorderBlockWidthProperty(),
                PropertyNames.BorderBlockStartWidth,
                PropertyNames.BorderBlockEndWidth);
            AddLonghand(PropertyNames.BorderBlockStartWidth, () => new BorderBlockStartWidthProperty(), true);
            AddLonghand(PropertyNames.BorderBlockEndWidth, () => new BorderBlockEndWidthProperty(), true);
            AddLogicalShorthand(PropertyNames.BorderBlockStyle, () => new BorderBlockStyleProperty(),
                PropertyNames.BorderBlockStartStyle,
                PropertyNames.BorderBlockEndStyle);
            AddLonghand(PropertyNames.BorderBlockStartStyle, () => new BorderBlockStartStyleProperty());
            AddLonghand(PropertyNames.BorderBlockEndStyle, () => new BorderBlockEndStyleProperty());
            AddLogicalShorthand(PropertyNames.BorderBlockColor, () => new BorderBlockColorProperty(),
                PropertyNames.BorderBlockStartColor,
                PropertyNames.BorderBlockEndColor);
            AddLonghand(PropertyNames.BorderBlockStartColor, () => new BorderBlockStartColorProperty(), true);
            AddLonghand(PropertyNames.BorderBlockEndColor, () => new BorderBlockEndColorProperty(), true);
            AddLogicalShorthand(PropertyNames.BorderInlineStart, () => new BorderInlineStartProperty(),
                PropertyNames.BorderInlineStartWidth,
                PropertyNames.BorderInlineStartStyle,
                PropertyNames.BorderInlineStartColor);
            AddLogicalShorthand(PropertyNames.BorderInlineEnd, () => new BorderInlineEndProperty(),
                PropertyNames.BorderInlineEndWidth,
                PropertyNames.BorderInlineEndStyle,
                PropertyNames.BorderInlineEndColor);
            AddLogicalShorthand(PropertyNames.BorderInline, () => new BorderInlineProperty(),
                PropertyNames.BorderInlineStartWidth,
                PropertyNames.BorderInlineStartStyle,
                PropertyNames.BorderInlineStartColor,
                PropertyNames.BorderInlineEndWidth,
                PropertyNames.BorderInlineEndStyle,
                PropertyNames.BorderInlineEndColor);
            AddLogicalShorthand(PropertyNames.BorderInlineWidth, () => new BorderInlineWidthProperty(),
                PropertyNames.BorderInlineStartWidth,
                PropertyNames.BorderInlineEndWidth);
            AddLonghand(PropertyNames.BorderInlineStartWidth, () => new BorderInlineStartWidthProperty(), true);
            AddLonghand(PropertyNames.BorderInlineEndWidth, () => new BorderInlineEndWidthProperty(), true);
            AddLogicalShorthand(PropertyNames.BorderInlineStyle, () => new BorderInlineStyleProperty(),
                PropertyNames.BorderInlineStartStyle,
                PropertyNames.BorderInlineEndStyle);
            AddLonghand(PropertyNames.BorderInlineStartStyle, () => new BorderInlineStartStyleProperty());
            AddLonghand(PropertyNames.BorderInlineEndStyle, () => new BorderInlineEndStyleProperty());
            AddLogicalShorthand(PropertyNames.BorderInlineColor, () => new BorderInlineColorProperty(),
                PropertyNames.BorderInlineStartColor,
                PropertyNames.BorderInlineEndColor);
            AddLonghand(PropertyNames.BorderInlineStartColor, () => new BorderInlineStartColorProperty(), true);
            AddLonghand(PropertyNames.BorderInlineEndColor, () => new BorderInlineEndColorProperty(), true);
            AddLonghand(PropertyNames.BorderStartStartRadius, () => new BorderStartStartRadiusProperty(), true);
            AddLonghand(PropertyNames.BorderStartEndRadius, () => new BorderStartEndRadiusProperty(), true);
            AddLonghand(PropertyNames.BorderEndStartRadius, () => new BorderEndStartRadiusProperty(), true);
            AddLonghand(PropertyNames.BorderEndEndRadius, () => new BorderEndEndRadiusProperty(), true);

            AddShorthand(PropertyNames.Columns, () => new ColumnsProperty(),
                PropertyNames.ColumnWidth,
                PropertyNames.ColumnCount);
            AddLonghand(PropertyNames.BufferedRendering, () => new BufferedRenderingProperty());
            AddLonghand(PropertyNames.CaretColor, () => new CaretColorProperty(), true);
            AddLonghand(PropertyNames.ColumnCount, () => new ColumnCountProperty(), true);
            AddLonghand(PropertyNames.ColumnWidth, () => new ColumnWidthProperty(), true);

            AddLonghand(PropertyNames.ColumnFill, () => new ColumnFillProperty());
            AddLonghand(PropertyNames.ColumnGap, () => new ColumnGapProperty(), true);
            AddLonghand(PropertyNames.ColumnSpan, () => new ColumnSpanProperty());

            AddLonghand(PropertyNames.FloatReference, () => new FloatReferenceProperty());
            AddLonghand(PropertyNames.FootnoteDisplay, () => new FootnoteDisplayProperty());
            AddLonghand(PropertyNames.FootnotePolicy, () => new FootnotePolicyProperty());
            AddLonghand(PropertyNames.ForcedColorAdjust, () => new ForcedColorAdjustProperty());
            AddLonghand(PropertyNames.GridTemplateColumns, () => new GridTemplateColumnsProperty());
            AddLonghand(PropertyNames.GridTemplateRows, () => new GridTemplateRowsProperty());
            AddLonghand(PropertyNames.GridTemplateAreas, () => new GridTemplateAreasProperty());
            AddLonghand(PropertyNames.GridAutoColumns, () => new GridAutoColumnsProperty());
            AddLonghand(PropertyNames.GridAutoRows, () => new GridAutoRowsProperty());
            AddLonghand(PropertyNames.GridAutoFlow, () => new GridAutoFlowProperty());

            // The grid mega-shorthands parse/expand like any other, but are excluded from serialization
            // reconstruction (via _logicalShorthands): reconstructing a `grid`/`grid-template` from its
            // longhands is not worth the complexity and could change existing output.
            AddLogicalShorthand(PropertyNames.GridTemplate, () => new GridTemplateProperty(),
                PropertyNames.GridTemplateRows, PropertyNames.GridTemplateColumns, PropertyNames.GridTemplateAreas);
            AddLogicalShorthand(PropertyNames.Grid, () => new GridProperty(),
                PropertyNames.GridTemplateRows, PropertyNames.GridTemplateColumns, PropertyNames.GridTemplateAreas,
                PropertyNames.GridAutoFlow, PropertyNames.GridAutoRows, PropertyNames.GridAutoColumns);

            AddLonghand(PropertyNames.GridColumnStart, () => new GridColumnStartProperty());
            AddLonghand(PropertyNames.GridColumnEnd, () => new GridColumnEndProperty());
            AddLonghand(PropertyNames.GridRowStart, () => new GridRowStartProperty());
            AddLonghand(PropertyNames.GridRowEnd, () => new GridRowEndProperty());

            AddShorthand(PropertyNames.GridColumn, () => new GridColumnProperty(),
                PropertyNames.GridColumnStart, PropertyNames.GridColumnEnd);
            AddShorthand(PropertyNames.GridRow, () => new GridRowProperty(),
                PropertyNames.GridRowStart, PropertyNames.GridRowEnd);
            AddShorthand(PropertyNames.GridArea, () => new GridAreaProperty(),
                PropertyNames.GridRowStart, PropertyNames.GridColumnStart,
                PropertyNames.GridRowEnd, PropertyNames.GridColumnEnd);

            AddLonghand(PropertyNames.ImageRendering, () => new ImageRenderingProperty());
            AddLonghand(PropertyNames.Interactivity, () => new InteractivityProperty());
            AddLonghand(PropertyNames.Isolation, () => new IsolationProperty());
            AddLonghand(PropertyNames.JustifyItems, () => new JustifyItemsProperty());
            AddLonghand(PropertyNames.JustifySelf, () => new JustifySelfProperty());
            AddShorthand(PropertyNames.PlaceItems, () => new PlaceItemsProperty(),
                PropertyNames.AlignItems, PropertyNames.JustifyItems);
            AddShorthand(PropertyNames.PlaceContent, () => new PlaceContentProperty(),
                PropertyNames.AlignContent, PropertyNames.JustifyContent);
            AddShorthand(PropertyNames.PlaceSelf, () => new PlaceSelfProperty(),
                PropertyNames.AlignSelf, PropertyNames.JustifySelf);

            AddShorthand(PropertyNames.ColumnRule, () => new ColumnRuleProperty(),
                PropertyNames.ColumnRuleWidth,
                PropertyNames.ColumnRuleStyle,
                PropertyNames.ColumnRuleColor);
            AddLonghand(PropertyNames.ColumnRuleColor, () => new ColumnRuleColorProperty(), true);
            AddLonghand(PropertyNames.ColumnRuleStyle, () => new ColumnRuleStyleProperty());
            AddLonghand(PropertyNames.ColumnRuleWidth, () => new ColumnRuleWidthProperty(), true);

            AddLonghand(PropertyNames.CaptionSide, () => new CaptionSideProperty());
            AddLonghand(PropertyNames.Clear, () => new ClearProperty());
            AddLonghand(PropertyNames.Clip, () => new ClipProperty(), true);
            AddLonghand(PropertyNames.Color, () => new ColorProperty(), true);
            AddLonghand(PropertyNames.ColorScheme, () => new ColorSchemeProperty());
            AddLonghand(PropertyNames.ContainerName, () => new ContainerNameProperty());
            AddLonghand(PropertyNames.ContainerType, () => new ContainerTypeProperty());
            AddLonghand(PropertyNames.Content, () => new ContentProperty());
            AddLonghand(PropertyNames.MathDepth, () => new MathDepthProperty());
            AddLonghand(PropertyNames.MathShift, () => new MathShiftProperty());
            AddLonghand(PropertyNames.MathStyle, () => new MathStyleProperty());
            AddLonghand(PropertyNames.MixBlendMode, () => new MixBlendModeProperty(), true);
            AddLonghand(PropertyNames.PointerEvents, () => new PointerEventsProperty());
            AddLonghand(PropertyNames.PrintColorAdjust, () => new PrintColorAdjustProperty());
            AddLonghand(PropertyNames.ShapeRendering, () => new ShapeRenderingProperty());
            AddLonghand(PropertyNames.Contain, () => new ContainProperty());
            AddShorthand(PropertyNames.ContainIntrinsicSize, () => new ContainIntrinsicSizeProperty(),
                PropertyNames.ContainIntrinsicWidth, PropertyNames.ContainIntrinsicHeight);
            AddLonghand(PropertyNames.ContainIntrinsicWidth, () => new ContainIntrinsicWidthProperty(), true);
            AddLonghand(PropertyNames.ContainIntrinsicHeight, () => new ContainIntrinsicHeightProperty(), true);
            AddLonghand(PropertyNames.ContainIntrinsicInlineSize, () => new ContainIntrinsicInlineSizeProperty(), true);
            AddLonghand(PropertyNames.ContainIntrinsicBlockSize, () => new ContainIntrinsicBlockSizeProperty(), true);
            AddShorthand(PropertyNames.Container, () => new ContainerProperty(),
                PropertyNames.ContainerName, PropertyNames.ContainerType);
            AddLonghand(PropertyNames.ContentVisibility, () => new ContentVisibilityProperty());
            AddLonghand(PropertyNames.StringSet, () => new StringSetProperty());
            AddLonghand(PropertyNames.CounterIncrement, () => new CounterIncrementProperty());
            AddLonghand(PropertyNames.CounterReset, () => new CounterResetProperty());
            AddLonghand(PropertyNames.CounterSet, () => new CounterSetProperty());
            AddLonghand(PropertyNames.BookmarkLabel, () => new BookmarkLabelProperty());
            AddLonghand(PropertyNames.BookmarkLevel, () => new BookmarkLevelProperty());
            AddLonghand(PropertyNames.BookmarkState, () => new BookmarkStateProperty());
            AddLonghand(PropertyNames.BookmarkTarget, () => new BookmarkTargetProperty());
            AddLonghand(PropertyNames.Cursor, () => new CursorProperty());
            AddLonghand(PropertyNames.Direction, () => new DirectionProperty());
            AddLonghand(PropertyNames.Display, () => new DisplayProperty());
            AddLonghand(PropertyNames.EmptyCells, () => new EmptyCellsProperty());
            AddLonghand(PropertyNames.Fill, () => new FillProperty(), true);
            AddLonghand(PropertyNames.FillOpacity, () => new FillOpacityProperty(), true);
            AddLonghand(PropertyNames.FillRule, () => new FillRuleProperty(), true);
            AddShorthand(PropertyNames.Flex, () => new FlexProperty(),
                PropertyNames.FlexGrow,
                PropertyNames.FlexShrink,
                PropertyNames.FlexBasis);
            AddLonghand(PropertyNames.FlexBasis, () => new FlexBasisProperty(), true);
            AddLonghand(PropertyNames.FlexDirection, () => new FlexDirectionProperty());
            AddShorthand(PropertyNames.FlexFlow, () => new FlexFlowProperty(),
                PropertyNames.FlexDirection, 
                PropertyNames.FlexWrap);
            AddLonghand(PropertyNames.FlexGrow, () => new FlexGrowProperty());
            AddLonghand(PropertyNames.FlexShrink, () => new FlexShrinkProperty());
            AddLonghand(PropertyNames.FlexWrap, () => new FlexWrapProperty());
            AddLonghand(PropertyNames.Float, () => new FloatProperty());

            AddShorthand(PropertyNames.Font, () => new FontProperty(),
                PropertyNames.FontFamily,
                PropertyNames.FontSize,
                PropertyNames.FontStretch,
                PropertyNames.FontStyle,
                PropertyNames.FontVariantCaps,
                PropertyNames.FontWeight,
                PropertyNames.LineHeight,
                // The longhands below cannot be written in "font" but are reset by it (CSS Fonts 4 7.7).
                PropertyNames.FontVariantLigatures,
                PropertyNames.FontVariantAlternates,
                PropertyNames.FontVariantNumeric,
                PropertyNames.FontVariantEastAsian,
                PropertyNames.FontVariantPosition,
                PropertyNames.FontVariantEmoji,
                PropertyNames.FontKerning,
                PropertyNames.FontOpticalSizing,
                PropertyNames.FontSizeAdjust,
                PropertyNames.FontVariationSettings,
                PropertyNames.FontFeatureSettings,
                PropertyNames.FontLanguageOverride);
            AddLonghand(PropertyNames.FontFamily, () => new FontFamilyProperty(), false, true);
            AddLonghand(PropertyNames.FontSize, () => new FontSizeProperty(), true);
            AddLonghand(PropertyNames.FontSizeAdjust, () => new FontSizeAdjustProperty(), true);
            AddLonghand(PropertyNames.FontStyle, () => new FontStyleProperty(), false, true);
            AddLonghand(PropertyNames.FontWeight, () => new FontWeightProperty(), true, true);
            AddLonghand(PropertyNames.FontStretch, () => new FontStretchProperty(), true, true);
            AddLonghand(PropertyNames.FontPalette, () => new FontPaletteProperty());
            AddLonghand(PropertyNames.FontKerning, () => new FontKerningProperty());
            AddLonghand(PropertyNames.FontOpticalSizing, () => new FontOpticalSizingProperty());
            AddLonghand(PropertyNames.FontFeatureSettings, () => new FontFeatureSettingsProperty());
            AddLonghand(PropertyNames.FontVariationSettings, () => new FontVariationSettingsProperty());
            AddLonghand(PropertyNames.FontLanguageOverride, () => new FontLanguageOverrideProperty());

            AddShorthand(PropertyNames.FontSynthesis, () => new FontSynthesisProperty(),
                PropertyNames.FontSynthesisWeight,
                PropertyNames.FontSynthesisStyle,
                PropertyNames.FontSynthesisSmallCaps,
                PropertyNames.FontSynthesisPosition);
            AddLonghand(PropertyNames.FontSynthesisWeight, () => new FontSynthesisWeightProperty());
            AddLonghand(PropertyNames.FontSynthesisStyle, () => new FontSynthesisStyleProperty());
            AddLonghand(PropertyNames.FontSynthesisSmallCaps, () => new FontSynthesisSmallCapsProperty());
            AddLonghand(PropertyNames.FontSynthesisPosition, () => new FontSynthesisPositionProperty());

            // font-variant is a shorthand of the font-variant-* longhands (CSS Fonts 4 6). The @font-face
            // font-variant descriptor is a different, flat property (FontFaceVariantProperty).
            AddShorthand(PropertyNames.FontVariant, () => new FontVariantProperty(),
                PropertyNames.FontVariantLigatures,
                PropertyNames.FontVariantAlternates,
                PropertyNames.FontVariantCaps,
                PropertyNames.FontVariantNumeric,
                PropertyNames.FontVariantEastAsian,
                PropertyNames.FontVariantPosition,
                PropertyNames.FontVariantEmoji);
            AddLonghand(PropertyNames.FontVariantLigatures, () => new FontVariantLigaturesProperty());
            AddLonghand(PropertyNames.FontVariantAlternates, () => new FontVariantAlternatesProperty());
            AddLonghand(PropertyNames.FontVariantCaps, () => new FontVariantCapsProperty());
            AddLonghand(PropertyNames.FontVariantNumeric, () => new FontVariantNumericProperty());
            AddLonghand(PropertyNames.FontVariantEastAsian, () => new FontVariantEastAsianProperty());
            AddLonghand(PropertyNames.FontVariantPosition, () => new FontVariantPositionProperty());
            AddLonghand(PropertyNames.FontVariantEmoji, () => new FontVariantEmojiProperty());
            _fonts.Add(PropertyNames.FontVariant, () => new FontFaceVariantProperty());

            AddShorthand(PropertyNames.Gap, () => new GapProperty(),
                PropertyNames.RowGap, 
                PropertyNames.ColumnGap);

            AddLonghand(PropertyNames.Height, () => new HeightProperty(), true);
            AddLonghand(PropertyNames.Hyphens, () => new HyphensProperty());
            AddLonghand(PropertyNames.HyphenateCharacter, () => new HyphenateCharacterProperty());
            AddLonghand(PropertyNames.HyphenateLimitChars, () => new HyphenateLimitCharsProperty());
            AddLonghand(PropertyNames.HyphenateLimitLast, () => new HyphenateLimitLastProperty());
            AddLonghand(PropertyNames.HyphenateLimitLines, () => new HyphenateLimitLinesProperty());
            AddLonghand(PropertyNames.HyphenateLimitZone, () => new HyphenateLimitZoneProperty());
            AddLonghand(PropertyNames.HangingPunctuation, () => new HangingPunctuationProperty());
            AddLonghand(PropertyNames.InitialLetter, () => new InitialLetterProperty());

            AddLonghand(PropertyNames.JustifyContent, () => new JustifyContentProperty());
            
            AddLonghand(PropertyNames.Left, () => new LeftProperty(), true);
            AddLonghand(PropertyNames.LetterSpacing, () => new LetterSpacingProperty());
            AddLonghand(PropertyNames.LineBreak, () => new LineBreakProperty());
            AddLonghand(PropertyNames.LineHeight, () => new LineHeightProperty(), true);
            AddLonghand(PropertyNames.LineClamp, () => new LineClampProperty());
            AddLonghand(PropertyNames.LineHeightStep, () => new LineHeightStepProperty());

            AddShorthand(PropertyNames.ListStyle, () => new ListStyleProperty(),
                PropertyNames.ListStyleType,
                PropertyNames.ListStyleImage,
                PropertyNames.ListStylePosition);
            AddLonghand(PropertyNames.ListStyleImage, () => new ListStyleImageProperty());
            AddLonghand(PropertyNames.ListStylePosition, () => new ListStylePositionProperty());
            AddLonghand(PropertyNames.ListStyleType, () => new ListStyleTypeProperty());

            AddShorthand(PropertyNames.Margin, () => new MarginProperty(),
                PropertyNames.MarginTop,
                PropertyNames.MarginRight,
                PropertyNames.MarginBottom,
                PropertyNames.MarginLeft);
            AddLonghand(PropertyNames.MarginRight, () => new MarginRightProperty(), true);
            AddLonghand(PropertyNames.MarginLeft, () => new MarginLeftProperty(), true);
            AddLonghand(PropertyNames.MarginTop, () => new MarginTopProperty(), true);
            AddLonghand(PropertyNames.MarginBottom, () => new MarginBottomProperty(), true);

            AddLonghand(PropertyNames.MaxHeight, () => new MaxHeightProperty(), true);
            AddLonghand(PropertyNames.MaxWidth, () => new MaxWidthProperty(), true);
            AddLonghand(PropertyNames.MinHeight, () => new MinHeightProperty(), true);
            AddLonghand(PropertyNames.MinWidth, () => new MinWidthProperty(), true);
            AddLonghand(PropertyNames.Opacity, () => new OpacityProperty(), true);
            AddLonghand(PropertyNames.Order, () => new OrderProperty(), true);
            AddLonghand(PropertyNames.Orphans, () => new OrphansProperty());

            AddShorthand(PropertyNames.Outline, () => new OutlineProperty(),
                PropertyNames.OutlineWidth,
                PropertyNames.OutlineStyle,
                PropertyNames.OutlineColor);
            AddLonghand(PropertyNames.OutlineColor, () => new OutlineColorProperty(), true);
            AddLonghand(PropertyNames.OutlineOffset, () => new OutlineOffsetProperty(), true);
            AddLonghand(PropertyNames.OutlineStyle, () => new OutlineStyleProperty());
            AddLonghand(PropertyNames.OutlineWidth, () => new OutlineWidthProperty(), true);

            AddLonghand(PropertyNames.Overflow, () => new OverflowProperty());
            AddLonghand(PropertyNames.OverflowX, () => new OverflowXProperty());
            AddLonghand(PropertyNames.OverflowY, () => new OverflowYProperty());
            AddLonghand(PropertyNames.OverflowClipMarginTop, () => new OverflowClipMarginTopProperty(), true);
            AddLonghand(PropertyNames.OverflowClipMarginRight, () => new OverflowClipMarginRightProperty(), true);
            AddLonghand(PropertyNames.OverflowClipMarginBottom, () => new OverflowClipMarginBottomProperty(), true);
            AddLonghand(PropertyNames.OverflowClipMarginLeft, () => new OverflowClipMarginLeftProperty(), true);
            AddLonghand(PropertyNames.OverflowClipMarginBlockStart, () => new OverflowClipMarginBlockStartProperty(), true);
            AddLonghand(PropertyNames.OverflowClipMarginBlockEnd, () => new OverflowClipMarginBlockEndProperty(), true);
            AddLonghand(PropertyNames.OverflowClipMarginInlineStart, () => new OverflowClipMarginInlineStartProperty(), true);
            AddLonghand(PropertyNames.OverflowClipMarginInlineEnd, () => new OverflowClipMarginInlineEndProperty(), true);
            AddShorthand(PropertyNames.OverflowClipMargin, () => new OverflowClipMarginProperty(),
                PropertyNames.OverflowClipMarginTop, PropertyNames.OverflowClipMarginRight,
                PropertyNames.OverflowClipMarginBottom, PropertyNames.OverflowClipMarginLeft);
            AddShorthand(PropertyNames.OverflowClipMarginBlock, () => new OverflowClipMarginBlockProperty(),
                PropertyNames.OverflowClipMarginBlockStart, PropertyNames.OverflowClipMarginBlockEnd);
            AddShorthand(PropertyNames.OverflowClipMarginInline, () => new OverflowClipMarginInlineProperty(),
                PropertyNames.OverflowClipMarginInlineStart, PropertyNames.OverflowClipMarginInlineEnd);
            AddLonghand(PropertyNames.OverflowWrap, () => new OverflowWrapProperty());

            AddShorthand(PropertyNames.Padding, () => new PaddingProperty(),
                PropertyNames.PaddingTop,
                PropertyNames.PaddingRight,
                PropertyNames.PaddingBottom,
                PropertyNames.PaddingLeft);
            AddLonghand(PropertyNames.PaddingTop, () => new PaddingTopProperty(), true);
            AddLonghand(PropertyNames.PaddingRight, () => new PaddingRightProperty(), true);
            AddLonghand(PropertyNames.PaddingLeft, () => new PaddingLeftProperty(), true);
            AddLonghand(PropertyNames.PaddingBottom, () => new PaddingBottomProperty(), true);

            AddLonghand(PropertyNames.PageBreakAfter, () => new PageBreakAfterProperty());
            AddLonghand(PropertyNames.PageBreakBefore, () => new PageBreakBeforeProperty());
            AddLonghand(PropertyNames.PageBreakInside, () => new PageBreakInsideProperty());
            AddLonghand(PropertyNames.Perspective, () => new PerspectiveProperty(), true);
            AddLonghand(PropertyNames.PerspectiveOrigin, () => new PerspectiveOriginProperty(), true);
            AddLonghand(PropertyNames.Position, () => new PositionProperty());
            AddShorthand(PropertyNames.OverscrollBehavior, () => new OverscrollBehaviorProperty(),
                PropertyNames.OverscrollBehaviorX,
                PropertyNames.OverscrollBehaviorY);
            AddLonghand(PropertyNames.OverscrollBehaviorX, () => new OverscrollBehaviorXProperty());
            AddLonghand(PropertyNames.OverscrollBehaviorY, () => new OverscrollBehaviorYProperty());

            AddLonghand(PropertyNames.Quotes, () => new QuotesProperty());
            AddLonghand(PropertyNames.Right, () => new RightProperty(), true);
            AddLonghand(PropertyNames.RowGap, () => new RowGapProperty(), true);
            AddLonghand(PropertyNames.ScrollBehavior, () => new ScrollBehaviorProperty());
            AddShorthand(PropertyNames.ScrollMargin, () => new ScrollMarginProperty(),
                PropertyNames.ScrollMarginTop,
                PropertyNames.ScrollMarginRight,
                PropertyNames.ScrollMarginBottom,
                PropertyNames.ScrollMarginLeft);
            AddLonghand(PropertyNames.ScrollMarginTop, () => new ScrollMarginTopProperty(), true);
            AddLonghand(PropertyNames.ScrollMarginRight, () => new ScrollMarginRightProperty(), true);
            AddLonghand(PropertyNames.ScrollMarginBottom, () => new ScrollMarginBottomProperty(), true);
            AddLonghand(PropertyNames.ScrollMarginLeft, () => new ScrollMarginLeftProperty(), true);
            AddShorthand(PropertyNames.ScrollPadding, () => new ScrollPaddingProperty(),
                PropertyNames.ScrollPaddingTop,
                PropertyNames.ScrollPaddingRight,
                PropertyNames.ScrollPaddingBottom,
                PropertyNames.ScrollPaddingLeft);
            AddLonghand(PropertyNames.ScrollPaddingTop, () => new ScrollPaddingTopProperty(), true);
            AddLonghand(PropertyNames.ScrollPaddingRight, () => new ScrollPaddingRightProperty(), true);
            AddLonghand(PropertyNames.ScrollPaddingBottom, () => new ScrollPaddingBottomProperty(), true);
            AddLonghand(PropertyNames.ScrollPaddingLeft, () => new ScrollPaddingLeftProperty(), true);
            AddLonghand(PropertyNames.ScrollSnapAlign, () => new ScrollSnapAlignProperty());
            AddLonghand(PropertyNames.ScrollSnapStop, () => new ScrollSnapStopProperty());
            AddLonghand(PropertyNames.ScrollSnapType, () => new ScrollSnapTypeProperty());
            AddLonghand(PropertyNames.ScrollbarColor, () => new ScrollbarColorProperty(), true);
            AddLonghand(PropertyNames.ScrollbarGutter, () => new ScrollbarGutterProperty());
            AddLonghand(PropertyNames.ScrollbarWidth, () => new ScrollbarWidthProperty());
            AddShorthand(PropertyNames.ScrollTimeline, () => new ScrollTimelineProperty(),
                PropertyNames.ScrollTimelineName,
                PropertyNames.ScrollTimelineAxis);
            AddLonghand(PropertyNames.ScrollTimelineAxis, () => new ScrollTimelineAxisProperty());
            AddLonghand(PropertyNames.ScrollTimelineName, () => new ScrollTimelineNameProperty());
            AddLonghand(PropertyNames.Stroke, () => new StrokeProperty(), true);
            AddLonghand(PropertyNames.StrokeDasharray, () => new StrokeDasharrayProperty(), true);
            AddLonghand(PropertyNames.StrokeDashoffset, () => new StrokeDashoffsetProperty(), true);
            AddLonghand(PropertyNames.StrokeLinecap, () => new StrokeLinecapProperty(), true);
            AddLonghand(PropertyNames.StrokeLinejoin, () => new StrokeLinejoinProperty(), true);
            AddLonghand(PropertyNames.StrokeMiterlimit, () => new StrokeMiterlimitProperty(), true);
            AddLonghand(PropertyNames.StrokeOpacity, () => new StrokeOpacityProperty(), true);
            AddLonghand(PropertyNames.StrokeWidth, () => new StrokeWidthProperty(), true);
            AddLonghand(PropertyNames.StopColor, () => new StopColorProperty(), true);
            AddLonghand(PropertyNames.StopOpacity, () => new StopOpacityProperty(), true);
            AddLonghand(PropertyNames.FloodColor, () => new FloodColorProperty(), true);
            AddLonghand(PropertyNames.FloodOpacity, () => new FloodOpacityProperty(), true);
            AddLonghand(PropertyNames.LightingColor, () => new LightingColorProperty(), true);
            AddLonghand(PropertyNames.MarkerStart, () => new MarkerStartProperty());
            AddLonghand(PropertyNames.MarkerMid, () => new MarkerMidProperty());
            AddLonghand(PropertyNames.MarkerEnd, () => new MarkerEndProperty());
            AddLonghand(PropertyNames.PaintOrder, () => new PaintOrderProperty());
            AddLonghand(PropertyNames.ClipRule, () => new ClipRuleProperty());
            AddLonghand(PropertyNames.ColorInterpolation, () => new ColorInterpolationProperty());
            AddLonghand(PropertyNames.ColorInterpolationFilters, () => new ColorInterpolationFiltersProperty());
            AddLonghand(PropertyNames.DominantBaseline, () => new DominantBaselineProperty());
            AddLonghand(PropertyNames.AlignBaseline, () => new AlignmentBaselineProperty());
            AddLonghand(PropertyNames.BaselineShift, () => new BaselineShiftProperty(), true);
            AddLonghand(PropertyNames.TransformBox, () => new TransformBoxProperty());
            AddLonghand(PropertyNames.InlineSize, () => new InlineSizeProperty(), true);
            AddLonghand(PropertyNames.X, () => new XProperty(), true);
            AddLonghand(PropertyNames.Y, () => new YProperty(), true);
            AddLonghand(PropertyNames.Cx, () => new CxProperty(), true);
            AddLonghand(PropertyNames.Cy, () => new CyProperty(), true);
            AddLonghand(PropertyNames.R, () => new RProperty(), true);
            AddLonghand(PropertyNames.Rx, () => new RxProperty(), true);
            AddLonghand(PropertyNames.Ry, () => new RyProperty(), true);
            AddLonghand(PropertyNames.D, () => new DProperty(), true);
            AddShorthand(PropertyNames.Marker, () => new MarkerProperty(),
                PropertyNames.MarkerStart,
                PropertyNames.MarkerMid,
                PropertyNames.MarkerEnd);
            AddLonghand(PropertyNames.TableLayout, () => new TableLayoutProperty());
            AddShorthand(PropertyNames.TextAlign, () => new TextAlignProperty(),
                PropertyNames.TextAlignAll,
                PropertyNames.TextAlignLast);
            AddLonghand(PropertyNames.TextAlignAll, () => new TextAlignAllProperty());
            AddLonghand(PropertyNames.TextAlignLast, () => new TextAlignLastProperty());
            AddLonghand(PropertyNames.TextOverflow, () => new TextOverflowProperty());
            AddLonghand(PropertyNames.TextAnchor, () => new TextAnchorProperty());

            AddShorthand(PropertyNames.TextDecoration, () => new TextDecorationProperty(),
                PropertyNames.TextDecorationLine,
                PropertyNames.TextDecorationStyle,
                PropertyNames.TextDecorationColor,
                PropertyNames.TextDecorationThickness);
            AddLonghand(PropertyNames.TextDecorationStyle, () => new TextDecorationStyleProperty());
            AddLonghand(PropertyNames.TextDecorationLine, () => new TextDecorationLineProperty());
            AddLonghand(PropertyNames.TextDecorationColor, () => new TextDecorationColorProperty(), true);
            AddLonghand(PropertyNames.TextDecorationThickness, () => new TextDecorationThicknessProperty());
            AddLonghand(PropertyNames.TextDecorationSkipInk, () => new TextDecorationSkipInkProperty());
            AddLonghand(PropertyNames.TextDecorationSkip, () => new TextDecorationSkipProperty());
            AddLonghand(PropertyNames.TextDecorationInset, () => new TextDecorationInsetProperty());

            AddShorthand(PropertyNames.TextEmphasis, () => new TextEmphasisProperty(),
                PropertyNames.TextEmphasisStyle,
                PropertyNames.TextEmphasisColor);
            AddLonghand(PropertyNames.TextEmphasisStyle, () => new TextEmphasisStyleProperty());
            AddLonghand(PropertyNames.TextEmphasisColor, () => new TextEmphasisColorProperty(), true);
            AddLonghand(PropertyNames.TextEmphasisPosition, () => new TextEmphasisPositionProperty());

            AddLonghand(PropertyNames.TextCombineUpright, () => new TextCombineUprightProperty());
            AddLonghand(PropertyNames.TextIndent, () => new TextIndentProperty(), true);
            AddLonghand(PropertyNames.TextJustify, () => new TextJustifyProperty());
            AddLonghand(PropertyNames.TextOrientation, () => new TextOrientationProperty());
            AddLonghand(PropertyNames.TextRendering, () => new TextRenderingProperty());
            AddLonghand(PropertyNames.TabSize, () => new TabSizeProperty());

            AddShorthand(PropertyNames.TextBox, () => new TextBoxProperty(),
                PropertyNames.TextBoxTrim,
                PropertyNames.TextBoxEdge);
            AddLonghand(PropertyNames.TextBoxTrim, () => new TextBoxTrimProperty());
            AddLonghand(PropertyNames.TextBoxEdge, () => new TextBoxEdgeProperty());
            AddLonghand(PropertyNames.TextTransform, () => new TextTransformProperty());
            AddLonghand(PropertyNames.TextUnderlineOffset, () => new TextUnderlineOffsetProperty());
            AddLonghand(PropertyNames.TextUnderlinePosition, () => new TextUnderlinePositionProperty());
            AddLonghand(PropertyNames.TextShadow, () => new TextShadowProperty(), true);
            AddShorthand(PropertyNames.TextWrap, () => new TextWrapProperty(),
                PropertyNames.TextWrapMode,
                PropertyNames.TextWrapStyle);
            AddLonghand(PropertyNames.TextWrapMode, () => new TextWrapModeProperty());
            AddLonghand(PropertyNames.TextWrapStyle, () => new TextWrapStyleProperty());
            AddLonghand(PropertyNames.TouchAction, () => new TouchActionProperty());
            AddLonghand(PropertyNames.TimelineScope, () => new TimelineScopeProperty());
            AddLonghand(PropertyNames.Transform, () => new TransformProperty(), true);
            AddLonghand(PropertyNames.ClipPath, () => new ClipPathProperty(), true);
            AddLonghand(PropertyNames.Filter, () => new FilterProperty(), true);
            AddLonghand(PropertyNames.BackdropFilter, () => new BackdropFilterProperty(), true);
            AddLonghand(PropertyNames.ShapeOutside, () => new ShapeOutsideProperty());
            AddLonghand(PropertyNames.ShapeMargin, () => new ShapeMarginProperty(), true);
            AddLonghand(PropertyNames.ShapeImageThreshold, () => new ShapeImageThresholdProperty(), true);
            AddLonghand(PropertyNames.TransformOrigin, () => new TransformOriginProperty(), true);
            AddLonghand(PropertyNames.TransformStyle, () => new TransformStyleProperty());
            AddLonghand(PropertyNames.Translate, () => new TranslateProperty(), true);
            AddLonghand(PropertyNames.Rotate, () => new RotateProperty(), true);
            AddLonghand(PropertyNames.Scale, () => new ScaleProperty(), true);

            AddShorthand(PropertyNames.Transition, () => new TransitionProperty(),
                PropertyNames.TransitionProperty,
                PropertyNames.TransitionDuration,
                PropertyNames.TransitionTimingFunction,
                PropertyNames.TransitionDelay,
                PropertyNames.TransitionBehavior);
            AddLonghand(PropertyNames.TransitionBehavior, () => new TransitionBehaviorProperty());
            AddLonghand(PropertyNames.TransitionDelay, () => new TransitionDelayProperty());
            AddLonghand(PropertyNames.TransitionDuration, () => new TransitionDurationProperty());
            AddLonghand(PropertyNames.TransitionTimingFunction, () => new TransitionTimingFunctionProperty());
            AddLonghand(PropertyNames.TransitionProperty, () => new TransitionPropertyProperty());

            AddLonghand(PropertyNames.Top, () => new TopProperty(), true);
            AddLonghand(PropertyNames.UnicodeBidirectional, () => new UnicodeBidirectionalProperty());
            AddLonghand(PropertyNames.UserSelect, () => new UserSelectProperty());
            AddLonghand(PropertyNames.VectorEffect, () => new VectorEffectProperty());
            AddLonghand(PropertyNames.VerticalAlign, () => new VerticalAlignProperty(), true);
            AddShorthand(PropertyNames.ViewTimeline, () => new ViewTimelineProperty(),
                PropertyNames.ViewTimelineName,
                PropertyNames.ViewTimelineAxis,
                PropertyNames.ViewTimelineInset);
            AddLonghand(PropertyNames.ViewTimelineAxis, () => new ViewTimelineAxisProperty());
            AddLonghand(PropertyNames.ViewTimelineInset, () => new ViewTimelineInsetProperty());
            AddLonghand(PropertyNames.ViewTimelineName, () => new ViewTimelineNameProperty());
            AddLonghand(PropertyNames.Visibility, () => new VisibilityProperty(), true);
            AddShorthand(PropertyNames.WhiteSpace, () => new WhiteSpaceProperty(),
                PropertyNames.WhiteSpaceCollapse,
                PropertyNames.TextWrapMode);
            AddLonghand(PropertyNames.WhiteSpaceCollapse, () => new WhiteSpaceCollapseProperty());
            AddLonghand(PropertyNames.Widows, () => new WidowsProperty());
            AddLonghand(PropertyNames.Width, () => new WidthProperty(), true);
            AddLonghand(PropertyNames.WordBreak, () => new WordBreakProperty(), true);
            AddLonghand(PropertyNames.WordSpacing, () => new WordSpacingProperty(), true);
            AddLonghand(PropertyNames.WordWrap, () => new OverflowWrapProperty());
            AddLonghand(PropertyNames.WritingMode, () => new WritingModeProperty());
            AddLonghand(PropertyNames.ZIndex, () => new ZIndexProperty(), true);
            AddLonghand(PropertyNames.ObjectFit, () => new ObjectFitProperty());
            AddLonghand(PropertyNames.PageName, () => new PageNameProperty());
            AddLonghand(PropertyNames.Size, () => new PageSizeProperty());
            AddLonghand(PropertyNames.Bleed, () => new BleedProperty());
            AddLonghand(PropertyNames.Marks, () => new MarksProperty());
            AddLonghand(PropertyNames.ObjectPosition, () => new ObjectPositionProperty(), true);

            _fonts.Add(PropertyNames.Src, () => new SrcProperty());
            _fonts.Add(PropertyNames.UnicodeRange, () => new UnicodeRangeProperty());
        }

        internal static PropertyFactory Instance => Lazy.Value;

        private void AddShorthand(string name, ShorthandCreator creator, params string[] longhands)
        {
            _shorthands.Add(name, creator);
            _mappings.Add(name, longhands);
        }

        private void AddLogicalShorthand(string name, ShorthandCreator creator, params string[] longhands)
        {
            AddShorthand(name, creator, longhands);
            _logicalShorthands.Add(name);
        }

        private void AddLonghand(string name, LonghandCreator creator, bool animatable = false, bool font = false)
        {
            _longhands.Add(name, creator);

            if (animatable) _animatables.Add(name);

            if (font) _fonts.Add(name, creator);
        }

        public Property Create(string name)
        {
            return CreateLonghand(name) ?? CreateShorthand(name) ?? CreateCustomProperty(name);
        }

        private static Property CreateCustomProperty(string name)
        {
            return IsCustomPropertyName(name) ? new CustomProperty(name) : null;
        }

        // A custom property name is at least two dashes followed by at least one code point (CSS Custom
        // Properties 1 2). "--" alone is reserved and not a valid custom property.
        internal static bool IsCustomPropertyName(string name)
        {
            return name != null && name.Length > 2 && name[0] == '-' && name[1] == '-';
        }

        public Property CreateFont(string name)
        {
            return _fonts.TryGetValue(name, out var propertyCreator) ? propertyCreator() : null;
        }

        // The @property descriptors (syntax / initial-value / inherits). Their values have no fixed grammar
        // - initial-value depends on the syntax, syntax is an arbitrary string - so each is stored raw via
        // an UnknownProperty (Converters.Any).
        public Property CreatePropertyDescriptor(string name)
        {
            return PropertyFactory.IsPropertyDescriptor(name) ? new UnknownProperty(name) : null;
        }

        private static bool IsPropertyDescriptor(string name)
        {
            return name.Is(PropertyNames.Syntax) || name.Is(PropertyNames.InitialValue) ||
                   name.Is(PropertyNames.Inherits);
        }

        // The @font-palette-values descriptors (font-family / base-palette / override-colors). Their values
        // are stored raw via an UnknownProperty (Converters.Any); the resolver re-tokenizes them later.
        public Property CreateFontPaletteDescriptor(string name)
        {
            return IsFontPaletteDescriptor(name) ? new UnknownProperty(name) : null;
        }

        private static bool IsFontPaletteDescriptor(string name)
        {
            return name.Is(PropertyNames.FontFamily) || name.Is(PropertyNames.BasePalette) ||
                   name.Is(PropertyNames.OverrideColors);
        }

        // @font-feature-values blocks hold author-chosen names (e.g. "nice-style: 1 2;"), so any ident is
        // accepted; the value must be one or more non-negative integers.
        public Property CreateFontFeatureValueDescriptor(string name)
        {
            return string.IsNullOrEmpty(name) ? null : new FontFeatureValueProperty(name);
        }

        // The @counter-style descriptors. Values are stored raw via an UnknownProperty (Converters.Any),
        // except `system`, whose small grammar is validated at parse time. Unknown descriptors are dropped.
        public Property CreateCounterStyleDescriptor(string name)
        {
            if (name.Isi(CounterSystemName)) return new CounterSystemDescriptorProperty();

            return Array.Exists(CounterStyleRule.DescriptorNames, d => d.Isi(name)) ? new UnknownProperty(name) : null;
        }

        private const string CounterSystemName = "system";

        public Property CreateViewport(string name)        {
            var feature = MediaFeatureFactory.Instance.Create(name);

            return feature != null ? new FeatureProperty(feature) : null;
        }

        public Property CreateLonghand(string name)
        {
            return _longhands.TryGetValue(name, out var createProperty) ? createProperty() : null;
        }

        public ShorthandProperty CreateShorthand(string name)
        {
            return _shorthands.TryGetValue(name, out var propertyCreator) ? propertyCreator() : null;
        }

        public Property[] CreateLonghandsFor(string name)
        {
            var propertyNames = GetLonghands(name);

            return propertyNames.Select(CreateLonghand).ToArray();
        }

        public bool IsShorthand(string name)
        {
            return _shorthands.ContainsKey(name);
        }

        public bool IsAnimatable(string name)
        {
            return _longhands.ContainsKey(name)
                ? _animatables.Contains(name)
                : GetLonghands(name).Any(_ => _animatables.Contains(name));
        }

        public string[] GetLonghands(string name)
        {
            if (name.Is(PropertyNames.All)) return _allLonghands.Value;

            return _mappings.TryGetValue(name, out var mapping)
                ? mapping
                : Array.Empty<string>();
        }

        public IEnumerable<string> GetShorthands(string name)
        {
            return from mapping in _mappings
                where !_logicalShorthands.Contains(mapping.Key)
                    && mapping.Value.Contains(name, StringComparison.OrdinalIgnoreCase)
                select mapping.Key;
        }

        private string[] GetAllLonghands()
        {
            return _longhands
                .Where(m => !m.Key.Is(PropertyNames.Direction) && !m.Key.Is(PropertyNames.UnicodeBidirectional)
                            && m.Value().Name.Is(m.Key))
                .Select(m => m.Key)
                .ToArray();
        }

        private delegate Property LonghandCreator();

        private delegate ShorthandProperty ShorthandCreator();
    }
}
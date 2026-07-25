using System;
using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class PropertyFactory
    {
        private static readonly Lazy<PropertyFactory> Lazy = new Lazy<PropertyFactory>(() => new PropertyFactory());

        private readonly List<string> _animatables = new List<string>();

        private readonly Dictionary<string, LonghandCreator> _fontsBuilder = new Dictionary<string, LonghandCreator>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, LonghandCreator> _longhandsBuilder = new Dictionary<string, LonghandCreator>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, string[]> _mappingsBuilder = new Dictionary<string, string[]>();

        private readonly Dictionary<string, ShorthandCreator> _shorthandsBuilder = new Dictionary<string, ShorthandCreator>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, LonghandCreator> _fonts;

        // @property descriptors (syntax / initial-value / inherits). Their values have no fixed CSS grammar
        // (initial-value depends on the syntax; syntax is an arbitrary string), so each is stored raw via an
        // UnknownProperty (Converters.Any) and validated later against the syntax string.
        private readonly Dictionary<string, LonghandCreator> _propertyDescriptors;

        private readonly Dictionary<string, LonghandCreator> _longhands;

        private readonly Dictionary<string, string[]> _mappings;

        private readonly Dictionary<string, ShorthandCreator> _shorthands;

        private PropertyFactory()
        {
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
                PropertyNames.AnimationPlayState);
            AddLonghand(PropertyNames.AnimationDelay, () => new AnimationDelayProperty());
            AddLonghand(PropertyNames.AnimationDirection, () => new AnimationDirectionProperty());
            AddLonghand(PropertyNames.AnimationDuration, () => new AnimationDurationProperty());
            AddLonghand(PropertyNames.AnimationFillMode, () => new AnimationFillModeProperty());
            AddLonghand(PropertyNames.AnimationIterationCount, () => new AnimationIterationCountProperty());
            AddLonghand(PropertyNames.AnimationName, () => new AnimationNameProperty());
            AddLonghand(PropertyNames.AnimationPlayState, () => new AnimationPlayStateProperty());
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
            AddLonghand(PropertyNames.BackgroundColor, () => new BackgroundColorProperty(), true);
            AddLonghand(PropertyNames.BackgroundClip, () => new BackgroundClipProperty());
            AddLonghand(PropertyNames.BackgroundOrigin, () => new BackgroundOriginProperty());
            AddLonghand(PropertyNames.BackgroundSize, () => new BackgroundSizeProperty(), true);
            AddLonghand(PropertyNames.BackgroundImage, () => new BackgroundImageProperty());
            AddLonghand(PropertyNames.BackgroundPosition, () => new BackgroundPositionProperty(), true);
            AddLonghand(PropertyNames.BackgroundRepeat, () => new BackgroundRepeatProperty());

            AddLonghand(PropertyNames.BorderSpacing, () => new BorderSpacingProperty());
            AddLonghand(PropertyNames.BorderCollapse, () => new BorderCollapseProperty());
            AddLonghand(PropertyNames.BoxSizing, () => new BoxSizingProperty());
            AddLonghand(PropertyNames.BoxShadow, () => new BoxShadowProperty(), true);
            AddLonghand(PropertyNames.BoxDecorationBreak, () => new BoxDecorationBreak());
            AddLonghand(PropertyNames.BreakAfter, () => new BreakAfterProperty());
            AddLonghand(PropertyNames.BreakBefore, () => new BreakBeforeProperty());
            AddLonghand(PropertyNames.BreakInside, () => new BreakInsideProperty());
            AddLonghand(PropertyNames.BackfaceVisibility, () => new BackfaceVisibilityProperty());

            AddShorthand(PropertyNames.BorderRadius, () => new BorderRadiusProperty(),
                PropertyNames.BorderTopLeftRadius,
                PropertyNames.BorderTopRightRadius,
                PropertyNames.BorderBottomRightRadius,
                PropertyNames.BorderBottomLeftRadius);
            AddLonghand(PropertyNames.BorderTopLeftRadius, () => new BorderTopLeftRadiusProperty(), true);
            AddLonghand(PropertyNames.BorderTopRightRadius, () => new BorderTopRightRadiusProperty(), true);
            AddLonghand(PropertyNames.BorderBottomLeftRadius, () => new BorderBottomLeftRadiusProperty(), true);
            AddLonghand(PropertyNames.BorderBottomRightRadius, () => new BorderBottomRightRadiusProperty(), true);

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

            AddShorthand(PropertyNames.Columns, () => new ColumnsProperty(),
                PropertyNames.ColumnWidth,
                PropertyNames.ColumnCount);
            AddLonghand(PropertyNames.ColumnCount, () => new ColumnCountProperty(), true);
            AddLonghand(PropertyNames.ColumnWidth, () => new ColumnWidthProperty(), true);

            AddLonghand(PropertyNames.ColumnFill, () => new ColumnFillProperty());
            AddLonghand(PropertyNames.ColumnGap, () => new ColumnGapProperty(), true);
            AddLonghand(PropertyNames.ColumnSpan, () => new ColumnSpanProperty());

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
            AddLonghand(PropertyNames.ContainerName, () => new ContainerNameProperty());
            AddLonghand(PropertyNames.ContainerType, () => new ContainerTypeProperty());
            AddLonghand(PropertyNames.Content, () => new ContentProperty());
            AddLonghand(PropertyNames.CounterIncrement, () => new CounterIncrementProperty());
            AddLonghand(PropertyNames.CounterReset, () => new CounterResetProperty());
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
                PropertyNames.FontVariant,
                PropertyNames.FontWeight,
                PropertyNames.LineHeight);
            AddLonghand(PropertyNames.FontFamily, () => new FontFamilyProperty(), false, true);
            AddLonghand(PropertyNames.FontSize, () => new FontSizeProperty(), true);
            AddLonghand(PropertyNames.FontSizeAdjust, () => new FontSizeAdjustProperty(), true);
            AddLonghand(PropertyNames.FontStyle, () => new FontStyleProperty(), false, true);
            AddLonghand(PropertyNames.FontVariant, () => new FontVariantProperty(), false, true);
            AddLonghand(PropertyNames.FontWeight, () => new FontWeightProperty(), true, true);
            AddLonghand(PropertyNames.FontStretch, () => new FontStretchProperty(), true, true);

            AddShorthand(PropertyNames.Gap, () => new GapProperty(),
                PropertyNames.RowGap,
                PropertyNames.ColumnGap);

            AddLonghand(PropertyNames.Height, () => new HeightProperty(), true);
            AddLonghand(PropertyNames.Hyphens, () => new HyphensProperty());

            AddLonghand(PropertyNames.JustifyContent, () => new JustifyContentProperty());

            AddLonghand(PropertyNames.Left, () => new LeftProperty(), true);
            AddLonghand(PropertyNames.LetterSpacing, () => new LetterSpacingProperty());
            AddLonghand(PropertyNames.LineHeight, () => new LineHeightProperty(), true);

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
            AddLonghand(PropertyNames.OutlineStyle, () => new OutlineStyleProperty());
            AddLonghand(PropertyNames.OutlineWidth, () => new OutlineWidthProperty(), true);

            AddLonghand(PropertyNames.Overflow, () => new OverflowProperty());
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
            AddLonghand(PropertyNames.Quotes, () => new QuotesProperty());
            AddLonghand(PropertyNames.Right, () => new RightProperty(), true);
            AddLonghand(PropertyNames.RowGap, () => new RowGapProperty(), true);
            AddLonghand(PropertyNames.Stroke, () => new StrokeProperty(), true);
            AddLonghand(PropertyNames.StrokeDasharray, () => new StrokeDasharrayProperty(), true);
            AddLonghand(PropertyNames.StrokeDashoffset, () => new StrokeDashoffsetProperty(), true);
            AddLonghand(PropertyNames.StrokeLinecap, () => new StrokeLinecapProperty(), true);
            AddLonghand(PropertyNames.StrokeLinejoin, () => new StrokeLinejoinProperty(), true);
            AddLonghand(PropertyNames.StrokeMiterlimit, () => new StrokeMiterlimitProperty(), true);
            AddLonghand(PropertyNames.StrokeOpacity, () => new StrokeOpacityProperty(), true);
            AddLonghand(PropertyNames.StrokeWidth, () => new StrokeWidthProperty(), true);
            AddLonghand(PropertyNames.StringSet, () => new StringSetProperty());
            AddLonghand(PropertyNames.PageName, () => new PageNameProperty());
            AddLonghand(PropertyNames.PdfTagType, () => new PdfTagTypeProperty());
            AddLonghand(PropertyNames.TableLayout, () => new TableLayoutProperty());
            AddLonghand(PropertyNames.TextAlign, () => new TextAlignProperty());
            AddLonghand(PropertyNames.TextAlignLast, () => new TextAlignLastProperty());
            AddLonghand(PropertyNames.TextAnchor, () => new TextAnchorProperty());

            AddShorthand(PropertyNames.TextDecoration, () => new TextDecorationProperty(),
                PropertyNames.TextDecorationLine,
                PropertyNames.TextDecorationStyle,
                PropertyNames.TextDecorationColor);
            AddLonghand(PropertyNames.TextDecorationStyle, () => new TextDecorationStyleProperty());
            AddLonghand(PropertyNames.TextDecorationLine, () => new TextDecorationLineProperty());
            AddLonghand(PropertyNames.TextDecorationColor, () => new TextDecorationColorProperty(), true);

            AddLonghand(PropertyNames.TextIndent, () => new TextIndentProperty(), true);
            AddLonghand(PropertyNames.TextJustify, () => new TextJustifyProperty());
            AddLonghand(PropertyNames.TextTransform, () => new TextTransformProperty());
            AddLonghand(PropertyNames.TextShadow, () => new TextShadowProperty(), true);
            AddLonghand(PropertyNames.Transform, () => new TransformProperty(), true);
            AddLonghand(PropertyNames.TransformOrigin, () => new TransformOriginProperty(), true);
            AddLonghand(PropertyNames.TransformStyle, () => new TransformStyleProperty());

            AddShorthand(PropertyNames.Transition, () => new TransitionProperty(),
                PropertyNames.TransitionProperty,
                PropertyNames.TransitionDuration,
                PropertyNames.TransitionTimingFunction,
                PropertyNames.TransitionDelay);
            AddLonghand(PropertyNames.TransitionDelay, () => new TransitionDelayProperty());
            AddLonghand(PropertyNames.TransitionDuration, () => new TransitionDurationProperty());
            AddLonghand(PropertyNames.TransitionTimingFunction, () => new TransitionTimingFunctionProperty());
            AddLonghand(PropertyNames.TransitionProperty, () => new TransitionPropertyProperty());

            AddLonghand(PropertyNames.Top, () => new TopProperty(), true);
            AddLonghand(PropertyNames.UnicodeBidirectional, () => new UnicodeBidirectionalProperty());
            AddLonghand(PropertyNames.VerticalAlign, () => new VerticalAlignProperty(), true);
            AddLonghand(PropertyNames.Visibility, () => new VisibilityProperty(), true);
            AddLonghand(PropertyNames.WhiteSpace, () => new WhiteSpaceProperty());
            AddLonghand(PropertyNames.Widows, () => new WidowsProperty());
            AddLonghand(PropertyNames.Width, () => new WidthProperty(), true);
            AddLonghand(PropertyNames.WordBreak, () => new WordBreakProperty(), true);
            AddLonghand(PropertyNames.WordSpacing, () => new WordSpacingProperty(), true);
            AddLonghand(PropertyNames.WordWrap, () => new OverflowWrapProperty());
            AddLonghand(PropertyNames.ZIndex, () => new ZIndexProperty(), true);
            AddLonghand(PropertyNames.ObjectFit, () => new ObjectFitProperty());
            AddLonghand(PropertyNames.ObjectPosition, () => new ObjectPositionProperty(), true);
            AddLonghand(PropertyNames.Size, () => new PageSizeProperty());

            _fontsBuilder.Add(PropertyNames.Src, () => new SrcProperty());
            _fontsBuilder.Add(PropertyNames.UnicodeRange, () => new UnicodeRangeProperty());

            _fonts = _fontsBuilder;

            _propertyDescriptors = new Dictionary<string, LonghandCreator>(StringComparer.OrdinalIgnoreCase)
            {
                { PropertyNames.Syntax, () => new UnknownProperty(PropertyNames.Syntax) },
                { PropertyNames.InitialValue, () => new UnknownProperty(PropertyNames.InitialValue) },
                { PropertyNames.Inherits, () => new UnknownProperty(PropertyNames.Inherits) },
            };

            _longhands = _longhandsBuilder;
            _mappings = _mappingsBuilder;
            _shorthands = _shorthandsBuilder;
        }

        internal static PropertyFactory Instance => Lazy.Value;

        private void AddShorthand(string name, ShorthandCreator creator, params string[] longhands)
        {
            _shorthandsBuilder.Add(name, creator);
            _mappingsBuilder.Add(name, longhands);
        }

        private void AddLonghand(string name, LonghandCreator creator, bool animatable = false, bool font = false)
        {
            _longhandsBuilder.Add(name, creator);

            if (animatable) _animatables.Add(name);

            if (font) _fontsBuilder.Add(name, creator);
        }

        public Property Create(string name)
        {
            return CreateLonghand(name) ?? CreateShorthand(name) ?? CreateCustomProperty(name);
        }

        private static Property CreateCustomProperty(string name)
        {
            return IsCustomPropertyName(name) ? new CustomProperty(name) : null;
        }

        internal static bool IsCustomPropertyName(string name)
        {
            return name != null && name.Length > 2 && name[0] == '-' && name[1] == '-';
        }

        public Property CreateFont(string name)
        {
            return _fonts.TryGetValue(name, out var propertyCreator) ? propertyCreator() : null;
        }

        public Property CreatePropertyDescriptor(string name)
        {
            return _propertyDescriptors.TryGetValue(name, out var propertyCreator) ? propertyCreator() : null;
        }

        public Property CreateViewport(string name)
        {
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
            return _mappings.TryGetValue(name, out var mapping)
                ? mapping
                : Array.Empty<string>();
        }

        public IEnumerable<string> GetShorthands(string name)
        {
            return from mapping in _mappings where mapping.Value.Contains(name, StringComparison.OrdinalIgnoreCase) select mapping.Key;
        }

        private delegate Property LonghandCreator();

        private delegate ShorthandProperty ShorthandCreator();
    }
}
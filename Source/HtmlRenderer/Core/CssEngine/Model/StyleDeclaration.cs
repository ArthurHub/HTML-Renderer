using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// ReSharper disable UnusedMember.Global

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class StyleDeclaration : StylesheetNode, IProperties
    {
        private readonly Rule _parent;
        private readonly StylesheetParser _parser;
        public event Action<string> Changed;

        private StyleDeclaration(Rule parent, StylesheetParser parser)
        {
            _parent = parent;
            _parser = parser;
        }

        internal StyleDeclaration(StylesheetParser parser) : this(null, parser)
        {
        }

        internal StyleDeclaration() : this(null, null)
        {
        }

        internal StyleDeclaration(Rule parent) : this(parent, parent.Parser)
        {
        }

        public void Update(string value)
        {
            Clear();

            if (!string.IsNullOrEmpty(value)) _parser.AppendDeclarations(this, value);
        }

        public override void ToCss(TextWriter writer, IStyleFormatter formatter)
        {
            var list = new List<string>();
            var serialized = new List<string>();
            foreach (var declaration in Declarations)
            {
                var property = declaration.Name;
                if (IsStrictMode)
                {
                    if (serialized.Contains(property)) continue;

                    var shorthands = PropertyFactory.Instance.GetShorthands(property).ToList();
                    if (shorthands.Any())
                    {
                        var longhands = Declarations.Where(m => !serialized.Contains(m.Name)).ToList();
                        foreach (var shorthand in shorthands.OrderByDescending(m =>
                            PropertyFactory.Instance.GetLonghands(m).Length))
                        {
                            var rule = PropertyFactory.Instance.CreateShorthand(shorthand);
                            var properties = PropertyFactory.Instance.GetLonghands(shorthand);
                            var currentLonghands = longhands.Where(m => properties.Contains(m.Name)).ToArray();

                            if (currentLonghands.Length == 0) continue;

                            var important = currentLonghands.Count(m => m.IsImportant);

                            if (important > 0 && important != currentLonghands.Length) continue;

                            if (properties.Length != currentLonghands.Length) continue;

                            var value = rule.Stringify(currentLonghands);

                            if (string.IsNullOrEmpty(value)) continue;

                            list.Add(CompressedStyleFormatter.Instance.Declaration(shorthand, value, important != 0));

                            foreach (var longhand in currentLonghands)
                            {
                                serialized.Add(longhand.Name);
                                longhands.Remove(longhand);
                            }
                        }
                    }

                    if (serialized.Contains(property)) continue;
                    serialized.Add(property);
                }

                list.Add(declaration.ToCss(formatter));
            }

            writer.Write(formatter.Declarations(list));
        }

        public string RemoveProperty(string propertyName)
        {
            var value = GetPropertyValue(propertyName);
            RemovePropertyByName(propertyName);
            RaiseChanged();

            return value;
        }

        private void RemovePropertyByName(string propertyName)
        {
            foreach (var declaration in Declarations)
            {
                if (!declaration.Name.Is(propertyName)) continue;
                RemoveChild(declaration);
                break;
            }

            if (!IsStrictMode || !PropertyFactory.Instance.IsShorthand(propertyName)) return;

            var longhands = PropertyFactory.Instance.GetLonghands(propertyName);
            foreach (var longhand in longhands) RemovePropertyByName(longhand);
        }

        public string GetPropertyPriority(string propertyName)
        {
            var property = GetProperty(propertyName);
            if (property != null && property.IsImportant) return Keywords.Important;
            if (!IsStrictMode || !PropertyFactory.Instance.IsShorthand(propertyName)) return string.Empty;

            var longhands = PropertyFactory.Instance.GetLonghands(propertyName);

            return longhands.Any(longhand => !GetPropertyPriority(longhand)
                .Isi(Keywords.Important))
                ? string.Empty
                : Keywords.Important;
        }

        public string GetPropertyValue(string propertyName)
        {
            var property = GetProperty(propertyName);
            if (property != null) return property.Value;

            if (!IsStrictMode || !PropertyFactory.Instance.IsShorthand(propertyName)) return string.Empty;

            var shortHand = PropertyFactory.Instance.CreateShorthand(propertyName);
            var declarations = PropertyFactory.Instance.GetLonghands(propertyName);
            var properties = new List<Property>();

            foreach (var declaration in declarations)
            {
                property = GetProperty(declaration);
                if (property == null) return string.Empty;
                properties.Add(property);
            }

            return shortHand.Stringify(properties.ToArray());
        }

        public void SetPropertyValue(string propertyName, string propertyValue)
        {
            SetProperty(propertyName, propertyValue);
        }

        public void SetPropertyPriority(string propertyName, string priority)
        {
            if (!string.IsNullOrEmpty(priority) && !priority.Isi(Keywords.Important)) return;

            var important = !string.IsNullOrEmpty(priority);
            var mappings = IsStrictMode && PropertyFactory.Instance.IsShorthand(propertyName)
                ? PropertyFactory.Instance.GetLonghands(propertyName)
                : Enumerable.Repeat(propertyName, 1);

            foreach (var mapping in mappings)
            {
                var property = GetProperty(mapping);
                if (property != null) property.IsImportant = important;
            }
        }

        public void SetProperty(string propertyName, string propertyValue, string priority = null)
        {
            if (!string.IsNullOrEmpty(propertyValue))
            {
                if (priority != null && !priority.Isi(Keywords.Important)) return;

                var value = _parser.ParseValue(propertyValue);
                if (value == null) return;

                var property = CreateProperty(propertyName);
                if (property == null || !property.TrySetValue(value)) return;

                property.IsImportant = priority != null;
                SetProperty(property);
                RaiseChanged();
            }
            else
            {
                RemoveProperty(propertyName);
            }
        }

        internal Property CreateProperty(string propertyName)
        {
            var property = GetProperty(propertyName);
            if (property != null) return property;

            property = PropertyFactory.Instance.Create(propertyName);
            if (property != null || IsStrictMode) return property;

            return new UnknownProperty(propertyName);
        }

        internal Property GetProperty(string name)
        {
            return Declarations.FirstOrDefault(m => m.Name.Isi(name));
        }

        internal void SetProperty(Property property)
        {
            if (property is ShorthandProperty shorthand)
            {
                SetShorthand(shorthand);
            }
            else
            {
                SetLonghand(property);
            }
        }

        internal void SetDeclarations(IEnumerable<Property> declarations)
        {
            ChangeDeclarations(declarations, _ => false, (o, n) => !o.IsImportant || n.IsImportant);
        }

        internal void UpdateDeclarations(IEnumerable<Property> declarations)
        {
            ChangeDeclarations(declarations, m => !m.CanBeInherited, (o, _) => o.IsInherited);
        }

        private void ChangeDeclarations(IEnumerable<Property> declarations, Predicate<Property> defaultSkip,
            Func<Property, Property, bool> removeExisting)
        {
            var propertyList = new List<Property>();
            foreach (var newDeclaration in declarations)
            {
                var skip = defaultSkip(newDeclaration);
                foreach (var oldDeclaration in Declarations)
                {
                    if (!oldDeclaration.Name.Is(newDeclaration.Name)) continue;

                    if (removeExisting(oldDeclaration, newDeclaration))
                        RemoveChild(oldDeclaration);
                    else
                        skip = true;
                    break;
                }

                if (!skip) propertyList.Add(newDeclaration);
            }

            foreach (var declaration in propertyList) AppendChild(declaration);
        }

        private void SetLonghand(Property property)
        {
            if (!_parser.Options.PreserveDuplicateProperties)
            {
                foreach (var declaration in Declarations)
                {
                    if (!declaration.Name.Is(property.Name)) continue;
                    RemoveChild(declaration);
                    break;
                }
            }

            AppendChild(property);
        }

        private void SetShorthand(ShorthandProperty shorthand)
        {
            if (shorthand.DeclaredValue.Original.ContainsFunction(FunctionNames.Var))
            {
                // A var() reference can't be split into per-longhand slices here — the referenced custom
                // property's value is only known per-element, at cascade time. Keep the shorthand
                // declaration whole so cascade-time substitution + shorthand expansion can handle it once resolved.
                SetLonghand(shorthand);
                return;
            }

            var properties = PropertyFactory.Instance.CreateLonghandsFor(shorthand.Name);
            shorthand.Export(properties);

            foreach (var property in properties) SetLonghand(property);
        }

        private void RaiseChanged()
        {
            Changed?.Invoke(CssText);
        }

        public IEnumerator<IProperty> GetEnumerator()
        {
            return Declarations.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IRule Parent => _parent;
        public string this[int index] => Declarations.GetItemByIndex(index).Name;
        public string this[string name] => GetPropertyValue(name);
        public int Length => Declarations.Count();

        public bool IsStrictMode => /* IsReadOnly ||*/ _parser.Options.IncludeUnknownDeclarations == false;

        public IEnumerable<Property> Declarations => Children.OfType<Property>();

        public string CssText
        {
            get => this.ToCss();
            set
            {
                Update(value);
                RaiseChanged();
            }
        }

        public string AlignContent
        {
            get => GetPropertyValue(PropertyNames.AlignContent);
            set => SetPropertyValue(PropertyNames.AlignContent, value);
        }

        public string AlignItems
        {
            get => GetPropertyValue(PropertyNames.AlignItems);
            set => SetPropertyValue(PropertyNames.AlignItems, value);
        }

        public string AlignSelf
        {
            get => GetPropertyValue(PropertyNames.AlignSelf);
            set => SetPropertyValue(PropertyNames.AlignSelf, value);
        }

        public string Accelerator
        {
            get => GetPropertyValue(PropertyNames.Accelerator);
            set => SetPropertyValue(PropertyNames.Accelerator, value);
        }

        public string AlignmentBaseline
        {
            get => GetPropertyValue(PropertyNames.AlignBaseline);
            set => SetPropertyValue(PropertyNames.AlignBaseline, value);
        }

        public string Animation
        {
            get => GetPropertyValue(PropertyNames.Animation);
            set => SetPropertyValue(PropertyNames.Animation, value);
        }

        public string AnimationDelay
        {
            get => GetPropertyValue(PropertyNames.AnimationDelay);
            set => SetPropertyValue(PropertyNames.AnimationDelay, value);
        }

        public string AnimationDirection
        {
            get => GetPropertyValue(PropertyNames.AnimationDirection);
            set => SetPropertyValue(PropertyNames.AnimationDirection, value);
        }

        public string AnimationDuration
        {
            get => GetPropertyValue(PropertyNames.AnimationDuration);
            set => SetPropertyValue(PropertyNames.AnimationDuration, value);
        }

        public string AnimationFillMode
        {
            get => GetPropertyValue(PropertyNames.AnimationFillMode);
            set => SetPropertyValue(PropertyNames.AnimationFillMode, value);
        }

        public string AnimationIterationCount
        {
            get => GetPropertyValue(PropertyNames.AnimationIterationCount);
            set => SetPropertyValue(PropertyNames.AnimationIterationCount, value);
        }

        public string AnimationName
        {
            get => GetPropertyValue(PropertyNames.AnimationName);
            set => SetPropertyValue(PropertyNames.AnimationName, value);
        }

        public string AnimationPlayState
        {
            get => GetPropertyValue(PropertyNames.AnimationPlayState);
            set => SetPropertyValue(PropertyNames.AnimationPlayState, value);
        }

        public string AnimationTimingFunction
        {
            get => GetPropertyValue(PropertyNames.AnimationTimingFunction);
            set => SetPropertyValue(PropertyNames.AnimationTimingFunction, value);
        }

        public string BackfaceVisibility
        {
            get => GetPropertyValue(PropertyNames.BackfaceVisibility);
            set => SetPropertyValue(PropertyNames.BackfaceVisibility, value);
        }

        public string Background
        {
            get => GetPropertyValue(PropertyNames.Background);
            set => SetPropertyValue(PropertyNames.Background, value);
        }

        public string BackgroundAttachment
        {
            get => GetPropertyValue(PropertyNames.BackgroundAttachment);
            set => SetPropertyValue(PropertyNames.BackgroundAttachment, value);
        }

        public string BackgroundClip
        {
            get => GetPropertyValue(PropertyNames.BackgroundClip);
            set => SetPropertyValue(PropertyNames.BackgroundClip, value);
        }

        public string BackgroundColor
        {
            get => GetPropertyValue(PropertyNames.BackgroundColor);
            set => SetPropertyValue(PropertyNames.BackgroundColor, value);
        }

        public string BackgroundImage
        {
            get => GetPropertyValue(PropertyNames.BackgroundImage);
            set => SetPropertyValue(PropertyNames.BackgroundImage, value);
        }

        public string BackgroundOrigin
        {
            get => GetPropertyValue(PropertyNames.BackgroundOrigin);
            set => SetPropertyValue(PropertyNames.BackgroundOrigin, value);
        }

        public string BackgroundPosition
        {
            get => GetPropertyValue(PropertyNames.BackgroundPosition);
            set => SetPropertyValue(PropertyNames.BackgroundPosition, value);
        }

        public string BackgroundPositionX
        {
            get => GetPropertyValue(PropertyNames.BackgroundPositionX);
            set => SetPropertyValue(PropertyNames.BackgroundPositionX, value);
        }

        public string BackgroundPositionY
        {
            get => GetPropertyValue(PropertyNames.BackgroundPositionY);
            set => SetPropertyValue(PropertyNames.BackgroundPositionY, value);
        }

        public string BackgroundRepeat
        {
            get => GetPropertyValue(PropertyNames.BackgroundRepeat);
            set => SetPropertyValue(PropertyNames.BackgroundRepeat, value);
        }

        public string BackgroundSize
        {
            get => GetPropertyValue(PropertyNames.BackgroundSize);
            set => SetPropertyValue(PropertyNames.BackgroundSize, value);
        }

        public string BaselineShift
        {
            get => GetPropertyValue(PropertyNames.BaselineShift);
            set => SetPropertyValue(PropertyNames.BaselineShift, value);
        }

        public string Behavior
        {
            get => GetPropertyValue(PropertyNames.Behavior);
            set => SetPropertyValue(PropertyNames.Behavior, value);
        }

        public string Bottom
        {
            get => GetPropertyValue(PropertyNames.Bottom);
            set => SetPropertyValue(PropertyNames.Bottom, value);
        }

        public string Border
        {
            get => GetPropertyValue(PropertyNames.Border);
            set => SetPropertyValue(PropertyNames.Border, value);
        }

        public string BorderBottom
        {
            get => GetPropertyValue(PropertyNames.BorderBottom);
            set => SetPropertyValue(PropertyNames.BorderBottom, value);
        }

        public string BorderBottomColor
        {
            get => GetPropertyValue(PropertyNames.BorderBottomColor);
            set => SetPropertyValue(PropertyNames.BorderBottomColor, value);
        }

        public string BorderBottomLeftRadius
        {
            get => GetPropertyValue(PropertyNames.BorderBottomLeftRadius);
            set => SetPropertyValue(PropertyNames.BorderBottomLeftRadius, value);
        }

        public string BorderBottomRightRadius
        {
            get => GetPropertyValue(PropertyNames.BorderBottomRightRadius);
            set => SetPropertyValue(PropertyNames.BorderBottomRightRadius, value);
        }

        public string BorderBottomStyle
        {
            get => GetPropertyValue(PropertyNames.BorderBottomStyle);
            set => SetPropertyValue(PropertyNames.BorderBottomStyle, value);
        }

        public string BorderBottomWidth
        {
            get => GetPropertyValue(PropertyNames.BorderBottomWidth);
            set => SetPropertyValue(PropertyNames.BorderBottomWidth, value);
        }

        public string BorderCollapse
        {
            get => GetPropertyValue(PropertyNames.BorderCollapse);
            set => SetPropertyValue(PropertyNames.BorderCollapse, value);
        }

        public string BorderColor
        {
            get => GetPropertyValue(PropertyNames.BorderColor);
            set => SetPropertyValue(PropertyNames.BorderColor, value);
        }

        public string BorderImage
        {
            get => GetPropertyValue(PropertyNames.BorderImage);
            set => SetPropertyValue(PropertyNames.BorderImage, value);
        }

        public string BorderImageOutset
        {
            get => GetPropertyValue(PropertyNames.BorderImageOutset);
            set => SetPropertyValue(PropertyNames.BorderImageOutset, value);
        }

        public string BorderImageRepeat
        {
            get => GetPropertyValue(PropertyNames.BorderImageRepeat);
            set => SetPropertyValue(PropertyNames.BorderImageRepeat, value);
        }

        public string BorderImageSlice
        {
            get => GetPropertyValue(PropertyNames.BorderImageSlice);
            set => SetPropertyValue(PropertyNames.BorderImageSlice, value);
        }

        public string BorderImageSource
        {
            get => GetPropertyValue(PropertyNames.BorderImageSource);
            set => SetPropertyValue(PropertyNames.BorderImageSource, value);
        }

        public string BorderImageWidth
        {
            get => GetPropertyValue(PropertyNames.BorderImageWidth);
            set => SetPropertyValue(PropertyNames.BorderImageWidth, value);
        }

        public string BorderLeft
        {
            get => GetPropertyValue(PropertyNames.BorderLeft);
            set => SetPropertyValue(PropertyNames.BorderLeft, value);
        }

        public string BorderLeftColor
        {
            get => GetPropertyValue(PropertyNames.BorderLeftColor);
            set => SetPropertyValue(PropertyNames.BorderLeftColor, value);
        }

        public string BorderLeftStyle
        {
            get => GetPropertyValue(PropertyNames.BorderLeftStyle);
            set => SetPropertyValue(PropertyNames.BorderLeftStyle, value);
        }

        public string BorderLeftWidth
        {
            get => GetPropertyValue(PropertyNames.BorderLeftWidth);
            set => SetPropertyValue(PropertyNames.BorderLeftWidth, value);
        }

        public string BorderRadius
        {
            get => GetPropertyValue(PropertyNames.BorderRadius);
            set => SetPropertyValue(PropertyNames.BorderRadius, value);
        }

        public string BorderRight
        {
            get => GetPropertyValue(PropertyNames.BorderRight);
            set => SetPropertyValue(PropertyNames.BorderRight, value);
        }

        public string BorderRightColor
        {
            get => GetPropertyValue(PropertyNames.BorderRightColor);
            set => SetPropertyValue(PropertyNames.BorderRightColor, value);
        }

        public string BorderRightStyle
        {
            get => GetPropertyValue(PropertyNames.BorderRightStyle);
            set => SetPropertyValue(PropertyNames.BorderRightStyle, value);
        }

        public string BorderRightWidth
        {
            get => GetPropertyValue(PropertyNames.BorderRightWidth);
            set => SetPropertyValue(PropertyNames.BorderRightWidth, value);
        }

        public string BorderSpacing
        {
            get => GetPropertyValue(PropertyNames.BorderSpacing);
            set => SetPropertyValue(PropertyNames.BorderSpacing, value);
        }

        public string BorderStyle
        {
            get => GetPropertyValue(PropertyNames.BorderStyle);
            set => SetPropertyValue(PropertyNames.BorderStyle, value);
        }

        public string BorderTop
        {
            get => GetPropertyValue(PropertyNames.BorderTop);
            set => SetPropertyValue(PropertyNames.BorderTop, value);
        }

        public string BorderTopColor
        {
            get => GetPropertyValue(PropertyNames.BorderTopColor);
            set => SetPropertyValue(PropertyNames.BorderTopColor, value);
        }

        public string BorderTopLeftRadius
        {
            get => GetPropertyValue(PropertyNames.BorderTopLeftRadius);
            set => SetPropertyValue(PropertyNames.BorderTopLeftRadius, value);
        }

        public string BorderTopRightRadius
        {
            get => GetPropertyValue(PropertyNames.BorderTopRightRadius);
            set => SetPropertyValue(PropertyNames.BorderTopRightRadius, value);
        }

        public string BorderTopStyle
        {
            get => GetPropertyValue(PropertyNames.BorderTopStyle);
            set => SetPropertyValue(PropertyNames.BorderTopStyle, value);
        }

        public string BorderTopWidth
        {
            get => GetPropertyValue(PropertyNames.BorderTopWidth);
            set => SetPropertyValue(PropertyNames.BorderTopWidth, value);
        }

        public string BorderWidth
        {
            get => GetPropertyValue(PropertyNames.BorderWidth);
            set => SetPropertyValue(PropertyNames.BorderWidth, value);
        }

        public string BoxShadow
        {
            get => GetPropertyValue(PropertyNames.BoxShadow);
            set => SetPropertyValue(PropertyNames.BoxShadow, value);
        }

        public string BoxSizing
        {
            get => GetPropertyValue(PropertyNames.BoxSizing);
            set => SetPropertyValue(PropertyNames.BoxSizing, value);
        }

        public string BreakAfter
        {
            get => GetPropertyValue(PropertyNames.BreakAfter);
            set => SetPropertyValue(PropertyNames.BreakAfter, value);
        }

        public string BreakBefore
        {
            get => GetPropertyValue(PropertyNames.BreakBefore);
            set => SetPropertyValue(PropertyNames.BreakBefore, value);
        }

        public string BreakInside
        {
            get => GetPropertyValue(PropertyNames.BreakInside);
            set => SetPropertyValue(PropertyNames.BreakInside, value);
        }

        public string CaptionSide
        {
            get => GetPropertyValue(PropertyNames.CaptionSide);
            set => SetPropertyValue(PropertyNames.CaptionSide, value);
        }

        public new string Clear
        {
            get => GetPropertyValue(PropertyNames.Clear);
            set => SetPropertyValue(PropertyNames.Clear, value);
        }

        public string Clip
        {
            get => GetPropertyValue(PropertyNames.Clip);
            set => SetPropertyValue(PropertyNames.Clip, value);
        }

        public string ClipBottom
        {
            get => GetPropertyValue(PropertyNames.ClipBottom);
            set => SetPropertyValue(PropertyNames.ClipBottom, value);
        }

        public string ClipLeft
        {
            get => GetPropertyValue(PropertyNames.ClipLeft);
            set => SetPropertyValue(PropertyNames.ClipLeft, value);
        }

        public string ClipPath
        {
            get => GetPropertyValue(PropertyNames.ClipPath);
            set => SetPropertyValue(PropertyNames.ClipPath, value);
        }

        public string ClipRight
        {
            get => GetPropertyValue(PropertyNames.ClipRight);
            set => SetPropertyValue(PropertyNames.ClipRight, value);
        }

        public string ClipRule
        {
            get => GetPropertyValue(PropertyNames.ClipRule);
            set => SetPropertyValue(PropertyNames.ClipRule, value);
        }

        public string ClipTop
        {
            get => GetPropertyValue(PropertyNames.ClipTop);
            set => SetPropertyValue(PropertyNames.ClipTop, value);
        }

        public string Color
        {
            get => GetPropertyValue(PropertyNames.Color);
            set => SetPropertyValue(PropertyNames.Color, value);
        }

        public string ColorInterpolationFilters
        {
            get => GetPropertyValue(PropertyNames.ColorInterpolationFilters);
            set => SetPropertyValue(PropertyNames.ColorInterpolationFilters, value);
        }

        public string ColumnCount
        {
            get => GetPropertyValue(PropertyNames.ColumnCount);
            set => SetPropertyValue(PropertyNames.ColumnCount, value);
        }

        public string ColumnFill
        {
            get => GetPropertyValue(PropertyNames.ColumnFill);
            set => SetPropertyValue(PropertyNames.ColumnFill, value);
        }

        public string ColumnGap
        {
            get => GetPropertyValue(PropertyNames.ColumnGap);
            set => SetPropertyValue(PropertyNames.ColumnGap, value);
        }

        public string ColumnRule
        {
            get => GetPropertyValue(PropertyNames.ColumnRule);
            set => SetPropertyValue(PropertyNames.ColumnRule, value);
        }

        public string ColumnRuleColor
        {
            get => GetPropertyValue(PropertyNames.ColumnRuleColor);
            set => SetPropertyValue(PropertyNames.ColumnRuleColor, value);
        }

        public string ColumnRuleStyle
        {
            get => GetPropertyValue(PropertyNames.ColumnRuleStyle);
            set => SetPropertyValue(PropertyNames.ColumnRuleStyle, value);
        }

        public string ColumnRuleWidth
        {
            get => GetPropertyValue(PropertyNames.ColumnRuleWidth);
            set => SetPropertyValue(PropertyNames.ColumnRuleWidth, value);
        }

        public string Columns
        {
            get => GetPropertyValue(PropertyNames.Columns);
            set => SetPropertyValue(PropertyNames.Columns, value);
        }

        public string ColumnSpan
        {
            get => GetPropertyValue(PropertyNames.ColumnSpan);
            set => SetPropertyValue(PropertyNames.ColumnSpan, value);
        }

        public string ColumnWidth
        {
            get => GetPropertyValue(PropertyNames.ColumnWidth);
            set => SetPropertyValue(PropertyNames.ColumnWidth, value);
        }

        public string ContainerName
        {
            get => GetPropertyValue(PropertyNames.ContainerName);
            set => SetPropertyValue(PropertyNames.ContainerName, value);
        }

        public string ContainerType
        {
            get => GetPropertyValue(PropertyNames.ContainerType);
            set => SetPropertyValue(PropertyNames.ContainerType, value);
        }

        public string Content
        {
            get => GetPropertyValue(PropertyNames.Content);
            set => SetPropertyValue(PropertyNames.Content, value);
        }

        public string CounterIncrement
        {
            get => GetPropertyValue(PropertyNames.CounterIncrement);
            set => SetPropertyValue(PropertyNames.CounterIncrement, value);
        }

        public string CounterReset
        {
            get => GetPropertyValue(PropertyNames.CounterReset);
            set => SetPropertyValue(PropertyNames.CounterReset, value);
        }

        public string Float
        {
            get => GetPropertyValue(PropertyNames.Float);
            set => SetPropertyValue(PropertyNames.Float, value);
        }

        public string Cursor
        {
            get => GetPropertyValue(PropertyNames.Cursor);
            set => SetPropertyValue(PropertyNames.Cursor, value);
        }

        public string Direction
        {
            get => GetPropertyValue(PropertyNames.Direction);
            set => SetPropertyValue(PropertyNames.Direction, value);
        }

        public string Display
        {
            get => GetPropertyValue(PropertyNames.Display);
            set => SetPropertyValue(PropertyNames.Display, value);
        }

        public string DominantBaseline
        {
            get => GetPropertyValue(PropertyNames.DominantBaseline);
            set => SetPropertyValue(PropertyNames.DominantBaseline, value);
        }

        public string EmptyCells
        {
            get => GetPropertyValue(PropertyNames.EmptyCells);
            set => SetPropertyValue(PropertyNames.EmptyCells, value);
        }

        public string EnableBackground
        {
            get => GetPropertyValue(PropertyNames.EnableBackground);
            set => SetPropertyValue(PropertyNames.EnableBackground, value);
        }

        public string Fill
        {
            get => GetPropertyValue(PropertyNames.Fill);
            set => SetPropertyValue(PropertyNames.Fill, value);
        }

        public string FillOpacity
        {
            get => GetPropertyValue(PropertyNames.FillOpacity);
            set => SetPropertyValue(PropertyNames.FillOpacity, value);
        }

        public string FillRule
        {
            get => GetPropertyValue(PropertyNames.FillRule);
            set => SetPropertyValue(PropertyNames.FillRule, value);
        }

        public string Filter
        {
            get => GetPropertyValue(PropertyNames.Filter);
            set => SetPropertyValue(PropertyNames.Filter, value);
        }

        public string Flex
        {
            get => GetPropertyValue(PropertyNames.Flex);
            set => SetPropertyValue(PropertyNames.Flex, value);
        }

        public string FlexBasis
        {
            get => GetPropertyValue(PropertyNames.FlexBasis);
            set => SetPropertyValue(PropertyNames.FlexBasis, value);
        }

        public string FlexDirection
        {
            get => GetPropertyValue(PropertyNames.FlexDirection);
            set => SetPropertyValue(PropertyNames.FlexDirection, value);
        }

        public string FlexFlow
        {
            get => GetPropertyValue(PropertyNames.FlexFlow);
            set => SetPropertyValue(PropertyNames.FlexFlow, value);
        }

        public string FlexGrow
        {
            get => GetPropertyValue(PropertyNames.FlexGrow);
            set => SetPropertyValue(PropertyNames.FlexGrow, value);
        }

        public string FlexShrink
        {
            get => GetPropertyValue(PropertyNames.FlexShrink);
            set => SetPropertyValue(PropertyNames.FlexShrink, value);
        }

        public string FlexWrap
        {
            get => GetPropertyValue(PropertyNames.FlexWrap);
            set => SetPropertyValue(PropertyNames.FlexWrap, value);
        }

        public string Font
        {
            get => GetPropertyValue(PropertyNames.Font);
            set => SetPropertyValue(PropertyNames.Font, value);
        }

        public string FontFamily
        {
            get => GetPropertyValue(PropertyNames.FontFamily);
            set => SetPropertyValue(PropertyNames.FontFamily, value);
        }

        public string FontFeatureSettings
        {
            get => GetPropertyValue(PropertyNames.FontFeatureSettings);
            set => SetPropertyValue(PropertyNames.FontFeatureSettings, value);
        }

        public string FontSize
        {
            get => GetPropertyValue(PropertyNames.FontSize);
            set => SetPropertyValue(PropertyNames.FontSize, value);
        }

        public string FontSizeAdjust
        {
            get => GetPropertyValue(PropertyNames.FontSizeAdjust);
            set => SetPropertyValue(PropertyNames.FontSizeAdjust, value);
        }

        public string FontStretch
        {
            get => GetPropertyValue(PropertyNames.FontStretch);
            set => SetPropertyValue(PropertyNames.FontStretch, value);
        }

        public string FontStyle
        {
            get => GetPropertyValue(PropertyNames.FontStyle);
            set => SetPropertyValue(PropertyNames.FontStyle, value);
        }

        public string FontVariant
        {
            get => GetPropertyValue(PropertyNames.FontVariant);
            set => SetPropertyValue(PropertyNames.FontVariant, value);
        }

        public string FontWeight
        {
            get => GetPropertyValue(PropertyNames.FontWeight);
            set => SetPropertyValue(PropertyNames.FontWeight, value);
        }

        public string Gap
        {
            get => GetPropertyValue(PropertyNames.Gap);
            set => SetPropertyValue(PropertyNames.Gap, value);
        }

        public string GlyphOrientationHorizontal
        {
            get => GetPropertyValue(PropertyNames.GlyphOrientationHorizontal);
            set => SetPropertyValue(PropertyNames.GlyphOrientationHorizontal, value);
        }

        public string GlyphOrientationVertical
        {
            get => GetPropertyValue(PropertyNames.GlyphOrientationVertical);
            set => SetPropertyValue(PropertyNames.GlyphOrientationVertical, value);
        }

        public string Height
        {
            get => GetPropertyValue(PropertyNames.Height);
            set => SetPropertyValue(PropertyNames.Height, value);
        }

        public string ImeMode
        {
            get => GetPropertyValue(PropertyNames.ImeMode);
            set => SetPropertyValue(PropertyNames.ImeMode, value);
        }

        public string JustifyContent
        {
            get => GetPropertyValue(PropertyNames.JustifyContent);
            set => SetPropertyValue(PropertyNames.JustifyContent, value);
        }

        public string LayoutGrid
        {
            get => GetPropertyValue(PropertyNames.LayoutGrid);
            set => SetPropertyValue(PropertyNames.LayoutGrid, value);
        }

        public string LayoutGridChar
        {
            get => GetPropertyValue(PropertyNames.LayoutGridChar);
            set => SetPropertyValue(PropertyNames.LayoutGridChar, value);
        }

        public string LayoutGridLine
        {
            get => GetPropertyValue(PropertyNames.LayoutGridLine);
            set => SetPropertyValue(PropertyNames.LayoutGridLine, value);
        }

        public string LayoutGridMode
        {
            get => GetPropertyValue(PropertyNames.LayoutGridMode);
            set => SetPropertyValue(PropertyNames.LayoutGridMode, value);
        }

        public string LayoutGridType
        {
            get => GetPropertyValue(PropertyNames.LayoutGridType);
            set => SetPropertyValue(PropertyNames.LayoutGridType, value);
        }

        public string Left
        {
            get => GetPropertyValue(PropertyNames.Left);
            set => SetPropertyValue(PropertyNames.Left, value);
        }

        public string LetterSpacing
        {
            get => GetPropertyValue(PropertyNames.LetterSpacing);
            set => SetPropertyValue(PropertyNames.LetterSpacing, value);
        }

        public string LineHeight
        {
            get => GetPropertyValue(PropertyNames.LineHeight);
            set => SetPropertyValue(PropertyNames.LineHeight, value);
        }

        public string ListStyle
        {
            get => GetPropertyValue(PropertyNames.ListStyle);
            set => SetPropertyValue(PropertyNames.ListStyle, value);
        }

        public string ListStyleImage
        {
            get => GetPropertyValue(PropertyNames.ListStyleImage);
            set => SetPropertyValue(PropertyNames.ListStyleImage, value);
        }

        public string ListStylePosition
        {
            get => GetPropertyValue(PropertyNames.ListStylePosition);
            set => SetPropertyValue(PropertyNames.ListStylePosition, value);
        }

        public string ListStyleType
        {
            get => GetPropertyValue(PropertyNames.ListStyleType);
            set => SetPropertyValue(PropertyNames.ListStyleType, value);
        }

        public string Margin
        {
            get => GetPropertyValue(PropertyNames.Margin);
            set => SetPropertyValue(PropertyNames.Margin, value);
        }

        public string MarginBottom
        {
            get => GetPropertyValue(PropertyNames.MarginBottom);
            set => SetPropertyValue(PropertyNames.MarginBottom, value);
        }

        public string MarginLeft
        {
            get => GetPropertyValue(PropertyNames.MarginLeft);
            set => SetPropertyValue(PropertyNames.MarginLeft, value);
        }

        public string MarginRight
        {
            get => GetPropertyValue(PropertyNames.MarginRight);
            set => SetPropertyValue(PropertyNames.MarginRight, value);
        }

        public string MarginTop
        {
            get => GetPropertyValue(PropertyNames.MarginTop);
            set => SetPropertyValue(PropertyNames.MarginTop, value);
        }

        public string Marker
        {
            get => GetPropertyValue(PropertyNames.Marker);
            set => SetPropertyValue(PropertyNames.Marker, value);
        }

        public string MarkerEnd
        {
            get => GetPropertyValue(PropertyNames.MarkerEnd);
            set => SetPropertyValue(PropertyNames.MarkerEnd, value);
        }

        public string MarkerMid
        {
            get => GetPropertyValue(PropertyNames.MarkerMid);
            set => SetPropertyValue(PropertyNames.MarkerMid, value);
        }

        public string MarkerStart
        {
            get => GetPropertyValue(PropertyNames.MarkerStart);
            set => SetPropertyValue(PropertyNames.MarkerStart, value);
        }

        public string Mask
        {
            get => GetPropertyValue(PropertyNames.Mask);
            set => SetPropertyValue(PropertyNames.Mask, value);
        }

        public string MaxHeight
        {
            get => GetPropertyValue(PropertyNames.MaxHeight);
            set => SetPropertyValue(PropertyNames.MaxHeight, value);
        }

        public string MaxWidth
        {
            get => GetPropertyValue(PropertyNames.MaxWidth);
            set => SetPropertyValue(PropertyNames.MaxWidth, value);
        }

        public string MinHeight
        {
            get => GetPropertyValue(PropertyNames.MinHeight);
            set => SetPropertyValue(PropertyNames.MinHeight, value);
        }

        public string MinWidth
        {
            get => GetPropertyValue(PropertyNames.MinWidth);
            set => SetPropertyValue(PropertyNames.MinWidth, value);
        }

        public string Opacity
        {
            get => GetPropertyValue(PropertyNames.Opacity);
            set => SetPropertyValue(PropertyNames.Opacity, value);
        }

        public string Order
        {
            get => GetPropertyValue(PropertyNames.Order);
            set => SetPropertyValue(PropertyNames.Order, value);
        }

        public string Orphans
        {
            get => GetPropertyValue(PropertyNames.Orphans);
            set => SetPropertyValue(PropertyNames.Orphans, value);
        }

        public string Outline
        {
            get => GetPropertyValue(PropertyNames.Outline);
            set => SetPropertyValue(PropertyNames.Outline, value);
        }

        public string OutlineColor
        {
            get => GetPropertyValue(PropertyNames.OutlineColor);
            set => SetPropertyValue(PropertyNames.OutlineColor, value);
        }

        public string OutlineStyle
        {
            get => GetPropertyValue(PropertyNames.OutlineStyle);
            set => SetPropertyValue(PropertyNames.OutlineStyle, value);
        }

        public string OutlineWidth
        {
            get => GetPropertyValue(PropertyNames.OutlineWidth);
            set => SetPropertyValue(PropertyNames.OutlineWidth, value);
        }

        public string Overflow
        {
            get => GetPropertyValue(PropertyNames.Overflow);
            set => SetPropertyValue(PropertyNames.Overflow, value);
        }

        public string OverflowX
        {
            get => GetPropertyValue(PropertyNames.OverflowX);
            set => SetPropertyValue(PropertyNames.OverflowX, value);
        }

        public string OverflowY
        {
            get => GetPropertyValue(PropertyNames.OverflowY);
            set => SetPropertyValue(PropertyNames.OverflowY, value);
        }

        public string OverflowWrap
        {
            get => GetPropertyValue(PropertyNames.WordWrap);
            set => SetPropertyValue(PropertyNames.WordWrap, value);
        }

        public string Padding
        {
            get => GetPropertyValue(PropertyNames.Padding);
            set => SetPropertyValue(PropertyNames.Padding, value);
        }

        public string PaddingBottom
        {
            get => GetPropertyValue(PropertyNames.PaddingBottom);
            set => SetPropertyValue(PropertyNames.PaddingBottom, value);
        }

        public string PaddingLeft
        {
            get => GetPropertyValue(PropertyNames.PaddingLeft);
            set => SetPropertyValue(PropertyNames.PaddingLeft, value);
        }

        public string PaddingRight
        {
            get => GetPropertyValue(PropertyNames.PaddingRight);
            set => SetPropertyValue(PropertyNames.PaddingRight, value);
        }

        public string PaddingTop
        {
            get => GetPropertyValue(PropertyNames.PaddingTop);
            set => SetPropertyValue(PropertyNames.PaddingTop, value);
        }

        public string PageBreakAfter
        {
            get => GetPropertyValue(PropertyNames.PageBreakAfter);
            set => SetPropertyValue(PropertyNames.PageBreakAfter, value);
        }

        public string PageBreakBefore
        {
            get => GetPropertyValue(PropertyNames.PageBreakBefore);
            set => SetPropertyValue(PropertyNames.PageBreakBefore, value);
        }

        public string PageBreakInside
        {
            get => GetPropertyValue(PropertyNames.PageBreakInside);
            set => SetPropertyValue(PropertyNames.PageBreakInside, value);
        }

        public string Perspective
        {
            get => GetPropertyValue(PropertyNames.Perspective);
            set => SetPropertyValue(PropertyNames.Perspective, value);
        }

        public string PerspectiveOrigin
        {
            get => GetPropertyValue(PropertyNames.PerspectiveOrigin);
            set => SetPropertyValue(PropertyNames.PerspectiveOrigin, value);
        }

        public string PointerEvents
        {
            get => GetPropertyValue(PropertyNames.PointerEvents);
            set => SetPropertyValue(PropertyNames.PointerEvents, value);
        }

        public string RowGap
        {
            get => GetPropertyValue(PropertyNames.RowGap);
            set => SetPropertyValue(PropertyNames.RowGap, value);
        }

        public string Quotes
        {
            get => GetPropertyValue(PropertyNames.Quotes);
            set => SetPropertyValue(PropertyNames.Quotes, value);
        }

        public string Position
        {
            get => GetPropertyValue(PropertyNames.Position);
            set => SetPropertyValue(PropertyNames.Position, value);
        }

        public string Right
        {
            get => GetPropertyValue(PropertyNames.Right);
            set => SetPropertyValue(PropertyNames.Right, value);
        }

        public string RubyAlign
        {
            get => GetPropertyValue(PropertyNames.RubyAlign);
            set => SetPropertyValue(PropertyNames.RubyAlign, value);
        }

        public string RubyOverhang
        {
            get => GetPropertyValue(PropertyNames.RubyOverhang);
            set => SetPropertyValue(PropertyNames.RubyOverhang, value);
        }

        public string RubyPosition
        {
            get => GetPropertyValue(PropertyNames.RubyPosition);
            set => SetPropertyValue(PropertyNames.RubyPosition, value);
        }

        public string Scrollbar3DLightColor
        {
            get => GetPropertyValue(PropertyNames.Scrollbar3dLightColor);
            set => SetPropertyValue(PropertyNames.Scrollbar3dLightColor, value);
        }

        public string ScrollbarArrowColor
        {
            get => GetPropertyValue(PropertyNames.ScrollbarArrowColor);
            set => SetPropertyValue(PropertyNames.ScrollbarArrowColor, value);
        }

        public string ScrollbarDarkShadowColor
        {
            get => GetPropertyValue(PropertyNames.ScrollbarDarkShadowColor);
            set => SetPropertyValue(PropertyNames.ScrollbarDarkShadowColor, value);
        }

        public string ScrollbarFaceColor
        {
            get => GetPropertyValue(PropertyNames.ScrollbarFaceColor);
            set => SetPropertyValue(PropertyNames.ScrollbarFaceColor, value);
        }

        public string ScrollbarHighlightColor
        {
            get => GetPropertyValue(PropertyNames.ScrollbarHighlightColor);
            set => SetPropertyValue(PropertyNames.ScrollbarHighlightColor, value);
        }

        public string ScrollbarShadowColor
        {
            get => GetPropertyValue(PropertyNames.ScrollbarShadowColor);
            set => SetPropertyValue(PropertyNames.ScrollbarShadowColor, value);
        }

        public string ScrollbarTrackColor
        {
            get => GetPropertyValue(PropertyNames.ScrollbarTrackColor);
            set => SetPropertyValue(PropertyNames.ScrollbarTrackColor, value);
        }

        public string Stroke
        {
            get => GetPropertyValue(PropertyNames.Stroke);
            set => SetPropertyValue(PropertyNames.Stroke, value);
        }

        public string StrokeDasharray
        {
            get => GetPropertyValue(PropertyNames.StrokeDasharray);
            set => SetPropertyValue(PropertyNames.StrokeDasharray, value);
        }

        public string StrokeDashoffset
        {
            get => GetPropertyValue(PropertyNames.StrokeDashoffset);
            set => SetPropertyValue(PropertyNames.StrokeDashoffset, value);
        }

        public string StrokeLinecap
        {
            get => GetPropertyValue(PropertyNames.StrokeLinecap);
            set => SetPropertyValue(PropertyNames.StrokeLinecap, value);
        }

        public string StrokeLinejoin
        {
            get => GetPropertyValue(PropertyNames.StrokeLinejoin);
            set => SetPropertyValue(PropertyNames.StrokeLinejoin, value);
        }

        public string StrokeMiterlimit
        {
            get => GetPropertyValue(PropertyNames.StrokeMiterlimit);
            set => SetPropertyValue(PropertyNames.StrokeMiterlimit, value);
        }

        public string StrokeOpacity
        {
            get => GetPropertyValue(PropertyNames.StrokeOpacity);
            set => SetPropertyValue(PropertyNames.StrokeOpacity, value);
        }

        public string StrokeWidth
        {
            get => GetPropertyValue(PropertyNames.StrokeWidth);
            set => SetPropertyValue(PropertyNames.StrokeWidth, value);
        }

        public string TableLayout
        {
            get => GetPropertyValue(PropertyNames.TableLayout);
            set => SetPropertyValue(PropertyNames.TableLayout, value);
        }

        public string TextAlign
        {
            get => GetPropertyValue(PropertyNames.TextAlign);
            set => SetPropertyValue(PropertyNames.TextAlign, value);
        }

        public string TextAlignLast
        {
            get => GetPropertyValue(PropertyNames.TextAlignLast);
            set => SetPropertyValue(PropertyNames.TextAlignLast, value);
        }

        public string TextAnchor
        {
            get => GetPropertyValue(PropertyNames.TextAnchor);
            set => SetPropertyValue(PropertyNames.TextAnchor, value);
        }

        public string TextAutospace
        {
            get => GetPropertyValue(PropertyNames.TextAutospace);
            set => SetPropertyValue(PropertyNames.TextAutospace, value);
        }

        public string TextDecoration
        {
            get => GetPropertyValue(PropertyNames.TextDecoration);
            set => SetPropertyValue(PropertyNames.TextDecoration, value);
        }

        public string TextIndent
        {
            get => GetPropertyValue(PropertyNames.TextIndent);
            set => SetPropertyValue(PropertyNames.TextIndent, value);
        }

        public string TextJustify
        {
            get => GetPropertyValue(PropertyNames.TextJustify);
            set => SetPropertyValue(PropertyNames.TextJustify, value);
        }

        public string TextOverflow
        {
            get => GetPropertyValue(PropertyNames.TextOverflow);
            set => SetPropertyValue(PropertyNames.TextOverflow, value);
        }

        public string TextShadow
        {
            get => GetPropertyValue(PropertyNames.TextShadow);
            set => SetPropertyValue(PropertyNames.TextShadow, value);
        }

        public string TextTransform
        {
            get => GetPropertyValue(PropertyNames.TextTransform);
            set => SetPropertyValue(PropertyNames.TextTransform, value);
        }

        public string TextUnderlinePosition
        {
            get => GetPropertyValue(PropertyNames.TextUnderlinePosition);
            set => SetPropertyValue(PropertyNames.TextUnderlinePosition, value);
        }

        public string Top
        {
            get => GetPropertyValue(PropertyNames.Top);
            set => SetPropertyValue(PropertyNames.Top, value);
        }

        public string Transform
        {
            get => GetPropertyValue(PropertyNames.Transform);
            set => SetPropertyValue(PropertyNames.Transform, value);
        }

        public string TransformOrigin
        {
            get => GetPropertyValue(PropertyNames.TransformOrigin);
            set => SetPropertyValue(PropertyNames.TransformOrigin, value);
        }

        public string TransformStyle
        {
            get => GetPropertyValue(PropertyNames.TransformStyle);
            set => SetPropertyValue(PropertyNames.TransformStyle, value);
        }

        public string Transition
        {
            get => GetPropertyValue(PropertyNames.Transition);
            set => SetPropertyValue(PropertyNames.Transition, value);
        }

        public string TransitionDelay
        {
            get => GetPropertyValue(PropertyNames.TransitionDelay);
            set => SetPropertyValue(PropertyNames.TransitionDelay, value);
        }

        public string TransitionDuration
        {
            get => GetPropertyValue(PropertyNames.TransitionDuration);
            set => SetPropertyValue(PropertyNames.TransitionDuration, value);
        }

        public string TransitionProperty
        {
            get => GetPropertyValue(PropertyNames.TransitionProperty);
            set => SetPropertyValue(PropertyNames.TransitionProperty, value);
        }

        public string TransitionTimingFunction
        {
            get => GetPropertyValue(PropertyNames.TransitionTimingFunction);
            set => SetPropertyValue(PropertyNames.TransitionTimingFunction, value);
        }

        public string UnicodeBidirectional
        {
            get => GetPropertyValue(PropertyNames.UnicodeBidirectional);
            set => SetPropertyValue(PropertyNames.UnicodeBidirectional, value);
        }

        public string VerticalAlign
        {
            get => GetPropertyValue(PropertyNames.VerticalAlign);
            set => SetPropertyValue(PropertyNames.VerticalAlign, value);
        }

        public string Visibility
        {
            get => GetPropertyValue(PropertyNames.Visibility);
            set => SetPropertyValue(PropertyNames.Visibility, value);
        }

        public string WhiteSpace
        {
            get => GetPropertyValue(PropertyNames.WhiteSpace);
            set => SetPropertyValue(PropertyNames.WhiteSpace, value);
        }

        public string Widows
        {
            get => GetPropertyValue(PropertyNames.Widows);
            set => SetPropertyValue(PropertyNames.Widows, value);
        }

        public string Width
        {
            get => GetPropertyValue(PropertyNames.Width);
            set => SetPropertyValue(PropertyNames.Width, value);
        }

        public string WordBreak
        {
            get => GetPropertyValue(PropertyNames.WordBreak);
            set => SetPropertyValue(PropertyNames.WordBreak, value);
        }

        public string WordSpacing
        {
            get => GetPropertyValue(PropertyNames.WordSpacing);
            set => SetPropertyValue(PropertyNames.WordSpacing, value);
        }

        public string WritingMode
        {
            get => GetPropertyValue(PropertyNames.WritingMode);
            set => SetPropertyValue(PropertyNames.WritingMode, value);
        }

        public string ZIndex
        {
            get => GetPropertyValue(PropertyNames.ZIndex);
            set => SetPropertyValue(PropertyNames.ZIndex, value);
        }

        public string Zoom
        {
            get => GetPropertyValue(PropertyNames.Zoom);
            set => SetPropertyValue(PropertyNames.Zoom, value);
        }

        public string Size
        {
            get => GetPropertyValue(PropertyNames.Size);
            set => SetPropertyValue(PropertyNames.Size, value);
        }

        public string PageName
        {
            get => GetPropertyValue(PropertyNames.PageName);
            set => SetPropertyValue(PropertyNames.PageName, value);
        }
    }
}

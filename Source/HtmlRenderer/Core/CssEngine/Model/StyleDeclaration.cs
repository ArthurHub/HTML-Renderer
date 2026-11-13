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

        // The standard spelling of a vendor-prefixed name when vendor aliasing is on; otherwise the name unchanged.
        private string Canonical(string name)
        {
            return _parser != null && _parser.Options.EnableVendorPrefixAliases
                ? VendorPropertyAliases.Resolve(name)
                : name;
        }

        private void RemovePropertyByName(string propertyName)
        {
            propertyName = Canonical(propertyName);

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
            propertyName = Canonical(propertyName);
            var property = GetProperty(propertyName);
            if (property is {IsImportant: true}) return Keywords.Important;
            if (!IsStrictMode || !PropertyFactory.Instance.IsShorthand(propertyName)) return string.Empty;

            var longhands = PropertyFactory.Instance.GetLonghands(propertyName);

            return longhands.Any(longhand => !GetPropertyPriority(longhand)
                .Isi(Keywords.Important))
                ? string.Empty
                : Keywords.Important;
        }

        public string GetPropertyValue(string propertyName)
        {
            propertyName = Canonical(propertyName);
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

            propertyName = Canonical(propertyName);
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
                if (property != null && _parser.Options.EnableVendorPrefixAliases)
                    value = VendorValueAliases.Normalize(propertyName, property.Name, value);

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

            property = _parser != null && _parser.Options.EnableVendorPrefixAliases
                ? VendorPropertyAliases.Create(propertyName)
                : PropertyFactory.Instance.Create(propertyName);
            if (property != null || IsStrictMode) return property;

            return new UnknownProperty(propertyName);
        }

        internal Property GetProperty(string name)
        {
            name = Canonical(name);
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
                // A var() reference can't be split into per-longhand slices here: the referenced custom
                // property's value is only known per-element, at cascade time. Keep the shorthand
                // declaration whole so substitution and expansion can happen once it is resolved (CSS
                // Variables 1 3.2).
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

        public string AccentColor
        {
            get => GetPropertyValue(PropertyNames.AccentColor);
            set => SetPropertyValue(PropertyNames.AccentColor, value);
        }

        public string BackgroundBlendMode
        {
            get => GetPropertyValue(PropertyNames.BackgroundBlendMode);
            set => SetPropertyValue(PropertyNames.BackgroundBlendMode, value);
        }

        public string BufferedRendering
        {
            get => GetPropertyValue(PropertyNames.BufferedRendering);
            set => SetPropertyValue(PropertyNames.BufferedRendering, value);
        }

        public string CaretColor
        {
            get => GetPropertyValue(PropertyNames.CaretColor);
            set => SetPropertyValue(PropertyNames.CaretColor, value);
        }

        public string CssText
        {
            get => this.ToCss();
            set
            {
                Update(value);
                RaiseChanged();
            }
        }

        public string All
        {
            get => GetPropertyValue(PropertyNames.All);
            set => SetPropertyValue(PropertyNames.All, value);
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

        public string BackdropFilter
        {
            get => GetPropertyValue(PropertyNames.BackdropFilter);
            set => SetPropertyValue(PropertyNames.BackdropFilter, value);
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

        public string BorderBlock
        {
            get => GetPropertyValue(PropertyNames.BorderBlock);
            set => SetPropertyValue(PropertyNames.BorderBlock, value);
        }

        public string BorderBlockColor
        {
            get => GetPropertyValue(PropertyNames.BorderBlockColor);
            set => SetPropertyValue(PropertyNames.BorderBlockColor, value);
        }

        public string BorderBlockEnd
        {
            get => GetPropertyValue(PropertyNames.BorderBlockEnd);
            set => SetPropertyValue(PropertyNames.BorderBlockEnd, value);
        }

        public string BorderBlockEndColor
        {
            get => GetPropertyValue(PropertyNames.BorderBlockEndColor);
            set => SetPropertyValue(PropertyNames.BorderBlockEndColor, value);
        }

        public string BorderBlockEndRadius
        {
            get => GetPropertyValue(PropertyNames.BorderBlockEndRadius);
            set => SetPropertyValue(PropertyNames.BorderBlockEndRadius, value);
        }

        public string BorderBlockEndStyle
        {
            get => GetPropertyValue(PropertyNames.BorderBlockEndStyle);
            set => SetPropertyValue(PropertyNames.BorderBlockEndStyle, value);
        }

        public string BorderBlockEndWidth
        {
            get => GetPropertyValue(PropertyNames.BorderBlockEndWidth);
            set => SetPropertyValue(PropertyNames.BorderBlockEndWidth, value);
        }

        public string BorderBlockStart
        {
            get => GetPropertyValue(PropertyNames.BorderBlockStart);
            set => SetPropertyValue(PropertyNames.BorderBlockStart, value);
        }

        public string BorderBlockStartColor
        {
            get => GetPropertyValue(PropertyNames.BorderBlockStartColor);
            set => SetPropertyValue(PropertyNames.BorderBlockStartColor, value);
        }

        public string BorderBlockStartRadius
        {
            get => GetPropertyValue(PropertyNames.BorderBlockStartRadius);
            set => SetPropertyValue(PropertyNames.BorderBlockStartRadius, value);
        }

        public string BorderBlockStartStyle
        {
            get => GetPropertyValue(PropertyNames.BorderBlockStartStyle);
            set => SetPropertyValue(PropertyNames.BorderBlockStartStyle, value);
        }

        public string BorderBlockStartWidth
        {
            get => GetPropertyValue(PropertyNames.BorderBlockStartWidth);
            set => SetPropertyValue(PropertyNames.BorderBlockStartWidth, value);
        }

        public string BorderBlockStyle
        {
            get => GetPropertyValue(PropertyNames.BorderBlockStyle);
            set => SetPropertyValue(PropertyNames.BorderBlockStyle, value);
        }

        public string BorderBlockWidth
        {
            get => GetPropertyValue(PropertyNames.BorderBlockWidth);
            set => SetPropertyValue(PropertyNames.BorderBlockWidth, value);
        }

        public string BorderBottomRadius
        {
            get => GetPropertyValue(PropertyNames.BorderBottomRadius);
            set => SetPropertyValue(PropertyNames.BorderBottomRadius, value);
        }

        public string BorderEndEndRadius
        {
            get => GetPropertyValue(PropertyNames.BorderEndEndRadius);
            set => SetPropertyValue(PropertyNames.BorderEndEndRadius, value);
        }

        public string BorderEndStartRadius
        {
            get => GetPropertyValue(PropertyNames.BorderEndStartRadius);
            set => SetPropertyValue(PropertyNames.BorderEndStartRadius, value);
        }

        public string BorderInline
        {
            get => GetPropertyValue(PropertyNames.BorderInline);
            set => SetPropertyValue(PropertyNames.BorderInline, value);
        }

        public string BorderInlineColor
        {
            get => GetPropertyValue(PropertyNames.BorderInlineColor);
            set => SetPropertyValue(PropertyNames.BorderInlineColor, value);
        }

        public string BorderInlineEnd
        {
            get => GetPropertyValue(PropertyNames.BorderInlineEnd);
            set => SetPropertyValue(PropertyNames.BorderInlineEnd, value);
        }

        public string BorderInlineEndColor
        {
            get => GetPropertyValue(PropertyNames.BorderInlineEndColor);
            set => SetPropertyValue(PropertyNames.BorderInlineEndColor, value);
        }

        public string BorderInlineEndRadius
        {
            get => GetPropertyValue(PropertyNames.BorderInlineEndRadius);
            set => SetPropertyValue(PropertyNames.BorderInlineEndRadius, value);
        }

        public string BorderInlineEndStyle
        {
            get => GetPropertyValue(PropertyNames.BorderInlineEndStyle);
            set => SetPropertyValue(PropertyNames.BorderInlineEndStyle, value);
        }

        public string BorderInlineEndWidth
        {
            get => GetPropertyValue(PropertyNames.BorderInlineEndWidth);
            set => SetPropertyValue(PropertyNames.BorderInlineEndWidth, value);
        }

        public string BorderInlineStart
        {
            get => GetPropertyValue(PropertyNames.BorderInlineStart);
            set => SetPropertyValue(PropertyNames.BorderInlineStart, value);
        }

        public string BorderInlineStartColor
        {
            get => GetPropertyValue(PropertyNames.BorderInlineStartColor);
            set => SetPropertyValue(PropertyNames.BorderInlineStartColor, value);
        }

        public string BorderInlineStartRadius
        {
            get => GetPropertyValue(PropertyNames.BorderInlineStartRadius);
            set => SetPropertyValue(PropertyNames.BorderInlineStartRadius, value);
        }

        public string BorderInlineStartStyle
        {
            get => GetPropertyValue(PropertyNames.BorderInlineStartStyle);
            set => SetPropertyValue(PropertyNames.BorderInlineStartStyle, value);
        }

        public string BorderInlineStartWidth
        {
            get => GetPropertyValue(PropertyNames.BorderInlineStartWidth);
            set => SetPropertyValue(PropertyNames.BorderInlineStartWidth, value);
        }

        public string BorderInlineStyle
        {
            get => GetPropertyValue(PropertyNames.BorderInlineStyle);
            set => SetPropertyValue(PropertyNames.BorderInlineStyle, value);
        }

        public string BorderInlineWidth
        {
            get => GetPropertyValue(PropertyNames.BorderInlineWidth);
            set => SetPropertyValue(PropertyNames.BorderInlineWidth, value);
        }

        public string BorderLeftRadius
        {
            get => GetPropertyValue(PropertyNames.BorderLeftRadius);
            set => SetPropertyValue(PropertyNames.BorderLeftRadius, value);
        }

        public string BorderRightRadius
        {
            get => GetPropertyValue(PropertyNames.BorderRightRadius);
            set => SetPropertyValue(PropertyNames.BorderRightRadius, value);
        }

        public string BorderStartEndRadius
        {
            get => GetPropertyValue(PropertyNames.BorderStartEndRadius);
            set => SetPropertyValue(PropertyNames.BorderStartEndRadius, value);
        }

        public string BorderStartStartRadius
        {
            get => GetPropertyValue(PropertyNames.BorderStartStartRadius);
            set => SetPropertyValue(PropertyNames.BorderStartStartRadius, value);
        }

        public string BorderTopRadius
        {
            get => GetPropertyValue(PropertyNames.BorderTopRadius);
            set => SetPropertyValue(PropertyNames.BorderTopRadius, value);
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

        public string ColorScheme
        {
            get => GetPropertyValue(PropertyNames.ColorScheme);
            set => SetPropertyValue(PropertyNames.ColorScheme, value);
        }

        public string ColorInterpolation
        {
            get => GetPropertyValue(PropertyNames.ColorInterpolation);
            set => SetPropertyValue(PropertyNames.ColorInterpolation, value);
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

        public string Contain
        {
            get => GetPropertyValue(PropertyNames.Contain);
            set => SetPropertyValue(PropertyNames.Contain, value);
        }

        public string ContainIntrinsicWidth
        {
            get => GetPropertyValue(PropertyNames.ContainIntrinsicWidth);
            set => SetPropertyValue(PropertyNames.ContainIntrinsicWidth, value);
        }

        public string ContainIntrinsicHeight
        {
            get => GetPropertyValue(PropertyNames.ContainIntrinsicHeight);
            set => SetPropertyValue(PropertyNames.ContainIntrinsicHeight, value);
        }

        public string ContainIntrinsicInlineSize
        {
            get => GetPropertyValue(PropertyNames.ContainIntrinsicInlineSize);
            set => SetPropertyValue(PropertyNames.ContainIntrinsicInlineSize, value);
        }

        public string ContainIntrinsicBlockSize
        {
            get => GetPropertyValue(PropertyNames.ContainIntrinsicBlockSize);
            set => SetPropertyValue(PropertyNames.ContainIntrinsicBlockSize, value);
        }

        public string ContainIntrinsicSize
        {
            get => GetPropertyValue(PropertyNames.ContainIntrinsicSize);
            set => SetPropertyValue(PropertyNames.ContainIntrinsicSize, value);
        }

        public string Container
        {
            get => GetPropertyValue(PropertyNames.Container);
            set => SetPropertyValue(PropertyNames.Container, value);
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

        public string ContentVisibility
        {
            get => GetPropertyValue(PropertyNames.ContentVisibility);
            set => SetPropertyValue(PropertyNames.ContentVisibility, value);
        }

        public string Corner
        {
            get => GetPropertyValue(PropertyNames.Corner);
            set => SetPropertyValue(PropertyNames.Corner, value);
        }

        public string CornerBlockEnd
        {
            get => GetPropertyValue(PropertyNames.CornerBlockEnd);
            set => SetPropertyValue(PropertyNames.CornerBlockEnd, value);
        }

        public string CornerBlockEndShape
        {
            get => GetPropertyValue(PropertyNames.CornerBlockEndShape);
            set => SetPropertyValue(PropertyNames.CornerBlockEndShape, value);
        }

        public string CornerBlockStart
        {
            get => GetPropertyValue(PropertyNames.CornerBlockStart);
            set => SetPropertyValue(PropertyNames.CornerBlockStart, value);
        }

        public string CornerBlockStartShape
        {
            get => GetPropertyValue(PropertyNames.CornerBlockStartShape);
            set => SetPropertyValue(PropertyNames.CornerBlockStartShape, value);
        }

        public string CornerBottom
        {
            get => GetPropertyValue(PropertyNames.CornerBottom);
            set => SetPropertyValue(PropertyNames.CornerBottom, value);
        }

        public string CornerBottomLeft
        {
            get => GetPropertyValue(PropertyNames.CornerBottomLeft);
            set => SetPropertyValue(PropertyNames.CornerBottomLeft, value);
        }

        public string CornerBottomLeftShape
        {
            get => GetPropertyValue(PropertyNames.CornerBottomLeftShape);
            set => SetPropertyValue(PropertyNames.CornerBottomLeftShape, value);
        }

        public string CornerBottomRight
        {
            get => GetPropertyValue(PropertyNames.CornerBottomRight);
            set => SetPropertyValue(PropertyNames.CornerBottomRight, value);
        }

        public string CornerBottomRightShape
        {
            get => GetPropertyValue(PropertyNames.CornerBottomRightShape);
            set => SetPropertyValue(PropertyNames.CornerBottomRightShape, value);
        }

        public string CornerBottomShape
        {
            get => GetPropertyValue(PropertyNames.CornerBottomShape);
            set => SetPropertyValue(PropertyNames.CornerBottomShape, value);
        }

        public string CornerEndEnd
        {
            get => GetPropertyValue(PropertyNames.CornerEndEnd);
            set => SetPropertyValue(PropertyNames.CornerEndEnd, value);
        }

        public string CornerEndEndShape
        {
            get => GetPropertyValue(PropertyNames.CornerEndEndShape);
            set => SetPropertyValue(PropertyNames.CornerEndEndShape, value);
        }

        public string CornerEndStart
        {
            get => GetPropertyValue(PropertyNames.CornerEndStart);
            set => SetPropertyValue(PropertyNames.CornerEndStart, value);
        }

        public string CornerEndStartShape
        {
            get => GetPropertyValue(PropertyNames.CornerEndStartShape);
            set => SetPropertyValue(PropertyNames.CornerEndStartShape, value);
        }

        public string CornerInlineEnd
        {
            get => GetPropertyValue(PropertyNames.CornerInlineEnd);
            set => SetPropertyValue(PropertyNames.CornerInlineEnd, value);
        }

        public string CornerInlineEndShape
        {
            get => GetPropertyValue(PropertyNames.CornerInlineEndShape);
            set => SetPropertyValue(PropertyNames.CornerInlineEndShape, value);
        }

        public string CornerInlineStart
        {
            get => GetPropertyValue(PropertyNames.CornerInlineStart);
            set => SetPropertyValue(PropertyNames.CornerInlineStart, value);
        }

        public string CornerInlineStartShape
        {
            get => GetPropertyValue(PropertyNames.CornerInlineStartShape);
            set => SetPropertyValue(PropertyNames.CornerInlineStartShape, value);
        }

        public string CornerLeft
        {
            get => GetPropertyValue(PropertyNames.CornerLeft);
            set => SetPropertyValue(PropertyNames.CornerLeft, value);
        }

        public string CornerLeftShape
        {
            get => GetPropertyValue(PropertyNames.CornerLeftShape);
            set => SetPropertyValue(PropertyNames.CornerLeftShape, value);
        }

        public string CornerRight
        {
            get => GetPropertyValue(PropertyNames.CornerRight);
            set => SetPropertyValue(PropertyNames.CornerRight, value);
        }

        public string CornerRightShape
        {
            get => GetPropertyValue(PropertyNames.CornerRightShape);
            set => SetPropertyValue(PropertyNames.CornerRightShape, value);
        }

        public string CornerShape
        {
            get => GetPropertyValue(PropertyNames.CornerShape);
            set => SetPropertyValue(PropertyNames.CornerShape, value);
        }

        public string CornerStartEnd
        {
            get => GetPropertyValue(PropertyNames.CornerStartEnd);
            set => SetPropertyValue(PropertyNames.CornerStartEnd, value);
        }

        public string CornerStartEndShape
        {
            get => GetPropertyValue(PropertyNames.CornerStartEndShape);
            set => SetPropertyValue(PropertyNames.CornerStartEndShape, value);
        }

        public string CornerStartStart
        {
            get => GetPropertyValue(PropertyNames.CornerStartStart);
            set => SetPropertyValue(PropertyNames.CornerStartStart, value);
        }

        public string CornerStartStartShape
        {
            get => GetPropertyValue(PropertyNames.CornerStartStartShape);
            set => SetPropertyValue(PropertyNames.CornerStartStartShape, value);
        }

        public string CornerTop
        {
            get => GetPropertyValue(PropertyNames.CornerTop);
            set => SetPropertyValue(PropertyNames.CornerTop, value);
        }

        public string CornerTopLeft
        {
            get => GetPropertyValue(PropertyNames.CornerTopLeft);
            set => SetPropertyValue(PropertyNames.CornerTopLeft, value);
        }

        public string CornerTopLeftShape
        {
            get => GetPropertyValue(PropertyNames.CornerTopLeftShape);
            set => SetPropertyValue(PropertyNames.CornerTopLeftShape, value);
        }

        public string CornerTopRight
        {
            get => GetPropertyValue(PropertyNames.CornerTopRight);
            set => SetPropertyValue(PropertyNames.CornerTopRight, value);
        }

        public string CornerTopRightShape
        {
            get => GetPropertyValue(PropertyNames.CornerTopRightShape);
            set => SetPropertyValue(PropertyNames.CornerTopRightShape, value);
        }

        public string CornerTopShape
        {
            get => GetPropertyValue(PropertyNames.CornerTopShape);
            set => SetPropertyValue(PropertyNames.CornerTopShape, value);
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

        public string FloodColor
        {
            get => GetPropertyValue(PropertyNames.FloodColor);
            set => SetPropertyValue(PropertyNames.FloodColor, value);
        }

        public string FloodOpacity
        {
            get => GetPropertyValue(PropertyNames.FloodOpacity);
            set => SetPropertyValue(PropertyNames.FloodOpacity, value);
        }

        public string Float
        {
            get => GetPropertyValue(PropertyNames.Float);
            set => SetPropertyValue(PropertyNames.Float, value);
        }

        public string Cx
        {
            get => GetPropertyValue(PropertyNames.Cx);
            set => SetPropertyValue(PropertyNames.Cx, value);
        }

        public string Cy
        {
            get => GetPropertyValue(PropertyNames.Cy);
            set => SetPropertyValue(PropertyNames.Cy, value);
        }

        public string Cursor
        {
            get => GetPropertyValue(PropertyNames.Cursor);
            set => SetPropertyValue(PropertyNames.Cursor, value);
        }

        public string D
        {
            get => GetPropertyValue(PropertyNames.D);
            set => SetPropertyValue(PropertyNames.D, value);
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

        public string FloatReference
        {
            get => GetPropertyValue(PropertyNames.FloatReference);
            set => SetPropertyValue(PropertyNames.FloatReference, value);
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

        public string FontKerning
        {
            get => GetPropertyValue(PropertyNames.FontKerning);
            set => SetPropertyValue(PropertyNames.FontKerning, value);
        }

        public string FontLanguageOverride
        {
            get => GetPropertyValue(PropertyNames.FontLanguageOverride);
            set => SetPropertyValue(PropertyNames.FontLanguageOverride, value);
        }

        public string FontOpticalSizing
        {
            get => GetPropertyValue(PropertyNames.FontOpticalSizing);
            set => SetPropertyValue(PropertyNames.FontOpticalSizing, value);
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

        public string FontSynthesis
        {
            get => GetPropertyValue(PropertyNames.FontSynthesis);
            set => SetPropertyValue(PropertyNames.FontSynthesis, value);
        }

        public string FontSynthesisPosition
        {
            get => GetPropertyValue(PropertyNames.FontSynthesisPosition);
            set => SetPropertyValue(PropertyNames.FontSynthesisPosition, value);
        }

        public string FontSynthesisSmallCaps
        {
            get => GetPropertyValue(PropertyNames.FontSynthesisSmallCaps);
            set => SetPropertyValue(PropertyNames.FontSynthesisSmallCaps, value);
        }

        public string FontSynthesisStyle
        {
            get => GetPropertyValue(PropertyNames.FontSynthesisStyle);
            set => SetPropertyValue(PropertyNames.FontSynthesisStyle, value);
        }

        public string FontSynthesisWeight
        {
            get => GetPropertyValue(PropertyNames.FontSynthesisWeight);
            set => SetPropertyValue(PropertyNames.FontSynthesisWeight, value);
        }

        public string FontVariant
        {
            get => GetPropertyValue(PropertyNames.FontVariant);
            set => SetPropertyValue(PropertyNames.FontVariant, value);
        }

        public string FontVariantAlternates
        {
            get => GetPropertyValue(PropertyNames.FontVariantAlternates);
            set => SetPropertyValue(PropertyNames.FontVariantAlternates, value);
        }

        public string FontVariantCaps
        {
            get => GetPropertyValue(PropertyNames.FontVariantCaps);
            set => SetPropertyValue(PropertyNames.FontVariantCaps, value);
        }

        public string FontVariantEastAsian
        {
            get => GetPropertyValue(PropertyNames.FontVariantEastAsian);
            set => SetPropertyValue(PropertyNames.FontVariantEastAsian, value);
        }

        public string FontVariantEmoji
        {
            get => GetPropertyValue(PropertyNames.FontVariantEmoji);
            set => SetPropertyValue(PropertyNames.FontVariantEmoji, value);
        }

        public string FontVariantLigatures
        {
            get => GetPropertyValue(PropertyNames.FontVariantLigatures);
            set => SetPropertyValue(PropertyNames.FontVariantLigatures, value);
        }

        public string FontVariantNumeric
        {
            get => GetPropertyValue(PropertyNames.FontVariantNumeric);
            set => SetPropertyValue(PropertyNames.FontVariantNumeric, value);
        }

        public string FontVariantPosition
        {
            get => GetPropertyValue(PropertyNames.FontVariantPosition);
            set => SetPropertyValue(PropertyNames.FontVariantPosition, value);
        }

        public string FontVariationSettings
        {
            get => GetPropertyValue(PropertyNames.FontVariationSettings);
            set => SetPropertyValue(PropertyNames.FontVariationSettings, value);
        }

        public string FontWeight
        {
            get => GetPropertyValue(PropertyNames.FontWeight);
            set => SetPropertyValue(PropertyNames.FontWeight, value);
        }

        public string FootnoteDisplay
        {
            get => GetPropertyValue(PropertyNames.FootnoteDisplay);
            set => SetPropertyValue(PropertyNames.FootnoteDisplay, value);
        }

        public string FootnotePolicy
        {
            get => GetPropertyValue(PropertyNames.FootnotePolicy);
            set => SetPropertyValue(PropertyNames.FootnotePolicy, value);
        }

        public string ForcedColorAdjust
        {
            get => GetPropertyValue(PropertyNames.ForcedColorAdjust);
            set => SetPropertyValue(PropertyNames.ForcedColorAdjust, value);
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

        public string ImageRendering
        {
            get => GetPropertyValue(PropertyNames.ImageRendering);
            set => SetPropertyValue(PropertyNames.ImageRendering, value);
        }

        public string ImageOrientation
        {
            get => GetPropertyValue(PropertyNames.ImageOrientation);
            set => SetPropertyValue(PropertyNames.ImageOrientation, value);
        }

        public string ImageResolution
        {
            get => GetPropertyValue(PropertyNames.ImageResolution);
            set => SetPropertyValue(PropertyNames.ImageResolution, value);
        }

        public string ImeMode
        {
            get => GetPropertyValue(PropertyNames.ImeMode);
            set => SetPropertyValue(PropertyNames.ImeMode, value);
        }

        public string Interactivity
        {
            get => GetPropertyValue(PropertyNames.Interactivity);
            set => SetPropertyValue(PropertyNames.Interactivity, value);
        }

        public string Isolation
        {
            get => GetPropertyValue(PropertyNames.Isolation);
            set => SetPropertyValue(PropertyNames.Isolation, value);
        }

        public string Inset
        {
            get => GetPropertyValue(PropertyNames.Inset);
            set => SetPropertyValue(PropertyNames.Inset, value);
        }

        public string InsetBlock
        {
            get => GetPropertyValue(PropertyNames.InsetBlock);
            set => SetPropertyValue(PropertyNames.InsetBlock, value);
        }

        public string InsetBlockEnd
        {
            get => GetPropertyValue(PropertyNames.InsetBlockEnd);
            set => SetPropertyValue(PropertyNames.InsetBlockEnd, value);
        }

        public string InsetBlockStart
        {
            get => GetPropertyValue(PropertyNames.InsetBlockStart);
            set => SetPropertyValue(PropertyNames.InsetBlockStart, value);
        }

        public string InsetInline
        {
            get => GetPropertyValue(PropertyNames.InsetInline);
            set => SetPropertyValue(PropertyNames.InsetInline, value);
        }

        public string InsetInlineEnd
        {
            get => GetPropertyValue(PropertyNames.InsetInlineEnd);
            set => SetPropertyValue(PropertyNames.InsetInlineEnd, value);
        }

        public string InsetInlineStart
        {
            get => GetPropertyValue(PropertyNames.InsetInlineStart);
            set => SetPropertyValue(PropertyNames.InsetInlineStart, value);
        }

        public string InlineSize
        {
            get => GetPropertyValue(PropertyNames.InlineSize);
            set => SetPropertyValue(PropertyNames.InlineSize, value);
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

        public string LineBreak
        {
            get => GetPropertyValue(PropertyNames.LineBreak);
            set => SetPropertyValue(PropertyNames.LineBreak, value);
        }

        public string LightingColor
        {
            get => GetPropertyValue(PropertyNames.LightingColor);
            set => SetPropertyValue(PropertyNames.LightingColor, value);
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

        public string MarginBlock
        {
            get => GetPropertyValue(PropertyNames.MarginBlock);
            set => SetPropertyValue(PropertyNames.MarginBlock, value);
        }

        public string MarginBlockEnd
        {
            get => GetPropertyValue(PropertyNames.MarginBlockEnd);
            set => SetPropertyValue(PropertyNames.MarginBlockEnd, value);
        }

        public string MarginBlockStart
        {
            get => GetPropertyValue(PropertyNames.MarginBlockStart);
            set => SetPropertyValue(PropertyNames.MarginBlockStart, value);
        }

        public string MarginBottom
        {
            get => GetPropertyValue(PropertyNames.MarginBottom);
            set => SetPropertyValue(PropertyNames.MarginBottom, value);
        }

        public string MarginInline
        {
            get => GetPropertyValue(PropertyNames.MarginInline);
            set => SetPropertyValue(PropertyNames.MarginInline, value);
        }

        public string MarginInlineEnd
        {
            get => GetPropertyValue(PropertyNames.MarginInlineEnd);
            set => SetPropertyValue(PropertyNames.MarginInlineEnd, value);
        }

        public string MarginInlineStart
        {
            get => GetPropertyValue(PropertyNames.MarginInlineStart);
            set => SetPropertyValue(PropertyNames.MarginInlineStart, value);
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

        public string MathDepth
        {
            get => GetPropertyValue(PropertyNames.MathDepth);
            set => SetPropertyValue(PropertyNames.MathDepth, value);
        }

        public string MathShift
        {
            get => GetPropertyValue(PropertyNames.MathShift);
            set => SetPropertyValue(PropertyNames.MathShift, value);
        }

        public string MathStyle
        {
            get => GetPropertyValue(PropertyNames.MathStyle);
            set => SetPropertyValue(PropertyNames.MathStyle, value);
        }

        public string MaskClip
        {
            get => GetPropertyValue(PropertyNames.MaskClip);
            set => SetPropertyValue(PropertyNames.MaskClip, value);
        }

        public string MaskComposite
        {
            get => GetPropertyValue(PropertyNames.MaskComposite);
            set => SetPropertyValue(PropertyNames.MaskComposite, value);
        }

        public string MaskImage
        {
            get => GetPropertyValue(PropertyNames.MaskImage);
            set => SetPropertyValue(PropertyNames.MaskImage, value);
        }

        public string MaskMode
        {
            get => GetPropertyValue(PropertyNames.MaskMode);
            set => SetPropertyValue(PropertyNames.MaskMode, value);
        }

        public string MaskOrigin
        {
            get => GetPropertyValue(PropertyNames.MaskOrigin);
            set => SetPropertyValue(PropertyNames.MaskOrigin, value);
        }

        public string MaskPosition
        {
            get => GetPropertyValue(PropertyNames.MaskPosition);
            set => SetPropertyValue(PropertyNames.MaskPosition, value);
        }

        public string MaskRepeat
        {
            get => GetPropertyValue(PropertyNames.MaskRepeat);
            set => SetPropertyValue(PropertyNames.MaskRepeat, value);
        }

        public string MaskSize
        {
            get => GetPropertyValue(PropertyNames.MaskSize);
            set => SetPropertyValue(PropertyNames.MaskSize, value);
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

        public string MixBlendMode
        {
            get => GetPropertyValue(PropertyNames.MixBlendMode);
            set => SetPropertyValue(PropertyNames.MixBlendMode, value);
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

        public string OutlineOffset
        {
            get => GetPropertyValue(PropertyNames.OutlineOffset);
            set => SetPropertyValue(PropertyNames.OutlineOffset, value);
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

        public string OverflowClipMargin
        {
            get => GetPropertyValue(PropertyNames.OverflowClipMargin);
            set => SetPropertyValue(PropertyNames.OverflowClipMargin, value);
        }

        public string OverflowClipMarginTop
        {
            get => GetPropertyValue(PropertyNames.OverflowClipMarginTop);
            set => SetPropertyValue(PropertyNames.OverflowClipMarginTop, value);
        }

        public string OverflowClipMarginRight
        {
            get => GetPropertyValue(PropertyNames.OverflowClipMarginRight);
            set => SetPropertyValue(PropertyNames.OverflowClipMarginRight, value);
        }

        public string OverflowClipMarginBottom
        {
            get => GetPropertyValue(PropertyNames.OverflowClipMarginBottom);
            set => SetPropertyValue(PropertyNames.OverflowClipMarginBottom, value);
        }

        public string OverflowClipMarginLeft
        {
            get => GetPropertyValue(PropertyNames.OverflowClipMarginLeft);
            set => SetPropertyValue(PropertyNames.OverflowClipMarginLeft, value);
        }

        public string OverflowClipMarginBlock
        {
            get => GetPropertyValue(PropertyNames.OverflowClipMarginBlock);
            set => SetPropertyValue(PropertyNames.OverflowClipMarginBlock, value);
        }

        public string OverflowClipMarginBlockStart
        {
            get => GetPropertyValue(PropertyNames.OverflowClipMarginBlockStart);
            set => SetPropertyValue(PropertyNames.OverflowClipMarginBlockStart, value);
        }

        public string OverflowClipMarginBlockEnd
        {
            get => GetPropertyValue(PropertyNames.OverflowClipMarginBlockEnd);
            set => SetPropertyValue(PropertyNames.OverflowClipMarginBlockEnd, value);
        }

        public string OverflowClipMarginInline
        {
            get => GetPropertyValue(PropertyNames.OverflowClipMarginInline);
            set => SetPropertyValue(PropertyNames.OverflowClipMarginInline, value);
        }

        public string OverflowClipMarginInlineStart
        {
            get => GetPropertyValue(PropertyNames.OverflowClipMarginInlineStart);
            set => SetPropertyValue(PropertyNames.OverflowClipMarginInlineStart, value);
        }

        public string OverflowClipMarginInlineEnd
        {
            get => GetPropertyValue(PropertyNames.OverflowClipMarginInlineEnd);
            set => SetPropertyValue(PropertyNames.OverflowClipMarginInlineEnd, value);
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

        public string PaddingBlock
        {
            get => GetPropertyValue(PropertyNames.PaddingBlock);
            set => SetPropertyValue(PropertyNames.PaddingBlock, value);
        }

        public string PaddingBlockEnd
        {
            get => GetPropertyValue(PropertyNames.PaddingBlockEnd);
            set => SetPropertyValue(PropertyNames.PaddingBlockEnd, value);
        }

        public string PaddingBlockStart
        {
            get => GetPropertyValue(PropertyNames.PaddingBlockStart);
            set => SetPropertyValue(PropertyNames.PaddingBlockStart, value);
        }

        public string PaddingBottom
        {
            get => GetPropertyValue(PropertyNames.PaddingBottom);
            set => SetPropertyValue(PropertyNames.PaddingBottom, value);
        }

        public string PaddingInline
        {
            get => GetPropertyValue(PropertyNames.PaddingInline);
            set => SetPropertyValue(PropertyNames.PaddingInline, value);
        }

        public string PaddingInlineEnd
        {
            get => GetPropertyValue(PropertyNames.PaddingInlineEnd);
            set => SetPropertyValue(PropertyNames.PaddingInlineEnd, value);
        }

        public string PaddingInlineStart
        {
            get => GetPropertyValue(PropertyNames.PaddingInlineStart);
            set => SetPropertyValue(PropertyNames.PaddingInlineStart, value);
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

        public string PaintOrder
        {
            get => GetPropertyValue(PropertyNames.PaintOrder);
            set => SetPropertyValue(PropertyNames.PaintOrder, value);
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

        public string PrintColorAdjust
        {
            get => GetPropertyValue(PropertyNames.PrintColorAdjust);
            set => SetPropertyValue(PropertyNames.PrintColorAdjust, value);
        }

        public string RowGap
        {
            get => GetPropertyValue(PropertyNames.RowGap);
            set => SetPropertyValue(PropertyNames.RowGap, value);
        }

        public string OverscrollBehavior
        {
            get => GetPropertyValue(PropertyNames.OverscrollBehavior);
            set => SetPropertyValue(PropertyNames.OverscrollBehavior, value);
        }

        public string OverscrollBehaviorX
        {
            get => GetPropertyValue(PropertyNames.OverscrollBehaviorX);
            set => SetPropertyValue(PropertyNames.OverscrollBehaviorX, value);
        }

        public string OverscrollBehaviorY
        {
            get => GetPropertyValue(PropertyNames.OverscrollBehaviorY);
            set => SetPropertyValue(PropertyNames.OverscrollBehaviorY, value);
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

        public string R
        {
            get => GetPropertyValue(PropertyNames.R);
            set => SetPropertyValue(PropertyNames.R, value);
        }

        public string Rx
        {
            get => GetPropertyValue(PropertyNames.Rx);
            set => SetPropertyValue(PropertyNames.Rx, value);
        }

        public string Ry
        {
            get => GetPropertyValue(PropertyNames.Ry);
            set => SetPropertyValue(PropertyNames.Ry, value);
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

        public string ScrollBehavior
        {
            get => GetPropertyValue(PropertyNames.ScrollBehavior);
            set => SetPropertyValue(PropertyNames.ScrollBehavior, value);
        }

        public string ScrollMargin
        {
            get => GetPropertyValue(PropertyNames.ScrollMargin);
            set => SetPropertyValue(PropertyNames.ScrollMargin, value);
        }

        public string ScrollMarginTop
        {
            get => GetPropertyValue(PropertyNames.ScrollMarginTop);
            set => SetPropertyValue(PropertyNames.ScrollMarginTop, value);
        }

        public string ScrollMarginRight
        {
            get => GetPropertyValue(PropertyNames.ScrollMarginRight);
            set => SetPropertyValue(PropertyNames.ScrollMarginRight, value);
        }

        public string ScrollMarginBottom
        {
            get => GetPropertyValue(PropertyNames.ScrollMarginBottom);
            set => SetPropertyValue(PropertyNames.ScrollMarginBottom, value);
        }

        public string ScrollMarginLeft
        {
            get => GetPropertyValue(PropertyNames.ScrollMarginLeft);
            set => SetPropertyValue(PropertyNames.ScrollMarginLeft, value);
        }

        public string ScrollPadding
        {
            get => GetPropertyValue(PropertyNames.ScrollPadding);
            set => SetPropertyValue(PropertyNames.ScrollPadding, value);
        }

        public string ScrollPaddingTop
        {
            get => GetPropertyValue(PropertyNames.ScrollPaddingTop);
            set => SetPropertyValue(PropertyNames.ScrollPaddingTop, value);
        }

        public string ScrollPaddingRight
        {
            get => GetPropertyValue(PropertyNames.ScrollPaddingRight);
            set => SetPropertyValue(PropertyNames.ScrollPaddingRight, value);
        }

        public string ScrollPaddingBottom
        {
            get => GetPropertyValue(PropertyNames.ScrollPaddingBottom);
            set => SetPropertyValue(PropertyNames.ScrollPaddingBottom, value);
        }

        public string ScrollPaddingLeft
        {
            get => GetPropertyValue(PropertyNames.ScrollPaddingLeft);
            set => SetPropertyValue(PropertyNames.ScrollPaddingLeft, value);
        }

        public string ScrollSnapAlign
        {
            get => GetPropertyValue(PropertyNames.ScrollSnapAlign);
            set => SetPropertyValue(PropertyNames.ScrollSnapAlign, value);
        }

        public string ScrollSnapStop
        {
            get => GetPropertyValue(PropertyNames.ScrollSnapStop);
            set => SetPropertyValue(PropertyNames.ScrollSnapStop, value);
        }

        public string ScrollSnapType
        {
            get => GetPropertyValue(PropertyNames.ScrollSnapType);
            set => SetPropertyValue(PropertyNames.ScrollSnapType, value);
        }

        public string ShapeImageThreshold
        {
            get => GetPropertyValue(PropertyNames.ShapeImageThreshold);
            set => SetPropertyValue(PropertyNames.ShapeImageThreshold, value);
        }

        public string ShapeMargin
        {
            get => GetPropertyValue(PropertyNames.ShapeMargin);
            set => SetPropertyValue(PropertyNames.ShapeMargin, value);
        }

        public string ShapeOutside
        {
            get => GetPropertyValue(PropertyNames.ShapeOutside);
            set => SetPropertyValue(PropertyNames.ShapeOutside, value);
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

        public string ScrollbarColor
        {
            get => GetPropertyValue(PropertyNames.ScrollbarColor);
            set => SetPropertyValue(PropertyNames.ScrollbarColor, value);
        }

        public string ScrollbarGutter
        {
            get => GetPropertyValue(PropertyNames.ScrollbarGutter);
            set => SetPropertyValue(PropertyNames.ScrollbarGutter, value);
        }

        public string ScrollbarWidth
        {
            get => GetPropertyValue(PropertyNames.ScrollbarWidth);
            set => SetPropertyValue(PropertyNames.ScrollbarWidth, value);
        }

        public string ShapeRendering
        {
            get => GetPropertyValue(PropertyNames.ShapeRendering);
            set => SetPropertyValue(PropertyNames.ShapeRendering, value);
        }

        public string StopColor
        {
            get => GetPropertyValue(PropertyNames.StopColor);
            set => SetPropertyValue(PropertyNames.StopColor, value);
        }

        public string StopOpacity
        {
            get => GetPropertyValue(PropertyNames.StopOpacity);
            set => SetPropertyValue(PropertyNames.StopOpacity, value);
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

        public string TextAlignAll
        {
            get => GetPropertyValue(PropertyNames.TextAlignAll);
            set => SetPropertyValue(PropertyNames.TextAlignAll, value);
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

        public string TextCombineUpright
        {
            get => GetPropertyValue(PropertyNames.TextCombineUpright);
            set => SetPropertyValue(PropertyNames.TextCombineUpright, value);
        }

        public string TextDecoration
        {
            get => GetPropertyValue(PropertyNames.TextDecoration);
            set => SetPropertyValue(PropertyNames.TextDecoration, value);
        }

        public string TextDecorationInset
        {
            get => GetPropertyValue(PropertyNames.TextDecorationInset);
            set => SetPropertyValue(PropertyNames.TextDecorationInset, value);
        }

        public string TextDecorationSkip
        {
            get => GetPropertyValue(PropertyNames.TextDecorationSkip);
            set => SetPropertyValue(PropertyNames.TextDecorationSkip, value);
        }

        public string TextDecorationSkipInk
        {
            get => GetPropertyValue(PropertyNames.TextDecorationSkipInk);
            set => SetPropertyValue(PropertyNames.TextDecorationSkipInk, value);
        }

        public string TextDecorationThickness
        {
            get => GetPropertyValue(PropertyNames.TextDecorationThickness);
            set => SetPropertyValue(PropertyNames.TextDecorationThickness, value);
        }

        public string TextEmphasis
        {
            get => GetPropertyValue(PropertyNames.TextEmphasis);
            set => SetPropertyValue(PropertyNames.TextEmphasis, value);
        }

        public string TextEmphasisColor
        {
            get => GetPropertyValue(PropertyNames.TextEmphasisColor);
            set => SetPropertyValue(PropertyNames.TextEmphasisColor, value);
        }

        public string TextEmphasisPosition
        {
            get => GetPropertyValue(PropertyNames.TextEmphasisPosition);
            set => SetPropertyValue(PropertyNames.TextEmphasisPosition, value);
        }

        public string TextEmphasisStyle
        {
            get => GetPropertyValue(PropertyNames.TextEmphasisStyle);
            set => SetPropertyValue(PropertyNames.TextEmphasisStyle, value);
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

        public string TextOrientation
        {
            get => GetPropertyValue(PropertyNames.TextOrientation);
            set => SetPropertyValue(PropertyNames.TextOrientation, value);
        }

        public string TextOverflow
        {
            get => GetPropertyValue(PropertyNames.TextOverflow);
            set => SetPropertyValue(PropertyNames.TextOverflow, value);
        }

        public string TextRendering
        {
            get => GetPropertyValue(PropertyNames.TextRendering);
            set => SetPropertyValue(PropertyNames.TextRendering, value);
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

        public string TextWrap
        {
            get => GetPropertyValue(PropertyNames.TextWrap);
            set => SetPropertyValue(PropertyNames.TextWrap, value);
        }

        public string TextWrapMode
        {
            get => GetPropertyValue(PropertyNames.TextWrapMode);
            set => SetPropertyValue(PropertyNames.TextWrapMode, value);
        }

        public string TextWrapStyle
        {
            get => GetPropertyValue(PropertyNames.TextWrapStyle);
            set => SetPropertyValue(PropertyNames.TextWrapStyle, value);
        }

        public string TextUnderlineOffset
        {
            get => GetPropertyValue(PropertyNames.TextUnderlineOffset);
            set => SetPropertyValue(PropertyNames.TextUnderlineOffset, value);
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

        public string TouchAction
        {
            get => GetPropertyValue(PropertyNames.TouchAction);
            set => SetPropertyValue(PropertyNames.TouchAction, value);
        }

        public string Transform
        {
            get => GetPropertyValue(PropertyNames.Transform);
            set => SetPropertyValue(PropertyNames.Transform, value);
        }

        public string TransformBox
        {
            get => GetPropertyValue(PropertyNames.TransformBox);
            set => SetPropertyValue(PropertyNames.TransformBox, value);
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

        public string Translate
        {
            get => GetPropertyValue(PropertyNames.Translate);
            set => SetPropertyValue(PropertyNames.Translate, value);
        }

        public string Rotate
        {
            get => GetPropertyValue(PropertyNames.Rotate);
            set => SetPropertyValue(PropertyNames.Rotate, value);
        }

        public string Scale
        {
            get => GetPropertyValue(PropertyNames.Scale);
            set => SetPropertyValue(PropertyNames.Scale, value);
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

        public string UserSelect
        {
            get => GetPropertyValue(PropertyNames.UserSelect);
            set => SetPropertyValue(PropertyNames.UserSelect, value);
        }

        public string VectorEffect
        {
            get => GetPropertyValue(PropertyNames.VectorEffect);
            set => SetPropertyValue(PropertyNames.VectorEffect, value);
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

        public string WhiteSpaceCollapse
        {
            get => GetPropertyValue(PropertyNames.WhiteSpaceCollapse);
            set => SetPropertyValue(PropertyNames.WhiteSpaceCollapse, value);
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

        public string X
        {
            get => GetPropertyValue(PropertyNames.X);
            set => SetPropertyValue(PropertyNames.X, value);
        }

        public string Y
        {
            get => GetPropertyValue(PropertyNames.Y);
            set => SetPropertyValue(PropertyNames.Y, value);
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

        public string Bleed
        {
            get => GetPropertyValue(PropertyNames.Bleed);
            set => SetPropertyValue(PropertyNames.Bleed, value);
        }

        public string Marks
        {
            get => GetPropertyValue(PropertyNames.Marks);
            set => SetPropertyValue(PropertyNames.Marks, value);
        }

        public string BookmarkLabel
        {
            get => GetPropertyValue(PropertyNames.BookmarkLabel);
            set => SetPropertyValue(PropertyNames.BookmarkLabel, value);
        }

        public string BookmarkLevel
        {
            get => GetPropertyValue(PropertyNames.BookmarkLevel);
            set => SetPropertyValue(PropertyNames.BookmarkLevel, value);
        }

        public string BookmarkState
        {
            get => GetPropertyValue(PropertyNames.BookmarkState);
            set => SetPropertyValue(PropertyNames.BookmarkState, value);
        }

        public string BookmarkTarget
        {
            get => GetPropertyValue(PropertyNames.BookmarkTarget);
            set => SetPropertyValue(PropertyNames.BookmarkTarget, value);
        }

        public string CounterSet
        {
            get => GetPropertyValue(PropertyNames.CounterSet);
            set => SetPropertyValue(PropertyNames.CounterSet, value);
        }

        public string PageName
        {
            get => GetPropertyValue(PropertyNames.PageName);
            set => SetPropertyValue(PropertyNames.PageName, value);
        }
    }
}
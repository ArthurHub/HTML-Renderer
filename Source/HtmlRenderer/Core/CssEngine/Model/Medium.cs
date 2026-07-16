using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class Medium : StylesheetNode
    {
        public IEnumerable<MediaFeature> Features => Children.OfType<MediaFeature>();
        public string Type { get; internal set; }
        public bool IsExclusive { get; internal set; }
        public bool IsInverse { get; internal set; }

        public string Constraints
        {
            get
            {
                var constraints = Features.Select(m => m.ToCss());
                return string.Join(" and ", constraints);
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is Medium other &&
                other.IsExclusive == IsExclusive &&
                other.IsInverse == IsInverse &&
                other.Type.Is(Type) &&
                other.Features.Count() == Features.Count())
            {
                return other.Features.Select(feature =>
                    Features.Any(m => m.Name.Is(feature.Name) && m.Value.Is(feature.Value))).All(isShared => isShared);
            }

            return false;
        }

        public override int GetHashCode()
        {
            // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
            return base.GetHashCode();
        }

        public override void ToCss(TextWriter writer, IStyleFormatter formatter)
        {
            writer.Write(formatter.Medium(IsExclusive, IsInverse, Type, Features));
        }
    }
}
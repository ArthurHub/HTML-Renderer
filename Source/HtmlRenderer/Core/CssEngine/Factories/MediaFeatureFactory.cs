using System;
using System.Collections.Generic;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class MediaFeatureFactory
    {
        private static readonly Lazy<MediaFeatureFactory> Lazy = new Lazy<MediaFeatureFactory>(() => new MediaFeatureFactory());

        #region Creators

        private readonly Dictionary<string, Creator> _creators =
            new Dictionary<string, Creator>(StringComparer.OrdinalIgnoreCase)
            {
                {FeatureNames.MinWidth, () => new WidthMediaFeature(FeatureNames.MinWidth)},
                {FeatureNames.MaxWidth, () => new WidthMediaFeature(FeatureNames.MaxWidth)},
                {FeatureNames.Width, () => new WidthMediaFeature(FeatureNames.Width)},
                {FeatureNames.MinHeight, () => new HeightMediaFeature(FeatureNames.MinHeight)},
                {FeatureNames.MaxHeight, () => new HeightMediaFeature(FeatureNames.MaxHeight)},
                {FeatureNames.Height, () => new HeightMediaFeature(FeatureNames.Height)},
                {FeatureNames.MinDeviceWidth, () => new DeviceWidthMediaFeature(FeatureNames.MinDeviceWidth)},
                {FeatureNames.MaxDeviceWidth, () => new DeviceWidthMediaFeature(FeatureNames.MaxDeviceWidth)},
                {FeatureNames.DeviceWidth, () => new DeviceWidthMediaFeature(FeatureNames.DeviceWidth)},
                {FeatureNames.MinDevicePixelRatio, () => new DevicePixelRatioFeature(FeatureNames.MinDevicePixelRatio)},
                {FeatureNames.MaxDevicePixelRatio, () => new DevicePixelRatioFeature(FeatureNames.MaxDevicePixelRatio)},
                {FeatureNames.DevicePixelRatio, () => new DevicePixelRatioFeature(FeatureNames.DevicePixelRatio)},
                {FeatureNames.MinDeviceHeight, () => new DeviceHeightMediaFeature(FeatureNames.MinDeviceHeight)},
                {FeatureNames.MaxDeviceHeight, () => new DeviceHeightMediaFeature(FeatureNames.MaxDeviceHeight)},
                {FeatureNames.DeviceHeight, () => new DeviceHeightMediaFeature(FeatureNames.DeviceHeight)},
                {FeatureNames.MinAspectRatio, () => new AspectRatioMediaFeature(FeatureNames.MinAspectRatio)},
                {FeatureNames.MaxAspectRatio, () => new AspectRatioMediaFeature(FeatureNames.MaxAspectRatio)},
                {FeatureNames.AspectRatio, () => new AspectRatioMediaFeature(FeatureNames.AspectRatio)},
                {
                    FeatureNames.MinDeviceAspectRatio,
                    () => new DeviceAspectRatioMediaFeature(FeatureNames.MinDeviceAspectRatio)
                },
                {
                    FeatureNames.MaxDeviceAspectRatio,
                    () => new DeviceAspectRatioMediaFeature(FeatureNames.MaxDeviceAspectRatio)
                },
                {
                    FeatureNames.DeviceAspectRatio,
                    () => new DeviceAspectRatioMediaFeature(FeatureNames.DeviceAspectRatio)
                },
                {FeatureNames.MinColor, () => new ColorMediaFeature(FeatureNames.MinColor)},
                {FeatureNames.MaxColor, () => new ColorMediaFeature(FeatureNames.MaxColor)},
                {FeatureNames.Color, () => new ColorMediaFeature(FeatureNames.Color)},
                {FeatureNames.MinColorIndex, () => new ColorIndexMediaFeature(FeatureNames.MinColorIndex)},
                {FeatureNames.MaxColorIndex, () => new ColorIndexMediaFeature(FeatureNames.MaxColorIndex)},
                {FeatureNames.ColorIndex, () => new ColorIndexMediaFeature(FeatureNames.ColorIndex)},
                {FeatureNames.MinMonochrome, () => new MonochromeMediaFeature(FeatureNames.MinMonochrome)},
                {FeatureNames.MaxMonochrome, () => new MonochromeMediaFeature(FeatureNames.MaxMonochrome)},
                {FeatureNames.Monochrome, () => new MonochromeMediaFeature(FeatureNames.Monochrome)},
                {FeatureNames.MinResolution, () => new ResolutionMediaFeature(FeatureNames.MinResolution)},
                {FeatureNames.MaxResolution, () => new ResolutionMediaFeature(FeatureNames.MaxResolution)},
                {FeatureNames.Resolution, () => new ResolutionMediaFeature(FeatureNames.Resolution)},
                {FeatureNames.Orientation, () => new OrientationMediaFeature()},
                {FeatureNames.Grid, () => new GridMediaFeature()},
                {FeatureNames.Scan, () => new ScanMediaFeature()},
                {FeatureNames.UpdateFrequency, () => new UpdateFrequencyMediaFeature()},
                {FeatureNames.Scripting, () => new ScriptingMediaFeature()},
                {FeatureNames.Pointer, () => new PointerMediaFeature()},
                {FeatureNames.Hover, () => new HoverMediaFeature()},
                {FeatureNames.InlineSize, () => new SizeMediaFeature(FeatureNames.InlineSize)},
                {FeatureNames.BlockSize, () => new SizeMediaFeature(FeatureNames.BlockSize)},
            };

        #endregion

        private MediaFeatureFactory()
        {
        }

        internal static MediaFeatureFactory Instance => Lazy.Value;

        public MediaFeature Create(string name)
        {
            return _creators.TryGetValue(name, out var creator)
                ? creator()
                : default;
        }

        private delegate MediaFeature Creator();
    }
}
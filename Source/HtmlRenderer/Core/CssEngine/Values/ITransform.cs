namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal interface ITransform
    {
        TransformMatrix ComputeMatrix();
    }
}
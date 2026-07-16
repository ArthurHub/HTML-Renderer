using System.Collections.Generic;
using System.IO;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class KeyframeSelector : StylesheetNode
    {
        private readonly List<Percent> _stops;

        public KeyframeSelector(IEnumerable<Percent> stops)
        {
            _stops = new List<Percent>(stops);
        }

        public override void ToCss(TextWriter writer, IStyleFormatter formatter)
        {
            if (_stops.Count <= 0) return;

            writer.Write(_stops[0].ToString());
            for (var i = 1; i < _stops.Count; i++)
            {
                writer.Write(", ");
                writer.Write(_stops[i].ToString());
            }
        }

        public IEnumerable<Percent> Stops => _stops;
        public string Text => this.ToCss();
    }

    /// <summary>
    /// One comma-separated entry of an <c>@page</c> selector list, e.g. the two entries of
    /// <c>@page chapter1:left, chapter2:left</c> are <c>(Name: "chapter1", Pseudo: "left")</c> and
    /// <c>(Name: "chapter2", Pseudo: "left")</c>. Either part may be absent: <c>@page chapter</c> has
    /// <c>Pseudo: null</c>; <c>@page :first</c> has <c>Name: null</c>.
    /// </summary>
    internal readonly struct PageSelectorEntry
    {
        public PageSelectorEntry(string name, string pseudo)
        {
            Name = name;
            Pseudo = pseudo;
        }

        public string Name { get; }
        public string Pseudo { get; }
    }

    internal sealed class PageSelector : StylesheetNode, ISelector
    {
        private readonly List<PageSelectorEntry> _entries;

        public PageSelector(IEnumerable<PageSelectorEntry> entries)
        {
            _entries = new List<PageSelectorEntry>(entries);
        }

        public IReadOnlyList<PageSelectorEntry> Entries => _entries;

        public override void ToCss(TextWriter writer, IStyleFormatter formatter)
        {
            for (var i = 0; i < _entries.Count; i++)
            {
                if (i > 0) writer.Write(", ");

                var entry = _entries[i];
                if (entry.Name != null) writer.Write(entry.Name);
                if (entry.Pseudo != null) writer.Write($":{entry.Pseudo}");
            }
        }

        public Priority Specificity => Priority.Inline;
        public string Text => this.ToCss();
    }
}
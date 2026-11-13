using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal abstract class Selectors : StylesheetNode, IEnumerable<ISelector>
    {
        protected readonly List<ISelector> _selectors;

        protected Selectors()
        {
            _selectors = new List<ISelector>();
        }

        // A CompoundSelector's specificity is the sum of its members - they all constrain the same
        // element. A ListSelector overrides this, because a comma-separated list's specificity is not a
        // sum (see ListSelector).
        public virtual Priority Specificity
        {
            get
            {
                var sum = new Priority();

                return _selectors.Aggregate(sum, (current, t) => current + t.Specificity);
            }
        }

        public string Text => this.ToCss();
        public int Length => _selectors.Count;

        public ISelector this[int index]
        {
            get => _selectors[index];
            set => _selectors[index] = value;
        }

        public void Add(ISelector selector)
        {
            _selectors.Add(selector);
        }

        public void Remove(ISelector selector)
        {
            _selectors.Remove(selector);
        }

        public IEnumerator<ISelector> GetEnumerator()
        {
            return _selectors.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TheArtOfDev.HtmlRenderer.Core.CssEngine
{
    internal sealed class RuleList : IRuleList
    {
        private readonly StylesheetNode _parent;

        internal RuleList(StylesheetNode parent)
        {
            _parent = parent;
        }

        public Rule this[int index] => Nodes.GetItemByIndex(index);
        IRule IRuleList.this[int index] => this[index];
        public bool HasDeclarativeRules => Nodes.Any(IsDeclarativeRule);
        public IEnumerable<Rule> Nodes => _parent.Children.OfType<Rule>();
        public int Length => Nodes.Count();

        private static bool IsDeclarativeRule(Rule rule)
        {
            var type = rule.Type;
            return type != RuleType.Import && type != RuleType.Charset && type != RuleType.Namespace;
        }

        internal void RemoveAt(int index)
        {
            if (index < 0 || index >= Length) throw new ParseException("Invalid index");

            var rule = this[index];

            if (rule.Type == RuleType.Namespace && HasDeclarativeRules)
                throw new ParseException("Cannot remove namespace or declarative rules");

            Remove(rule);
        }

        internal void Remove(Rule rule)
        {
            if (rule != null)
                _parent.RemoveChild(rule);
        }

        internal void Insert(int index, Rule rule)
        {
            if (rule == null) throw new ParseException("Rule argument cannot be null");

            if (rule.Type == RuleType.Charset) throw new ParseException("Cannot insert Charset rule");

            if (index > Length || index < 0) throw new ParseException("Invalid index");

            if (rule.Type == RuleType.Namespace && HasDeclarativeRules)
                throw new ParseException("Cannot insert namespace or declarative rules");

            if (index == Length)
                _parent.AppendChild(rule);
            else
                _parent.InsertBefore(this[index], rule);
        }

        internal void Add(Rule rule)
        {
            if (rule != null) _parent.AppendChild(rule);
        }

        public IEnumerator<IRule> GetEnumerator()
        {
            return Nodes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
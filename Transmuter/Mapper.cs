using System;
using System.Collections.Generic;
using OneOf;

namespace Transmuter
{
    public class Mapper<TOut>
    {
        public delegate RuleOutput ToRule(object target);
        public class RuleOutput  
        {
            private RuleOutput() { }

            public OneOf<TOut, Intermediate, NA> Value { get; private set; }

            public static implicit operator RuleOutput(TOut x) => new RuleOutput(){ Value = x };
            public static implicit operator RuleOutput(NA x) => new RuleOutput() { Value = x };
            public static implicit operator RuleOutput(Intermediate x) => new RuleOutput() { Value = x };
        }

        public IList<(string name, ToRule rule)> Rules { get; } = new List<(string name, ToRule rule)>();

        public OneOf<TOut, NotMappable> Map(object target)
        {
            return DoMap(target).Match(value => (OneOf<TOut, NotMappable>) value, na => new NotMappable());
            OneOf<TOut, NA> DoMap(object target2, int depth = 0)
            {
                if (depth > MaxDepth) throw new Exception("MaxDepth detected");
                foreach (var rule in Rules)
                {
                    var ruleResult = rule.rule(target2).Value;
                    if (ruleResult.TryPickT0(out TOut mv, out var objectOrNa))
                        return mv;
                    if (objectOrNa.TryPickT0(out Intermediate obj, out NA notApplicable))
                    {
                        return DoMap(obj.Value, ++depth);
                    }
                }

                return new NA();
            }
        }

        public int MaxDepth { get; set; } = 10;
    }
}
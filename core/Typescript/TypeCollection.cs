using System.Collections.Generic;
using OneOf;

namespace Glow.Core.Typescript
{
    public class TypeCollection
    {
        public Dictionary<string, TsType> Types { get; set; }
        public Dictionary<string, TsEnum> Enums { get; set; }
        public IEnumerable<OneOf<TsType,TsEnum>> All()
        {
            var result = new List<OneOf<TsType, TsEnum>>();
            foreach (TsType v in Types.Values)
            {
                result.Add(v);
            }
            foreach (TsEnum v in Enums.Values)
            {
                result.Add(v);
            }
            return result;
        }
    }
}

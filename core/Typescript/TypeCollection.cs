using System.Collections.Generic;

namespace Glow.Core.Typescript
{
    public class TypeCollection
    {
        public Dictionary<string, TsType> Types { get; set; }
        public Dictionary<string, TsEnum> Enums { get; set; }
    }
}

using System.Collections.Generic;

namespace Glow.Core.Typescript
{
    public class TsEnum
    {
        public string Id
        {
            get
            {
                return FullName;
            }
        }

        public string FullName { get; set; }
        public string Name { get; set; }
        public IEnumerable<string> Values { get; set; }
        public string DefaultValue { get; set; }
    }
}

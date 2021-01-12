using System;
using System.Collections.Generic;
using System.Reflection;
using OneOf;

namespace Glow.Core.Typescript
{

    public class TsType
    {
        //public string Id
        //{
        //    get
        //    {
        //        return FullName;
        //    }
        //}
        public string Id { get; set; }
        public bool IsPrimitive { get; set; }
        public string FullName { get; set; }
        public string Name { get; set; }
        public string Namespace { get; set; }
        public Type Type { get; set; }
        public bool IsCollection { get; set; }

        public string DefaultValue { get; set; }

        public List<Property> Properties { get; set; }
        public PropertyInfo[] PropertyInfos { get; set; }
    }

    public class Property
    {
        public string PropertyName { get; set; }
        public string TypeName { get; set; }
        public string DefaultValue { get; set; }
        public OneOf<TsType, TsEnum> TsType { get; set; }
    }
}

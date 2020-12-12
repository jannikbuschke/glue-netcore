using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Glow.TypeScript;

namespace Glow.Core.Typescript
{
    public class TypeBuilder
    {
        private readonly IList<Type> types = new List<Type>();
        private readonly Dictionary<string, TsType> tsTypes = new Dictionary<string, TsType>();

        public void Add<T>()
        {
            types.Add(typeof(T));
        }

        public void Add(Type type)
        {
            types.Add(type);
        }

        public Dictionary<string, TsType> Generate()
        {
            foreach (Type type in types)
            {
                TsType result = CreateOrGet(type);
            }
            return tsTypes;
        }

        private TsType CreateOrGet(Type type)
        {
            if (!tsTypes.ContainsKey(type.FullName))
            {
                tsTypes.Add(type.FullName, Create(type));
            }
            return tsTypes[type.FullName];
        }

        private TsType Create(Type type)
        {
            var value = new TsType
            {
                FullName = type.FullName,
                Name = type.Name,
                Namespace = type.Namespace,
                Type = type,
                Properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .ToDictionary(v => v.Name.CamelCase(), v => CreateOrGet(v.PropertyType))
            };
            return value;

            //type.GetProperties(BindingFlags.Public | BindingFlags.Instance).ForEach(v =>
            //{
            //    builder.AppendLine($"  {v.Name.CamelCase()}: {v.PropertyType.ToTsType()}");
            //});

            //foreach (PropertyInfo v in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            //{
            //    //var value = v.PropertyType.DefaultValue();// v.PropertyType.IsValueType ? Activator.CreateInstance(v.PropertyType) : "null";


            //    //builder.AppendLine($"  {v.Name.CamelCase()}: {value ?? "null"},");
            //}
        }
    }

    public class TsType
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
        public string Namespace { get; set; }
        public Type Type { get; set; }

        public string DefaultValue { get; set; }

        public IEnumerable<TsType> Dependencies
        {
            get { return Properties.Values; }
        }

        public Dictionary<string, TsType> Properties { get; set; }
    }
}

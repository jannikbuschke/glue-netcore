using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Glow.TypeScript;
using OneOf;

namespace Glow.Core.Typescript
{
    public class TypeCollectionBuilder
    {
        private readonly IList<Type> types = new List<Type>();
        private readonly Dictionary<string, TsType> tsTypes = new Dictionary<string, TsType>();
        private readonly Dictionary<string, TsEnum> tsEnums = new Dictionary<string, TsEnum>();

        public void Add<T>()
        {
            types.Add(typeof(T));
        }

        public void Add(Type type)
        {
            types.Add(type);
        }

        public void AddRange(IEnumerable<Type> types)
        {
            foreach (Type v in types)
            {
                Add(v);
            }
        }

        public TypeCollection Generate()
        {
            foreach (Type type in types)
            {
                OneOf<TsType, TsEnum> result = CreateOrGet(type);
            }
            return new TypeCollection {
                Types = tsTypes,
                Enums = tsEnums
            };
        }

        private OneOf<TsType, TsEnum> CreateOrGet(Type type, bool skipDependencies = false)
        {
            if (type.IsEnum)
            {
                TsEnum e = AsEnum(type);
                tsEnums.TryAdd(e.Id, e);
                Console.WriteLine("add enum " + e.Name);
                return e;
            }
            else
            {
                if (IsPrimitive(type))
                {
                    TsType valueType = GetPrimitive(type);
                    if (valueType != null)
                    {
                        return valueType;
                    }
                }

                //if (IsDictionary(type))
                //{
                //    return AsDictionary(type);
                //}

                if (IsEnumerableType(type))
                {
                    // to early?
                    return AsEnumerable(type);
                }

                var id = type.FullName ?? (type.IsGenericParameter ? "T" : null);
                if (!tsTypes.ContainsKey(id))
                {
                    TsType tsType = Create(type, skipDependencies);
                    tsType.Id = id;
                    tsTypes.Add(id, tsType);
                    PopuplateProperties(tsType);
                }
                return tsTypes[id];
            }
        }

        private TsType AsEnumerable(Type type)
        {
            Type[] args = type.GenericTypeArguments;
            TsType argTsType = type.IsArray? CreateOrGet(type.GetElementType()).AsT0:  CreateOrGet(args.First()).AsT0;
            return new TsType
            {
                Name = argTsType.Name+"[]",
                DefaultValue = "[]",
                Properties = new List<Property>()
            };
 
        }

        private Tuple<string, string> AsDictionary(Type type)
        {
            TsType keyTsType = GetPrimitive(type.GenericTypeArguments[0]);
            
            return new Tuple<string, string>
            (
                $"{{ [key: {keyTsType.Name}]: {CreateOrGet(type).AsT0.Name} }}",
                "{}"
            );
        }

        private bool IsDictionary(Type t)
        {
            var isDict = t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Dictionary<,>);
            return isDict;
        }

        private bool IsEnumerableType(Type type)
        {
            return (type.GetInterface(nameof(IEnumerable)) != null);
        }

        private bool IsPrimitive(Type type)
        {
            return primitives.ContainsKey(type);
        }

        private TsEnum AsEnum(Type t)
        {
            IEnumerable<string> values = GetEnumValues(t);
            return new TsEnum
            {
                Name = t.Name,
                FullName = t.FullName,
                DefaultValue = values.First(),
                Values = values
            };
        }

        private static IEnumerable<string> GetEnumValues(Type t)
        {
            Array values = Enum.GetValues(t);
            foreach (var val in values)
            {
                yield return Enum.GetName(t, val);
            }
        }

        private TsType GetPrimitive(Type type)
        {
            if (primitives.ContainsKey(type))
            {
                (var name, var defaultValue) = primitives[type];
                return new TsType
                {
                    Name = name,
                    DefaultValue = defaultValue,
                    IsPrimitive = true,
                    Properties = new List<Property>()
                };
            }
            if (IsDictionary(type))
            {
                (var name, var defaultValue) = primitives[type];
                return new TsType
                {
                    Name = name,
                    DefaultValue = defaultValue,
                    IsPrimitive = true,
                    Properties = new List<Property>()
                };
            }
            return null;
        }

        private readonly Dictionary<Type, Tuple<string, string>> primitives = new Dictionary<Type, Tuple<string, string>>
        {
            { typeof(string), new Tuple<string, string>("string | null", "null") },
            { typeof(double), new Tuple<string, string>("number", "0") },
            { typeof(double?), new Tuple<string, string>("number | null", "null") },
            { typeof(float), new Tuple<string, string>("number", "0") },
            { typeof(float?), new Tuple<string, string>("number | null", "null") },
            { typeof(int), new Tuple<string, string>("number","0") },
            { typeof(int?), new Tuple<string, string>("number | null", "null") },
            { typeof(decimal), new Tuple<string, string>("number", "0") },
            { typeof(decimal?), new Tuple<string, string>("number | null", "null") },
            { typeof(DateTime), new Tuple<string, string>("string", @"""1/1/0001 12:00:00 AM""") },
            { typeof(DateTime?), new Tuple<string, string>("string | null", "null") },
            { typeof(Guid), new Tuple<string, string>("string", @"""00000000-0000-0000-0000-000000000000""") },
            { typeof(Guid?),new Tuple<string, string>( "string | null", "null") },
            { typeof(bool),new Tuple<string, string>( "boolean", "false") },
            { typeof(bool?), new Tuple<string, string>("boolean | null", "null") },
            { typeof(Dictionary<string, string>), new Tuple<string, string>("{ [key: string]: string }", "{}") },
            { typeof(Dictionary<string, int>), new Tuple<string, string>("{ [key: string]: number }", "{}") },
            { typeof(Dictionary<string, object>), new Tuple<string, string>("{ [key: string]: any }", "{}") },
            { typeof(object), new Tuple<string, string>("any", "null") }
        };

        private void PopuplateProperties(TsType type)
        {
            type.Properties = type.PropertyInfos
                .Select(v =>
                {
                    // problem, current type does not yet exist on dependency
                    OneOf<TsType, TsEnum> tsType = CreateOrGet(v.PropertyType, skipDependencies: true);
                    var defaultValue = tsType.Match(v1 => v1.DefaultValue, v2 => $@"""{v2.DefaultValue}""");
                    var typeName = tsType.Match(v1 => v1.Name, v2 => v2.Name);
                    return new Property
                    {
                        PropertyName = v.Name.CamelCase(),
                        DefaultValue = defaultValue,
                        TsType = tsType.IsT0 ? tsType.AsT0 : null,
                        TypeName = typeName,
                    };
                }).ToList();
        }

        private TsType Create(Type type, bool skipDependencies = false)
        {
            if (type.Name.StartsWith("QueryResu"))
            {
                Type[] params1 = type.GetGenericArguments();
                Type[] args = type.GenericTypeArguments;
                var hasParams = type.ContainsGenericParameters;
            }

            //type.GenericTypeParameters

            if (type.IsGenericParameter)
            {
                return new TsType
                {
                    FullName = type.FullName??type.Name?? "T",
                    DefaultValue = "{}",
                    IsPrimitive = false,
                    Name = type.Name?? "T",
                    Type = type,
                    Properties = new List<Property>()
                };
            }
            Type[] genericTypeArguments = type.GenericTypeArguments;
            Type[] genericArguments = type.GetGenericArguments();

            var name = genericTypeArguments.Length != 0
                ? type.Name.Replace(".","").Replace("`","") + string.Join("", genericTypeArguments.Select(v => v.Name))
                : genericArguments.Length != 0
                ? Regex.Replace(type.Name, "`.*$", "<" + string.Join(", ", genericArguments.Select(v => v.Name)) + ">")
                : type.Name;

            PropertyInfo[] props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            var value = new TsType
            {
                FullName = type.FullName,
                Name = name,
                Namespace = type.Namespace,
                DefaultValue = genericArguments.Length != 0 ? null : "default" + name,
                Type = type,
                PropertyInfos = props,
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
}
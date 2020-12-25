using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Glow.TypeScript;
using OneOf;

namespace Glow.Core.Typescript
{
    public class TypeCollection
    {
        public Dictionary<string, TsType> Types { get; set; }
        public Dictionary<string, TsEnum> Enums { get; set; }
    }

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

        private OneOf<TsType,TsEnum> CreateOrGet(Type type)
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
                    return AsEnumerable(type);
                }

                var id = type.FullName ?? (type.IsGenericParameter ? "T" : null);
                if (!tsTypes.ContainsKey(id))
                {
                    TsType tsType = Create(type);
                    tsType.Id = id;
                    tsTypes.Add(id, tsType);
                }
                return tsTypes[id];
            }
        }

        private TsType AsEnumerable(Type type)
        {
            Type[] args = type.GenericTypeArguments;
            TsType argTsType = CreateOrGet(args.First()).AsT0;
            return new TsType
            {
                Name = argTsType.Name+"[]",
                DefaultValue = "[]",
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
                    IsPrimitive = true
                };
            }
            if (IsDictionary(type))
            {
                (var name, var defaultValue) = primitives[type];
                return new TsType
                {
                    Name = name,
                    DefaultValue = defaultValue,
                    IsPrimitive = true
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

        private TsType Create(Type type)
        {
            if (type.GenericTypeArguments.Length != 0)
            {

            }

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
                Properties = props
                    // AsT0
                    .Select(v =>
                    {
                        OneOf<TsType, TsEnum> tsType = CreateOrGet(v.PropertyType);
                        var defaultValue = tsType.Match(v1 => v1.DefaultValue, v2 => $@"""{v2.DefaultValue}""");
                        var typeName = tsType.Match(v1 => v1.Name, v2 => v2.Name);
                        return new Property {
                            PropertyName = v.Name.CamelCase(),
                            DefaultValue = defaultValue,
                            TsType =tsType.IsT0 ? tsType.AsT0 : null,
                            TypeName = typeName,
                        };
                    }).ToList()
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

        public string DefaultValue { get; set; }

        public IEnumerable<TsType> Dependencies
        {
            get { return Properties.Where(v => v.TsType != null).Select(v => v.TsType); }
        }

        //public Dictionary<string, TsType> Properties { get; set; }
        public List<Property> Properties { get; set; }
    }

    public class Property
    {
        public string PropertyName { get; set; }
        public string TypeName { get; set; }
        public string DefaultValue { get; set; }
        public TsType TsType { get; set; }
    }

    public static class Render
    {
        public static void ToDisk(TypeCollection types, string path)
        {
            var builder = new StringBuilder();

            builder.AppendLine($"/* this file is autogenerated. Do not edit. */");
            builder.AppendLine("");

            foreach ((var key, TsEnum tsEnum) in types.Enums)
            {
                RenderTsEnum(tsEnum, builder);
            }

            foreach ((var key, TsType tsType) in types.Types)
            {
                RenderTsType(tsType, builder);
            }

            //builder.AppendLine("export declare module Entities {");

            //builder.AppendLine(entities.ToString());

            //builder.AppendLine($"  export type All = {string.Join(" | ", allEntityNames)}");
            //builder.AppendLine("}");
            builder.Insert(0, "\r\n");

            //builder.Insert(0, "/* eslint-disable prettier/prettier */");
            var text = builder.ToString();
            File.WriteAllText(path, text);
        }

        private static void RenderTsEnum(TsEnum type, StringBuilder builder)
        {
            var name = type.Name;
            builder.AppendLine($"export type {name} = {string.Join(" | ", type.Values.Select(v=>$@"""{v}"""))}");
            builder.AppendLine($@"export const default{name} = ""{type.DefaultValue}""");
            builder.AppendLine("");
        }

        private static void RenderTsType(TsType type, StringBuilder builder)
        {
            //var name = type.GenericTypeArguments.Length == 0
            //    ? type.Name.Replace(".", "")
            //    : type.GenericTypeArguments.Length == 1
            //        ? type.Name
            //            .Replace("`1", type.GenericTypeArguments[0].FullName)
            //            .Replace(".", "")
            //        : type.Name
            //            .Replace("`2", type.GenericTypeArguments[0].FullName + type.GenericTypeArguments[1].FullName)
            //            .Replace(".", "");
            //var fullName = type.FullName.Replace(".", "");

            var name = type.Name;
            builder.AppendLine($"export interface {name} {{");
            foreach (Property v in type.Properties)
            {
                builder.AppendLine($"  {v.PropertyName}: {v.TypeName}");
            }

            builder.AppendLine("}");
            builder.AppendLine("");

            if (type.DefaultValue != null)
            {
                builder.AppendLine($"export const default{name}: {name} = {{");
                foreach (Property property in type.Properties)
                {
                    builder.AppendLine($"  {property.PropertyName}: {property.DefaultValue ?? "null"},");
                }
                builder.AppendLine("}");
            }
            builder.AppendLine("");
        }
    }
}

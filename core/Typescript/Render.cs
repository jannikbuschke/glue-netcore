using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Glow.Core.Linq;
using OneOf;

namespace Glow.Core.Typescript
{
    public class Module
    {
        public string Namespace { get; set; }
        public IEnumerable<OneOf<TsType, TsEnum>> Types { get; set; }
    }

    public static class Render
    {
        public static void ToDisk(TypeCollection types, string path)
        {
            if (!path.EndsWith("/"))
            {
                throw new ArgumentException("path must end with /");
            }
            Directory.CreateDirectory(path);

            IEnumerable<IGrouping<string, OneOf<TsType, TsEnum>>> byNamespace = types.All().GroupBy(v => v.Match(v1 => v1.Namespace, v2 => v2.Namespace));

            var modules = byNamespace.Select(v => new Module { Namespace = v.Key, Types = v }).ToList();

            foreach (Module v in modules)
            {
                RenderModule(v, path);
            }
        }

        internal record Dependency
        {
            internal string Id { get; init; }
            internal string Name { get; init; }
            internal string Namespace { get; init; }
            internal bool IsPrimitive { get; init; }
        }

        private static void RenderModule(Module module, string path)
        {
            var builder = new StringBuilder();

            var t12332 = module.Types
               .Where(v => v.IsT0)
               .Select(v => v.AsT0)
               .Where(v => v.Properties != null)
               .SelectMany(v => v.Properties)
               .Where(v => v.PropertyName == "registrations")
               .ToList();

            if (t12332.Count != 0)
            {

            }

            IEnumerable<IGrouping<string, Dependency>> dependencies = module.Types
                .Where(v => v.IsT0)
                .Select(v => v.AsT0)
                .Where(v => v.Properties != null)
                .SelectMany(v => v.Properties)
                .Select(v => v.TsType.Match(
                    v => new Dependency { Id =  v.Id, Namespace = v.Namespace, Name = v.Name, IsPrimitive = v.IsPrimitive },
                    v => new Dependency { Id = v.Id, Namespace = v.Namespace, Name = v.Name, IsPrimitive = false }))
                .Where(v => !v.IsPrimitive)
                .Where(v => v.Namespace != module.Namespace && v.Name != "any")
                .DistinctBy(v => v.Id)
                .GroupBy(v => v.Namespace);

            foreach (IGrouping<string, Dependency> group in dependencies)
            {
                builder.AppendLine($"import {{ {string.Join(", ", group.Select(v => v.Name.Replace("[]","")))} }} from \"./{group.Key}\"");
                builder.AppendLine($"import {{ {string.Join(", ", group.Select(v => "default" + v.Name.Replace("[]","")))} }} from \"./{group.Key}\"");
            }

            builder.AppendLine("");
            builder.AppendLine("");

            IEnumerable<TsEnum> enumerables = module.Types.Where(v => v.IsT1).Select(v => v.AsT1);
            foreach (TsEnum v in enumerables)
            {
                RenderTsEnum(v, builder);
            }

            IEnumerable<TsType> t = module.Types.Where(v => v.IsT0).Select(v => v.AsT0).Where(v => !v.IsPrimitive);

            IList<TsType> sorted = TopologicalSort(
                t.DistinctBy(v => v.Id),
                v => v.Properties?.Where(v => v.TsType.IsT0).Select(v => v.TsType.AsT0));

            sorted = sorted.Where(v => t.Contains(v)).ToList();

            foreach (TsType tsType in sorted)
            {
                RenderTsType(tsType, builder);
            }

            var missing = t.Where(v => !sorted.Contains(v)).ToList();

            foreach (TsType tsType in missing)
            {
                RenderTsType(tsType, builder);
            }

            File.WriteAllText(path + module.Namespace + ".ts", builder.ToString());
        }

        private static void RenderTsEnum(TsEnum type, StringBuilder builder)
        {
            var name = type.Name;
            //builder.AppendLine($"export namespace {type.Namespace} {{");
            builder.AppendLine($"export type {name} = {string.Join(" | ", type.Values.Select(v => $@"""{v}"""))}");
            builder.AppendLine($@"export const default{name} = ""{type.DefaultValue ?? "NULL"}""");
            //builder.AppendLine("}");
            builder.AppendLine("");
        }

        private static void RenderTsType(TsType type, StringBuilder builder)
        {
            var name = type.Name;
            //builder.AppendLine($"export namespace {type.Namespace} {{");
            builder.AppendLine($"export interface {name} {{");
            if (type.Properties != null)
            {
                foreach (Property v in type.Properties)
                {
                    builder.AppendLine($"  {v.PropertyName}: {v.TypeName}");
                }
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

            //builder.AppendLine("}");

            builder.AppendLine("");
        }

        public static IList<T> TopologicalSort<T>(
            IEnumerable<T> source,
            Func<T, IEnumerable<T>> getDependencies
        ) where T: TsType
        {
            var sorted = new List<T>();
            var visited = new Dictionary<T, bool>();

            foreach (T item in source)
            {
                try
                {
                    Visit(item, getDependencies, sorted, visited);
                }
                catch (ArgumentException) { }
            }

            return sorted;
        }

        public static void Visit<T>(
            T item,
            Func<T, IEnumerable<T>> getDependencies,
            List<T> sorted,
            Dictionary<T, bool> visited
        ) where T: TsType
        {
            var alreadyVisited = visited.TryGetValue(item, out var inProcess);

            if (alreadyVisited)
            {
                if (inProcess)
                {
                    throw new ArgumentException($"Cyclic dependency found. ({item.Name})");
                }
            }
            else
            {
                visited[item] = true;

                IEnumerable<T> dependencies = getDependencies(item);
                if (dependencies != null)
                {
                    foreach (T dependency in dependencies)
                    {
                        Visit(dependency, getDependencies, sorted, visited);
                    }
                }

                visited[item] = false;
                sorted.Add(item);
            }
        }
    }
}

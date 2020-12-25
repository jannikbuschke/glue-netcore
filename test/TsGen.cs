using System;
using System.Collections.Generic;
using Glow.Core.Typescript;
using Xunit;

namespace Glow.Test
{
    public class Bar
    {
        public string Foo { get; set; }
        public int Value { get; set; }
        public FooBar FooBar { get; set; }
    }

    public class FooBar
    {
        public decimal Value { get; set; }
        public double Value2 { get; set; }
        public IEnumerable<double> Value3 { get; set; }
        public Dictionary<string,decimal> MyProperty { get; set; }
        public Dictionary<string,string> MyProperty2 { get; set; }
        public Dictionary<string, Foo> MyProperty3 { get; set; }
    }

    public class Foo {
        public string DisplayName { get; set; }
    }

    public class Generic<T>
    {
        public IEnumerable<T> GenericValues { get; set; }
    }

    public class TsGen
    {
        [Fact]
        public void Foo()
        {
            var builder = new TypeCollectionBuilder();

            builder.Add<IEnumerable<string>>();
            builder.Add<Dictionary<string, object>>();

            builder.Add<Generic<Foo>>();

            builder.Add<string>();
            builder.Add<Bar>();
            builder.Add<FooBar>();

            builder.Add<KeyValuePair<string, int>>();
            builder.Add<KeyValuePair<string, string>>();
            builder.Add<KeyValuePair<string, Foo>>();

            builder.Generate();

            Render.ToDisk(builder.Generate(), "./models.ts");
        }
    }
}

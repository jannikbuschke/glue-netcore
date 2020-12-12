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
    }

    public class TsGen
    {
        [Fact]
        public void Foo()
        {
            var builder = new TypeBuilder();
            builder.Add<string>();
            builder.Add<Bar>();
            builder.Add<FooBar>();
            builder.Generate();

        }
    }
}

using EfConfigurationProvider.Core;

namespace Glow.Sample.Configurations
{
    [GeneratedController("sample-configuration", Title = "Sample Configuration", Id = "sample-configuration", SectionId = "sample-configuration")]
    public class SampleConfiguration
    {
        public string Prop1 { get; set; }
        public int Prop2 { get; set; }
    }
}

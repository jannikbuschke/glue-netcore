using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
using Glow.Configurations;
using Glow.Core.Tests;
using Glow.Sample;
using Glow.Sample.Configurations;
using Glow.Sample.Users;
using Glow.Users;
using Xunit;

namespace Glow.Test
{
    [Collection("integration-tests")]
    public class ConfigurationsShould : BaseIntegrationTest<Startup>
    {
        public ConfigurationsShould(IntegrationTestFixture fixture) : base(fixture.Factory) { }

        [Fact]
        public async Task BeUpdateable()
        {
            UserDto user = TestUsers.TestUser();

            SampleConfiguration cfg0 = await Get()
                .From("/api/configurations/sample-configuration")
                .As(user)
                .Read<SampleConfiguration>();

            IEnumerable<Meta> meta = await Get()
                .From("/api/configuration-schemas")
                .As(user)
                .Read<IEnumerable<Meta>>();

            meta.Should().NotBeEmpty();
            Meta sampleConfiguration = meta.Single(v => v.Id == "sample-configuration");

            var prop1 = Faker.Lorem.Word();
            var prop2 = Faker.Random.Number(1, 1000);
            _ = await Send(new UpdateRaw<SampleConfiguration>
            {
                ConfigurationId = "sample-configuration",
                Value = new SampleConfiguration
                {
                    Prop1 = prop1,
                    Prop2 = prop2
                }
            })
                .As(user)
                .To("/api/configurations/sample-configuration")
                .ExecuteAndRead();

            SampleConfiguration cfg = await Get()
                .From("/api/configurations/sample-configuration")
                .As(user)
                .Read<SampleConfiguration>();

            cfg.Prop1.Should().Be(prop1);
            cfg.Prop2.Should().Be(prop2);
        }
    }
}

using System.Net.Http;
using Xunit;

namespace Glow.Test
{
    [Collection("integration-tests")]
    public class ApiShould : BaseIntegrationTestClass
    {
        public ApiShould(IntegrationTestFixture fixture) : base(fixture.Factory) { }

        [Fact]
        public async void Not_Throw()
        {
            HttpResponseMessage response = await client.GetAsync("/hello");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("hello world", content);
        }
    }
}

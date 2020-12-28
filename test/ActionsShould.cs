using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Glow.Core.Tests;
using Glow.Sample;
using Glow.Sample.Actions;
using Glow.Sample.Users;
using Glow.Users;
using Xunit;

namespace Glow.Test
{
    [Collection("integration-tests")]
    public class ActionsShould : BaseIntegrationTest<Startup>
    {
        public ActionsShould(IntegrationTestFixture fixture) : base(fixture.Factory) { }

        [Fact]
        public async Task NotThrow()
        {
            UserDto user = TestUsers.TestUser();
            MediatR.Unit response = await Send(new SampleAction { Foo = "foo" })
                .To("/api/actions/sample")
                .As(user)
                .ExecuteAndRead();

            UserDto user2 = TestUsers.NonPrivilegedUser();
            System.Net.Http.HttpResponseMessage response2 = await Send(new SampleAction { Foo = "foo" })
                .To("/api/actions/sample")
                .As(user2)
                .Execute();

            response2.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task NotThrow2()
        {
            UserDto user = TestUsers.TestUser();
            Response response = await Send(new SampleAction2 { Message = "foo" })
                .To("/api/actions/sample-2")
                .As(user)
                .ExecuteAndRead();

            response.Value.Should().Be("Hello World foo");
        }
    }
}

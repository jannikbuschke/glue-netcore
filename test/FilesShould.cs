using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
using Glow.Core.Files;
using Glow.Core.Tests;
using Glow.Files;
using Glow.Sample;
using Glow.Sample.Files;
using Glow.Sample.Users;
using Glow.Users;
using Xunit;

namespace Glow.Test
{
    [Collection("integration-tests")]
    public class FilesShould : BaseIntegrationTest<Startup>
    {
        public FilesShould(IntegrationTestFixture fixture) : base(fixture.Factory) { }

        [Fact(Skip = "refactor odata")]
        public async Task CreateAndUpdateFiles()
        {
            UserDto user = TestUsers.TestUser();
            IList<Portfolio> portfolios = await Get()
                .From("/api/portfolios/examples")
                .As(user)
                .Read<IList<Portfolio>>();
            Portfolio portfolio = Faker.PickRandom(portfolios);
            portfolios.Remove(portfolio);

            ICollection<PortfolioFile> files = portfolio.Files;
            //files.Should().HaveCountGreaterOrEqualTo(2);

            IEnumerable<IFile> stagedFiles = await client.UploadFiles(files, "/api/portfolios/stage-files");

            stagedFiles.Should().NotBeEmpty();
            stagedFiles.Should().BeEquivalentTo(files, options => options.Excluding(v => v.Id).Excluding(v => v.Path));

            System.Guid createdPortfolioId = (await Send(new CreatePortfolio
            {
                Files = stagedFiles.Select(v => new PutPortfolioFile { Id = v.Id, Name = v.Name })
            })
                .To("/api/portfolios/create")
                .As(user)
                .ExecuteAndRead()).Id;

            Portfolio createdPortfolio = await Query($"/odata/portfolios/{createdPortfolioId}?$expand=files")
                .As(user)
                .Read<Portfolio>();

            createdPortfolio.Files.Should().BeEquivalentTo(files, options => options.Excluding(v => v.Id).Excluding(v => v.Path));

            ICollection<PortfolioFile> files2 = Faker.PickRandom(portfolios).Files;

            files2.Select(v => v.Id).Should().NotBeEquivalentTo(files.Select(v => v.Id));

            IEnumerable<PortfolioFile> stagedFiles2 = await client.UploadFiles(files2, "/api/portfolios/stage-files");

            var putFiles2 = new List<PortfolioFile>();
            putFiles2.AddRange(createdPortfolio.Files);
            putFiles2[0].Name = Faker.Lorem.Word();
            putFiles2.RemoveAt(putFiles2.Count - 1);
            putFiles2.AddRange(stagedFiles2);

            Portfolio updatedPortfolio = await Send(new UpdatePortfolio
            {
                Id = createdPortfolio.Id,
                Files = putFiles2.Select(v => new PutPortfolioFile { Id = v.Id, Name = v.Name })
            })
                .To("/api/portfolios/update")
                .As(TestUsers.TestUser())
                .ExecuteAndRead();

            updatedPortfolio.Files.Should().BeEquivalentTo(putFiles2, options => options.Excluding(v => v.Id).Excluding(v => v.Path));
        }
    }
}

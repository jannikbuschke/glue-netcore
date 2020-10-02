using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.EquivalencyExpression;
using Glow.Core.FakeData;
using Glow.TypeScript;
using Glue.Files;
using JannikB.AspNetCore.Utils.Module;
using JannikB.Glue;
using MediatR;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Glow.Sample.Files
{
    public class Test : TypeScriptProfile
    {
        public Test()
        {
            Add<PutPortfolioFile>();
            Add<CreatePortfolio>();
            Add<UpdatePortfolio>();
            Add<Portfolio>();
            Add<PortfolioFile>();
        }
    }

    public class FilesProfile : Profile
    {
        public FilesProfile()
        {
            CreateMap<PutPortfolioFile, PortfolioFile>().EqualityComparison((v1, v2) => v1.Id == v2.Id);
            CreateMap<CreatePortfolio, Portfolio>();
            CreateMap<UpdatePortfolio, Portfolio>();
        }
    }

    public class PutPortfolioFile
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public class CreatePortfolio : IRequest<Portfolio>
    {
        public string DisplayName { get; set; }
        public IEnumerable<PutPortfolioFile> Files { get; set; }
    }

    public class UpdatePortfolio : IRequest<Portfolio>
    {
        public string DisplayName { get; set; }
        public int Id { get; set; }
        public IEnumerable<PutPortfolioFile> Files { get; set; }
    }

    public class DeletePortfolio : IRequest
    {
        public int Id { get; set; }
    }

    public class Portfolio
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }
        public ICollection<PortfolioFile> Files { get; set; }
    }

    public class PortfolioFile : IFile
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
    }

    public class FakePortfolios : FakeBase<Portfolio>
    {
        public FakePortfolios() : base(f =>
        {
            return f
            .RuleFor(v => v.Id, f => f.IndexFaker)
            .RuleFor(v => v.Files, f => Enumerable.Range(2, f.Random.Number(1, 5)).Select(v => new PortfolioFile
            {
                Id = Guid.NewGuid(),
                Name = f.Name.JobTitle(),
            }).ToList())
                .Generate(10);
        })
        {
        }
    }

    public class CreateOrUpdatePortfolioaHandler
        : IRequestHandler<CreatePortfolio, Portfolio>
        , IRequestHandler<UpdatePortfolio, Portfolio>
        , IRequestHandler<DeletePortfolio, Unit>
    {
        private readonly DataContext ctx;
        private readonly IMapper mapper;

        public CreateOrUpdatePortfolioaHandler(DataContext ctx, IMapper mapper)
        {
            this.ctx = ctx;
            this.mapper = mapper;
        }

        public async Task<Portfolio> Handle(CreatePortfolio request, CancellationToken cancellationToken)
        {
            List<PortfolioFile> files = await ctx.PortfolioFiles.Where(v => request.Files.Select(v => v.Id).Contains(v.Id)).ToListAsync();
            Portfolio result = mapper.Map(request, new Portfolio { Files = files });
            ctx.Portfolios.Add(result);
            await ctx.SaveChangesAsync();
            return result;
        }

        public async Task<Portfolio> Handle(UpdatePortfolio request, CancellationToken cancellationToken)
        {
            Portfolio result = await ctx.Portfolios.Include(v => v.Files).SingleAsync(v => v.Id == request.Id);
            mapper.Map(request, result);
            await ctx.SaveChangesAsync();
            return result;
        }

        public async Task<Unit> Handle(DeletePortfolio request, CancellationToken cancellationToken)
        {
            Portfolio v = await ctx.Portfolios.Include(v => v.Files).SingleAsync(v => v.Id == request.Id);
            ctx.Remove(v);
            ctx.RemoveRange(v.Files);
            await ctx.SaveChangesAsync();
            return Unit.Value;
        }
    }

    [Route("api/portfolios")]
    [Route("api/portfolio")]
    [ApiController]
    public class PortfoliosController : ControllerBase
    {
        private static readonly FakePortfolios examples = new FakePortfolios();
        private readonly FileService fileService;
        private readonly DataContext ctx;
        private readonly IMediator mediator;

        public PortfoliosController(FileService fileService, DataContext ctx, IMediator mediator)
        {
            this.fileService = fileService;
            this.ctx = ctx;
            this.mediator = mediator;
        }

        [Validatable]
        [HttpPost("create")]
        public async Task<ActionResult<Portfolio>> Create(CreatePortfolio request)
        {
            Portfolio result = await mediator.Send(request);
            return result;
        }

        [Validatable]
        [HttpPost("update")]
        public async Task<ActionResult<Portfolio>> Update(UpdatePortfolio request)
        {
            Portfolio result = await mediator.Send(request);
            return result;
        }

        [Validatable]
        [HttpPost("delete")]
        public async Task<ActionResult<Unit>> Delete(DeletePortfolio request)
        {
            Unit result = await mediator.Send(request);
            return result;
        }

        [HttpPost("stage-files")]
        [RequestSizeLimit(52428800)]
        public async Task<ActionResult<IEnumerable<PortfolioFile>>> Stage(Unit request)
        {
            IList<PortfolioFile> result = await fileService.WriteFormfilesToPath<PortfolioFile>(Request.Form.Files, "runtime-files");
            ctx.PortfolioFiles.AddRange(result);
            await ctx.SaveChangesAsync();
            return Ok(result);
        }

        [HttpGet("examples")]
        public IEnumerable<Portfolio> Examples()
        {
            return examples.Data;
        }

        [HttpGet("list")]
        public IQueryable<Portfolio> List()
        {
            return ctx.Portfolios.Include(v => v.Files);
        }

        [HttpGet("{id}")]
        public Portfolio Single(int id)
        {
            return ctx.Portfolios.Include(v => v.Files).Single(v => v.Id == id);
        }
    }

    [ODataRoutePrefix("Portfolios")]
    public class PortfoliosOdataController : BaseOdataController<Portfolio, int>
    {
        public PortfoliosOdataController(DataContext ctx)
            : base(ctx.Portfolios, key => ctx.Portfolios.Where(v => v.Id == key))
        {

        }
    }
}

using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Glow.TypeScript;
using Glow.Glue;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Glow.Validation;

namespace Glow.Sample.Forms
{
    [GenerateTsInterface]
    public class CreateUser : IRequest<Unit>
    {
        [Required, MinLength(3)]
        public string DisplayName { get; set; }
        [Required, EmailAddress]
        public string Email { get; set; }
    }

    public class CreateUserHandler : IRequestHandler<CreateUser, Unit>
    {
        public Task<Unit> Handle(CreateUser request, CancellationToken cancellationToken)
        {
            // left out
            return Unit.Task;
        }
    }

    [Route("api/form")]
    [ApiController]
    public class FormController : ControllerBase
    {
        private readonly IMediator mediator;

        public FormController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [Validatable]
        [HttpPost("create-user")]
        public async Task<Unit> CreateUser(CreateUser request)
        {
            Unit result = await mediator.Send(request);
            return result;
        }
    }
}

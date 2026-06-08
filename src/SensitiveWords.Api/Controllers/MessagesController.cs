using Microsoft.AspNetCore.Mvc;
using SensitiveWords.Application.DTOs;
using SensitiveWords.Application.Interfaces;
using SensitiveWords.Application.Validators;

namespace SensitiveWords.Api.Controllers
{
    [ApiController]
    [Route("api/messages")]
    [Produces("application/json")]
    public sealed class MessagesController(IMessageSanitiserService sanitiserService) : ControllerBase
    {
        [HttpPost("sanitise")]
        [ProducesResponseType(typeof(SanitiseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Sanitise([FromBody] SanitiseRequest request, CancellationToken cancellationToken)
        {
            var errors = SanitiseRequestValidator.Validate(request).ToList();
            if (errors.Count > 0)
                return ValidationProblem(new ValidationProblemDetails(
                    new Dictionary<string, string[]> { ["input"] = errors.ToArray() }));

            var response = await sanitiserService.SanitiseAsync(request, cancellationToken);
            return Ok(response);
        }
    }
}

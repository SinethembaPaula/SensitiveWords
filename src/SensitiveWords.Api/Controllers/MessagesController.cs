using Microsoft.AspNetCore.Mvc;
using SensitiveWords.Application.DTOs;
using SensitiveWords.Application.Interfaces;

namespace SensitiveWords.Api.Controllers
{
    [ApiController]
    [Route("api/messages")]
    [Produces("application/json")]
    public sealed class MessagesController(IMessageSanitiserService sanitiserService) : ControllerBase
    {
        [HttpPost("sanitise")]
        [ProducesResponseType(typeof(SanitiseResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Sanitise([FromBody] SanitiseRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Input))
                return BadRequest("Input cannot be empty.");

            var response = await sanitiserService.SanitiseAsync(request, cancellationToken);
            return Ok(response);
        }
    }
}

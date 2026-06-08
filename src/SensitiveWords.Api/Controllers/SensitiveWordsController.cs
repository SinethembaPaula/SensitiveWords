using Microsoft.AspNetCore.Mvc;
using SensitiveWords.Application.DTOs;
using SensitiveWords.Application.Interfaces;
using SensitiveWords.Application.Validators;

namespace SensitiveWords.Api.Controllers
{
    [ApiController]
    [Route("api/sensitive-words")]
    [Produces("application/json")]
    public sealed class SensitiveWordsController(ISensitiveWordRepository repository) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<SensitiveWordDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var words = await repository.GetAllAsync(cancellationToken);
            return Ok(words);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(SensitiveWordDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var word = await repository.GetByIdAsync(id, cancellationToken);
            return word is null ? NotFound() : Ok(word);
        }

        [HttpPost]
        [ProducesResponseType(typeof(SensitiveWordDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateSensitiveWordRequest request, CancellationToken cancellationToken)
        {
            var errors = CreateSensitiveWordRequestValidator.Validate(request).ToList();
            if (errors.Count > 0)
                return ValidationProblem(new ValidationProblemDetails(
                    new Dictionary<string, string[]> { ["word"] = errors.ToArray() }));

            var id = await repository.CreateAsync(request, cancellationToken);
            var created = await repository.GetByIdAsync(id, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id }, created);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSensitiveWordRequest request, CancellationToken cancellationToken)
        {
            var errors = UpdateSensitiveWordRequestValidator.Validate(request).ToList();
            if (errors.Count > 0)
                return ValidationProblem(new ValidationProblemDetails(
                    new Dictionary<string, string[]> { ["word"] = errors.ToArray() }));

            var updated = await repository.UpdateAsync(id, request, cancellationToken);
            return updated ? NoContent() : NotFound();
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var deleted = await repository.DeleteAsync(id, cancellationToken);
            return deleted ? NoContent() : NotFound();
        }
    }
}

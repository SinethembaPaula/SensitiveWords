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
        /// <summary>Returns all sensitive words.</summary>
        /// <response code="200">List of all sensitive words.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<SensitiveWordDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var words = await repository.GetAllAsync(cancellationToken);
            return Ok(words);
        }

        /// <summary>Returns a sensitive word by ID.</summary>
        /// <param name="id">The ID of the sensitive word.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <response code="200">The sensitive word.</response>
        /// <response code="404">Word not found.</response>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(SensitiveWordDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
        {
            var word = await repository.GetByIdAsync(id, cancellationToken);
            return word is null ? NotFound() : Ok(word);
        }

        /// <summary>Adds a new sensitive word.</summary>
        /// <param name="request">The word to add.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <response code="201">Word created successfully.</response>
        /// <response code="400">Validation failed.</response>
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

        /// <summary>Updates an existing sensitive word.</summary>
        /// <param name="id">The ID of the word to update.</param>
        /// <param name="request">The updated word value.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <response code="204">Word updated successfully.</response>
        /// <response code="400">Validation failed.</response>
        /// <response code="404">Word not found.</response>
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

        /// <summary>Deletes a sensitive word.</summary>
        /// <param name="id">The ID of the word to delete.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <response code="204">Word deleted successfully.</response>
        /// <response code="404">Word not found.</response>
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

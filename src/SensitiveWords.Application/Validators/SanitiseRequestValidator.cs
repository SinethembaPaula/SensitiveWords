using SensitiveWords.Application.DTOs;

namespace SensitiveWords.Application.Validators
{
    public static class SanitiseRequestValidator
    {
        public static IEnumerable<string> Validate(SanitiseRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Input))
                yield return "Input cannot be empty.";

            if (request.Input?.Length > 10_000)
                yield return "Input cannot exceed 10,000 characters.";
        }
    }
}

using SensitiveWords.Application.DTOs;

namespace SensitiveWords.Application.Validators
{
    public static class CreateSensitiveWordRequestValidator
    {
        public static IEnumerable<string> Validate(CreateSensitiveWordRequest request)
            => WordValidator.Validate(request.Word);
    }
}

using SensitiveWords.Application.DTOs;

namespace SensitiveWords.Application.Validators
{
    public static class UpdateSensitiveWordRequestValidator
    {
        public static IEnumerable<string> Validate(UpdateSensitiveWordRequest request)
            => WordValidator.Validate(request.Word);
    }
}

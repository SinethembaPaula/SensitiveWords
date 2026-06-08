using SensitiveWords.Application.DTOs;
using SensitiveWords.Application.Validators;
using FluentAssertions;

namespace SensitiveWords.UnitTests.Validators
{
    public sealed class WordValidatorTests
    {
        [Fact]
        public void Validate_ReturnsError_WhenWordIsNull()
        {
            var request = new CreateSensitiveWordRequest(null!);
            var errors = CreateSensitiveWordRequestValidator.Validate(request).ToList();
            errors.Should().ContainSingle(e => e == "Word cannot be empty.");
        }

        [Fact]
        public void Validate_ReturnsError_WhenWordIsEmpty()
        {
            var request = new CreateSensitiveWordRequest(string.Empty);
            var errors = CreateSensitiveWordRequestValidator.Validate(request).ToList();
            errors.Should().ContainSingle(e => e == "Word cannot be empty.");
        }

        [Fact]
        public void Validate_ReturnsError_WhenWordIsWhitespace()
        {
            var request = new CreateSensitiveWordRequest("   ");
            var errors = CreateSensitiveWordRequestValidator.Validate(request).ToList();
            errors.Should().ContainSingle(e => e == "Word cannot be empty.");
        }

        [Fact]
        public void Validate_ReturnsError_WhenWordExceeds100Characters()
        {
            var request = new CreateSensitiveWordRequest(new string('A', 101));
            var errors = CreateSensitiveWordRequestValidator.Validate(request).ToList();
            errors.Should().ContainSingle(e => e == "Word cannot exceed 100 characters.");
        }

        [Fact]
        public void Validate_ReturnsError_WhenWordContainsSingleQuote()
        {
            var request = new CreateSensitiveWordRequest("DROP'TABLE");
            var errors = CreateSensitiveWordRequestValidator.Validate(request).ToList();
            errors.Should().ContainSingle(e => e == "Word contains invalid characters.");
        }

        [Fact]
        public void Validate_ReturnsNoErrors_WhenWordIsValid()
        {
            var request = new CreateSensitiveWordRequest("SELECT");
            var errors = CreateSensitiveWordRequestValidator.Validate(request).ToList();
            errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_ReturnsNoErrors_WhenWordIsExactly100Characters()
        {
            var request = new CreateSensitiveWordRequest(new string('A', 100));
            var errors = CreateSensitiveWordRequestValidator.Validate(request).ToList();
            errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_UpdateRequest_ReturnsError_WhenWordIsEmpty()
        {
            var request = new UpdateSensitiveWordRequest(string.Empty);
            var errors = UpdateSensitiveWordRequestValidator.Validate(request).ToList();
            errors.Should().ContainSingle(e => e == "Word cannot be empty.");
        }

        [Fact]
        public void Validate_UpdateRequest_ReturnsNoErrors_WhenWordIsValid()
        {
            var request = new UpdateSensitiveWordRequest("DROP");
            var errors = UpdateSensitiveWordRequestValidator.Validate(request).ToList();
            errors.Should().BeEmpty();
        }
    }
}

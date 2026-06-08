using SensitiveWords.Application.DTOs;
using SensitiveWords.Application.Validators;
using FluentAssertions;

namespace SensitiveWords.UnitTests.Validators
{
    public sealed class SanitiseRequestValidatorTests
    {
        [Fact]
        public void Validate_ReturnsError_WhenInputIsNull()
        {
            var request = new SanitiseRequest(null!);
            var errors = SanitiseRequestValidator.Validate(request).ToList();
            errors.Should().ContainSingle(e => e == "Input cannot be empty.");
        }

        [Fact]
        public void Validate_ReturnsError_WhenInputIsEmpty()
        {
            var request = new SanitiseRequest(string.Empty);
            var errors = SanitiseRequestValidator.Validate(request).ToList();
            errors.Should().ContainSingle(e => e == "Input cannot be empty.");
        }

        [Fact]
        public void Validate_ReturnsError_WhenInputIsWhitespace()
        {
            var request = new SanitiseRequest("   ");
            var errors = SanitiseRequestValidator.Validate(request).ToList();
            errors.Should().ContainSingle(e => e == "Input cannot be empty.");
        }

        [Fact]
        public void Validate_ReturnsError_WhenInputExceeds10000Characters()
        {
            var request = new SanitiseRequest(new string('A', 10_001));
            var errors = SanitiseRequestValidator.Validate(request).ToList();
            errors.Should().ContainSingle(e => e == "Input cannot exceed 10,000 characters.");
        }

        [Fact]
        public void Validate_ReturnsNoErrors_WhenInputIsValid()
        {
            var request = new SanitiseRequest("Hello world");
            var errors = SanitiseRequestValidator.Validate(request).ToList();
            errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_ReturnsNoErrors_WhenInputIsExactly10000Characters()
        {
            var request = new SanitiseRequest(new string('A', 10_000));
            var errors = SanitiseRequestValidator.Validate(request).ToList();
            errors.Should().BeEmpty();
        }
    }
}

using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SensitiveWords.Application.DTOs;
using SensitiveWords.Application.Interfaces;
using SensitiveWords.Application.Services;

namespace SensitiveWords.UnitTests.Services
{
    public sealed class MessageSanitiserServiceTests
    {
        private readonly Mock<ISensitiveWordRepository> _repositoryMock;
        private readonly MessageSanitiserService _sut;

        public MessageSanitiserServiceTests()
        {
            _repositoryMock = new Mock<ISensitiveWordRepository>();
            _sut = new MessageSanitiserService(
                _repositoryMock.Object,
                NullLogger<MessageSanitiserService>.Instance);
        }

        private void SetupWords(params string[] words)
        {
            var dtos = words.Select((w, i) => new SensitiveWordDto(i + 1, w, DateTime.UtcNow, null));
            _repositoryMock
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(dtos);
        }

        [Fact]
        public async Task SanitiseAsync_ReplacesExactWord_WithAsterisks()
        {
            SetupWords("SELECT");

            var response = await _sut.SanitiseAsync(new SanitiseRequest("SELECT * FROM users"), CancellationToken.None);

            response.Output.Should().Be("****** * FROM users");
        }

        [Fact]
        public async Task SanitiseAsync_IsCaseInsensitive()
        {
            SetupWords("SELECT");

            var response = await _sut.SanitiseAsync(new SanitiseRequest("select * FROM users"), CancellationToken.None);

            response.Output.Should().Be("****** * FROM users");
        }

        [Fact]
        public async Task SanitiseAsync_DoesNotReplaceSubstring()
        {
            SetupWords("SELECT");

            var response = await _sut.SanitiseAsync(new SanitiseRequest("SELECTION is not a keyword"), CancellationToken.None);

            response.Output.Should().Be("SELECTION is not a keyword");
        }

        [Fact]
        public async Task SanitiseAsync_MatchesLongestPhraseFirst()
        {
            SetupWords("SELECT * FROM", "SELECT", "FROM");

            var response = await _sut.SanitiseAsync(new SanitiseRequest("SELECT * FROM users"), CancellationToken.None);

            response.Output.Should().Be("************* users");
        }

        [Fact]
        public async Task SanitiseAsync_ReplacesMultipleSensitiveWords()
        {
            SetupWords("SELECT", "DELETE", "WHERE");

            var response = await _sut.SanitiseAsync(new SanitiseRequest("SELECT * FROM users WHERE DELETE FROM table"), CancellationToken.None);

            response.Output.Should().Be("****** * FROM users ***** ****** FROM table");
        }

        [Fact]
        public async Task SanitiseAsync_ReturnsUnchangedMessage_WhenNoSensitiveWords()
        {
            SetupWords("SELECT");

            var response = await _sut.SanitiseAsync(new SanitiseRequest("Hello world"), CancellationToken.None);

            response.Output.Should().Be("Hello world");
        }

        [Fact]
        public async Task SanitiseAsync_ReturnsEmptyString_WhenInputIsEmpty()
        {
            SetupWords("SELECT");

            var response = await _sut.SanitiseAsync(new SanitiseRequest(string.Empty), CancellationToken.None);

            response.Output.Should().BeEmpty();
        }

        [Fact]
        public async Task SanitiseAsync_ReplacesAllOccurrences_OfSameWord()
        {
            SetupWords("SELECT");

            var response = await _sut.SanitiseAsync(new SanitiseRequest("SELECT name, SELECT age"), CancellationToken.None);

            response.Output.Should().Be("****** name, ****** age");
        }

        [Fact]
        public async Task SanitiseAsync_PreservesNonSensitiveWords()
        {
            SetupWords("DROP");

            var response = await _sut.SanitiseAsync(new SanitiseRequest("Please DROP the table"), CancellationToken.None);

            response.Output.Should().Be("Please **** the table");
        }
    }
}

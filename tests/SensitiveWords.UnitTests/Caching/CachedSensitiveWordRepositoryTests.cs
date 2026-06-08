using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using SensitiveWords.Application.DTOs;
using SensitiveWords.Application.Interfaces;
using SensitiveWords.Infrastructure.Caching;

namespace SensitiveWords.UnitTests.Caching
{
    public sealed class CachedSensitiveWordRepositoryTests
    {
        private readonly Mock<ISensitiveWordRepository> _innerMock;
        private readonly IMemoryCache _cache;
        private readonly CachedSensitiveWordRepository _sut;

        private static readonly List<SensitiveWordDto> SampleWords =
        [
            new(1, "SELECT", DateTime.UtcNow, null),
        new(2, "DROP", DateTime.UtcNow, null)
        ];

        public CachedSensitiveWordRepositoryTests()
        {
            _innerMock = new Mock<ISensitiveWordRepository>();
            _cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            _sut = new CachedSensitiveWordRepository(
                _innerMock.Object,
                _cache,
                NullLogger<CachedSensitiveWordRepository>.Instance);
        }

        [Fact]
        public async Task GetAllAsync_CallsInner_OnCacheMiss()
        {
            _innerMock
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(SampleWords);

            await _sut.GetAllAsync(CancellationToken.None);

            _innerMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsCachedResult_OnSecondCall()
        {
            _innerMock
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(SampleWords);

            await _sut.GetAllAsync(CancellationToken.None);
            await _sut.GetAllAsync(CancellationToken.None);

            _innerMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_InvalidatesCache()
        {
            _innerMock
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(SampleWords);
            _innerMock
                .Setup(r => r.CreateAsync(It.IsAny<CreateSensitiveWordRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(3);

            await _sut.GetAllAsync(CancellationToken.None);
            await _sut.CreateAsync(new CreateSensitiveWordRequest("INSERT"), CancellationToken.None);
            await _sut.GetAllAsync(CancellationToken.None);

            _innerMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [Fact]
        public async Task UpdateAsync_InvalidatesCache_WhenWordExists()
        {
            _innerMock
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(SampleWords);
            _innerMock
                .Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<UpdateSensitiveWordRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await _sut.GetAllAsync(CancellationToken.None);
            await _sut.UpdateAsync(1, new UpdateSensitiveWordRequest("UPDATED"), CancellationToken.None);
            await _sut.GetAllAsync(CancellationToken.None);

            _innerMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [Fact]
        public async Task UpdateAsync_DoesNotInvalidateCache_WhenWordNotFound()
        {
            _innerMock
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(SampleWords);
            _innerMock
                .Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<UpdateSensitiveWordRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            await _sut.GetAllAsync(CancellationToken.None);
            await _sut.UpdateAsync(999, new UpdateSensitiveWordRequest("UPDATED"), CancellationToken.None);
            await _sut.GetAllAsync(CancellationToken.None);

            _innerMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_InvalidatesCache_WhenWordExists()
        {
            _innerMock
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(SampleWords);
            _innerMock
                .Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await _sut.GetAllAsync(CancellationToken.None);
            await _sut.DeleteAsync(1, CancellationToken.None);
            await _sut.GetAllAsync(CancellationToken.None);

            _innerMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [Fact]
        public async Task DeleteAsync_DoesNotInvalidateCache_WhenWordNotFound()
        {
            _innerMock
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(SampleWords);
            _innerMock
                .Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            await _sut.GetAllAsync(CancellationToken.None);
            await _sut.DeleteAsync(999, CancellationToken.None);
            await _sut.GetAllAsync(CancellationToken.None);

            _innerMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}

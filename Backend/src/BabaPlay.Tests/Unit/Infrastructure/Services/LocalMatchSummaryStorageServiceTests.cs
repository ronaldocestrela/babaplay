using BabaPlay.Application.Interfaces;
using BabaPlay.Infrastructure.Services;
using BabaPlay.Infrastructure.Settings;
using FluentAssertions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace BabaPlay.Tests.Unit.Infrastructure.Services;

public class LocalMatchSummaryStorageServiceTests : IDisposable
{
    private readonly string _tempRoot = Path.Combine(Path.GetTempPath(), $"babaplay-storage-tests-{Guid.NewGuid():N}");

    [Fact]
    public async Task SaveAndRead_ValidRequest_ShouldPersistAndReturnBytes()
    {
        var sut = CreateSut(_tempRoot);

        var stored = await sut.SaveAsync(new MatchSummaryFileSaveRequest(Guid.NewGuid(), Guid.NewGuid(), [1, 2, 3, 4]));

        var read = await sut.ReadAsync(stored.StoragePath);

        read.Should().NotBeNull();
        read.Should().Equal([1, 2, 3, 4]);
        stored.ContentType.Should().Be("application/pdf");
        stored.SizeBytes.Should().Be(4);
    }

    [Fact]
    public async Task Read_MissingFile_ShouldReturnNull()
    {
        var sut = CreateSut(_tempRoot);

        var read = await sut.ReadAsync("match-summaries/tenant/not-found.pdf");

        read.Should().BeNull();
    }

    [Fact]
    public async Task Read_PathTraversalAttempt_ShouldReturnNull()
    {
        var sut = CreateSut(_tempRoot);

        var read = await sut.ReadAsync("../outside.pdf");

        read.Should().BeNull();
    }

    private IMatchSummaryStorageService CreateSut(string rootPath)
    {
        var hostEnv = new FakeHostEnvironment { ContentRootPath = rootPath };
        var options = Options.Create(new MatchSummaryStorageSettings { RootPath = rootPath });
        return new LocalMatchSummaryStorageService(hostEnv, options);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
            Directory.Delete(_tempRoot, recursive: true);
    }

    private sealed class FakeHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Test";
        public string ApplicationName { get; set; } = "BabaPlay.Tests";
        public string ContentRootPath { get; set; } = string.Empty;
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } =
            new Microsoft.Extensions.FileProviders.NullFileProvider();
    }
}

using NOQTA.Invoice.Infrastructure.Storage;

namespace NOQTA.Invoice.Infrastructure.Tests.Storage;

public sealed class LocalFileStorageTests
{
    [Fact]
    public async Task SaveAsync_WritesFileAndReturnsFullPath()
    {
        var rootDirectory =
            Path.Combine(
                Path.GetTempPath(),
                "NOQTA-Invoice-Tests",
                Guid.NewGuid().ToString("N"));

        try
        {
            var storage =
                new LocalFileStorage(rootDirectory);

            var content =
                new byte[] { 1, 2, 3, 4, 5 };

            var path =
                await storage.SaveAsync(
                    "Invoice-INV-2026-0001.pdf",
                    content);

            Assert.True(
                Path.IsPathFullyQualified(path));

            Assert.Equal(
                Path.Combine(
                    Path.GetFullPath(rootDirectory),
                    "Invoice-INV-2026-0001.pdf"),
                path);

            var savedContent =
                await File.ReadAllBytesAsync(path);

            Assert.Equal(
                content,
                savedContent);
        }
        finally
        {
            if (Directory.Exists(rootDirectory))
            {
                Directory.Delete(
                    rootDirectory,
                    recursive: true);
            }
        }
    }

    [Fact]
    public async Task SaveAsync_RejectsDirectoryTraversalFileName()
    {
        var rootDirectory =
            Path.Combine(
                Path.GetTempPath(),
                "NOQTA-Invoice-Tests",
                Guid.NewGuid().ToString("N"));

        try
        {
            var storage =
                new LocalFileStorage(rootDirectory);

            await Assert.ThrowsAsync<ArgumentException>(
                () => storage.SaveAsync(
                    "../outside.pdf",
                    new byte[] { 1, 2, 3 }));
        }
        finally
        {
            if (Directory.Exists(rootDirectory))
            {
                Directory.Delete(
                    rootDirectory,
                    recursive: true);
            }
        }
    }

    [Fact]
    public async Task SaveAsync_CreatesMissingStorageDirectory()
    {
        var rootDirectory =
            Path.Combine(
                Path.GetTempPath(),
                "NOQTA-Invoice-Tests",
                Guid.NewGuid().ToString("N"));

        try
        {
            Assert.False(
                Directory.Exists(rootDirectory));

            var storage =
                new LocalFileStorage(rootDirectory);

            var path =
                await storage.SaveAsync(
                    "Invoice-Test.pdf",
                    new byte[] { 10, 20, 30 });

            Assert.True(
                Directory.Exists(rootDirectory));

            Assert.True(
                File.Exists(path));
        }
        finally
        {
            if (Directory.Exists(rootDirectory))
            {
                Directory.Delete(
                    rootDirectory,
                    recursive: true);
            }
        }
    }
}
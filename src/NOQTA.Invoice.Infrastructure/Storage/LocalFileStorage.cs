using NOQTA.Invoice.Application.Abstractions.Files;

namespace NOQTA.Invoice.Infrastructure.Storage;

public sealed class LocalFileStorage : IFileStorage
{
    private readonly string _rootDirectory;

    public LocalFileStorage(
        string rootDirectory)
    {
        if (string.IsNullOrWhiteSpace(rootDirectory))
        {
            throw new ArgumentException(
                "Storage root directory is required.",
                nameof(rootDirectory));
        }

        _rootDirectory =
            Path.GetFullPath(rootDirectory);
    }

    public async Task<string> SaveAsync(
        string fileName,
        byte[] content,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException(
                "File name is required.",
                nameof(fileName));
        }

        ArgumentNullException.ThrowIfNull(content);

        var safeFileName =
            Path.GetFileName(fileName);

        if (!string.Equals(
                safeFileName,
                fileName,
                StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "File name must not contain directory components.",
                nameof(fileName));
        }

        Directory.CreateDirectory(
            _rootDirectory);

        var path =
            Path.Combine(
                _rootDirectory,
                safeFileName);

        await File.WriteAllBytesAsync(
            path,
            content,
            cancellationToken);

        return path;
    }
}
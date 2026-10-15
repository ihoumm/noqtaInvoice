namespace NOQTA.Invoice.Application.Abstractions.Files;

public interface IFileStorage
{
    Task<string> SaveAsync(
        string fileName,
        byte[] content,
        CancellationToken cancellationToken = default);
}
namespace NettoRetail.Infrastructure.Interfaces;

public interface IAzureBlobService
{
    Task UploadAsync(string fileName, Stream stream, string containerName);
    Task UpdateAsync(string fileName, Stream stream, string containerName);
}
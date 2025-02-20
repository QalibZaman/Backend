namespace NettoHealth.Infrastructure.Interfaces;

public interface IAzureBlobService
{
    Task UploadAsync(string fileName, Stream stream, string containerName);
}
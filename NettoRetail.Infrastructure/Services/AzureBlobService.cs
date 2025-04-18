using Azure.Storage.Blobs;
using NettoRetail.Infrastructure.Interfaces;

namespace NettoRetail.Infrastructure.Services;

public class AzureBlobService(BlobServiceClient blobServiceClient) : IAzureBlobService
{
    public async Task UploadAsync(string fileName, Stream stream, string containerName)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

        var blobClient = containerClient.GetBlobClient(fileName);

        await blobClient.UploadAsync(stream);
    }

    public async Task DeleteAsync(string fileName, string containerName)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

        var blobClient = containerClient.GetBlobClient(fileName);

        await blobClient.DeleteAsync();
    }

    public async Task UpdateAsync(string fileName, Stream stream, string containerName)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

        var blobClient = containerClient.GetBlobClient(fileName);

        await blobClient.DeleteAsync();
        await blobClient.UploadAsync(stream);
    }
}
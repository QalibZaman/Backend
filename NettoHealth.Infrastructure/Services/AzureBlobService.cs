using Azure.Storage.Blobs;
using NettoHealth.Infrastructure.Interfaces;

namespace NettoHealth.Infrastructure.Services;

public class AzureBlobService(BlobServiceClient blobServiceClient) : IAzureBlobService
{
    public async Task UploadAsync(string fileName, Stream stream, string containerName)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

        var blobClient = containerClient.GetBlobClient(fileName);

        await blobClient.UploadAsync(stream);
    }
}
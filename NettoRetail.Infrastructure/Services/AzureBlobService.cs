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
}
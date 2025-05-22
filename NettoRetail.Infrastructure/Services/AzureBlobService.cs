using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Enums;

namespace NettoRetail.Infrastructure.Services;

public class AzureBlobService(BlobServiceClient blobServiceClient) : IAzureBlobService
{
    public async Task<string> UploadAsync(
           string fileName,
           Stream stream,
           string containerName,
           BlobContentCategoryEnum category,
           bool overwrite = true)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

        string prefix = category.GetDirectory();

        var blobClient = containerClient.GetBlobClient($"{prefix}{fileName}");

        await blobClient.UploadAsync(stream, overwrite: overwrite);

        return blobClient.Uri.ToString();
    }

    public async Task DeleteByUrlAsync(string blobUrl)
    {
        if (string.IsNullOrWhiteSpace(blobUrl))
            throw new ArgumentException("Blob URL cannot be null or empty.", nameof(blobUrl));

        var uri = new Uri(blobUrl);
        var segments = uri.AbsolutePath.TrimStart('/').Split('/');

        // Azurite URLs: [account]/[container]/[folder/...]/[filename]
        // Azure URLs:   [container]/[folder/...]/[filename]

        bool isAzurite = uri.Host.Contains("127.0.0.1") || uri.Host.Contains("localhost");
        int containerIndex = isAzurite ? 1 : 0;

        if (segments.Length <= containerIndex)
            throw new InvalidOperationException("Invalid blob URL format.");

        string containerName = segments[containerIndex];
        string blobName = string.Join('/', segments.Skip(containerIndex + 1));

        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        await blobClient.DeleteIfExistsAsync();
    }

    public string GenerateContainerSasToken(
        string containerName,
        BlobContainerSasPermissions permissions,
        TimeSpan? expiryDuration = null,
        string? storedPolicyName = null)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

        if (!containerClient.CanGenerateSasUri)
        {
            throw new InvalidOperationException("Can't generate SAS uri");
        }

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = containerName,
            // "c" for container
            Resource = "c",
        };

        if (!string.IsNullOrWhiteSpace(storedPolicyName))
        {
            sasBuilder.Identifier = storedPolicyName;
        }
        else
        {
            sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.Add(expiryDuration ?? TimeSpan.FromHours(1));
            sasBuilder.SetPermissions(permissions);
        }

        Uri sasUri = containerClient.GenerateSasUri(sasBuilder);
        return sasUri.Query.TrimStart('?');
    }

    public string GenerateBlobSasToken(string blobName, BlobContainerSasPermissions permissions, TimeSpan? expiryDuration = null, string? storedPolicyName = null)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(blobName);

        if (!containerClient.CanGenerateSasUri)
        {
            throw new InvalidOperationException("Can't generate SAS uri");
        }

        var sasBuilder = new BlobSasBuilder
        {
            BlobName = blobName,
            // "b" for blob
            Resource = "b",
        };

        if (!string.IsNullOrWhiteSpace(storedPolicyName))
        {
            sasBuilder.Identifier = storedPolicyName;
        }
        else
        {
            sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.Add(expiryDuration ?? TimeSpan.FromHours(1));
            sasBuilder.SetPermissions(permissions);
        }

        Uri sasUri = containerClient.GenerateSasUri(sasBuilder);
        return sasUri.Query.TrimStart('?');
    }
}

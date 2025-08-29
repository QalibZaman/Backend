using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
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
         bool overwrite = true,
         string? contentType = null)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        string prefix = category.GetDirectory();
        var blobClient = containerClient.GetBlobClient($"{prefix}{fileName}");

        var headers = new BlobHttpHeaders
        {
            ContentType = contentType ?? "application/octet-stream"
        };

        await blobClient.UploadAsync(
            stream,
            new BlobUploadOptions { HttpHeaders = headers },
            cancellationToken: default);

        return blobClient.Uri.ToString();
    }

    public string GetBlobUrl(string containerName, string blobName)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);
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

    public async Task DeleteBlobAsync(string blobName)
    {
        if (string.IsNullOrWhiteSpace(blobName))
            return;

        var containerClient = blobServiceClient.GetBlobContainerClient("dot-net");
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

    public string GenerateBlobSasToken(
        string containerName,
        string blobName,
        BlobSasPermissions permissions,
        TimeSpan? expiryDuration = null,
        string? storedPolicyName = null)
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        if (!blobClient.CanGenerateSasUri)
        {
            throw new InvalidOperationException("Can't generate SAS URI for the blob.");
        }

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = containerName,
            BlobName = blobName,
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

        Uri sasUri = blobClient.GenerateSasUri(sasBuilder);
        return sasUri.Query.TrimStart('?');
    }
}

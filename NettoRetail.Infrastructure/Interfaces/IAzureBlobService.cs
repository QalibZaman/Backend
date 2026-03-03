using Azure.Storage.Sas;
using NettoRetail.Shared.Enums;

namespace NettoRetail.Infrastructure.Interfaces;

public interface IAzureBlobService
{
    Task<string> UploadAsync(string fileName, Stream stream, string containerName, BlobContentCategoryEnum category,  bool overwrite = true, string? contentType = null);

    Task DeleteByUrlAsync(string blobUrl, CancellationToken cancellationToken = default);

    Task DeleteBlobAsync(string blobName);

    string GetBlobUrl(string containerName, string blobName);

    string GenerateContainerSasToken(
        string containerName,
        BlobContainerSasPermissions permissions,
        TimeSpan? expiryDuration = null,
        string? storedPolicyName = null);

    string GenerateBlobSasToken(
        string containerName,
        string blobName,
        BlobSasPermissions permissions,
        TimeSpan? expiryDuration = null,
        string? storedPolicyName = null);

}

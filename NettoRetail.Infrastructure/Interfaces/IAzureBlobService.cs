using NettoRetail.Shared.Enums;

namespace NettoRetail.Infrastructure.Interfaces;

public interface IAzureBlobService
{
    Task<string> UploadAsync(string fileName, Stream stream, string containerName, BlobContentCategoryEnum category, bool overwrite = true);
    Task DeleteByUrlAsync(string blobUrl);
}

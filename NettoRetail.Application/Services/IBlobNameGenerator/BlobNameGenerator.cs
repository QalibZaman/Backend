using NettoRetail.Shared.Enums;

namespace NettoRetail.Application.Services.IBlobNameGenerator;

public class BlobNameGenerator : IBlobNameGenerator
{
    public string GenerateBlobName(string fileName, int shopId, bool isPrivate)
    {
        string resultFileName = $"{Guid.NewGuid()}-{shopId}-{(isPrivate ? 1 : 0)}{Path.GetExtension(fileName)}";
        return resultFileName;
    }
}

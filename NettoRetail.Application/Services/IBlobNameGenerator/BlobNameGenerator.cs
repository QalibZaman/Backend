using NettoRetail.Shared.Enums;

namespace NettoRetail.Application.Services.IBlobNameGenerator;

public class BlobNameGenerator : IBlobNameGenerator
{
    public string GenerateBlobName(string fileName, ImageCategoriesEnum imageCategory, int shopId, bool isPrivate)
    {
        string resultFileName = $"{ImageCategoriesEnum.Brands.GetDirectory()}/{Guid.NewGuid()}-{shopId}-{(isPrivate ? 1 : 0)}{Path.GetExtension(fileName)}";
        return resultFileName;
    }
}

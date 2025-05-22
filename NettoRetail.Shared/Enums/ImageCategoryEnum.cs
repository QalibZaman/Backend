namespace NettoRetail.Shared.Enums;

public enum ImageCategoryEnum
{
    BrandLogos,

}

public static class ImageCategoryEnumExtensions
{
    public static string GetDirectory(this ImageCategoryEnum category)
    {
        return category switch
        {
            ImageCategoryEnum.BrandLogos => "brand-logos/",
            _ => "misc/"
        };
    }
}
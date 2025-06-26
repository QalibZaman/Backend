namespace NettoRetail.Shared.Enums;

public enum ImageCategoriesEnum
{
    Brands,
}

public static class ImageCategoriesEnumExtensions
{
    public static string GetDirectory(this ImageCategoriesEnum category)
    {
        return category switch
        {
            ImageCategoriesEnum.Brands => "brand-logos/",
            _ => "misc/"
        };
    }
}
namespace NettoRetail.Shared.Enums;

public enum BlobContentCategoryEnum
{
    Image,
    Document,
    Video,
    Other
}

public static class BlobContentCategoryEnumExtensions
{
    public static string GetDirectory(this BlobContentCategoryEnum category)
    {
        return category switch
        {
            BlobContentCategoryEnum.Image => "images/",
            BlobContentCategoryEnum.Document => "docs/",
            BlobContentCategoryEnum.Video => "videos/",
            _ => "misc/"
        };
    }
}
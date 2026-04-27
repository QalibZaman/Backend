using NettoRetail.Application.Models.Response;
using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Utilities;

public static class ContentResponseFactory
{
    public static ContentBlockResponse CreateEmptyBlock(SectionTypeEnum sectionType)
    {
        return new ContentBlockResponse
        {
            Id = 0,
            SectionType = sectionType.ToString(),
            Content = string.Empty,
            IsVisible = false,
            LastUpdated = null,
            UpdatedBy = null,
        };
    }

    public static ContentPreviewResponse CreatePreview(
        int shopId,
        SectionTypeEnum sectionType,
        string content,
        bool isVisible)
    {
        return new ContentPreviewResponse
        {
            ShopId = shopId,
            SectionType = sectionType.ToString(),
            Content = content,
            IsVisible = isVisible,
        };
    }
}

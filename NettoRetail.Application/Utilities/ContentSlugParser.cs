using NettoRetail.Application.Exceptions;
using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Utilities;

public static class ContentSlugParser
{
    public static (int ShopId, SectionTypeEnum SectionType)? ParseContentSlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new BadRequestException("Slug cannot be null or empty.");
        }

        var parts = slug.Split('-');

        if (parts.Length != 2)
        {
            throw new BadRequestException("Slug must be in the format 'shopId-sectionType'.");
        }

        if (!int.TryParse(parts[0], out int shopId))
        {
            throw new BadRequestException("Invalid shopId format.");
        }

        if (!Enum.TryParse<SectionTypeEnum>(parts[1], true, out var sectionType))
        {
            throw new BadRequestException($"Unknown section type: {parts[1]}");
        }

        return (shopId, sectionType);
    }
}

using System;
using NettoRetail.Domain.Enums;

namespace NettoRetail.Domain.Entities;

public class TeaserSectionEntity
{
    public Guid Id { get; set; }
    public int ShopId { get; set; }
    public bool IsVisible { get; set; }

    public string Heading { get; set; }
    public string? AdditionalText { get; set; }

    public string? Link { get; set; }

    public string? TeaserImageUrl { get; set; }

    public TextAlignmentEnum HeadingAlignment { get; set; }
    public TextAlignmentEnum AdditionalTextAlignment { get; set; }

    public string? HeadingColor { get; set; }
    public string? AdditionalTextColor { get; set; }
}

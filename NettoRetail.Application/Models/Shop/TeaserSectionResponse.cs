using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Responses.TeaserSection;

public class TeaserSectionResponse
{
    public bool IsVisible { get; set; }

    public string Heading { get; set; } = default!;
    public string? AdditionalText { get; set; }

    public string? Link { get; set; }

    public string? TeaserImageUrl { get; set; }

    public TextAlignmentEnum HeadingAlignment { get; set; }
    public TextAlignmentEnum AdditionalTextAlignment { get; set; }

    public string? HeadingColor { get; set; }
    public string? AdditionalTextColor { get; set; }
}

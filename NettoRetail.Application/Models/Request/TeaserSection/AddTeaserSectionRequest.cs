using Microsoft.AspNetCore.Http;
using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Requests.TeaserSection;

public class CreateTeaserSectionRequest : ITeaserSectionUpsertRequest
{
    public bool IsVisible { get; set; }

    public string Heading { get; set; }
    public string? AdditionalText { get; set; }
    public string? Link { get; set; }

    public IFormFile? TeaserImage { get; set; }

    public TextAlignmentEnum HeadingAlignment { get; set; }
    public TextAlignmentEnum AdditionalTextAlignment { get; set; }

    public string? HeadingColor { get; set; }
    public string? AdditionalTextColor { get; set; }
}

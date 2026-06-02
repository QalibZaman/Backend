using Microsoft.AspNetCore.Http;
using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Requests.TeaserSection;

public interface ITeaserSectionUpsertRequest
{
     bool IsVisible { get; set; }

     string Heading { get; set; }
     string? AdditionalText { get; set; }
     string? Link { get; set; }

     IFormFile? TeaserImage { get; set; }

     TextAlignmentEnum HeadingAlignment { get; set; }
     TextAlignmentEnum AdditionalTextAlignment { get; set; }

     string? HeadingColor { get; set; }
     string? AdditionalTextColor { get; set; }
}
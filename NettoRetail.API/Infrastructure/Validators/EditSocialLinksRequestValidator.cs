using FluentValidation;
using NettoRetail.Application.Models.Link;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Infrastructure.Validators
{
    public class EditSocialLinksRequestValidator
    : AbstractValidator<EditSocialLinksRequest>
    {
        public EditSocialLinksRequestValidator()
        {
            RuleFor(x => x.Links)
                .Must(HaveUniqueSocialNetworks)
                .WithMessage(ValidationErrorMessages.Shop.InvalidUrlErrorMessage);

            RuleForEach(x => x.Links)
                .SetValidator(new ShopLinkRequestValidator());
        }

        private static bool HaveUniqueSocialNetworks(
            List<ShopLinkRequest> links)
        {
            return links
                .Select(x => x.SocialNetwork)
                .Distinct()
                .Count() == links.Count;
        }
    }
}

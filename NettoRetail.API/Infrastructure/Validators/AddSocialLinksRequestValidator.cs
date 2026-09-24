using FluentValidation;
using NettoRetail.Application.Models.Link;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Infrastructure.Validators
{
    public class AddSocialLinksRequestValidator
    : AbstractValidator<AddSocialLinksRequest>
    {
        public AddSocialLinksRequestValidator()
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

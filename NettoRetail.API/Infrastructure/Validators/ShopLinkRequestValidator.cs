using FluentValidation;
using NettoRetail.Application.Models.Link;
using NettoRetail.Domain.Entities.Enums;
using NettoRetail.Shared.Constants;

namespace NettoRetail.API.Infrastructure.Validators
{
    public class ShopLinkRequestValidator
     : AbstractValidator<ShopLinkRequest>
    {
        private static readonly Dictionary<SocialNetworkType, string[]> AllowedDomains = new()
        {
            [SocialNetworkType.Facebook] = ["facebook.com"],
            [SocialNetworkType.Instagram] = ["instagram.com"],
            [SocialNetworkType.YouTube] = ["youtube.com", "youtu.be"],
            [SocialNetworkType.TikTok] = ["tiktok.com"],
            [SocialNetworkType.X] = ["x.com", "twitter.com"]
        };

        public ShopLinkRequestValidator()
        {
            RuleFor(x => x.SocialNetwork)
                .IsInEnum()
                .WithMessage(ValidationErrorMessages.Shop.InvalidUrlErrorMessage);

            RuleFor(x => x.Url)
                .NotEmpty()
                .WithMessage(ValidationErrorMessages.Shop.InvalidUrlErrorMessage)
                .Must(BeValidUrl)
                .WithMessage(ValidationErrorMessages.Shop.InvalidUrlErrorMessage);

            RuleFor(x => x)
                .Must(HaveValidSocialNetworkDomain)
                .WithMessage(ValidationErrorMessages.Shop.InvalidUrlErrorMessage);
        }

        private static bool BeValidUrl(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                return false;
            }

            return uri.Scheme == Uri.UriSchemeHttp ||
                   uri.Scheme == Uri.UriSchemeHttps;
        }

        private static bool HaveValidSocialNetworkDomain(
            ShopLinkRequest request)
        {
            if (!Uri.TryCreate(
                    request.Url,
                    UriKind.Absolute,
                    out var uri))
            {
                return false;
            }

            if (!AllowedDomains.TryGetValue(
                    request.SocialNetwork,
                    out var allowedDomains))
            {
                return false;
            }

            return allowedDomains.Any(domain =>
                uri.Host.Equals(
                    domain,
                    StringComparison.OrdinalIgnoreCase)
                || uri.Host.EndsWith(
                    $".{domain}",
                    StringComparison.OrdinalIgnoreCase));
        }
    }
}
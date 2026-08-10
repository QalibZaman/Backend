using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using NettoRetail.Application.CQRS.Users.Queries.Onboarding;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;

public class GetOnboardingQueryHandler(
    IStaffRepository staffRepository,
    IHttpContextAccessor httpContextAccessor)
    : IRequestHandler<GetOnboardingQuery, OnboardingResponse?>
{
    public async Task<OnboardingResponse?> Handle(
        GetOnboardingQuery request,
        CancellationToken cancellationToken)
    {
        var userIdClaim = httpContextAccessor.HttpContext?
    .User
    .FindFirst(ClaimTypes.NameIdentifier)?
    .Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            throw new MyUnauthorizedException("User is not authorized.");
        }

        var specification = new GetStaffByIdSpecification(userId);

        var staff = await staffRepository.GetAsync(specification, cancellationToken);

        if (staff is null)
        {
            throw new NotFoundException("Staff account was not found.");
        }

        if (staff.IsOnboardingCompleted)
        {
            return null;
        }

        return new OnboardingResponse
        {
            OnboardingSeen = false,
            Slides =
        [
            new OnboardingSlideResponse
        {
            SlideId = 1,
            Title = OnboardingConstants.Welcome.Title,
            ImageUrl = OnboardingConstants.Welcome.ImageUrl,
            Headline = OnboardingConstants.Welcome.Headline,
            DetailedText = OnboardingConstants.Welcome.DetailedText
        },
        new OnboardingSlideResponse
        {
            SlideId = 2,
            Title = OnboardingConstants.Navigation.Title,
            ImageUrl = OnboardingConstants.Navigation.ImageUrl,
            Headline = OnboardingConstants.Navigation.Headline,
            DetailedText = OnboardingConstants.Navigation.DetailedText
        }
            ]
        };
    }
}

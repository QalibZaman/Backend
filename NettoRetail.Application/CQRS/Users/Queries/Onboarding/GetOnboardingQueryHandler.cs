using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using NettoRetail.Application.CQRS.Users.Queries.Onboarding;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Staff;
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
                    Title = "Onboarding",
                    ImageUrl = "netto.com/onboarding/images/1",
                    Headline = "Welcome",
                    DetailedText = "The RetailTech application is a comprehensive and user-friendly platform that empowers retailers by providing efficient inventory management,\r\n sales tracking, customer relationship management, and analytics capabilities. It aims to revolutionize the way retailers operate by providing a comprehensive set of features\r\n and tools that cater to their specific needs. With this application, RetailTech Inc. empowers retailers to enhance their operational efficiency, improve customer satisfaction,\r\n and drive business growth."
                },
                new OnboardingSlideResponse
                {
                    SlideId = 2,
                    Title = "Navigation",
                    ImageUrl = "netto.com/onboarding/images/2",
                    Headline = "Navigation",
                    DetailedText = "Walls achieving strength di planes clicking junior statements behaviour cm overnight genres rpg band european modem heavy aggregate lanka\r\n therapeutic judgment syracuse airfare accent headquarters privilege nation hull informational upset hood intellectual shape crucial poverty facts"
                }
            ]
        };
    }
}

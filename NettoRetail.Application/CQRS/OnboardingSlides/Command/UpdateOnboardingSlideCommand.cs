using Ganss.Xss;
using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.OnboardingSlides;
using NettoRetail.Application.Models.Response.OnboardingSlides;
using NettoRetail.DataAccess.Specifications.OnboardingSlides;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.OnboardingSlides.Commands;

public record UpdateOnboardingSlideCommand(int Id, UpdateOnboardingSlideRequest Request, Guid UpdatedBy) : IRequest<OnboardingSlideResponse>;

public class UpdateOnboardingSlideCommandHandler(
    IOnboardingSlideRepository slideRepository
) : IRequestHandler<UpdateOnboardingSlideCommand, OnboardingSlideResponse>
{
    public async Task<OnboardingSlideResponse> Handle(UpdateOnboardingSlideCommand request, CancellationToken cancellationToken)
    {
        var spec = new GetOnboardingSlideByIdSpecification(request.Id);
        var slide = await slideRepository.GetAsync(spec, cancellationToken)
            ?? throw new NotFoundException($"Onboarding slide with ID {request.Id} not found.");

        var sanitizer = new HtmlSanitizer();
        slide.Title = request.Request.Title;
        slide.Content = sanitizer.Sanitize(request.Request.Content);
        slide.IsVisible = request.Request.IsVisible;
        slide.UpdatedAt = DateTime.UtcNow;
        slide.UpdatedBy = request.UpdatedBy;

        await slideRepository.UpdateAsync(slide, cancellationToken);

        return slide.Adapt<OnboardingSlideResponse>();
    }
}

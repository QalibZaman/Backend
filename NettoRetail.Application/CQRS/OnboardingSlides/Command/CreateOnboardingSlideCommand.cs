using Ganss.Xss;
using MapsterMapper;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.OnboardingSlides;
using NettoRetail.Application.Models.Response.OnboardingSlides;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.OnboardingSlides.Commands;

public record CreateOnboardingSlideCommand(CreateOnboardingSlideRequest Request, Guid CreatedBy) : IRequest<OnboardingSlideResponse>;

public class CreateOnboardingSlideCommandHandler(
    IOnboardingSlideRepository slideRepository,
    IMapper mapper
) : IRequestHandler<CreateOnboardingSlideCommand, OnboardingSlideResponse>
{
    private const int MaxSlides = 10;

    public async Task<OnboardingSlideResponse> Handle(CreateOnboardingSlideCommand request, CancellationToken cancellationToken)
    {
        var slides = await slideRepository.GetAllAsync(cancellationToken);
        if (slides.Count() >= MaxSlides)
            throw new ConflictException("Maximum number of slides reached");

        int nextOrder = slides.Count() == 0 ? 0 : slides.Max(x => x.Order) + 1;

        var sanitizer = new HtmlSanitizer();
        var safeContent = sanitizer.Sanitize(request.Request.Content);

        var newSlide = new OnboardingSlideEntity
        {
            Title = request.Request.Title,
            Content = safeContent,
            Order = nextOrder,
            IsVisible = request.Request.IsVisible,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.CreatedBy,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = request.CreatedBy,
        };

        var result = await slideRepository.AddAsync(newSlide, cancellationToken);

        return mapper.Map<OnboardingSlideResponse>(result);
    }
}

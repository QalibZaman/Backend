using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.OnboardingSlides;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.OnboardingSlides.Commands;

public record DeleteOnboardingSlideCommand(int Id, Guid DeletedBy) : IRequest;

public class DeleteOnboardingSlideCommandHandler(
IOnboardingSlideRepository slideRepository) : IRequestHandler<DeleteOnboardingSlideCommand>
{
    public async Task Handle(DeleteOnboardingSlideCommand request, CancellationToken cancellationToken)
    {
        var slides = await slideRepository.GetAllAsync(cancellationToken);

        if (slides.Count() <= 1)
            throw new ConflictException("At least one onboarding slide must remain.");

        var spec = new GetOnboardingSlideByIdSpecification(request.Id);
        var slide = await slideRepository.GetAsync(spec, cancellationToken)
            ?? throw new NotFoundException($"Onboarding slide with ID {request.Id} not found.");

        await slideRepository.DeleteAsync(slide, cancellationToken);
    }
}

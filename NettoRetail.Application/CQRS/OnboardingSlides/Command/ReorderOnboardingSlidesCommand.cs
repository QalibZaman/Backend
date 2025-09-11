using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.OnboardingSlides;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.OnboardingSlides.Commands;

public record ReorderOnboardingSlidesCommand(ReorderOnboardingSlidesRequest Request) : IRequest;

public class ReorderOnboardingSlidesCommandHandler(
    IOnboardingSlideRepository slideRepository
) : IRequestHandler<ReorderOnboardingSlidesCommand>
{
    public async Task Handle(ReorderOnboardingSlidesCommand request, CancellationToken cancellationToken)
    {
        var slides = await slideRepository.GetAllAsync(cancellationToken);
        var requestIds = request.Request.Slides.Select(x => x.Id).ToHashSet();

        var allSlideIds = slides.Select(x => x.Id).ToHashSet();
        if (!requestIds.IsSubsetOf(allSlideIds))
            throw new NotFoundException("Some slides not found.");

        var requestedOrders = request.Request.Slides.Select(x => x.Order).ToList();
        if (requestedOrders.Count != requestedOrders.Distinct().Count())
            throw new ConflictException("Order values must be unique.");

        foreach (var item in request.Request.Slides)
        {
            var slide = slides.First(x => x.Id == item.Id);
            slide.Order = item.Order;
        }

        await slideRepository.UpdateBulkAsync(slides, cancellationToken);
    }
}

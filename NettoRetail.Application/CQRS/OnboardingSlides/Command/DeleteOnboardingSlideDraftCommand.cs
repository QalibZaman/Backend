using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.OnboardingSlides;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.OnboardingSlides.Commands;

public record DeleteOnboardingSlideDraftCommand(int? SlideId, Guid UserId) : IRequest;

public class DeleteOnboardingSlideDraftCommandHandler(
    IOnboardingSlideDraftRepository draftRepository
) : IRequestHandler<DeleteOnboardingSlideDraftCommand>
{
    public async Task Handle(DeleteOnboardingSlideDraftCommand request, CancellationToken cancellationToken)
    {
        var spec = new GetOnboardingSlideDraftByUserAndSlideIdSpecification(request.UserId, request.SlideId);
        var draft = await draftRepository.GetAsync(spec, cancellationToken);

        if (draft is null)
            throw new NotFoundException("Draft not found.");

        await draftRepository.DeleteAsync(draft, cancellationToken);
    }
}

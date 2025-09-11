using Ganss.Xss;
using MediatR;
using NettoRetail.Application.Models.Request.OnboardingSlides;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.DataAccess.Specifications.OnboardingSlides;

namespace NettoRetail.Application.CQRS.OnboardingSlides.Commands;

public record CreateOrUpdateOnboardingSlideDraftCommand(CreateOrUpdateOnboardingSlideDraftRequest Request, Guid UserId)
    : IRequest;

public class CreateOrUpdateOnboardingSlideDraftCommandHandler(
    IOnboardingSlideDraftRepository draftRepository
) : IRequestHandler<CreateOrUpdateOnboardingSlideDraftCommand>
{
    public async Task Handle(CreateOrUpdateOnboardingSlideDraftCommand request, CancellationToken cancellationToken)
    {
        var spec = new GetOnboardingSlideDraftByUserAndSlideIdSpecification(request.UserId, request.Request.SlideId);
        var existingDraft = await draftRepository.GetAsync(spec, cancellationToken);

        var sanitizer = new HtmlSanitizer();
        var safeContent = sanitizer.Sanitize(request.Request.Content);

        if (existingDraft is not null)
        {
            existingDraft.Title = request.Request.Title;
            existingDraft.Content = safeContent;
            existingDraft.DateSaved = DateTime.UtcNow;

            await draftRepository.UpdateAsync(existingDraft, cancellationToken);
        }
        else
        {
            var newDraft = new OnboardingSlideDraftEntity
            {
                SlideId = request.Request.SlideId,
                UserId = request.UserId,
                Title = request.Request.Title,
                Content = safeContent,
                DateSaved = DateTime.UtcNow,
            };

            await draftRepository.AddAsync(newDraft, cancellationToken);
        }
    }
}

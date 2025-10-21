using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.Staff;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Users.Commands;

public record UpdateCommunicationPreferencesCommand(Guid UserId, UpdateCommunicationPreferencesRequest Model) : IRequest;
public class UpdateCommunicationCommandPreferencesHandler(ICommunicationPreferencesRepository communicationPreferencesRepository) : IRequestHandler<UpdateCommunicationPreferencesCommand>
{
    public async Task Handle(UpdateCommunicationPreferencesCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetCommunicationPreferencesByAccountIdSpecification(request.UserId);

        var communicationPreferencesToUpdate = await communicationPreferencesRepository.GetAsync(specification, cancellationToken)
            ?? throw new NotFoundException("Preferences not found");

        request.Model.Adapt(communicationPreferencesToUpdate);

        await communicationPreferencesRepository.UpdateAsync(communicationPreferencesToUpdate, cancellationToken);
    }
}

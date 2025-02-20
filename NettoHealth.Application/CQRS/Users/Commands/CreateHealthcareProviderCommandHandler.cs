using Mapster;
using MediatR;
using NettoHealth.Application.Models.Entities;
using NettoHealth.Domain.Entities;
using NettoHealth.Shared.Interfaces.Repositories;
using NettoHealth.Shared.Utilities;

namespace NettoHealth.Application.CQRS.Users.Commands;

public record CreateHealthcareProviderCommand(HealthcareProviderRegistrationModel Model) : IRequest;

public class CreateHealthcareProviderCommandHandler(IHealthcareProviderRepository healthcareProviderRepository)
    : IRequestHandler<CreateHealthcareProviderCommand>
{
    public async Task Handle(CreateHealthcareProviderCommand request, CancellationToken cancellationToken)
    {
        var healthcareProvider = request.Model.Adapt<HealthcareProviderEntity>();
        healthcareProvider.PasswordHash = PasswordHasher.HashPassword(healthcareProvider.PasswordHash);

        await healthcareProviderRepository.AddAsync(healthcareProvider, cancellationToken);
    }
}
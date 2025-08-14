using Mapster;
using MediatR;
using NettoRetail.Application.Models.Response;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Users.Queries.Staff;

public record GetAllPlatformAdminsQuery() : IRequest<List<PlatformAdminResponse>>;

public class GetAllPlatformAdminsQueryHandler(IAccountRepository accountRepo)
    : IRequestHandler<GetAllPlatformAdminsQuery, List<PlatformAdminResponse>>
{
    public async Task<List<PlatformAdminResponse>> Handle(GetAllPlatformAdminsQuery request, CancellationToken cancellationToken)
    {
        var specBuilder = new SpecificationBuilder<AccountEntity>();

        specBuilder.AddInclude(a => a.Sessions!);
        specBuilder.AddInclude(a => a.Role);
        specBuilder.AddInclude(a => a.Role.PermissionRoles);
        specBuilder.AddCriteria(a => a.Role.Name.StartsWith("PlatformAdmin"));

        var specification = specBuilder
            .AddOrderBy(a => a.DateCreated, isDesc: true)
            .Build();

        var accounts = await accountRepo.GetAllBySpecAsync(specification, cancellationToken);

        var response = accounts.Adapt<List<PlatformAdminResponse>>();

        return response;
    }
}
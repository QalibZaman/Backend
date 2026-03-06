using Mapster;
using MediatR;
using NettoRetail.Application.Models.Response;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Users.Queries.Staff;

public record GetAllPlatformAdminsQuery() : IRequest<List<PlatformAdminResponse>>;

public class GetAllPlatformAdminsQueryHandler(IStaffRepository accountRepo)
    : IRequestHandler<GetAllPlatformAdminsQuery, List<PlatformAdminResponse>>
{
    public async Task<List<PlatformAdminResponse>> Handle(GetAllPlatformAdminsQuery request, CancellationToken cancellationToken)
    {
        var specification = new SpecificationBuilder<StaffEntity>()
            .AddOrderBy(a => a.DateCreated, isDesc: true)
            .AddCriteria(
                a => a.Role.Name.StartsWith(PlatformAdminConstants.PlatformAdminPrefix))
            .AddInclude(a => a.Sessions!)
            .AddInclude(a => a.Role)
            .AddInclude(a => a.Role.PermissionRoles)
            .Build();

        var accounts = await accountRepo.GetAllBySpecAsync(specification, cancellationToken);

        var response = accounts.Adapt<List<PlatformAdminResponse>>();

        return response;
    }
}
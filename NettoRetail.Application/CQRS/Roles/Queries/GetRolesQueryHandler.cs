using Mapster;
using MediatR;
using NettoRetail.Application.Models.Request.Role;
using NettoRetail.Application.Models.Response;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Roles.Queries;

public record GetRolesQuery(int PageNumber, int PageSize) : IRequest<PaginatedResponse<RoleResponse>>;

public class GetRolesQueryHandler(IRoleRepository roleRepository)
    : IRequestHandler<GetRolesQuery, PaginatedResponse<RoleResponse>>
{
    public async Task<PaginatedResponse<RoleResponse>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        var specificationBuilder = new SpecificationBuilder<RoleEntity>();
        var getSpecification = specificationBuilder
            .Skip(request.PageNumber * request.PageSize)
            .Take(request.PageSize)
            .Build()
            .AddInclude(r => r.Accounts);

        var roles = await roleRepository.GetAllPaginatedAsync(getSpecification, cancellationToken);
        var totalAmount = await roleRepository.GetAmountAsync(cancellationToken);

        var responseCollection = roles.Adapt<List<RoleResponse>>();

        var responseModel = new PaginatedResponse<RoleResponse>
        {
            Items = responseCollection,
            TotalAmount = totalAmount
        };

        return responseModel;
    }
}
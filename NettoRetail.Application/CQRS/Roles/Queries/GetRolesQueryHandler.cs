using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.Role;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Models.Response.Roles;
using NettoRetail.Application.Utilities;
using NettoRetail.DataAccess.Specifications.Role;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Roles.Queries;

public record GetRolesQuery(RoleSearchModel Model, int ShopId) : IRequest<PaginatedResponse<RoleResponse>>;

public class GetRolesQueryHandler(IRoleRepository roleRepository, IShopRepository shopRepository)
    : IRequestHandler<GetRolesQuery, PaginatedResponse<RoleResponse>>
{
    public async Task<PaginatedResponse<RoleResponse>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        var shopExists = await shopRepository.Exists(new GetShopByIdSpecification(request.ShopId), cancellationToken);
        if (!shopExists)
        {
            throw new NotFoundException($"No shop with Id: {request.ShopId}");
        }

        var (items, totalAmount) = await roleRepository.GetAllPaginatedAsync(GetRoleFilterSpecification(request), cancellationToken);

        var responseCollection = items
            .Select(r => new RoleResponse
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description ?? string.Empty,
                UserCount = r.Accounts.Count(a => a.ShopId == request.ShopId),
                IsEditable = r.Id != RoleConstants.Admin,
                IsDeletable = r.Id != RoleConstants.Admin
            })
            .ToList();

        var responseModel = new PaginatedResponse<RoleResponse>
        {
            Items = responseCollection,
            TotalAmount = totalAmount
        };

        return responseModel;
    }

    private static Specification<RoleEntity> GetRoleFilterSpecification(GetRolesQuery request)
    {
        var specificationBuilder = new SpecificationBuilder<RoleEntity>();
        var skip = PaginationHelper.CalculateSkip(request.Model.PageNumber, request.Model.PageSize);

        if (request.Model.Name is not null)
        {
            specificationBuilder
                .AddCriteria((role) => role.Name.ToLower()
                    .Contains(request.Model.Name.ToLower()));
        }

        return specificationBuilder
            .Add<GetRolesByShopIdSpecification>(request.ShopId)
            .Skip(skip)
            .Take(request.Model.PageSize)
            .AddInclude(r => r.Accounts)
            .Build();
    }
}

using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Request.Role;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Role;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Roles.Queries;

public record GetRolesQuery(PaginationRequest Model, int ShopId) : IRequest<PaginatedResponse<RoleResponse>>;

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

        var specificationBuilder = new SpecificationBuilder<RoleEntity>();
        var skip = (request.Model.PageNumber - 1) * request.Model.PageSize;
        var getSpecification = specificationBuilder
            .Add<GetRolesByShopIdSpecification>(request.ShopId)
            .Skip(skip)
            .Take(request.Model.PageSize)
            .Build()
            .AddInclude(r => r.Accounts);

        var (items, totalAmount) = await roleRepository.GetAllPaginatedAsync(getSpecification, cancellationToken);

        var responseCollection = items
            .Select(r => new RoleResponse
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                UserCount = r.Accounts.Count(a => a.ShopId == request.ShopId)
            })
            .ToList();

        var responseModel = new PaginatedResponse<RoleResponse>
        {
            Items = responseCollection,
            TotalAmount = totalAmount
        };

        return responseModel;
    }
}
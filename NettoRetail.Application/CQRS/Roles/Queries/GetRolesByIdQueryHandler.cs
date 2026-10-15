using MediatR;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.IdentityModel.Abstractions;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Utilities;
using NettoRetail.DataAccess.Repositories;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Roles.Queries
{
    public class GetRolesByIdQueryHandler(IRoleRepository roleRepository, IShopRepository shopRepository)
        : IRequestHandler<GetRolesQueryById, RoleResponse>
    {
        public async Task<RoleResponse> Handle(GetRolesQueryById request, CancellationToken cancellationToken)
        {
            var shopExists = await shopRepository.ExistsAsync(new GetShopByIdSpecification(request.ShopId), cancellationToken);
            var roleExists = await roleRepository.ExistsAsync(GetRoleFilterSpecification(request), cancellationToken);
            if (!roleExists || !shopExists)
                throw new NotFoundException($"Role with provided roleId and shopId not found");

            var (items, totalAmount) = await roleRepository.GetAllPaginatedAsync(GetRoleFilterSpecification(request), cancellationToken);

            var responseItem = items
                .Select(r => new RoleResponse
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description ?? string.Empty,
                    UserCount = r.StaffMembers.Count(a => a.ShopId == request.ShopId),
                    IsEditable = r.Id != RoleConstants.Admin && r.Id != RoleConstants.SuperAdmin,
                    IsDeletable = r.Id != RoleConstants.Admin && r.Id != RoleConstants.SuperAdmin
                }).ToList();

            if (responseItem is not null)
            {
                return responseItem.First();
            }

            throw new NotFoundException($"Role with id = {request.id} not found");
        }

        private static Specification<RoleEntity> GetRoleFilterSpecification(GetRolesQueryById request)
        {
            var specificationBuilder = new SpecificationBuilder<RoleEntity>();

            specificationBuilder.
                AddCriteria((role) => role.Id.Equals(request.id))
                .AddCriteria((role) => role.ShopId == request.ShopId);

            return specificationBuilder
                .Build();
        }
    }
}

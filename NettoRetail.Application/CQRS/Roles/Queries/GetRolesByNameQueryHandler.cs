using Bogus.DataSets;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Repositories;
using NettoRetail.DataAccess.Specifications.Shop;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Roles.Queries
{
    public class GetRolesByNameQueryHandler(IRoleRepository roleRepository, IShopRepository shopRepository)
        : IRequestHandler<GetRolesQueryByName, RoleResponse>
    {
        public async Task<RoleResponse> Handle(GetRolesQueryByName request, CancellationToken cancellationToken)
        {
            var shopExists = await shopRepository.ExistsAsync(new GetShopByIdSpecification(request.ShopId), cancellationToken);
            var roleExists = await roleRepository.ExistsAsync(GetRoleFilterSpecification(request), cancellationToken);
            if (!roleExists || !shopExists)
                throw new NotFoundException($"Role with name = {request.Name} and shopId = {request.ShopId} not found");

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
            return responseItem.First();
        }

        private static Specification<RoleEntity> GetRoleFilterSpecification(GetRolesQueryByName request)
        {
            var specificationBuilder = new SpecificationBuilder<RoleEntity>();

            specificationBuilder
               .AddCriteria((role) => role.Name.ToLower()
                   .Equals(request.Name.ToLower()))
                   .AddCriteria((role) => role.ShopEntityId == request.ShopId);

            return specificationBuilder
                .Build();
        }
    }
}

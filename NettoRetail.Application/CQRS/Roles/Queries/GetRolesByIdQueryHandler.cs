using MediatR;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.IdentityModel.Abstractions;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Utilities;
using NettoRetail.DataAccess.Repositories;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Roles.Queries
{
    public class GetRolesQueryPaginatedHandler(IRoleRepository roleRepository) : IRequestHandler<GetRolesQueryById, RoleResponse>
    {
        public async Task<RoleResponse> Handle(GetRolesQueryById request, CancellationToken cancellationToken)
        {
            var (items, totalAmount) = await roleRepository.GetAllPaginatedAsync(GetRoleFilterSpecification(request.id), cancellationToken);

            var responseItem = items
                .Select(r => new RoleResponse
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description ?? string.Empty
                }).ToList().First();

            if (responseItem is not null)
            {
                return responseItem;
            }

            throw new NotFoundException($"Role with id = {request.id} not found");
        }

        private static Specification<RoleEntity> GetRoleFilterSpecification(int id)
        {
            var specificationBuilder = new SpecificationBuilder<RoleEntity>();

            specificationBuilder.
                AddCriteria((role) => role.Id.Equals(id));

            return specificationBuilder
                .Build();
        }
    }
}

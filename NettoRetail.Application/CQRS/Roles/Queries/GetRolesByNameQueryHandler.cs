using Bogus.DataSets;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Repositories;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Roles.Queries
{
    public class GetRolesByNameQueryHandler(IRoleRepository roleRepository) : IRequestHandler<GetRolesQueryByName, RoleResponse>
    {
        public async Task<RoleResponse> Handle(GetRolesQueryByName request, CancellationToken cancellationToken)
        {
            var (items, totalAmount) = await roleRepository.GetAllPaginatedAsync(GetRoleFilterSpecification(request.Name), cancellationToken);

            var responseItem = items
                .Select(r => new RoleResponse
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description ?? string.Empty,
                    IsEditable = r.Id != RoleConstants.Admin && r.Id != RoleConstants.SuperAdmin,
                    IsDeletable = r.Id != RoleConstants.Admin && r.Id != RoleConstants.SuperAdmin
                }).ToList().First();
            if (responseItem is not null)
            {
                return responseItem;
            }

            throw new NotFoundException($"Role with name {request.Name} not found");
        }

        private static Specification<RoleEntity> GetRoleFilterSpecification(string name)
        {
            var specificationBuilder = new SpecificationBuilder<RoleEntity>();

            specificationBuilder
               .AddCriteria((role) => role.Name.ToLower()
                   .Equals(name.ToLower()));

            return specificationBuilder
                .Build();
        }
    }
}

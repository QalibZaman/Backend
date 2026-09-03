using MediatR;
using Microsoft.Azure.Cosmos.Linq;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Utilities;
using NettoRetail.DataAccess.Repositories;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Users.Queries.Role
{
    public class GetRolesQueryPaginatedHandler(IRoleRepository roleRepository) : IRequestHandler<GetRolesQueryPaginated, PaginatedResponse<RoleResponse>>
    {
        public async Task<PaginatedResponse<RoleResponse>> Handle(GetRolesQueryPaginated request, CancellationToken cancellationToken)
        {
            var (items, totalAmount) = await roleRepository.GetAllPaginatedAsync(GetRoleFilterSpecification(request), cancellationToken);

            var responseCollection = items
                .Select(r => new RoleResponse
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description ?? string.Empty
                }).ToList();
            var responseModel = new PaginatedResponse<RoleResponse>
            {
                Items = responseCollection,
                TotalAmount = responseCollection.Count
            };
            return responseModel;
        }

        private static Specification<RoleEntity> GetRoleFilterSpecification(GetRolesQueryPaginated request)
        {
            var specificationBuilder = new SpecificationBuilder<RoleEntity>();
            int skip = PaginationHelper.CalculateSkip(request.Model.PageNumber, request.Model.PageSize);

            if (request.Model.Name is not null)
            {
                specificationBuilder
                    .AddCriteria((role) => role.Name.ToLower()
                        .Contains(request.Model.Name.ToLower()));
            }

            if (request.Model.Id is not null)
            {
                specificationBuilder.
                    AddCriteria((role) => role.Id.Equals(request.Model.Id));
            }

            return specificationBuilder
                .Skip(skip)
                .Take(request.Model.PageSize)
                .Build();
        }
    }
}

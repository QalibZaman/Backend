using MediatR;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Utilities;
using NettoRetail.DataAccess.Repositories;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Users.Queries.Role
{
    public class GetAllRolesQueryHandler(IRoleRepository roleRepository) : IRequestHandler<GetRolesQuery, PaginatedResponse<RoleResponse>>
    {
        public async Task<PaginatedResponse<RoleResponse>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
        {
            var (items, totalAmount) = await roleRepository.GetAllPaginatedAsync(GetRoleFilterSpecification(request), cancellationToken);

            var responseCollection = items
                .Select(r => new RoleResponse
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description ?? string.Empty,
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

            return specificationBuilder
                .Skip(skip)
                .Take(request.Model.PageSize)
                .Build();
        }
    }
}

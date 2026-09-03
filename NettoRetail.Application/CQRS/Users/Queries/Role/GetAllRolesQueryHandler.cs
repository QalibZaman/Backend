using MediatR;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Repositories;

namespace NettoRetail.Application.CQRS.Users.Queries.Role
{
    public class GetAllRolesQueryHandler(IRoleRepository roleRepository) : IRequestHandler<GetRolesQuery, List<RoleResponse>>
    {
        public async Task<List<RoleResponse>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
        {
            var items = await roleRepository.GetAllAsync(cancellationToken);

            var responseCollection = items
                .Select(r => new RoleResponse
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description ?? string.Empty
                }).ToList();

            return responseCollection;
        }
    }
}

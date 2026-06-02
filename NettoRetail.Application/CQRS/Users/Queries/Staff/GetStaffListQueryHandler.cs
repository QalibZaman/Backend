using System.Linq.Expressions;
using MapsterMapper;
using MediatR;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Request.Staff;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Utilities;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Users.Queries.Staff;

public record GetStaffListQuery(
    PaginationRequest PaginationModel,
    GetFilteredStaffRequest Model,
    int ShopId) : IRequest<PaginatedResponse<StaffListResponse>>;

public class GetStaffListQueryHandler(
    IStaffRepository staffRepository,
    IMapper mapper) : IRequestHandler<GetStaffListQuery, PaginatedResponse<StaffListResponse>>
{
    private static readonly Dictionary<string, Expression<Func<StaffEntity, object>>> SortingExpressions = new()
    {
        ["firstname"] = item => item.FirstName,
        ["lastname"] = item => item.LastName,
        ["email"] = item => item.Email,
        ["role"] = item => item.Role.Name,
        ["status"] = item => item.Status
    };

    public async Task<PaginatedResponse<StaffListResponse>> Handle(GetStaffListQuery request, CancellationToken cancellationToken)
    {
        var specificationBuilder = BuildSpecification(request);

        var (items, totalCount) = await staffRepository.GetStaffAccountsPaginated(specificationBuilder.Build(), cancellationToken);

        return new PaginatedResponse<StaffListResponse>
        {
            Items = mapper.Map<ICollection<StaffListResponse>>(items),
            TotalAmount = totalCount
        };
    }

    private static SpecificationBuilder<StaffEntity> BuildSpecification(GetStaffListQuery request)
    {
        var filter = request.Model;
        var pagination = request.PaginationModel;

        var builder = new SpecificationBuilder<StaffEntity>()
            .Add<GetStaffByShopIdSpecification>(request.ShopId)
            .Add<GetStaffByPartialFirstNameSpecification>(request.Model.FirstName)
            .Add<GetStaffByPartialLastNameSpecification>(request.Model.LastName)
            .Add<GetStaffByPartialEmailSpecification>(request.Model.Email)
            .Add<GetStaffByRoleIdSpecification>(request.Model.RoleId)
            .AddInclude(x => x.Role);

        if (Enum.TryParse(request.Model.Status, true, out StaffStatusEnum status))
        {
            builder.Add<GetStaffByStatusSpecification>(status);
        }

        foreach (var sort in filter.SortingOptions ?? [])
        {
            var expr = SortingExpressions.GetValueOrDefault(sort.Field) ?? (x => x.FirstName);
            builder.AddOrderBy(expr, sort.IsDescending);
        }

        builder
            .Skip(PaginationHelper.CalculateSkip(pagination.PageNumber, pagination.PageSize))
            .Take(pagination.PageSize)
            .Build();

        return builder;
    }
}

using Mapster;
using MediatR;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Users.Queries.Staff;

public record GetStaffListQuery(GetStaffListRequest? Model, int ShopId) : IRequest<PaginatedResponse<StaffListResponse>>;

public class GetStaffListQueryHandler(IAccountRepository accountRepository)
    : IRequestHandler<GetStaffListQuery, PaginatedResponse<StaffListResponse>>
{
    public async Task<PaginatedResponse<StaffListResponse>> Handle(GetStaffListQuery request, CancellationToken cancellationToken)
    {
        var specificationBuilder = new SpecificationBuilder<AccountEntity>();
        var pageSize = request.Model?.PageSize ?? PaginationConstants.DefaultPageSize;
        var skip = request.Model?.PageNumber * pageSize ?? 0;
        var getSpecification = specificationBuilder
            .Add<GetAccountsByShopIdSpecification>(request.ShopId)
            .Skip(skip)
        .Take(pageSize)
        .Build()
        .AddInclude(r => r.Role);

        var staff = await accountRepository.GetAllPaginatedAsync(getSpecification, cancellationToken);
        var (items, totalAmount) = staff;

        var responseCollection = items.Adapt<List<StaffListResponse>>();

        var responseModel = new PaginatedResponse<StaffListResponse>
        {
            Items = responseCollection,
            TotalAmount = totalAmount
        };

        return responseModel;
    }
}
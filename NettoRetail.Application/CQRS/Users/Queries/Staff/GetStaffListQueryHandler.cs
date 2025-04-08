using Mapster;
using MediatR;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Users.Queries.Staff;

public record GetStaffListQuery(PaginationRequest Model, int ShopId) : IRequest<PaginatedResponse<StaffListResponse>>;

public class GetStaffListQueryHandler(IAccountRepository accountRepository)
    : IRequestHandler<GetStaffListQuery, PaginatedResponse<StaffListResponse>>
{
    public async Task<PaginatedResponse<StaffListResponse>> Handle(GetStaffListQuery request, CancellationToken cancellationToken)
    {
        var specificationBuilder = new SpecificationBuilder<AccountEntity>();
        var skip = request.Model.PageNumber * request.Model.PageSize;
        var getSpecification = specificationBuilder
            .Add<GetAccountsByShopIdSpecification>(request.ShopId)
            .Skip(skip)
        .Take(request.Model.PageSize)
        .Build()
        .AddInclude(r => r.Role);

        var (items, totalAmount) = await accountRepository.GetAllPaginatedAsync(getSpecification, cancellationToken);

        var responseCollection = items.Adapt<List<StaffListResponse>>();

        var responseModel = new PaginatedResponse<StaffListResponse>
        {
            Items = responseCollection,
            TotalAmount = totalAmount
        };

        return responseModel;
    }
}
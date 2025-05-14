using System.Linq.Expressions;
using Mapster;
using MediatR;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Request.Staff;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Users.Queries.Staff;

public record GetStaffListQuery(PaginationRequest PaginationModel, GetFilteredStaffRequest Model, int ShopId) : IRequest<PaginatedResponse<StaffListResponse>>;

public class GetStaffListQueryHandler(IAccountRepository accountRepository)
    : IRequestHandler<GetStaffListQuery, PaginatedResponse<StaffListResponse>>
{
    public async Task<PaginatedResponse<StaffListResponse>> Handle(GetStaffListQuery request, CancellationToken cancellationToken)
    {
        var specificationBuilder = new SpecificationBuilder<AccountEntity>();

        specificationBuilder.Add<GetAccountsByShopIdSpecification>(request.ShopId)
            .Add<GetAccountsByPartialFirstNameSpecification>(request.Model.FirstName)
            .Add<GetAccountsByPartialLastNameSpecification>(request.Model.LastName)
            .Add<GetAccountsByPartialEmailSpecification>(request.Model.Email)
            .Add<GetAccountsByRoleIdSpecification>(request.Model.RoleId)
            .AddInclude(x => x.Role);

        if (Enum.TryParse(request.Model.Status, true, out StaffStatusEnum status))
        {
            specificationBuilder.Add<GetAccountsByStatusSpecification>(status);
        }

        if (request.Model.SortingOptions != null && request.Model.SortingOptions.Any())
        {
            foreach (var sortingOption in request.Model.SortingOptions)
            {
                specificationBuilder.AddOrderBy(GetSortingExpression(sortingOption.Field), sortingOption.IsDescending);
            }
        }

        specificationBuilder.Skip((request.PaginationModel.PageNumber - 1) * request.PaginationModel.PageSize)
            .Take(request.PaginationModel.PageSize);

        var staffEntitiesTuple = await accountRepository.GetStaffAccountsPaginated(specificationBuilder.Build(), cancellationToken);
        var staffCollection = staffEntitiesTuple.Items.Adapt<ICollection<StaffListResponse>>();

        var paginatedResponse = new PaginatedResponse<StaffListResponse>()
        {
            Items = staffCollection,
            TotalAmount = staffEntitiesTuple.TotalCount
        };

        return paginatedResponse;
    }

    private static Expression<Func<AccountEntity, object>> GetSortingExpression(string fieldName)
    {
        return fieldName.ToLower() switch
        {
            "firstname" => item => item.FirstName,
            "lastname" => item => item.LastName,
            "email" => item => item.Email,
            "role" => item => item.Role.Name,
            "status" => item => item.Status,
            _ => item => item.FirstName
        };
    }
}

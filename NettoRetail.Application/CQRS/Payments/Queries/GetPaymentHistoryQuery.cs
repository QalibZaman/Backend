using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Payments.Queries;

public record GetPaymentHistoryQuery(
    PaginationRequest PageModel,
    Guid AccountId,
    bool? SortDescending)
    : IRequest<PaginatedResponse<PaymentHistoryResponse>>;

public class GetPaymentHistoryQueryHandler(IAccountRepository accountRepository, IPaymentHistoryRepository paymentHistoryRepository)
    : IRequestHandler<GetPaymentHistoryQuery, PaginatedResponse<PaymentHistoryResponse>>
{
    public async Task<PaginatedResponse<PaymentHistoryResponse>> Handle(GetPaymentHistoryQuery request, CancellationToken cancellationToken)
    {
        var target = await accountRepository.GetAsync(new GetAccountByIdSpecification(request.AccountId), cancellationToken) ??
            throw new NotFoundException("Target account not found.");

        var skip = (request.PageModel.PageNumber - 1) * request.PageModel.PageSize;
        var take = request.PageModel.PageSize;

        bool sortDesc = request.SortDescending ?? true;
        var specification = new SpecificationBuilder<PaymentHistoryEntity>()
            .AddCriteria(x => x.AccountId == target.Id)
            .AddInclude(x => x.Account)
            .AddInclude(x => x.Account.Shop)
            .AddOrderBy(x => x.PaymentDate, isDesc: sortDesc)
            .Skip(skip)
            .Take(take)
            .Build();

        var (items, totalCount) = await paymentHistoryRepository.GetAllPaginatedAsync(specification, cancellationToken);

        var responseItems = items.Adapt<List<PaymentHistoryResponse>>();

        return new PaginatedResponse<PaymentHistoryResponse>
        {
            Items = responseItems,
            TotalAmount = totalCount
        };
    }
}

using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response.Account;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Users.Queries.Account;

public record GetPersonalInfoQuery(Guid Id) : IRequest<AccountDetailsResponse>;

public class GetPersonalInfoQueryHandler(
    IAccountRepository accountRepository)
    : IRequestHandler<GetPersonalInfoQuery, AccountDetailsResponse>
{
    public async Task<AccountDetailsResponse> Handle(GetPersonalInfoQuery request, CancellationToken cancellationToken)
    {
        var spec = new GetAccountByIdSpecification(request.Id);
        var account = await accountRepository.GetAsync(spec, cancellationToken)
            ?? throw new NotFoundException("Account not found");

        return account.Adapt<AccountDetailsResponse>();
    }
}
using Mapster;
using MediatR;
using NettoRetail.Application.CQRS.Users.Queries.Staff;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Models.Response.Account;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Users.Queries.Account;

public record GetAccountDetailsByIdQuery(Guid Id) : IRequest<AccountDetailsResponse>;

public class GetAccountDetailsByIdQueryHandler(
    IStaffRepository accountRepository)
    : IRequestHandler<GetAccountDetailsByIdQuery, AccountDetailsResponse>
{
    public async Task<AccountDetailsResponse> Handle(GetAccountDetailsByIdQuery request, CancellationToken cancellationToken)
    {
        var spec = new GetStaffByIdSpecification(request.Id);
        var account = await accountRepository.GetAsync(spec, cancellationToken)
            ?? throw new NotFoundException("Staff member not found");

        return account.Adapt<AccountDetailsResponse>();
    }
}

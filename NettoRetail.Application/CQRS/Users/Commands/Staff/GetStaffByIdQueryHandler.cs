using Mapster;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Users.Commands.Staff;
public record GetStaffByIdQuery(Guid Id, int ShopId) : IRequest<StaffDetailsResponse>;

public class GetStaffByIdQueryHandler(
    IAccountRepository accountRepository)
    : IRequestHandler<GetStaffByIdQuery, StaffDetailsResponse>
{
    public async Task<StaffDetailsResponse> Handle(GetStaffByIdQuery request, CancellationToken cancellationToken)
    {
        var spec = new GetAccountByIdSpecification(request.Id);
        var staff = await accountRepository.GetAsync(spec, cancellationToken)
            ?? throw new NotFoundException("Staff member not found");

        if (staff.ShopId != request.ShopId)
            throw new BadRequestException("Access denied to this staff member.");

        return staff.Adapt<StaffDetailsResponse>();
    }
}

using MapsterMapper;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Users.Queries.Staff;
public record GetStaffByIdQuery(Guid Id, int ShopId) : IRequest<StaffDetailsResponse>;

public class GetStaffByIdQueryHandler(
    IStaffRepository accountRepository,
    IMapper mapper)
    : IRequestHandler<GetStaffByIdQuery, StaffDetailsResponse>
{
    public async Task<StaffDetailsResponse> Handle(GetStaffByIdQuery request, CancellationToken cancellationToken)
    {
        var spec = new GetStaffByIdSpecification(request.Id);
        var staff = await accountRepository.GetAsync(spec, cancellationToken)
            ?? throw new NotFoundException(StaffConstants.StaffMemberNotFound);

        if (staff.ShopId != request.ShopId)
            throw new ForbiddenException(StaffConstants.StaffMemberAccessDenied);

        return mapper.Map<StaffDetailsResponse>(staff);
    }
}

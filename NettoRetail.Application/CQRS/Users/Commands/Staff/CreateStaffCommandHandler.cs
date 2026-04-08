using MapsterMapper;
using MediatR;
using NettoRetail.Application.Models.Request.Staff;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Services.IStaffService;

namespace NettoRetail.Application.CQRS.Users.Commands.Staff;

public record CreateStaffCommand(CreateStaffRequest Model, int ShopId) : IRequest<CreateStaffResponse>;
public class CreateStaffCommandHandler(
    IStaffService staffService,
    IMapper mapper)
    : IRequestHandler<CreateStaffCommand, CreateStaffResponse>
{
    public async Task<CreateStaffResponse> Handle(CreateStaffCommand request, CancellationToken cancellationToken)
    {
        var createdAccount = await staffService.CreateStaffAsync(request.Model, request.ShopId, cancellationToken);
        return mapper.Map<CreateStaffResponse>(createdAccount);
    }
}

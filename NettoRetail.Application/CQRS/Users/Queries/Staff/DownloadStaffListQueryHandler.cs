using MediatR;
using NettoRetail.Application.Models.Request.Staff;
using NettoRetail.Application.Services.IStaffService;

namespace NettoRetail.Application.CQRS.Users.Queries.Staff;

public record DownloadStaffListQuery(int UserShopId, GetFilteredStaffCsvRequest Model) : IRequest<byte[]>;

public class DownloadStaffListQueryHandler(IStaffService staffService)
    : IRequestHandler<DownloadStaffListQuery, byte[]>
{
    public async Task<byte[]> Handle(DownloadStaffListQuery request, CancellationToken cancellationToken)
    {
        return await staffService.DownloadStaffCsvAsync(request.UserShopId, request.Model, cancellationToken);
    }
}

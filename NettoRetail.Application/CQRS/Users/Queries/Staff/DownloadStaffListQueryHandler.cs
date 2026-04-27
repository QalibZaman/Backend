using MediatR;
using NettoRetail.Application.Models.Request.Staff;
using NettoRetail.Application.Services.IStaffCsvExportService;

namespace NettoRetail.Application.CQRS.Users.Queries.Staff;

public record DownloadStaffListQuery(int UserShopId, GetFilteredStaffCsvRequest Model) : IRequest<byte[]>;

public class DownloadStaffListQueryHandler(IStaffCsvExportService staffCsvExportService)
    : IRequestHandler<DownloadStaffListQuery, byte[]>
{
    public async Task<byte[]> Handle(DownloadStaffListQuery request, CancellationToken cancellationToken)
    {
        return await staffCsvExportService.DownloadStaffCsvAsync(request.UserShopId, request.Model, cancellationToken);
    }
}

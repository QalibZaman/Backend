using MediatR;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.CsvFileModel;
using NettoRetail.Application.Models.Request.Staff;
using NettoRetail.Application.Services.IStaffService;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Users.Queries.Staff;

public record DownloadStaffListQuery(int UserShopId, GetFilteredStaffCsvRequest Model) : IRequest<byte[]>;

public class DownloadStaffListQueryHandler(IAccountRepository accountRepository,
    IStaffService staffService,
    ICsvFileService csvFileService)
    : IRequestHandler<DownloadStaffListQuery, byte[]>
{
    public async Task<byte[]> Handle(DownloadStaffListQuery request, CancellationToken cancellationToken)
    {
        var staffStatus = staffService.ParseStaffStatus(request.Model.Status);

        var spec = new GetFilteredStaffSpecification(
            shopId: request.UserShopId,
            firstName: request.Model.FirstName,
            lastName: request.Model.LastName,
            email: request.Model.Email,
            roleId: request.Model.RoleId,
            status: staffStatus);

        var staff = await accountRepository.GetStaffAccounts(spec, cancellationToken);

        var staffData = await csvFileService.GetEntitiesCsvData<StaffCsvModel, AccountEntity>(staff, cancellationToken);

        return staffData;
    }
}

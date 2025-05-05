using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.CsvFileModel;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Users.Commands.Staff;

public record DownloadStaffListQuery(int UserShopId) : IRequest<byte[]>;

public class DownloadStaffListQueryHandler(IAccountRepository accountRepository, ICsvFileService csvFileService) : IRequestHandler<DownloadStaffListQuery, byte[]>
{
    public async Task<byte[]> Handle(DownloadStaffListQuery request, CancellationToken cancellationToken)
    {
        var staff = await accountRepository.GetStaffAccounts(request.UserShopId, cancellationToken) ?? throw new NotFoundException("There are no staff members in the list.");

        var staffData = await csvFileService.GetEntitiesCsvData<StaffCsvModel, AccountEntity>(staff, cancellationToken);

        return staffData;
    }
}

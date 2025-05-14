using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.CsvFileModel;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Users.Commands.Staff;

public record DownloadStaffListQuery(int UserShopId) : IRequest<byte[]>;

public class DownloadStaffListQueryHandler(IAccountRepository accountRepository, ICsvFileService csvFileService) : IRequestHandler<DownloadStaffListQuery, byte[]>
{
    public async Task<byte[]> Handle(DownloadStaffListQuery request, CancellationToken cancellationToken)
    {
        var specification = new GetAccountsByShopIdSpecification(request.UserShopId)
            .AddInclude(x => x.Role);
        var staff = await accountRepository.GetStaffAccounts(specification, cancellationToken) ?? throw new NotFoundException("There are no staff members in the list.");

        var staffData = await csvFileService.GetEntitiesCsvData<StaffCsvModel, AccountEntity>(staff, cancellationToken);

        return staffData;
    }
}

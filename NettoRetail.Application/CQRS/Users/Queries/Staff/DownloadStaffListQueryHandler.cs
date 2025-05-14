using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.CsvFileModel;
using NettoRetail.Application.Models.Request.Staff;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Users.Queries.Staff;

public record DownloadStaffListQuery(int UserShopId, GetFilteredUnsortedStaffRequest Model) : IRequest<byte[]>;

public class DownloadStaffListQueryHandler(IAccountRepository accountRepository, ICsvFileService csvFileService) : IRequestHandler<DownloadStaffListQuery, byte[]>
{
    public async Task<byte[]> Handle(DownloadStaffListQuery request, CancellationToken cancellationToken)
    {
        var specificationBuilder = new SpecificationBuilder<AccountEntity>();

        specificationBuilder.Add<GetAccountsByShopIdSpecification>(request.UserShopId)
            .Add<GetAccountsByPartialFirstNameSpecification>(request.Model.FirstName)
            .Add<GetAccountsByPartialLastNameSpecification>(request.Model.LastName)
            .Add<GetAccountsByPartialEmailSpecification>(request.Model.Email)
            .Add<GetAccountsByRoleIdSpecification>(request.Model.RoleId)
            .AddInclude(x => x.Role);

        if (Enum.TryParse(request.Model.Status, true, out StaffStatusEnum status))
        {
            specificationBuilder.Add<GetAccountsByStatusSpecification>(status);
        }

        var staff = await accountRepository.GetStaffAccounts(specificationBuilder.Build(), cancellationToken) ?? throw new NotFoundException("There are no staff members in the list.");

        var staffData = await csvFileService.GetEntitiesCsvData<StaffCsvModel, AccountEntity>(staff, cancellationToken);

        return staffData;
    }
}

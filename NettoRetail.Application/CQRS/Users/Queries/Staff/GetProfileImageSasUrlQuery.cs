using Azure.Storage.Sas;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Users.Queries.Staff;

public record GetProfileImageSasUrlQuery(Guid Id) : IRequest<string>;

public class GetProfileImageSasUrlQueryHandler(
    IStaffRepository accountRepository,
    IAzureBlobService azureBlobService)
    : IRequestHandler<GetProfileImageSasUrlQuery, string>
{
    private static readonly TimeSpan SasExpiry = TimeSpan.FromMinutes(10);

    public async Task<string> Handle(GetProfileImageSasUrlQuery request, CancellationToken cancellationToken)
    {
        var spec = new GetStaffByIdSpecification(request.Id);
        var staff = await accountRepository.GetAsync(spec, cancellationToken)
            ?? throw new NotFoundException("Staff member not found");

        if (string.IsNullOrWhiteSpace(staff.ProfileImageUrl))
            throw new NotFoundException("Profile image not found");

        return azureBlobService.GenerateBlobSasUrlFromUrl(
            staff.ProfileImageUrl,
            BlobSasPermissions.Read,
            SasExpiry);
    }
}

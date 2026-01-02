using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Enums;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Options;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.Users.Commands;

public record UpdateProfileImageCommand(Guid Id, IFormFile Image) : IRequest;
public class UpdateProfileImageCommandHandler(IAccountRepository accountRepository,
    IUnitOfWork unitOfWork,
    IAzureBlobService azureBlobService,
    IOptions<ConnectionStrings> connectionStrings,
    IOptions<AzureBlobStorageOptions> azureBlobStorageOptions) : IRequestHandler<UpdateProfileImageCommand>
{
    public async Task Handle(UpdateProfileImageCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetAccountByIdSpecification(request.Id);
        var account = await accountRepository.GetAsync(specification, cancellationToken) ?? throw new NotFoundException("No user with such id");

        var azureConnectionString = connectionStrings.Value.AzureConnection;
        var accountLogoContainerName = azureBlobStorageOptions.Value.ContainerName;
        if (request.Image == null) throw new BadRequestException("No image provided");

        var fileName = $"profile-images/{Guid.NewGuid()}-{account.ShopId}-{(int)BlobAccess.Private}{Path.GetExtension(request.Image.FileName)}";

        var transaction = unitOfWork.BeginTransaction();

        try
        {
            if (fileName != null)
            {
                await using var stream = request.Image!.OpenReadStream();
                var imageUrl = await azureBlobService.UploadAsync(fileName, stream, accountLogoContainerName,BlobContentCategoryEnum.Image,true);
                account.ProfileImageUrl = imageUrl;
            }

            await accountRepository.UpdateAsync(account, cancellationToken);

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}

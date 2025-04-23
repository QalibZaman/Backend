using Bogus.DataSets;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Options;
using NettoRetail.DataAccess.Repositories;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.DataAccess.UOW;
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

        string? fileName = null;

        if (request.Image == null) throw new BadRequestException("No image provided");

        fileName = $"{Guid.NewGuid()}{Path.GetExtension(request.Image.FileName)}";

        var transaction = unitOfWork.BeginTransaction();

        try
        {
            if (fileName != null)
            {
                account.ProfileImageUrl = Path.Combine(
                    azureConnectionString,
                    accountLogoContainerName,
                    fileName);
            }

            await accountRepository.AddAsync(account, cancellationToken);

            if (fileName != null)
            {
                await using var stream = request.Image!.OpenReadStream();
                await azureBlobService.UploadAsync(fileName, stream, accountLogoContainerName,BlobContentCategoryEnum.Image,true);
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}

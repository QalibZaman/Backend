using Azure.Storage.Sas;
using MediatR;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Models.Azure;
using NettoRetail.DataAccess.Options;
using NettoRetail.Infrastructure.Interfaces;

namespace NettoRetail.Application.CQRS.Azure.Queries;

public record GetSasTokenQuery() : IRequest<SasTokenModel>;

public class GetSasTokenQueryHandler(IAzureBlobService azureBlobService,
    IOptions<ConnectionStrings> connectionStrings,
    IOptions<AzureBlobStorageOptions> azureBlobStorageOptions) : IRequestHandler<GetSasTokenQuery, SasTokenModel>
{
    public Task<SasTokenModel> Handle(GetSasTokenQuery request, CancellationToken cancellationToken)
    {
        var azureConnectionString = connectionStrings.Value.AzureConnection;
        var containerName = azureBlobStorageOptions.Value.ContainerName;

        var sas = azureBlobService.GenerateContainerSasToken(containerName, BlobContainerSasPermissions.Read);

        return Task.FromResult(new SasTokenModel() { SasToken = sas });
    }
}

using Azure.Storage.Sas;
using MediatR;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Models.Azure;
using NettoRetail.DataAccess.Options;
using NettoRetail.Infrastructure.Interfaces;

namespace NettoRetail.Application.CQRS.Azure.Queries;

public record GetBlobSasTokenQuery(string FileUrl, int ShopId) : IRequest<SasTokenModel>;

public class GetBlobSasTokenQueryHandler(
    IAzureBlobService azureBlobService,
    IOptions<ConnectionStrings> connectionStrings,
    IOptions<AzureBlobStorageOptions> azureBlobStorageOptions)
    : IRequestHandler<GetBlobSasTokenQuery, SasTokenModel>
{
    public Task<SasTokenModel> Handle(GetBlobSasTokenQuery request, CancellationToken cancellationToken)
    {
        var azureConnectionString = connectionStrings.Value.AzureConnection;
        var containerName = azureBlobStorageOptions.Value.ContainerName;

        // Extract the filename from the full URL
        string blobUrl = request.FileUrl;
        string actualFileName = Path.GetFileName(new Uri(blobUrl).AbsolutePath); // e.g., 9e4b67ab-b9b6-4fae-9c15-38b84a1d4c52-123-1.jpg
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(actualFileName); // e.g., 9e4b67ab-b9b6-4fae-9c15-38b84a1d4c52-123-1

        var parts = fileNameWithoutExtension.Split('-');

        if (parts.Length < 3 ||
            !int.TryParse(parts[^2], out int fileShopId) ||
            !int.TryParse(parts[^1], out int isPrivate))
        {
            throw new InvalidOperationException("Invalid file name format.");
        }

        if (isPrivate == 1 && fileShopId != request.ShopId)
        {
            throw new UnauthorizedAccessException("You are not authorized to access this private file.");
        }

        var uri = new Uri(blobUrl);
        var blobPath = uri.AbsolutePath.TrimStart('/');

        if (blobPath.StartsWith($"{containerName}/", StringComparison.OrdinalIgnoreCase))
        {
            blobPath = blobPath.Substring(containerName.Length + 1);
        }

        var sas = azureBlobService.GenerateBlobSasToken(containerName, blobPath, BlobSasPermissions.Read);

        return Task.FromResult(new SasTokenModel { SasToken = sas });
    }
}

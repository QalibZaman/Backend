using Azure.Storage.Sas;
using MediatR;
using Microsoft.AspNetCore.Http;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Enums;

namespace NettoRetail.Application.CQRS.InventoryItems.Commands;
public record UploadDescriptionImageCommand(IFormFile File) : IRequest<string>;
public class UploadDescriptionImageCommandHandler(IAzureBlobService blobService) : IRequestHandler<UploadDescriptionImageCommand, string>
{
    public async Task<string> Handle(UploadDescriptionImageCommand request, CancellationToken cancellationToken)
    {
        if (request.File == null || request.File.Length == 0)
            throw new ArgumentException("No file provided.");

        const string containerName = "dot-net";
        var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(request.File.FileName)}";

        using var stream = request.File.OpenReadStream();

        await blobService.UploadAsync(
            uniqueFileName,
            stream,
            containerName,
            BlobContentCategoryEnum.Image,
            contentType: request.File.ContentType);

        var blobPath = $"images/{uniqueFileName}";

        var blobUrl = blobService.GetBlobUrl(containerName, blobPath);

        var sasToken = blobService.GenerateBlobSasToken(
            containerName,
            blobPath,
            BlobSasPermissions.Read,
            TimeSpan.FromHours(1));

        return $"{blobUrl}?{sasToken}";
    }
}

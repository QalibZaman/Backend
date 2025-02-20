using MediatR;
using Microsoft.AspNetCore.Http;
using NettoHealth.Infrastructure.Interfaces;

namespace NettoHealth.Application.CQRS.Files.Commands;

public record UploadFileCommand(IFormFile File) : IRequest<bool>;

public class UploadFileCommandHandler(IAzureBlobService azureBlobService) : IRequestHandler<UploadFileCommand, bool>
{
    public async Task<bool> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        var fileName = request.File.FileName;
        await using var stream = request.File.OpenReadStream();
        var containerName = "some-container";
        await azureBlobService.UploadAsync(fileName, stream, containerName);
        return true;
    }
}
using MediatR;
using Microsoft.AspNetCore.Http;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.CsvFileModel;
using NettoRetail.Application.Services.IStaffService;
using NettoRetail.Shared.Constants;

namespace NettoRetail.Application.CQRS.Users.Commands.Staff;

public record UploadStaffListCommand(IFormFile File, int ShopId) : IRequest;

public class UploadStaffListCommandHandler
    (ICsvFileService csvFileService,
    IStaffService staffService)
    : IRequestHandler<UploadStaffListCommand>
{
    public async Task Handle(UploadStaffListCommand request, CancellationToken cancellationToken)
    {
        if (request.File == null || request.File.Length == 0)
            throw new BadHttpRequestException(CsvUploadErrors.InvalidFileFormat);
        if (!string.Equals(Path.GetExtension(request.File.FileName), ".csv", StringComparison.OrdinalIgnoreCase))
            throw new BadHttpRequestException(CsvUploadErrors.InvalidFileFormat);
        var records = new List<StaffCsvModel>();

        await foreach (var item in csvFileService.ConvertEntitiesCsvFileToEntityData<StaffCsvModel>(request.File))
        {
            if (item == null || !StaffCsvModel.IsValid(item))
            {
                throw new BadRequestException("Invalid record found in CSV. Please correct the errors in the CSV file.");
            }

            records.Add(item);
        }

        foreach (var record in records)
        {
            await staffService.CreateStaffAsync(record, request.ShopId, cancellationToken);
        }
    }
}

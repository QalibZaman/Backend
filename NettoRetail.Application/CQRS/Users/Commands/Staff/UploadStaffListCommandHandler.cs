using MediatR;
using Microsoft.AspNetCore.Http;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.CsvFileModel;
using NettoRetail.Application.Services.IStaffService;

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
            throw new BadHttpRequestException("CSV file cannot be null. Please Upload CSV file.");
        if (!string.Equals(Path.GetExtension(request.File.FileName), ".csv", StringComparison.OrdinalIgnoreCase))
            throw new BadHttpRequestException("Invalid file Format. Please Upload CSV file.");

        List<StaffCsvModel> records = (List<StaffCsvModel>)csvFileService.ConvertEntitiesCsvFileToEntityData<StaffCsvModel>(request.File);

        var isRecordValid = StaffCsvModel.IsCollectionValid(records);
        if (!isRecordValid)
        {
            throw new BadHttpRequestException("Unable to upload Staff list. Please correct the errors in the CSV file.", statusCode: 422);
        }

        foreach (var record in records)
        {
            await staffService.CreateStaffAsync(record, request.ShopId, cancellationToken);
        }
    }
}
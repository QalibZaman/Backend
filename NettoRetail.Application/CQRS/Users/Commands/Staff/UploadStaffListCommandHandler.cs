using MediatR;
using Microsoft.AspNetCore.Http;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.CsvFileModel;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Users.Commands.Staff;
public record UploadStaffListCommand(IFormFile File, int ShopId) : IRequest;

public class UploadStaffListCommandHandler
    (IAccountRepository userRepository,
    IRoleRepository roleRepository,
    IEmailSender emailSender,
    IEmailBuilderService emailBuilderService,
    ICsvFileService csvFileService)
    : IRequestHandler<UploadStaffListCommand>
{
    public async Task Handle(UploadStaffListCommand request, CancellationToken cancellationToken)
    {
        List<StaffCsvModel> records;

        if (request.File == null || request.File.Length == 0)
            throw new BadHttpRequestException("CSV file cannot be null. Please Upload CSV file.");
        if (!string.Equals(Path.GetExtension(request.File.FileName), ".csv", StringComparison.OrdinalIgnoreCase))
            throw new BadHttpRequestException("Invalid file Format. Please Upload CSV file.");

        try
        {
            records = (List<StaffCsvModel>)csvFileService.ConvertEntitiesCsvFileToEntityData<StaffCsvModel>(request.File);
        }
        catch (Exception)
        {
            throw new BadHttpRequestException("Unable to upload Staff list. Please correct the errors in the CSV file.", statusCode: 422);
        }

        var createStaffRequestCollection = StaffCsvModel.AdaptCsvModelToCreateStaffRequest(records);
        var createStaffRequestHanler = new CreateStaffCommandHandler(userRepository, roleRepository, emailBuilderService, emailSender);
        foreach (var staffRequest in createStaffRequestCollection)
        {
            var createstaffCommand = new CreateStaffCommand(staffRequest, request.ShopId);
            await createStaffRequestHanler.Handle(createstaffCommand, cancellationToken);
        }
    }
}
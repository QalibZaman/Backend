using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.CsvFileModel;
using NettoRetail.Application.Services.IStaffService;

namespace NettoRetail.Application.CQRS.Users.Commands.Staff;

public record UploadStaffListCommand(IFormFile File, int ShopId) : IRequest;

public class UploadStaffListCommandHandler
    (ICsvFileService csvFileService,
    IValidator<List<StaffCsvModel>> listValidator,
    IStaffService staffService)
    : IRequestHandler<UploadStaffListCommand>
{
    public async Task Handle(UploadStaffListCommand request, CancellationToken cancellationToken)
    {
        var staffList = await csvFileService.ReadStaffListAsync(request.File, cancellationToken);

        await listValidator.ValidateAsync(staffList, cancellationToken);

        await staffService.CreateStaffAsync(staffList, request.ShopId, cancellationToken);
    }
}

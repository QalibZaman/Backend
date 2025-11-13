using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.CsvFileModel;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Services;
using NettoRetail.Domain.Entities;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.Application.CQRS.Users.Commands.Staff;
public record UploadStaffListCommand(IFormFile File, int ShopId) : IRequest<UploadStaffResult>;

public class UploadStaffListCommandHandler
    (IAccountRepository userRepository,
    IRoleRepository roleRepository,
    IEmailSender emailSender,
    IEmailBuilderService emailBuilderService,
    IUnitOfWork unitOfWork,
    IShopRepository shopRepository,
    ICsvFileService csvFileService)
    : IRequestHandler<UploadStaffListCommand, UploadStaffResult>
{
    public async Task<UploadStaffResult> Handle(UploadStaffListCommand request, CancellationToken cancellationToken)
    {
        var result = new UploadStaffResult();
        var errors = new List<UploadStaffError>();
        List<StaffCsvModel> records;

        if (request.File == null || request.File.Length == 0)
        {
            throw new BadHttpRequestException("CSV file cannot be null. Please Upload CSV file.");
        }

        if (!string.Equals(Path.GetExtension(request.File.FileName), ".csv", StringComparison.OrdinalIgnoreCase))
        {
            throw new BadHttpRequestException("Invalid file Format. Please Upload CSV file.");
        }

        try
        {
            records = (List<StaffCsvModel>)csvFileService.ConvertEntitiesCsvFileToEntityData<StaffCsvModel>(request.File);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error reading CSV file: {ex.Message}");
        }

        if (records.Count == 0)
        {
            throw new Exception("The CSV file is empty.");
        }

        for (int i = 0; i < records.Count; i++)
        {
            var row = records[i];
            var rowNumber = i + 2;

            errors.AddRange(ValidateCsvRow(row, rowNumber));
        }

        if (errors.Count > 0)
        {
            result.Success = false;
            result.Errors = errors;
            return result;
        }

        var shop = await shopRepository.GetByIdAsync(request.ShopId, cancellationToken);

        if (shop == null)
        {
            result.Success = false;
            result.Errors = new List<UploadStaffError> {
                new UploadStaffError { RowNumber = 0, ErrorMessage = "Invalid shop ID." }
            };
            return result;
        }

        var allRoles = await roleRepository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var accountsToAdd = records.Adapt<List<AccountEntity>>();
        var transaction = unitOfWork.BeginTransaction();

        for (int i = 0; i < accountsToAdd.Count; i++)
        {
            var account = accountsToAdd[i];
            var csvRoleName = records[i].Role;

            var matchingRole = allRoles.FirstOrDefault(r =>
                string.Equals(r.Name, csvRoleName, StringComparison.OrdinalIgnoreCase));

            if (matchingRole == null)
                throw new Exception($"Role '{csvRoleName}' not found.");

            account.Role = matchingRole;

            account.ShopId = shop.Id;
        }

        try
        {
            const int batchSize = 200;

            for (int i = 0; i < accountsToAdd.Count; i += batchSize)
            {
                var batch = accountsToAdd.Skip(i).Take(batchSize).ToList();

                foreach (var account in batch)
                {
                    var tempPassword = Guid.NewGuid().ToString("N").Substring(0, 12);
                    account.PasswordHash = PasswordHasher.HashPassword(tempPassword);
                }

                await userRepository.AddRangeAsync(batch, cancellationToken);

                foreach (var account in batch)
                {

                    try
                    {
                        var emailModel = await emailBuilderService.BuildResetPasswordEmailAsync(account.Email, account.FirstName);
                        await emailSender.SendAsync(emailModel);
                    }
                    catch (Exception ex)
                    {
                        errors.Add(new UploadStaffError
                        {
                            RowNumber = 0,
                            ErrorMessage = $"Email sending failed for {account.Email}: {ex.Message}"
                        });
                    }
                }
            }

            transaction.Commit();
        }
        catch (Exception ex)
        {
            transaction.Rollback();

            errors.Add(new UploadStaffError
            {
                RowNumber = 0,
                ErrorMessage = $"Database error: {ex.Message}"
            });

            result.Success = false;
            result.Errors = errors;
        }


        result.Success = true;
        return result;
    }

    private static List<UploadStaffError> ValidateCsvRow(StaffCsvModel row, int rowNumber)
    {
        var errors = new List<UploadStaffError>();

        if (string.IsNullOrWhiteSpace(row.FirstName))
            errors.Add(new UploadStaffError { RowNumber = rowNumber, ErrorMessage = "First name is required." });

        if (string.IsNullOrWhiteSpace(row.LastName))
            errors.Add(new UploadStaffError { RowNumber = rowNumber, ErrorMessage = "Last name is required." });

        if (string.IsNullOrWhiteSpace(row.Email) || !row.Email.Contains("@"))
            errors.Add(new UploadStaffError { RowNumber = rowNumber, ErrorMessage = "A valid email is required." });

        if (string.IsNullOrWhiteSpace(row.Role))
            errors.Add(new UploadStaffError { RowNumber = rowNumber, ErrorMessage = "Role is required." });

        if (string.IsNullOrWhiteSpace(row.Status))
            errors.Add(new UploadStaffError { RowNumber = rowNumber, ErrorMessage = "Status is required" });

        return errors;
    }
}
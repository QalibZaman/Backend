using Microsoft.AspNetCore.Http;
using NettoRetail.Application.Models.CsvFileModel;

namespace NettoRetail.Application.Interfaces;

public interface ICsvFileService
{
    Task<byte[]> GetEntitiesCsvData<T, TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken);
    IAsyncEnumerable<T> ConvertEntitiesCsvFileToEntityData<T>(IFormFile csvItemsFile);
    Task<List<InventoryItemCsvModel>> ReadCsvItemsAsync(IFormFile csvFile, CancellationToken cancellationToken);
    Task<List<StaffCsvModel>> ReadStaffListAsync(IFormFile csvFile, CancellationToken cancellationToken);
}

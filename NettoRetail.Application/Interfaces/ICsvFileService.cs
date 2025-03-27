namespace NettoRetail.Application.Interfaces;

public interface ICsvFileService
{
    Task<byte[]> GetEntitiesCsvData<T, TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken);
}
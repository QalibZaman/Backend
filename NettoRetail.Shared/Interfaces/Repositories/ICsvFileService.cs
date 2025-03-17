namespace NettoRetail.Shared.Interfaces.Repositories;

public interface ICsvFileService
{
    public string GetEntitiesCsvData<T>(IEnumerable<T> items, string header, Func<T, string> entityData);
}
using System.Text;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.Services;

public class CsvFileService : ICsvFileService
{
    public string GetEntitiesCsvData<T>(IEnumerable<T> entities, string header, Func<T, string> entityData)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine(header);

        foreach (var entity in entities)
        {
            stringBuilder.AppendLine(entityData(entity));
        }

        return stringBuilder.ToString();
    }
}
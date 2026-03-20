using NettoRetail.Application.Models.Response;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.Interfaces;

public interface ICategoriesSectionService
{
    Task<GetCategoriesSectionResponse> GetCategoriesSectionAsync(int shopId, CancellationToken cancellationToken);
    Task UpdateCategoriesSectionAsync(int shopId, string name, bool visibility, IReadOnlyList<int> categoryIds, CancellationToken cancellationToken);
}

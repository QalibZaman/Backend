using NettoRetail.Shared.Constants;

namespace NettoRetail.Application.Models.Request;

public class PaginationRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = PaginationConstants.DefaultPageSize;
}

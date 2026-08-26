namespace NettoRetail.Application.Models.Response;

public class PaginatedResponse<T>
{
    public ICollection<T> Items { get; set; } = new List<T>();
    public int TotalAmount { get; set; }
}

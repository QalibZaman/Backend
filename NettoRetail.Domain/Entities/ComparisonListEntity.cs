namespace NettoRetail.Domain.Entities;

public class ComparisonListEntity
{
    public int Id { get; set; }

    public int CategoryId { get; set; }

    public required Guid CustomerId { get; set; }
    public required CustomerEntity Customer { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public ICollection<ComparisonListItemEntity> ComparisonListItems { get; set; } = new List<ComparisonListItemEntity>();
}
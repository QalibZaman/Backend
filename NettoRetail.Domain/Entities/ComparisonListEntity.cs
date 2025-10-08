namespace NettoRetail.Domain.Entities
{
    public class ComparisonListEntity
    {
        public int Id { get; set; }

        public required string UserId { get; set; }
        public int CategoryId { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public ICollection<ComparisonListItemEntity> ComparisonListItems { get; set; } = new List<ComparisonListItemEntity>();
    }
}

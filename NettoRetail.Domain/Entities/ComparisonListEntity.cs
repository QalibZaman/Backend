namespace NettoRetail.Domain.Entities
{
    public class ComparisonListEntity
    {
        public int Id { get; set; }

        public required string UserId { get; set; } //User id for authenticated users only (FE handles guests)
        public int CategoryId { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public ICollection<ComparisonListItemEntity> ComparisonListItems { get; set; } = new List<ComparisonListItemEntity>();
    }
}

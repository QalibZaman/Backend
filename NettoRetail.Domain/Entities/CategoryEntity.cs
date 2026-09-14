namespace NettoRetail.Domain.Entities;

    public class CategoryEntity
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string URL { get; set; }
        public required bool Visibility { get; set; }
        public int? CategoriesSectionOrder { get; set; }
        public int Level { get; set; }
        public int? ParentId { get; set; }
        public required int ShopId { get; set; }
        public DateTime CreatedAt { get; set; }
        public ShopEntity? Shop { get; set; }
        public CategoryEntity? ParentCategory { get; set; }
    }

using System;

namespace NettoRetail.Domain.Entities;

public class OnboardingSlideDraftEntity
{
    public int Id { get; set; }
    public int? SlideId { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = default!;
    public string Content { get; set; } = default!;
    public DateTime DateSaved { get; set; }
}

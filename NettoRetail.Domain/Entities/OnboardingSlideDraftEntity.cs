using System;

namespace NettoRetail.Domain.Entities;

public class OnboardingSlideDraftEntity
{
    public int Id { get; set; }
    public int? SlideId { get; set; } // Может быть null, если это черновик нового слайда
    public Guid UserId { get; set; } // Кому принадлежит черновик
    public string Title { get; set; } = default!;
    public string Content { get; set; } = default!;
    public DateTime DateSaved { get; set; }
}
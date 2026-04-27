namespace NettoRetail.Application.Models.Response
{
    public record ToggleLinkProductsResponse(
     Guid Id,
     string Name,
     string Upc,
     bool IsLinked,
     bool HasConflicts = false,
     List<AttributeConflict>? Conflicts = null);

    public record AttributeConflict(
        string AttributeName,
        string? ParentValue,
        string? ChildValue);
}

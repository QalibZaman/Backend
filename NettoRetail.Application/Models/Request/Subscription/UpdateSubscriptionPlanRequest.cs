public class UpdateSubscriptionPlanRequest
{
    public string Name { get; set; } = default!;
    public int Price { get; set; }
    public string? Description { get; set; }
    public string? Motto { get; set; }
    public List<int>? FeatureIds { get; set; }
}

namespace NettoRetail.Application.Models.Request;
public class SortingOption
{
    public string Field { get; set; } = string.Empty;
    public bool IsDescending { get; set; }
}
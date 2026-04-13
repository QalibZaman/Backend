namespace NettoRetail.Application.Models.Response;

public class GetCategoryProductStatusesResponse
{
    public int New { get; set; }
    public int Available { get; set; }
    public int Committed { get; set; }
    public int Unavailable { get; set; }
    public int Incoming { get; set; }

}

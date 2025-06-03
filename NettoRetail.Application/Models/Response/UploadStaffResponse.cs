namespace NettoRetail.Application.Models.Response;
public class UploadStaffResult
{
    public bool Success { get; set; }
    public List<UploadStaffError> Errors { get; set; } = new();
}

public class UploadStaffError
{
    public int RowNumber { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}

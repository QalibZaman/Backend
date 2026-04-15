namespace NettoRetail.Shared.Constants;

public static class StaffDownloadErrors
{
    public const string EmptyFilteredList = "No matching Staff Member found";
    public const string CsvGenerationFailed = "Unable to download Staff list due to an internal error. Please try again.";
    public const string PartialDataUnavailable = "Unable to download Staff list due to incomplete data. Please try again.";
}

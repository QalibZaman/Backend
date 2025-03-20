using OtpNet;

namespace NettoRetail.Shared.Options;
public class TotpOptions
{
    public string Issuer { get; set; }
    public int Digits { get; set; }
    public int PeriodInSeconds { get; set; }
    public OtpHashMode HashMode { get; set; }
}
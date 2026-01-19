using QuestPDF.Infrastructure;

namespace NettoRetail.Application.Services.IPdfGenerator.Utilities;

internal static class UserColors
{
    public static readonly Color PrimaryWhite = Color.FromHex("#FFF");
    public static readonly Color PrimaryBlack = Color.FromHex("#242426");
    public static readonly Color SecondaryBlack = Color.FromHex("#000");
    public static readonly Color PrimaryGray = Color.FromHex("#444");
    public static readonly Color PurpleGray = Color.FromHex("#F5F6FB");
    public static readonly Color BorderColor = Color.FromHex("#C8C8C8");
    public static readonly Color FooterDividerColor = Color.FromHex("#D9D9D9");
    public static readonly Color[] SectionHeaderGradientColors = [
        Color.FromHex("#8A6BE2"),
        Color.FromHex("#6438E3")
    ];
}
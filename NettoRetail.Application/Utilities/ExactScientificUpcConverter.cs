using System.Text.RegularExpressions;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

namespace NettoRetail.Application.Utilities;

public class ExactScientificUpcConverter : ITypeConverter
{
    private static readonly Regex ScientificRegex =
        new(@"^([\+\-]?[0-9]+)[\.,]?([0-9]*)[eE]\+?([0-9]+)$");

    public object ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new CsvHelperException(row.Context, $"UPC value cannot be empty.");

        string s = text.Trim().Replace("\"", "");

        // 1) If already digits, keep as-is (preserve leading zeros)
        if (Regex.IsMatch(s, @"^\d+$"))
            return s;

        // 2) Try scientific notation WITHOUT using double
        var match = ScientificRegex.Match(s.Replace(",", "."));
        if (match.Success)
        {
            string integerPart = match.Groups[1].Value;   // before decimal
            string fractionalPart = match.Groups[2].Value; // after decimal
            int exponent = int.Parse(match.Groups[3].Value);

            // Build full number string manually
            string digits = integerPart + fractionalPart;

            int zerosToAdd = exponent - fractionalPart.Length;
            if (zerosToAdd < 0)
                throw new CsvHelperException(row.Context, $"Scientific notation has too many decimals: {text}");

            digits += new string('0', zerosToAdd);

            return digits;
        }

        throw new CsvHelperException(row.Context, $"Cannot convert '{text}' to a valid UPC.");
    }

    public string ConvertToString(object? value, IWriterRow row, MemberMapData memberMapData)
        => value?.ToString() ?? "";
}

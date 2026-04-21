using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.CsvFileModel;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Constants;

namespace NettoRetail.Application.Services;

public class CsvFileService : ICsvFileService
{
    private readonly IMapper _mapper;

    public CsvFileService(IMapper mapper)
    {
        _mapper = mapper;
    }

    private static readonly string[] InventoryItemHeaders =
    [
        "Product UPC",
        "Product Name",
        "Product Location",
        "Brand",
        "Product Category",
        "Available Amount",
        "Reserved Amount",
        "On-hand Amount",
        "Price",
        "Status",
        "Description"
    ];

    private static readonly string[] StaffHeaders =
    [
        "First Name",
        "Last Name",
        "Email",
        "Role"
    ];

    public async Task<byte[]> GetEntitiesCsvData<T, TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken)
    {
        var mappedEntities = _mapper.Map<IEnumerable<T>>(entities);

        using var memoryStream = new MemoryStream();
        await using var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8, leaveOpen: true);
        await using (var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture))
        {
            csvWriter.WriteHeader<T>();
            await csvWriter.NextRecordAsync();
            await csvWriter.WriteRecordsAsync(mappedEntities, cancellationToken);
        }

        return memoryStream.ToArray();
    }

    public async IAsyncEnumerable<T> ConvertEntitiesCsvFileToEntityData<T>(IFormFile csvItemsFile)
    {
        if (csvItemsFile == null)
            yield break;

        using var stream = csvItemsFile.OpenReadStream();
        using var streamReader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
        var delimiter = DetectDelimiter(streamReader);

        stream.Position = 0;
        streamReader.DiscardBufferedData();

        var config = CreateCsvConfiguration(delimiter);

        using var csvReader = new CsvReader(streamReader, config);
        await foreach (var entityData in csvReader.GetRecordsAsync<T>())
            yield return entityData;
    }

    public async Task<List<InventoryItemCsvModel>> ReadCsvItemsAsync(
        IFormFile csvFile, CancellationToken cancellationToken)
    {
        if (csvFile == null)
            throw new BadRequestException(CsvUploadErrors.InvalidFileFormat);

        if (!csvFile.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            throw new BadRequestException(CsvUploadErrors.InvalidFileFormat);

        if (csvFile.Length == 0)
            throw new BadRequestException(CsvUploadErrors.ItemsListEmpty);

        try
        {
            using var stream = csvFile.OpenReadStream();
            using var streamReader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
            var delimiter = DetectDelimiter(streamReader);

            stream.Position = 0;
            streamReader.DiscardBufferedData();

            var config = CreateCsvConfiguration(delimiter);
            using var csvReader = new CsvReader(streamReader, config);

            if (!await csvReader.ReadAsync() || !csvReader.ReadHeader())
                throw new BadRequestException(BuildItemsListErrorMessage("CSV header is missing or invalid."));

            ValidateInventoryHeaders(csvReader.HeaderRecord);

            csvReader.Context.TypeConverterOptionsCache.GetOptions<InventoryItemStatusEnum>().EnumIgnoreCase = true;

            var list = new List<InventoryItemCsvModel>();
            await foreach (var item in csvReader.GetRecordsAsync<InventoryItemCsvModel>().WithCancellation(cancellationToken))
            {
                list.Add(item);
            }

            if (list.Count == 0)
                throw new BadRequestException(CsvUploadErrors.ItemsListEmpty);

            return list;
        }
        catch (CsvHelperException ex)
        {
            throw new BadRequestException(BuildItemsListErrorMessage(DescribeCsvHelperError(ex)));
        }
    }

    public async Task<List<StaffCsvModel>> ReadStaffListAsync(
        IFormFile csvFile, CancellationToken cancellationToken)
    {
        if (csvFile == null)
            throw new BadRequestException(CsvUploadErrors.InvalidFileFormat);

        if (!csvFile.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            throw new BadRequestException(CsvUploadErrors.InvalidFileFormat);

        if (csvFile.Length == 0)
            throw new BadRequestException(CsvUploadErrors.StaffListEmpty);

        try
        {
            using var stream = csvFile.OpenReadStream();
            using var streamReader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
            var delimiter = DetectDelimiter(streamReader);

            stream.Position = 0;
            streamReader.DiscardBufferedData();

            var config = CreateCsvConfiguration(delimiter);
            using var csvReader = new CsvReader(streamReader, config);

            if (!await csvReader.ReadAsync() || !csvReader.ReadHeader())
                throw new BadRequestException(BuildStaffListErrorMessage("CSV header is missing or invalid."));

            ValidateStaffHeaders(csvReader.HeaderRecord);

            var staff = new List<StaffCsvModel>();
            await foreach (var item in csvReader.GetRecordsAsync<StaffCsvModel>().WithCancellation(cancellationToken))
            {
                staff.Add(item);
            }

            if (staff.Count == 0)
                throw new BadRequestException(CsvUploadErrors.StaffListEmpty);

            return staff;
        }
        catch (CsvHelperException ex)
        {
            throw new BadRequestException(BuildStaffListErrorMessage(DescribeCsvHelperError(ex)));
        }
    }

    private static CsvConfiguration CreateCsvConfiguration(string delimiter)
        => new(CultureInfo.InvariantCulture)
        {
            Delimiter = delimiter,
            BadDataFound = null,
            MissingFieldFound = null,
            PrepareHeaderForMatch = args => NormalizeHeader(args.Header ?? string.Empty)
        };

    private static string DetectDelimiter(StreamReader reader)
    {
        var headerLine = reader.ReadLine();
        if (string.IsNullOrEmpty(headerLine))
            return ",";

        var commaCount = headerLine.Count(c => c == ',');
        var semicolonCount = headerLine.Count(c => c == ';');

        return semicolonCount > commaCount ? ";" : ",";
    }

    private static void ValidateInventoryHeaders(string[]? headerRecord)
    {
        if (headerRecord == null || headerRecord.Length == 0)
            throw new BadRequestException(BuildItemsListErrorMessage("CSV header is missing or invalid."));

        var headerSet = headerRecord
            .Select(NormalizeHeader)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var missingHeaders = InventoryItemHeaders
            .Where(required => !headerSet.Contains(required))
            .ToList();

        if (missingHeaders.Count > 0)
        {
            var missingText = string.Join(", ", missingHeaders);
            throw new BadRequestException(BuildItemsListErrorMessage($"Missing required headers: {missingText}."));
        }
    }

    private static void ValidateStaffHeaders(string[]? headerRecord)
    {
        if (headerRecord == null || headerRecord.Length == 0)
            throw new BadRequestException(BuildStaffListErrorMessage("CSV header is missing or invalid."));

        var actualHeaders = headerRecord
            .Take(StaffHeaders.Length)
            .Select(NormalizeHeader)
            .ToArray();

        if (headerRecord.Length != StaffHeaders.Length ||
            !actualHeaders.SequenceEqual(StaffHeaders, StringComparer.OrdinalIgnoreCase))
        {
            throw new BadRequestException(
                BuildStaffListErrorMessage($"CSV header must contain columns in this order: {string.Join(", ", StaffHeaders)}."));
        }
    }

    private static string BuildItemsListErrorMessage(string details)
        => $"{CsvUploadErrors.ItemsListValidationFailed} {details}".Trim();

    private static string BuildStaffListErrorMessage(string details)
        => $"{CsvUploadErrors.StaffListValidationFailed} {details}".Trim();

    private static string NormalizeHeader(string header)
        => header.Trim().TrimStart('\uFEFF');

    private static string DescribeCsvHelperError(CsvHelperException ex)
    {
        var row = ex.Context?.Parser?.Row;
        var rowInfo = row.HasValue ? $"Row {row.Value}: " : string.Empty;
        return $"{rowInfo}{ex.Message}";
    }
}

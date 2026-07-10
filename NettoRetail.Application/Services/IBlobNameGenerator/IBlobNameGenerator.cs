using NettoRetail.Shared.Enums;

namespace NettoRetail.Application.Services.IBlobNameGenerator;

/// <summary>
/// Provides a method for generating standardized blob storage file names
/// based on input parameters such as the original file name, image category,
/// shop ID, and privacy flag.
/// </summary>
public interface IBlobNameGenerator
{
    /// <summary>
    /// Generates a unique blob name for storing an image in blob storage.
    /// The name includes a GUID, the shop ID, and a privacy flag (0 or 1),
    /// appended to the file extension, and is organized under the appropriate category directory.
    /// </summary>
    /// <param name="fileName">The original name of the file, used to preserve the extension.</param>
    /// <param name="imageCategory">The category under which the image should be stored (e.g., Brands, Products).</param>
    /// <param name="shopId">The identifier of the shop that owns the image.</param>
    /// <param name="isPrivate">Determines whether the file is private (true = 1) or public (false = 0).</param>
    /// <returns>A uniquely structured blob file name including directory, GUID, shop ID, and privacy flag.</returns>
    string GenerateBlobName(string fileName, int shopId, bool isPrivate);
}

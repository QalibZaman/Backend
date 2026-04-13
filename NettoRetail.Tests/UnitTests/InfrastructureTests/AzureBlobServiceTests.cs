using System.Text;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using NettoRetail.Infrastructure.Services;

namespace NettoRetail.Tests.UnitTests.InfrastructureTests;

public class AzureBlobServiceTests
{
    [Fact]
    public void GenerateBlobSasUrlFromUrl_ShouldReturnSasUrl_ForAzureBlobUrl()
    {
        // Arrange
        var service = CreateService("nettostoragepl", new Uri("https://nettostoragepl.blob.core.windows.net"));
        var blobUrl = "https://nettostoragepl.blob.core.windows.net/dot-net/images/profile-images/test.jpg";

        // Act
        var sasUrl = service.GenerateBlobSasUrlFromUrl(blobUrl, BlobSasPermissions.Read, TimeSpan.FromMinutes(5));

        // Assert
        Assert.StartsWith(blobUrl + "?", sasUrl);
        Assert.Contains("sp=", sasUrl);
        Assert.Contains("se=", sasUrl);
    }

    [Fact]
    public void GenerateBlobSasUrlFromUrl_ShouldReturnSasUrl_ForAzuriteBlobUrl()
    {
        // Arrange
        var service = CreateService(
            "devstoreaccount1",
            new Uri("http://127.0.0.1:10000/devstoreaccount1"));

        var blobUrl = "http://127.0.0.1:10000/devstoreaccount1/dot-net/images/profile-images/test.jpg";

        // Act
        var sasUrl = service.GenerateBlobSasUrlFromUrl(blobUrl, BlobSasPermissions.Read, TimeSpan.FromMinutes(5));

        // Assert
        Assert.StartsWith(blobUrl + "?", sasUrl);
        Assert.Contains("sp=", sasUrl);
        Assert.Contains("se=", sasUrl);
    }

    [Fact]
    public void GenerateBlobSasUrlFromUrl_ShouldThrow_WhenUrlIsEmpty()
    {
        // Arrange
        var service = CreateService("nettostoragepl", new Uri("https://nettostoragepl.blob.core.windows.net"));

        // Act
        var ex = Assert.Throws<ArgumentException>(() =>
            service.GenerateBlobSasUrlFromUrl(string.Empty, BlobSasPermissions.Read));

        // Assert
        Assert.Equal("blobUrl", ex.ParamName);
    }

    private static AzureBlobService CreateService(string accountName, Uri serviceUri)
    {
        var keyBytes = Encoding.UTF8.GetBytes("test-key");
        var key = Convert.ToBase64String(keyBytes);
        var credential = new StorageSharedKeyCredential(accountName, key);
        var blobServiceClient = new BlobServiceClient(serviceUri, credential);

        return new AzureBlobService(blobServiceClient);
    }
}

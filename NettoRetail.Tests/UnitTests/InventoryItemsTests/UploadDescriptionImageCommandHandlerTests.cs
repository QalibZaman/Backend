using Microsoft.AspNetCore.Http;
using NettoRetail.Application.CQRS.InventoryItems.Commands;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Enums;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.InventoryItems
{
    public class UploadDescriptionImageCommandHandlerTests
    {
        private readonly IAzureBlobService _blobService;
        private readonly UploadDescriptionImageCommandHandler _handler;

        public UploadDescriptionImageCommandHandlerTests()
        {
            _blobService = Substitute.For<IAzureBlobService>();
            _handler = new UploadDescriptionImageCommandHandler(_blobService);
        }

        [Fact]
        public async Task Handle_ShouldUploadImage_AndReturnUrl()
        {
            // Arrange
            var mockFile = CreateFormFile("photo.png", "image/png", new byte[] { 1, 2, 3 });
            _blobService.GetBlobUrl(Arg.Any<string>(), Arg.Any<string>())
                .Returns("https://storage.blob.core.windows.net/dot-net/images/test.png");

            // Act
            var result = await _handler.Handle(new UploadDescriptionImageCommand(mockFile), CancellationToken.None);

            // Assert
            Assert.Contains("https://storage.blob.core.windows.net/dot-net/images/", result);
            await _blobService.Received(1).UploadAsync(
                Arg.Any<string>(),
                Arg.Any<Stream>(),
                "dot-net",
                BlobContentCategoryEnum.Image,
                true,
                "image/png");
        }

        [Fact]
        public async Task Handle_ShouldThrowArgumentException_WhenFileIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _handler.Handle(new UploadDescriptionImageCommand(null!), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldThrowArgumentException_WhenFileIsEmpty()
        {
            // Arrange
            var emptyFile = Substitute.For<IFormFile>();
            emptyFile.Length.Returns(0);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _handler.Handle(new UploadDescriptionImageCommand(emptyFile), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldUseUniqueFileName()
        {
            // Arrange
            var mockFile = CreateFormFile("product.jpg", "image/jpeg", new byte[] { 4, 5, 6 });
            string capturedFileName = "";

            _blobService.UploadAsync(
                Arg.Do<string>(x => capturedFileName = x),
                Arg.Any<Stream>(),
                Arg.Any<string>(),
                Arg.Any<BlobContentCategoryEnum>(),
                Arg.Any<bool>(),
                Arg.Any<string?>())
                .Returns("ok");

            // Act
            await _handler.Handle(new UploadDescriptionImageCommand(mockFile), CancellationToken.None);

            // Assert
            Assert.EndsWith(".jpg", capturedFileName);
            Assert.True(Guid.TryParse(Path.GetFileNameWithoutExtension(capturedFileName), out _),
                "File name should start with a GUID");
        }

        [Fact]
        public async Task Handle_ShouldUploadWithCorrectContentType()
        {
            // Arrange
            var mockFile = CreateFormFile("banner.jpg", "image/jpeg", new byte[] { 1 });
            await _handler.Handle(new UploadDescriptionImageCommand(mockFile), CancellationToken.None);

            // Assert
            await _blobService.Received(1).UploadAsync(
                Arg.Any<string>(),
                Arg.Any<Stream>(),
                "dot-net",
                BlobContentCategoryEnum.Image,
                true,
                "image/jpeg");
        }

        [Fact]
        public async Task Handle_ShouldHandleMultipleSequentialUploadsSuccessfully()
        {
            // Arrange
            var file1 = CreateFormFile("img1.png", "image/png", new byte[] { 1 });
            var file2 = CreateFormFile("img2.png", "image/png", new byte[] { 2 });
            _blobService.GetBlobUrl(Arg.Any<string>(), Arg.Any<string>())
                .Returns("https://storage/images/img1.png", "https://storage/images/img2.png");

            // Act
            var result1 = await _handler.Handle(new UploadDescriptionImageCommand(file1), CancellationToken.None);
            var result2 = await _handler.Handle(new UploadDescriptionImageCommand(file2), CancellationToken.None);

            // Assert
            Assert.NotEqual(result1, result2);
            await _blobService.Received(2).UploadAsync(
                Arg.Any<string>(),
                Arg.Any<Stream>(),
                "dot-net",
                BlobContentCategoryEnum.Image,
                true,
                Arg.Any<string>());
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenBlobServiceFails()
        {
            // Arrange
            var mockFile = CreateFormFile("fail.png", "image/png", new byte[] { 1, 2, 3 });

            _blobService
                .When(x => x.UploadAsync(
                    Arg.Any<string>(),
                    Arg.Any<Stream>(),
                    Arg.Any<string>(),
                    Arg.Any<BlobContentCategoryEnum>(),
                    Arg.Any<bool>(),
                    Arg.Any<string?>()))
                .Do(_ => throw new InvalidOperationException("Simulated blob failure"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _handler.Handle(new UploadDescriptionImageCommand(mockFile), CancellationToken.None));

            Assert.Equal("Simulated blob failure", ex.Message);
        }

        private static IFormFile CreateFormFile(string name, string contentType, byte[] content)
        {
            var stream = new MemoryStream(content);
            var file = Substitute.For<IFormFile>();
            file.FileName.Returns(name);
            file.Length.Returns(content.Length);
            file.ContentType.Returns(contentType);
            file.OpenReadStream().Returns(stream);
            return file;
        }
    }
}

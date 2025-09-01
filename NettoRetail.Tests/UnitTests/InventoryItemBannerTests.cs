using Azure.Storage.Sas;
using MediatR;
using Microsoft.AspNetCore.Http;
using Moq;
using NettoRetail.Application.CQRS.ProductBanners.Commands;
using NettoRetail.Application.CQRS.ProductBanners.Queries;
using NettoRetail.Application.Exceptions;
using NettoRetail.Domain.Entities;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Tests.UnitTests
{
    public class InventoryItemBannersCommandHandlerTests
    {
        [Fact]
        public async Task Handle_WithFiles_UploadsAndReturnsResponses()
        {
            // Arrange
            var bannerRepoMock = new Mock<IInventoryItemBannerRepository>();
            var blobServiceMock = new Mock<IAzureBlobService>();
            var inventoryItemId = Guid.NewGuid();


            bannerRepoMock
                .Setup(r => r.GetMaxOrderIndexAsync(inventoryItemId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);


            bannerRepoMock
                .Setup(r => r.AddAsync(It.IsAny<InventoryItemBannerEntity>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((InventoryItemBannerEntity banner, CancellationToken _) =>
                {
                    banner.Id = Guid.NewGuid();
                    return banner;
                });


            blobServiceMock.Setup(s => s.UploadAsync(
                    It.IsAny<string>(),
                    It.IsAny<Stream>(),
                    It.IsAny<string>(),
                    It.IsAny<BlobContentCategoryEnum>(),
                    true,
                    "image/png"))
                .ReturnsAsync("mock-file-id");


            blobServiceMock.Setup(s => s.GetBlobUrl(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((string container, string path) => $"https://mock.blob/{path}");

            blobServiceMock.Setup(s => s.GenerateBlobSasToken(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<BlobSasPermissions>(),
                    null,
                    null))
                .Returns("mock-sas-token");

            var handler = new AddInventoryItemBannerCommandHandler(bannerRepoMock.Object, blobServiceMock.Object);


            var fileMock = new Mock<IFormFile>();
            var content = "Fake image content";
            var fileName = "test.png";
            var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));

            fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.ContentType).Returns("image/png");
            fileMock.Setup(f => f.Length).Returns(ms.Length);

            var command = new AddInventoryItemBannersCommand(
                inventoryItemId,
                new[] { fileMock.Object });

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Single(result);
            var response = result.First();
            Assert.Equal(fileName, response.FileName);
            Assert.Contains("https://mock.blob/images/", response.FileUrl);
            Assert.Contains("mock-sas-token", response.FileUrl);


            bannerRepoMock.Verify(r => r.AddAsync(It.IsAny<InventoryItemBannerEntity>(), It.IsAny<CancellationToken>()), Times.Once);
            blobServiceMock.Verify(s => s.UploadAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<BlobContentCategoryEnum>(), true, "image/png"), Times.Once);
            blobServiceMock.Verify(s => s.GenerateBlobSasToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<BlobSasPermissions>(), null, null), Times.Once);
        }


        [Fact]
        public async Task Handle_AddBanners_WithNonZeroMaxOrderIndex_IncrementsCorrectly()
        {
            // Arrange
            var bannerRepoMock = new Mock<IInventoryItemBannerRepository>();
            var blobServiceMock = new Mock<IAzureBlobService>();
            var inventoryItemId = Guid.NewGuid();

            bannerRepoMock.Setup(r => r.GetMaxOrderIndexAsync(inventoryItemId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(5);

            bannerRepoMock.Setup(r => r.AddAsync(It.IsAny<InventoryItemBannerEntity>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((InventoryItemBannerEntity banner, CancellationToken _) =>
                {
                    banner.Id = Guid.NewGuid();
                    return banner;
                });

            blobServiceMock.Setup(s => s.UploadAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<BlobContentCategoryEnum>(), true, "image/png"))
                .ReturnsAsync("mock-file-id");
            blobServiceMock.Setup(s => s.GetBlobUrl(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((string _, string path) => $"https://mock.blob/{path}");
            blobServiceMock.Setup(s => s.GenerateBlobSasToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<BlobSasPermissions>(), null, null))
                .Returns("sas");

            var handler = new AddInventoryItemBannerCommandHandler(bannerRepoMock.Object, blobServiceMock.Object);

            var fileMock = new Mock<IFormFile>();
            var content = "content";
            var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
            fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
            fileMock.Setup(f => f.FileName).Returns("test.png");
            fileMock.Setup(f => f.ContentType).Returns("image/png");
            fileMock.Setup(f => f.Length).Returns(ms.Length);

            var command = new AddInventoryItemBannersCommand(inventoryItemId, new[] { fileMock.Object });

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Single(result);
            bannerRepoMock.Verify(r => r.GetMaxOrderIndexAsync(inventoryItemId, It.IsAny<CancellationToken>()), Times.Once);
            Assert.Equal(6, result[0].OrderIndex);
        }



        [Fact]
        public async Task Handle_WithNoFiles_ReturnsEmptyList()
        {
            // Arrange
            var bannerRepoMock = new Mock<IInventoryItemBannerRepository>();
            var blobServiceMock = new Mock<IAzureBlobService>();
            var handler = new AddInventoryItemBannerCommandHandler(bannerRepoMock.Object, blobServiceMock.Object);

            var command = new AddInventoryItemBannersCommand(Guid.NewGuid(), Array.Empty<IFormFile>());

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Empty(result);

            bannerRepoMock.Verify(r => r.AddAsync(It.IsAny<InventoryItemBannerEntity>(), It.IsAny<CancellationToken>()), Times.Never);
            blobServiceMock.Verify(s => s.UploadAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<BlobContentCategoryEnum>(), true, "image/png"), Times.Never);
            blobServiceMock.Verify(s => s.GenerateBlobSasToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<BlobSasPermissions>(), null, null), Times.Never);
        }

        [Fact]
        public async Task Handle_WithMultipleFiles_UploadsAllAndReturnsResponses()
        {
            // Arrange
            var bannerRepoMock = new Mock<IInventoryItemBannerRepository>();
            var blobServiceMock = new Mock<IAzureBlobService>();
            var inventoryItemId = Guid.NewGuid();

            bannerRepoMock
                .Setup(r => r.GetMaxOrderIndexAsync(inventoryItemId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(0);

            bannerRepoMock
                .Setup(r => r.AddAsync(It.IsAny<InventoryItemBannerEntity>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((InventoryItemBannerEntity banner, CancellationToken _) =>
                {
                    banner.Id = Guid.NewGuid();
                    return banner;
                });

            blobServiceMock.Setup(s => s.UploadAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<BlobContentCategoryEnum>(), true, "image/png"))
                .ReturnsAsync("mock-file-id");

            blobServiceMock.Setup(s => s.GetBlobUrl(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((string container, string path) => $"https://mock.blob/{path}");

            blobServiceMock.Setup(s => s.GenerateBlobSasToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<BlobSasPermissions>(), null, null))
                .Returns("mock-sas-token");

            var handler = new AddInventoryItemBannerCommandHandler(bannerRepoMock.Object, blobServiceMock.Object);


            var files = Enumerable.Range(1, 2).Select(i =>
            {
                var fileMock = new Mock<IFormFile>();
                var content = $"Fake content {i}";
                var fileName = $"test{i}.png";
                var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));

                fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
                fileMock.Setup(f => f.FileName).Returns(fileName);
                fileMock.Setup(f => f.ContentType).Returns("image/png");
                fileMock.Setup(f => f.Length).Returns(ms.Length);
                return fileMock.Object;
            }).ToArray();

            var command = new AddInventoryItemBannersCommand(inventoryItemId, files);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(2, result.Count);
            foreach (var response in result)
            {
                Assert.Contains("https://mock.blob/images/", response.FileUrl);
                Assert.Contains("mock-sas-token", response.FileUrl);
            }

            bannerRepoMock.Verify(r => r.AddAsync(It.IsAny<InventoryItemBannerEntity>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
            blobServiceMock.Verify(s => s.UploadAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<BlobContentCategoryEnum>(), true, "image/png"), Times.Exactly(2));
            blobServiceMock.Verify(s => s.GenerateBlobSasToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<BlobSasPermissions>(), null, null), Times.Exactly(2));


        }











        [Fact]
        public async Task Handle_WithValidBanner_DeletesBannerAndReorders_1Based()
        {
            // Arrange
            var bannerRepoMock = new Mock<IInventoryItemBannerRepository>();
            var blobServiceMock = new Mock<IAzureBlobService>();

            var bannerId = Guid.NewGuid();
            var inventoryItemId = Guid.NewGuid();

            var existingBanner = new InventoryItemBannerEntity
            {
                Id = bannerId,
                InventoryItemId = inventoryItemId,
                FileIdentifier = "file-123",
                OrderIndex = 3
            };

            var bannersToReorder = new List<InventoryItemBannerEntity>
            {
                new InventoryItemBannerEntity { Id = Guid.NewGuid(), InventoryItemId = inventoryItemId, OrderIndex = 4, FileIdentifier = "file-456" },
                new InventoryItemBannerEntity { Id = Guid.NewGuid(), InventoryItemId = inventoryItemId, OrderIndex = 5, FileIdentifier = "file-789" }
            };

            bannerRepoMock.Setup(r => r.GetByIdAsync(bannerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingBanner);

            bannerRepoMock.Setup(r => r.DeleteAsync(existingBanner, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            bannerRepoMock.Setup(r => r.GetByInventoryItemIdAsync(
                    inventoryItemId,
                    It.IsAny<Specification<InventoryItemBannerEntity>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(bannersToReorder);

            blobServiceMock.Setup(s => s.DeleteBlobAsync(existingBanner.FileIdentifier))
                .Returns(Task.CompletedTask);

            var handler = new DeleteInventoryItemBannerCommandHandler(bannerRepoMock.Object, blobServiceMock.Object);
            var command = new DeleteInventoryItemBannerCommand(bannerId);


            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert

            Assert.Equal(Unit.Value, result);

            bannerRepoMock.Verify(r => r.GetByIdAsync(bannerId, It.IsAny<CancellationToken>()), Times.Once);
            blobServiceMock.Verify(s => s.DeleteBlobAsync("file-123"), Times.Once);
            bannerRepoMock.Verify(r => r.DeleteAsync(existingBanner, It.IsAny<CancellationToken>()), Times.Once);
            bannerRepoMock.Verify(r => r.UpdateManyAsync(It.IsAny<IEnumerable<InventoryItemBannerEntity>>(), It.IsAny<CancellationToken>()), Times.Once);


            Assert.Equal(3, bannersToReorder[0].OrderIndex);
            Assert.Equal(4, bannersToReorder[1].OrderIndex);
        }



        [Fact]
        public async Task Handle_BannerNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var bannerRepoMock = new Mock<IInventoryItemBannerRepository>();
            var blobServiceMock = new Mock<IAzureBlobService>();

            var bannerId = Guid.NewGuid();

            bannerRepoMock.Setup(r => r.GetByIdAsync(bannerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((InventoryItemBannerEntity?)null);

            var handler = new DeleteInventoryItemBannerCommandHandler(bannerRepoMock.Object, blobServiceMock.Object);

            var command = new DeleteInventoryItemBannerCommand(bannerId);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));

            bannerRepoMock.Verify(r => r.GetByIdAsync(bannerId, It.IsAny<CancellationToken>()), Times.Once);
            blobServiceMock.Verify(s => s.DeleteBlobAsync(It.IsAny<string>()), Times.Never);
            bannerRepoMock.Verify(r => r.DeleteAsync(It.IsAny<InventoryItemBannerEntity>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_BannerWithNoFileIdentifier_SkipsBlobDeletion()
        {
            // Arrange
            var bannerRepoMock = new Mock<IInventoryItemBannerRepository>();
            var blobServiceMock = new Mock<IAzureBlobService>();

            var bannerId = Guid.NewGuid();
            var inventoryItemId = Guid.NewGuid();

            var existingBanner = new InventoryItemBannerEntity
            {
                Id = bannerId,
                InventoryItemId = inventoryItemId,
                FileIdentifier = null,
                OrderIndex = 1
            };

            bannerRepoMock.Setup(r => r.GetByIdAsync(bannerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingBanner);

            bannerRepoMock.Setup(r => r.DeleteAsync(existingBanner, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            bannerRepoMock.Setup(r => r.GetByInventoryItemIdAsync(
                    inventoryItemId,
                    It.IsAny<Specification<InventoryItemBannerEntity>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<InventoryItemBannerEntity>());

            var handler = new DeleteInventoryItemBannerCommandHandler(bannerRepoMock.Object, blobServiceMock.Object);

            var command = new DeleteInventoryItemBannerCommand(bannerId);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(Unit.Value, result);

            blobServiceMock.Verify(s => s.DeleteBlobAsync(It.IsAny<string>()), Times.Never);
            bannerRepoMock.Verify(r => r.DeleteAsync(existingBanner, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenExceptionOccurs_ThrowsBadRequestException()
        {
            // Arrange
            var bannerRepoMock = new Mock<IInventoryItemBannerRepository>();
            var blobServiceMock = new Mock<IAzureBlobService>();

            var bannerId = Guid.NewGuid();

            var existingBanner = new InventoryItemBannerEntity
            {
                Id = bannerId,
                InventoryItemId = Guid.NewGuid(),
                FileIdentifier = "file-123",
                OrderIndex = 0
            };

            bannerRepoMock.Setup(r => r.GetByIdAsync(bannerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingBanner);

            blobServiceMock.Setup(s => s.DeleteBlobAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception("blob service failure"));

            var handler = new DeleteInventoryItemBannerCommandHandler(bannerRepoMock.Object, blobServiceMock.Object);

            var command = new DeleteInventoryItemBannerCommand(bannerId);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Contains("blob service failure", ex.Message);

            blobServiceMock.Verify(s => s.DeleteBlobAsync("file-123"), Times.Once);
            bannerRepoMock.Verify(r => r.DeleteAsync(It.IsAny<InventoryItemBannerEntity>(), It.IsAny<CancellationToken>()), Times.Never);
        }











        [Fact]
        public async Task Handle_WithValid_UpdatesOrderAndReturnsResponses_1Based()
        {
            // Arrange
            var bannerRepoMock = new Mock<IInventoryItemBannerRepository>();
            var blobServiceMock = new Mock<IAzureBlobService>();

            var bannerId = Guid.NewGuid();
            var inventoryItemId = Guid.NewGuid();

            var banners = new List<InventoryItemBannerEntity>
            {
                new InventoryItemBannerEntity { Id = Guid.NewGuid(), InventoryItemId = inventoryItemId, OrderIndex = 1, FileIdentifier = "file-0", FileName = "banner0.png" },
                new InventoryItemBannerEntity { Id = Guid.NewGuid(), InventoryItemId = inventoryItemId, OrderIndex = 2, FileIdentifier = "file-1", FileName = "banner1.png" },
                new InventoryItemBannerEntity { Id = bannerId, InventoryItemId = inventoryItemId, OrderIndex = 3, FileIdentifier = "file-2", FileName = "banner2.png" },
                new InventoryItemBannerEntity { Id = Guid.NewGuid(), InventoryItemId = inventoryItemId, OrderIndex = 4, FileIdentifier = "file-3", FileName = "banner3.png" }
            };

            var newOrderIndex = 2;

            bannerRepoMock.Setup(r => r.GetByIdAsync(bannerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(banners[2]);

            bannerRepoMock.Setup(r => r.GetByInventoryItemIdAsync(
                    inventoryItemId,
                    It.IsAny<Specification<InventoryItemBannerEntity>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(banners);

            bannerRepoMock.Setup(r => r.UpdateManyAsync(It.IsAny<IEnumerable<InventoryItemBannerEntity>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            blobServiceMock.Setup(s => s.GetBlobUrl(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((string container, string path) => $"https://mock.blob/{path}");

            blobServiceMock.Setup(s =>
                s.GenerateBlobSasToken(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<BlobSasPermissions>(),
                    It.IsAny<TimeSpan>(),
                    It.IsAny<string?>()
                ))
                .Returns("mock-sas-token");

            var handler = new ReorderInventoryItemBannerOrderCommandHandler(bannerRepoMock.Object, blobServiceMock.Object);
            var command = new ReorderInventoryItemBannerOrderCommand(bannerId, newOrderIndex);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(4, result.Count);
            Assert.Equal(bannerId, result[1].BannerId);
            Assert.Equal(2, result[1].OrderIndex);
            Assert.Equal(1, result[0].OrderIndex);
            Assert.Equal(3, result[2].OrderIndex);
            Assert.Equal(4, result[3].OrderIndex);

            bannerRepoMock.Verify(r => r.UpdateManyAsync(It.Is<IEnumerable<InventoryItemBannerEntity>>(list => list.Count() == 4), It.IsAny<CancellationToken>()), Times.Once);

        }



        [Fact]
        public async Task Handle_BannerNotFoundReorder_ThrowsNotFoundException()
        {
            // Arrange
            var bannerRepoMock = new Mock<IInventoryItemBannerRepository>();
            var blobServiceMock = new Mock<IAzureBlobService>();
            var bannerId = Guid.NewGuid();

            bannerRepoMock.Setup(r => r.GetByIdAsync(bannerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((InventoryItemBannerEntity?)null);

            var handler = new ReorderInventoryItemBannerOrderCommandHandler(bannerRepoMock.Object, blobServiceMock.Object);
            var command = new ReorderInventoryItemBannerOrderCommand(bannerId, 1);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_NewOrderIndexOutOfRange_ThrowsBadRequestException()
        {
            // Arrange
            var bannerRepoMock = new Mock<IInventoryItemBannerRepository>();
            var blobServiceMock = new Mock<IAzureBlobService>();
            var bannerId = Guid.NewGuid();
            var inventoryItemId = Guid.NewGuid();

            var banner = new InventoryItemBannerEntity
            {
                Id = bannerId,
                InventoryItemId = inventoryItemId,
                OrderIndex = 0,
                FileIdentifier = "file-0"
            };

            bannerRepoMock.Setup(r => r.GetByIdAsync(bannerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(banner);

            bannerRepoMock.Setup(r => r.GetByInventoryItemIdAsync(
                    inventoryItemId,
                    It.IsAny<Specification<InventoryItemBannerEntity>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<InventoryItemBannerEntity> { banner });

            var handler = new ReorderInventoryItemBannerOrderCommandHandler(bannerRepoMock.Object, blobServiceMock.Object);
            var command = new ReorderInventoryItemBannerOrderCommand(bannerId, 5);

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(command, CancellationToken.None));
        }


        [Fact]
        public async Task Handle_WithNoBanners_DoesNothingAndReturnsEmptyList()
        {
            // Arrange
            var bannerRepoMock = new Mock<IInventoryItemBannerRepository>();
            var blobServiceMock = new Mock<IAzureBlobService>();
            var bannerId = Guid.NewGuid();
            var inventoryItemId = Guid.NewGuid();

            bannerRepoMock.Setup(r => r.GetByIdAsync(bannerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((InventoryItemBannerEntity?)null);

            var handler = new ReorderInventoryItemBannerOrderCommandHandler(bannerRepoMock.Object, blobServiceMock.Object);
            var command = new ReorderInventoryItemBannerOrderCommand(bannerId, 1);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Contains("not found", ex.Message);

            bannerRepoMock.Verify(r => r.UpdateAsync(It.IsAny<InventoryItemBannerEntity>(), It.IsAny<CancellationToken>()), Times.Never);
            blobServiceMock.VerifyNoOtherCalls();
        }











        [Fact]
        public async Task Handle_WithBanners_ReturnsOrderedResponses()
        {
            // Arrange
            var bannerRepoMock = new Mock<IInventoryItemBannerRepository>();
            var blobServiceMock = new Mock<IAzureBlobService>();
            var inventoryItemId = Guid.NewGuid();

            var banners = new List<InventoryItemBannerEntity>
            {
                new InventoryItemBannerEntity
                {
                    Id = Guid.NewGuid(),
                    InventoryItemId = inventoryItemId,
                    OrderIndex = 1,
                    FileIdentifier = "file-1",
                    FileName = "banner1.png"
                },
                new InventoryItemBannerEntity
                {
                    Id = Guid.NewGuid(),
                    InventoryItemId = inventoryItemId,
                    OrderIndex = 0,
                    FileIdentifier = "file-0",
                    FileName = "banner0.png"
                }
            };

            bannerRepoMock
                .Setup(r => r.GetByInventoryItemIdAsync(
                    inventoryItemId,
                    It.IsAny<Specification<InventoryItemBannerEntity>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(banners);

            blobServiceMock.Setup(s => s.GetBlobUrl(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((string _, string path) => $"https://mock.blob/{path}");

            blobServiceMock.Setup(s => s.GenerateBlobSasToken(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<BlobSasPermissions>(),
                    It.IsAny<TimeSpan?>(),
                    It.IsAny<string?>()))
                .Returns("mock-sas-token");

            var handler = new GetInventoryItemBannersQueryHandler(bannerRepoMock.Object, blobServiceMock.Object);
            var query = new GetInventoryItemBannersQuery(inventoryItemId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(2, result.Count);

            Assert.Equal(banners[1].Id, result[0].BannerId);
            Assert.Equal(banners[0].Id, result[1].BannerId);

            Assert.All(result, r => Assert.Contains("mock-sas-token", r.FileUrl));

            bannerRepoMock.Verify(r => r.GetByInventoryItemIdAsync(
                inventoryItemId,
                It.IsAny<Specification<InventoryItemBannerEntity>>(),
                It.IsAny<CancellationToken>()), Times.Once);

            blobServiceMock.Verify(s => s.GetBlobUrl(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
            blobServiceMock.Verify(s => s.GenerateBlobSasToken(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<BlobSasPermissions>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<string?>()), Times.Exactly(2));
        }



        [Fact]
        public async Task Handle_WithNoBanners_ReturnsEmptyList()
        {
            // Arrange
            var bannerRepoMock = new Mock<IInventoryItemBannerRepository>();
            var blobServiceMock = new Mock<IAzureBlobService>();
            var inventoryItemId = Guid.NewGuid();

            bannerRepoMock.Setup(r => r.GetByInventoryItemIdAsync(
                    inventoryItemId,
                    It.IsAny<Specification<InventoryItemBannerEntity>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<InventoryItemBannerEntity>());

            var handler = new GetInventoryItemBannersQueryHandler(bannerRepoMock.Object, blobServiceMock.Object);

            // Act
            var result = await handler.Handle(new GetInventoryItemBannersQuery(inventoryItemId), CancellationToken.None);

            // Assert
            Assert.Empty(result);
        }

    }

}


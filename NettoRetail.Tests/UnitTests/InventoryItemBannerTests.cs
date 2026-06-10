using Azure.Storage.Sas;
using MediatR;
using Microsoft.AspNetCore.Http;
using NettoRetail.Application.CQRS.ProductBanners.Commands;
using NettoRetail.Application.CQRS.ProductBanners.Queries;
using NettoRetail.Application.Exceptions;
using NettoRetail.Domain.Entities;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace NettoRetail.Tests.UnitTests
{
    public class InventoryItemBannersCommandHandlerTests
    {
        [Fact]
        public async Task Handle_WithFiles_UploadsAndReturnsResponses()
        {
            // Arrange
            var bannerRepo = Substitute.For<IInventoryItemBannerRepository>();
            var blobService = Substitute.For<IAzureBlobService>();
            var inventoryItemId = Guid.NewGuid();

            bannerRepo.GetMaxOrderIndexAsync(inventoryItemId, Arg.Any<CancellationToken>()).Returns(0);

            bannerRepo.AddAsync(Arg.Any<InventoryItemBannerEntity>(), Arg.Any<CancellationToken>())
                .Returns(call =>
                {
                    var banner = call.Arg<InventoryItemBannerEntity>();
                    banner.Id = Guid.NewGuid();
                    return banner;
                });

            blobService.UploadAsync(
                    Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(),
                    Arg.Any<BlobContentCategoryEnum>(), true, "image/png")
                .Returns("mock-file-id");

            blobService.GetBlobUrl(Arg.Any<string>(), Arg.Any<string>())
                .Returns(call =>
                {
                    var container = call.ArgAt<string>(0);
                    var path = call.ArgAt<string>(1);
                    return $"https://mock.blob/{path}";
                });

            blobService.GenerateBlobSasToken(
                    Arg.Any<string>(), Arg.Any<string>(), Arg.Any<BlobSasPermissions>(), null, null)
                .Returns("mock-sas-token");

            var handler = new AddInventoryItemBannerCommandHandler(bannerRepo, CreateInventoryItemRepository(), blobService);

            var fileMock = Substitute.For<IFormFile>();
            var content = "Fake image content";
            var fileName = "test.png";
            var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));

            fileMock.OpenReadStream().Returns(ms);
            fileMock.FileName.Returns(fileName);
            fileMock.ContentType.Returns("image/png");
            fileMock.Length.Returns(ms.Length);

            var command = new AddInventoryItemBannersCommand(inventoryItemId, new[] { fileMock });

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Single(result);
            var response = result.First();
            Assert.Equal(fileName, response.FileName);
            Assert.Contains("https://mock.blob/images/", response.FileUrl);
            Assert.Contains("mock-sas-token", response.FileUrl);

            await bannerRepo.Received(1).AddAsync(Arg.Any<InventoryItemBannerEntity>(), Arg.Any<CancellationToken>());
            await blobService.Received(1).UploadAsync(Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<BlobContentCategoryEnum>(), true, "image/png");
            blobService.Received(1).GenerateBlobSasToken(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<BlobSasPermissions>(), null, null);
        }


        [Fact]
        public async Task Handle_AddBanners_WithNonZeroMaxOrderIndex_IncrementsCorrectly()
        {
            // Arrange
            var bannerRepo = Substitute.For<IInventoryItemBannerRepository>();
            var blobService = Substitute.For<IAzureBlobService>();
            var inventoryItemId = Guid.NewGuid();

            bannerRepo.GetMaxOrderIndexAsync(inventoryItemId, Arg.Any<CancellationToken>()).Returns(5);

            bannerRepo.AddAsync(Arg.Any<InventoryItemBannerEntity>(), Arg.Any<CancellationToken>())
                .Returns(call =>
                {
                    var banner = call.Arg<InventoryItemBannerEntity>();
                    banner.Id = Guid.NewGuid();
                    return banner;
                });

            blobService.UploadAsync(
                    Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(),
                    Arg.Any<BlobContentCategoryEnum>(), true, "image/png")
                .Returns("mock-file-id");

            blobService.GetBlobUrl(Arg.Any<string>(), Arg.Any<string>())
                .Returns(call => {
                    var container = call.ArgAt<string>(0);
                    var path = call.ArgAt<string>(1);
                    return $"https://mock.blob/{path}";
                });

            blobService.GenerateBlobSasToken(
                    Arg.Any<string>(), Arg.Any<string>(), Arg.Any<BlobSasPermissions>(), null, null)
                .Returns("sas");

            var handler = new AddInventoryItemBannerCommandHandler(bannerRepo, CreateInventoryItemRepository(), blobService);

            var fileMock = Substitute.For<IFormFile>();
            var content = "content";
            var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
            fileMock.OpenReadStream().Returns(ms);
            fileMock.FileName.Returns("test.png");
            fileMock.ContentType.Returns("image/png");
            fileMock.Length.Returns(ms.Length);

            var command = new AddInventoryItemBannersCommand(inventoryItemId, new[] { fileMock });

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Single(result);
            Assert.Equal(6, result[0].OrderIndex);
            await bannerRepo.Received(1).GetMaxOrderIndexAsync(inventoryItemId, Arg.Any<CancellationToken>());
        }


        [Fact]
        public async Task Handle_WithNoFiles_ReturnsEmptyList()
        {
            // Arrange
            var bannerRepo = Substitute.For<IInventoryItemBannerRepository>();
            var blobService = Substitute.For<IAzureBlobService>();
            var handler = new AddInventoryItemBannerCommandHandler(bannerRepo, CreateInventoryItemRepository(), blobService);

            var command = new AddInventoryItemBannersCommand(Guid.NewGuid(), Array.Empty<IFormFile>());

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Empty(result);
            await bannerRepo.DidNotReceiveWithAnyArgs().AddAsync(default, default);
            await blobService.DidNotReceiveWithAnyArgs().UploadAsync(default, default, default, default, default, default);
            blobService.DidNotReceiveWithAnyArgs().GenerateBlobSasToken(default, default, default, default, default);
        }


        [Fact]
        public async Task Handle_WithMultipleFiles_UploadsAllAndReturnsResponses()
        {
            // Arrange
            var bannerRepo = Substitute.For<IInventoryItemBannerRepository>();
            var blobService = Substitute.For<IAzureBlobService>();
            var inventoryItemId = Guid.NewGuid();

            bannerRepo.GetMaxOrderIndexAsync(inventoryItemId, Arg.Any<CancellationToken>()).Returns(0);

            bannerRepo.AddAsync(Arg.Any<InventoryItemBannerEntity>(), Arg.Any<CancellationToken>())
                .Returns(call =>
                {
                    var banner = call.Arg<InventoryItemBannerEntity>();
                    banner.Id = Guid.NewGuid();
                    return banner;
                });

            blobService.UploadAsync(
                    Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(),
                    Arg.Any<BlobContentCategoryEnum>(), true, "image/png")
                .Returns("mock-file-id");

            blobService.GetBlobUrl(Arg.Any<string>(), Arg.Any<string>())
                .Returns(call =>
                {
                    var container = call.ArgAt<string>(0);
                    var path = call.ArgAt<string>(1);
                    return $"https://mock.blob/{path}";
                });

            blobService.GenerateBlobSasToken(
                    Arg.Any<string>(), Arg.Any<string>(), Arg.Any<BlobSasPermissions>(), null, null)
                .Returns("mock-sas-token");

            var handler = new AddInventoryItemBannerCommandHandler(bannerRepo, CreateInventoryItemRepository(), blobService);

            var files = Enumerable.Range(1, 2).Select(i =>
            {
                var fileMock = Substitute.For<IFormFile>();
                var content = $"Fake content {i}";
                var fileName = $"test{i}.png";
                var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));

                fileMock.OpenReadStream().Returns(ms);
                fileMock.FileName.Returns(fileName);
                fileMock.ContentType.Returns("image/png");
                fileMock.Length.Returns(ms.Length);

                return fileMock;
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

            await bannerRepo.Received(2).AddAsync(Arg.Any<InventoryItemBannerEntity>(), Arg.Any<CancellationToken>());
            await blobService.Received(2).UploadAsync(Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<BlobContentCategoryEnum>(), true, "image/png");
            blobService.Received(2).GenerateBlobSasToken(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<BlobSasPermissions>(), null, null);
        }

        [Fact]
        public async Task Handle_InventoryItemDoesNotExist_ThrowsNotFoundException()
        {
            // Arrange
            var bannerRepo = Substitute.For<IInventoryItemBannerRepository>();
            var inventoryRepo = Substitute.For<IInventoryItemRepository>();
            var blobService = Substitute.For<IAzureBlobService>();
            var inventoryItemId = Guid.NewGuid();

            inventoryRepo.ExistsAsync(Arg.Any<Specification<InventoryItemEntity>>(), Arg.Any<CancellationToken>())
                .Returns(false);

            var handler = new AddInventoryItemBannerCommandHandler(bannerRepo, inventoryRepo, blobService);

            var fileMock = Substitute.For<IFormFile>();
            var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("Fake image content"));
            fileMock.OpenReadStream().Returns(ms);
            fileMock.FileName.Returns("test.png");
            fileMock.ContentType.Returns("image/png");
            fileMock.Length.Returns(ms.Length);

            var command = new AddInventoryItemBannersCommand(inventoryItemId, new[] { fileMock });

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
            await bannerRepo.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
        }

        [Fact]
        public async Task Handle_TotalImagesWouldExceedLimit_ThrowsBadRequestException()
        {
            // Arrange
            var bannerRepo = Substitute.For<IInventoryItemBannerRepository>();
            var blobService = Substitute.For<IAzureBlobService>();
            var inventoryItemId = Guid.NewGuid();

            bannerRepo.GetByInventoryItemIdAsync(
                    inventoryItemId,
                    Arg.Any<Specification<InventoryItemBannerEntity>>(),
                    Arg.Any<CancellationToken>())
                .Returns(Enumerable.Range(1, 8).Select(i => new InventoryItemBannerEntity
                {
                    Id = Guid.NewGuid(),
                    InventoryItemId = inventoryItemId,
                    FileIdentifier = $"file-{i}",
                    OrderIndex = i
                }).ToList());

            var handler = new AddInventoryItemBannerCommandHandler(bannerRepo, CreateInventoryItemRepository(), blobService);

            var fileMock = Substitute.For<IFormFile>();
            var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("Fake image content"));
            fileMock.OpenReadStream().Returns(ms);
            fileMock.FileName.Returns("test.png");
            fileMock.ContentType.Returns("image/png");
            fileMock.Length.Returns(ms.Length);

            var command = new AddInventoryItemBannersCommand(inventoryItemId, new[] { fileMock });

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(command, CancellationToken.None));
            await bannerRepo.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
        }


        [Fact]
        public async Task Handle_WithValidBanner_DeletesBannerAndReorders_1Based()
        {
            // Arrange
            var bannerRepo = Substitute.For<IInventoryItemBannerRepository>();
            var blobService = Substitute.For<IAzureBlobService>();

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

            bannerRepo.GetByIdAsync(bannerId, Arg.Any<CancellationToken>()).Returns(existingBanner);
            bannerRepo.DeleteAsync(existingBanner, Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
            bannerRepo.GetByInventoryItemIdAsync(
                inventoryItemId,
                Arg.Any<Specification<InventoryItemBannerEntity>>(),
                Arg.Any<CancellationToken>()).Returns(bannersToReorder);

            blobService.DeleteBlobAsync($"images/{existingBanner.FileIdentifier}").Returns(Task.CompletedTask);

            var handler = new DeleteInventoryItemBannerCommandHandler(bannerRepo, blobService);
            var command = new DeleteInventoryItemBannerCommand(bannerId);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(Unit.Value, result);

            await bannerRepo.Received(1).GetByIdAsync(bannerId, Arg.Any<CancellationToken>());
            await blobService.Received(1).DeleteBlobAsync("images/file-123");
            await bannerRepo.Received(1).DeleteAsync(existingBanner, Arg.Any<CancellationToken>());
            await bannerRepo.Received(1).UpdateManyAsync(Arg.Any<IEnumerable<InventoryItemBannerEntity>>(), Arg.Any<CancellationToken>());

            Assert.Equal(3, bannersToReorder[0].OrderIndex);
            Assert.Equal(4, bannersToReorder[1].OrderIndex);
        }


        [Fact]
        public async Task Handle_BannerNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var bannerRepo = Substitute.For<IInventoryItemBannerRepository>();
            var blobService = Substitute.For<IAzureBlobService>();

            var bannerId = Guid.NewGuid();
            bannerRepo.GetByIdAsync(bannerId, Arg.Any<CancellationToken>()).Returns((InventoryItemBannerEntity?)null);

            var handler = new DeleteInventoryItemBannerCommandHandler(bannerRepo, blobService);
            var command = new DeleteInventoryItemBannerCommand(bannerId);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));

            await bannerRepo.Received(1).GetByIdAsync(bannerId, Arg.Any<CancellationToken>());
            await blobService.DidNotReceiveWithAnyArgs().DeleteBlobAsync(default!);
            await bannerRepo.DidNotReceiveWithAnyArgs().DeleteAsync(Arg.Any<InventoryItemBannerEntity>(), Arg.Any<CancellationToken>());
        }


        [Fact]
        public async Task Handle_BannerWithNoFileIdentifier_SkipsBlobDeletion()
        {
            // Arrange
            var bannerRepo = Substitute.For<IInventoryItemBannerRepository>();
            var blobService = Substitute.For<IAzureBlobService>();

            var bannerId = Guid.NewGuid();
            var inventoryItemId = Guid.NewGuid();

            var existingBanner = new InventoryItemBannerEntity
            {
                Id = bannerId,
                InventoryItemId = inventoryItemId,
                FileIdentifier = null,
                OrderIndex = 1
            };

            bannerRepo.GetByIdAsync(bannerId, Arg.Any<CancellationToken>()).Returns(existingBanner);
            bannerRepo.DeleteAsync(existingBanner, Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
            bannerRepo.GetByInventoryItemIdAsync(inventoryItemId, Arg.Any<Specification<InventoryItemBannerEntity>>(), Arg.Any<CancellationToken>())
                .Returns(new List<InventoryItemBannerEntity>());

            var handler = new DeleteInventoryItemBannerCommandHandler(bannerRepo, blobService);
            var command = new DeleteInventoryItemBannerCommand(bannerId);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(Unit.Value, result);

            await blobService.DidNotReceiveWithAnyArgs().DeleteBlobAsync(default!);
            await bannerRepo.Received(1).DeleteAsync(existingBanner, Arg.Any<CancellationToken>());
        }


        [Fact]
        public async Task Handle_WhenExceptionOccurs_ThrowsOriginalException()
        {
            // Arrange
            var bannerRepo = Substitute.For<IInventoryItemBannerRepository>();
            var blobService = Substitute.For<IAzureBlobService>();

            var bannerId = Guid.NewGuid();

            var existingBanner = new InventoryItemBannerEntity
            {
                Id = bannerId,
                InventoryItemId = Guid.NewGuid(),
                FileIdentifier = "file-123",
                OrderIndex = 0
            };

            bannerRepo.GetByIdAsync(bannerId, Arg.Any<CancellationToken>()).Returns(existingBanner);
            blobService.DeleteBlobAsync(Arg.Any<string>()).ThrowsAsync(new Exception("blob service failure"));

            var handler = new DeleteInventoryItemBannerCommandHandler(bannerRepo, blobService);
            var command = new DeleteInventoryItemBannerCommand(bannerId);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => handler.Handle(command, CancellationToken.None));
            Assert.Contains("blob service failure", ex.Message);

            await blobService.Received(1).DeleteBlobAsync("images/file-123");
            await bannerRepo.DidNotReceiveWithAnyArgs().DeleteAsync(Arg.Any<InventoryItemBannerEntity>(), Arg.Any<CancellationToken>());
        }


        [Fact]
        public async Task Handle_WithValid_UpdatesOrderAndReturnsResponses_1Based()
        {
            // Arrange
            var bannerRepo = Substitute.For<IInventoryItemBannerRepository>();
            var blobService = Substitute.For<IAzureBlobService>();

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

            bannerRepo.GetByIdAsync(bannerId, Arg.Any<CancellationToken>()).Returns(banners[2]);
            bannerRepo.GetByInventoryItemIdAsync(inventoryItemId, Arg.Any<Specification<InventoryItemBannerEntity>>(), Arg.Any<CancellationToken>())
                .Returns(banners);
            bannerRepo.UpdateManyAsync(Arg.Any<IEnumerable<InventoryItemBannerEntity>>(), Arg.Any<CancellationToken>())
                .Returns(Task.CompletedTask);

            blobService.GetBlobUrl(Arg.Any<string>(), Arg.Any<string>())
                .Returns(callInfo => $"https://mock.blob/{callInfo.ArgAt<string>(1)}");
            blobService.GenerateBlobSasToken(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<BlobSasPermissions>(), Arg.Any<TimeSpan>(), Arg.Any<string?>())
                .Returns("mock-sas-token");

            var handler = new ReorderInventoryItemBannerOrderCommandHandler(bannerRepo, blobService);
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

            await bannerRepo.Received(1).UpdateManyAsync(
                Arg.Is<IEnumerable<InventoryItemBannerEntity>>(list => list.Count() == 4),
                Arg.Any<CancellationToken>());
        }


        [Fact]
        public async Task Handle_BannerNotFoundReorder_ThrowsNotFoundException()
        {
            // Arrange
            var bannerRepo = Substitute.For<IInventoryItemBannerRepository>();
            var blobService = Substitute.For<IAzureBlobService>();

            var bannerId = Guid.NewGuid();
            bannerRepo.GetByIdAsync(bannerId, Arg.Any<CancellationToken>()).Returns((InventoryItemBannerEntity?)null);

            var handler = new ReorderInventoryItemBannerOrderCommandHandler(bannerRepo, blobService);
            var command = new ReorderInventoryItemBannerOrderCommand(bannerId, 1);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
        }


        [Fact]
        public async Task Handle_NewOrderIndexOutOfRange_ThrowsBadRequestException()
        {
            // Arrange
            var bannerRepo = Substitute.For<IInventoryItemBannerRepository>();
            var blobService = Substitute.For<IAzureBlobService>();

            var bannerId = Guid.NewGuid();
            var inventoryItemId = Guid.NewGuid();

            var banner = new InventoryItemBannerEntity
            {
                Id = bannerId,
                InventoryItemId = inventoryItemId,
                OrderIndex = 0,
                FileIdentifier = "file-0"
            };

            bannerRepo.GetByIdAsync(bannerId, Arg.Any<CancellationToken>()).Returns(banner);
            bannerRepo.GetByInventoryItemIdAsync(inventoryItemId, Arg.Any<Specification<InventoryItemBannerEntity>>(), Arg.Any<CancellationToken>())
                .Returns(new List<InventoryItemBannerEntity> { banner });

            var handler = new ReorderInventoryItemBannerOrderCommandHandler(bannerRepo, blobService);
            var command = new ReorderInventoryItemBannerOrderCommand(bannerId, 5);

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(command, CancellationToken.None));
        }


        [Fact]
        public async Task Handle_WithNoBanners_DoesNothingAndReturnsEmptyList()
        {
            // Arrange
            var bannerRepo = Substitute.For<IInventoryItemBannerRepository>();
            var blobService = Substitute.For<IAzureBlobService>();

            var bannerId = Guid.NewGuid();
            var inventoryItemId = Guid.NewGuid();

            bannerRepo.GetByIdAsync(bannerId, Arg.Any<CancellationToken>()).Returns((InventoryItemBannerEntity?)null);

            var handler = new ReorderInventoryItemBannerOrderCommandHandler(bannerRepo, blobService);
            var command = new ReorderInventoryItemBannerOrderCommand(bannerId, 1);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Contains("not found", ex.Message);

            await bannerRepo.DidNotReceiveWithAnyArgs().UpdateAsync(default!, default);
            blobService.DidNotReceiveWithAnyArgs().GetBlobUrl(default!, default!);
            blobService.DidNotReceiveWithAnyArgs().GenerateBlobSasToken(default!, default!, default, default, default);
        }


        [Fact]
        public async Task Handle_WithBanners_ReturnsOrderedResponses()
        {
            // Arrange
            var bannerRepo = Substitute.For<IInventoryItemBannerRepository>();
            var blobService = Substitute.For<IAzureBlobService>();
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
    
            bannerRepo.GetByInventoryItemIdAsync(
                inventoryItemId,
                Arg.Any<Specification<InventoryItemBannerEntity>>(),
                Arg.Any<CancellationToken>())
                .Returns(banners);

            blobService.GetBlobUrl(Arg.Any<string>(), Arg.Any<string>())
                .Returns(callInfo => $"https://mock.blob/{callInfo.ArgAt<string>(1)}");

            blobService.GenerateBlobSasToken(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<BlobSasPermissions>(),
                Arg.Any<TimeSpan?>(),
                Arg.Any<string?>())
                .Returns("mock-sas-token");

            var handler = new GetInventoryItemBannersQueryHandler(bannerRepo, blobService);
            var query = new GetInventoryItemBannersQuery(inventoryItemId);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Equal(2, result.Count);

            Assert.Equal(banners[1].Id, result[0].BannerId);
            Assert.Equal(banners[0].Id, result[1].BannerId);

            Assert.All(result, r => Assert.Contains("mock-sas-token", r.FileUrl));

            await bannerRepo.Received(1).GetByInventoryItemIdAsync(
                inventoryItemId,
                Arg.Any<Specification<InventoryItemBannerEntity>>(),
                Arg.Any<CancellationToken>());

            blobService.Received(2).GetBlobUrl(Arg.Any<string>(), Arg.Any<string>());
            blobService.GenerateBlobSasToken(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<BlobSasPermissions>(),
                Arg.Any<TimeSpan?>(),
                Arg.Any<string?>())
                .Returns("mock-sas-token");

        }


        [Fact]
        public async Task Handle_WithNoBanners_ReturnsEmptyList()
        {
            // Arrange
            var bannerRepo = Substitute.For<IInventoryItemBannerRepository>();
            var blobService = Substitute.For<IAzureBlobService>();
            var inventoryItemId = Guid.NewGuid();

            bannerRepo.GetByInventoryItemIdAsync(
                    inventoryItemId,
                    Arg.Any<Specification<InventoryItemBannerEntity>>(),
                    Arg.Any<CancellationToken>())
                .Returns(new List<InventoryItemBannerEntity>());

            var handler = new GetInventoryItemBannersQueryHandler(bannerRepo, blobService);

            // Act
            var result = await handler.Handle(new GetInventoryItemBannersQuery(inventoryItemId), CancellationToken.None);

            // Assert
            Assert.Empty(result);

            await bannerRepo.Received(1).GetByInventoryItemIdAsync(
                inventoryItemId,
                Arg.Any<Specification<InventoryItemBannerEntity>>(),
                Arg.Any<CancellationToken>());

            blobService.DidNotReceiveWithAnyArgs().GetBlobUrl(default!, default!);
            blobService.DidNotReceiveWithAnyArgs().GenerateBlobSasToken(default!, default!, default, default, default);
        }

        private static IInventoryItemRepository CreateInventoryItemRepository()
        {
            var inventoryRepo = Substitute.For<IInventoryItemRepository>();
            inventoryRepo.ExistsAsync(Arg.Any<Specification<InventoryItemEntity>>(), Arg.Any<CancellationToken>())
                .Returns(true);
            return inventoryRepo;
        }
    }
}



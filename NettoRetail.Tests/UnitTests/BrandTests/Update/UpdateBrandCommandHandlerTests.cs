using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NettoRetail.Application.CQRS.Brands.Commands;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.Brand;
using NettoRetail.Application.Services.IBlobNameGenerator;
using NettoRetail.DataAccess.Options;
using NettoRetail.DataAccess.Specifications.Brands;
using NettoRetail.Domain.Entities;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace NettoRetail.Tests.UnitTests.BrandTests.Update;

public class UpdateBrandCommandHandlerTests
{
    private readonly IBrandRepository _brandRepository;
    private readonly IAzureBlobService _azureBlobService;
    private readonly IBlobNameGenerator _blobNameGenerator;
    private readonly IOptions<AzureBlobStorageOptions> _azureBlobStorageOptions;
    private readonly IMapper _mapper;

    private readonly UpdateBrandCommandHandler _handler;
    public UpdateBrandCommandHandlerTests()
    {
        _brandRepository = Substitute.For<IBrandRepository>();
        _azureBlobService = Substitute.For<IAzureBlobService>();
        _blobNameGenerator = Substitute.For<IBlobNameGenerator>();
        _azureBlobStorageOptions = Substitute.For<IOptions<AzureBlobStorageOptions>>();
        _mapper = Substitute.For<IMapper>();

        _azureBlobStorageOptions.Value.Returns(new AzureBlobStorageOptions
        {
            ContainerName = "test-container"
        });

        _handler = new UpdateBrandCommandHandler(_brandRepository, _azureBlobService, _blobNameGenerator, _azureBlobStorageOptions, _mapper);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenBrandDoesNotExist()
    {
        // Arrange
        var request = CreateRequest();
        var command = new UpdateBrandCommand(1, request, 10);

        _brandRepository.GetAsync(Arg.Any<GetBrandByIdAndShopIdSpecification>(), Arg.Any<CancellationToken>())
            .ReturnsNull<BrandEntity>();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));

        Assert.Equal(HandlerConstants.Brand.BrandNotFoundMessage, exception.Message);
        await _brandRepository.DidNotReceive().UpdateAsync(Arg.Any<BrandEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldUpdateBrandWithoutLogo_WhenLogoFileIsNotProvided()
    {
        // Arrange
        var brand = CreateBrand();
        var request = CreateRequest(withLogo: false);
        var command = new UpdateBrandCommand(brand.Id, request, 10);

        _brandRepository.GetAsync(Arg.Any<GetBrandByIdAndShopIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(brand);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _azureBlobService.DidNotReceiveWithAnyArgs().UploadAsync(default!, default!, default!, default, default);
        await _brandRepository.Received(1).UpdateAsync(brand, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldUpdateBrandAndUploadLogo_WhenLogoFileIsProvided()
    {
        // Arrange
        var brand = CreateBrand();
        var request = CreateRequest(withLogo: true);
        var command = new UpdateBrandCommand(brand.Id, request, 10);

        var generatedBlobPath = "brands/10/logo.png";
        var uploadedLogoUrl = "https://azure.storage.com/test-container/brands/10/logo.png";

        _brandRepository.GetAsync(Arg.Any<GetBrandByIdAndShopIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(brand);

        _blobNameGenerator.GenerateBlobName(request.LogoFile!.FileName, command.ShopId, false)
            .Returns(generatedBlobPath);

        _azureBlobService.UploadAsync(
                generatedBlobPath,
                Arg.Any<Stream>(),
                _azureBlobStorageOptions.Value.ContainerName,
                BlobContentCategoryEnum.Image,
                true)
            .Returns(uploadedLogoUrl);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(uploadedLogoUrl, brand.LogoImageUrl);

        _blobNameGenerator.Received(1).GenerateBlobName(request.LogoFile.FileName, command.ShopId, false);
        await _azureBlobService.Received(1).UploadAsync(
            generatedBlobPath,
            Arg.Any<Stream>(),
            _azureBlobStorageOptions.Value.ContainerName,
            BlobContentCategoryEnum.Image,
            true);

        await _brandRepository.Received(1).UpdateAsync(brand, Arg.Any<CancellationToken>());
    }

    private static UpdateBrandRequest CreateRequest(bool withLogo = false)
    {
        IFormFile? logoFile = null;

        if (withLogo)
        {
            logoFile = Substitute.For<IFormFile>();
            logoFile.Length.Returns(1024);
            logoFile.FileName.Returns("logo.png");
            logoFile.OpenReadStream().Returns(new MemoryStream());
        }

        return new UpdateBrandRequest
        {
            Name = "Updated Brand Name",
            Description = "Updated Description",
            LogoFile = logoFile
        };
    }

    private static BrandEntity CreateBrand(int id = 1)
    {
        return new BrandEntity
        {
            Id = id,
            Name = "Original Brand Name",
            Description = "Original Description",
            LogoImageUrl = "https://azure.storage.com/old-logo.png",
            ShopId = 2
        };
    }

}

using Azure.Storage.Sas;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Enums;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Options;
using NettoRetail.DataAccess.Repositories;
using NettoRetail.DataAccess.Specifications.Customer;
using NettoRetail.DataAccess.Specifications.ProductReview;
using NettoRetail.DataAccess.UOW;
using NettoRetail.Domain.Entities;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Infrastructure.Services;
using NettoRetail.Shared.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;
using NettoRetail.Shared.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;

namespace NettoRetail.Application.CQRS.ProductReviews.Commands
{
    public record UploadProductReviewImageCommand(IFormFile Image, Guid ProductReviewId) : IRequest;
    public class UploadProductReviewImageCommandHandler(IProductReviewRepository productReviewRepository,
        IUnitOfWork unitOfWork,
        IOptions<AzureBlobStorageOptions> azureBlobStorageOptions,
        IAzureBlobService azureBlobService) : IRequestHandler<UploadProductReviewImageCommand>
    {
        public async Task Handle(UploadProductReviewImageCommand request, CancellationToken cancellationToken)
        {
            var specification = new GetProductReviewByIdSpecification(request.ProductReviewId);
            var review = await productReviewRepository.GetAsync(specification, cancellationToken);
            if (review is null)
                throw new NotFoundException("Product review with such id not found.");

            var containerName = azureBlobStorageOptions.Value.ContainerName;
            var fileName = $"product review image/{Guid.NewGuid()}{(int)BlobAccess.Private}{Path.GetExtension(request.Image.FileName)}";

            await using var stream = request.Image.OpenReadStream();
            var imageUrl = await azureBlobService.UploadAsync(
                fileName, stream, containerName, BlobContentCategoryEnum.Image, true);

            var existingUrls = review.PhotoUrls != null 
                ? JsonSerializer.Deserialize<List<string>>(review.PhotoUrls) ?? new List<string>() : new List<string>();

            existingUrls.Add(imageUrl);
            review.PhotoUrls = JsonSerializer.Serialize(existingUrls);
            using var transaction = unitOfWork.BeginTransaction();
            await productReviewRepository.UpdateAsync(review, cancellationToken);
        }
    }
}

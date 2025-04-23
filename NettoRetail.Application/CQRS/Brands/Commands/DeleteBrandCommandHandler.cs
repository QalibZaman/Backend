using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Brands;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Brands.Commands;

public record DeleteBrandCommand(int Id, int ShopId) : IRequest;

public class DeleteBrandCommandHandler(IBrandRepository brandRepository,
    IAzureBlobService azureBlobService) : IRequestHandler<DeleteBrandCommand>
{
    public async Task Handle(DeleteBrandCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetBrandsByIdSpecification(request.Id);

        var brand = await brandRepository.GetAsync(specification, cancellationToken);

        if (brand == null)
            throw new NotFoundException($"There is no brand with Id: {request.Id}");

        if (brand.ShopId != request.ShopId)
            throw new MyUnauthorizedException("This brand is inaccessible");

        if (brand.LogoImageUrl != null)
            await azureBlobService.DeleteByUrlAsync(brand.LogoImageUrl);

        await brandRepository.DeleteAsync(brand, cancellationToken);
    }
}

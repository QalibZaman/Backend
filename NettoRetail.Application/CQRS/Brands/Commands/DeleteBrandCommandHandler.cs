using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Brands;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Brands.Commands;

public record DeleteBrandCommand(int Id, int ShopId) : IRequest;

public class DeleteBrandCommandHandler(IBrandRepository brandRepository) : IRequestHandler<DeleteBrandCommand>
{
    public async Task Handle(DeleteBrandCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetBrandByIdSpecification(request.Id);

        var brand = await brandRepository.GetAsync(specification, cancellationToken);

        if (brand == null)
            throw new NotFoundException($"There is no brand with Id: {request.Id}");

        if (brand.ShopId != request.ShopId)
            throw new MyUnauthorizedException("This brand is inaccessible");

        await brandRepository.DeleteAsync(brand, cancellationToken);
    }
}

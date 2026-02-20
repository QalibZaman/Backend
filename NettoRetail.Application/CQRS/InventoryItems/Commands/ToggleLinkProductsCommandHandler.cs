using MediatR;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Services.IInventoryService;
using NettoRetail.Shared.Interfaces.Utilities;
namespace NettoRetail.Application.CQRS.InventoryItems.Commands
{
    public record ToggleLinkProductsCommand(Guid ParentId, Guid ChildId) : IRequest<ToggleLinkProductsResponse>;
    public class ToggleLinkProductsCommandHandler(IInventoryService service, IUnitOfWork unitOfWork) : IRequestHandler<ToggleLinkProductsCommand, ToggleLinkProductsResponse>
    {

        public async Task<ToggleLinkProductsResponse> Handle(ToggleLinkProductsCommand request, CancellationToken cancellationToken)
        {
            using var transaction = unitOfWork.BeginTransaction();

            var result = await service.ToggleLinkProductsAsync(request.ParentId, request.ChildId, cancellationToken);

            transaction.Commit();
            return result;
        }
    }
}

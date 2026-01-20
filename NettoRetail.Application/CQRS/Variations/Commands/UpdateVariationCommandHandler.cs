using MediatR;
using NettoRetail.Application.Models.Request.Variation;
using NettoRetail.Application.Services.VariationService;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.Variations.Commands
{
    public record UpdateVariationCommand(UpdateVariationRequest Model, int ShopId) : IRequest;

    public class UpdateVariationCommandHandler(
     IVariationService variationService,
     IUnitOfWork unitOfWork)
     : IRequestHandler<UpdateVariationCommand>
    {
        public async Task Handle(UpdateVariationCommand command, CancellationToken cancellationToken)
        {
            using var trx = unitOfWork.BeginTransaction();

            await variationService.UpdateVariationAsync(command.Model, command.ShopId, cancellationToken);

            trx.Commit();
        }
    }

}

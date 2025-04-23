using Mapster;
using MediatR;
using NettoRetail.Application.Models.Payments;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Payments.Commands
{
    public record AddPaymentCommand(Guid UserId, string Token) : IRequest<PaymentResponse>;

    public class AddPaymentCommandHandler(IPaymentRepository paymentRepository) : IRequestHandler<AddPaymentCommand, PaymentResponse>
    {
        public async Task<PaymentResponse> Handle(AddPaymentCommand request, CancellationToken cancellationToken)
        {
            var payment = request.Adapt<PaymentEntity>();
            payment.CreatedAt = DateTime.UtcNow;

            var result = await paymentRepository.AddAsync(payment, cancellationToken);

            return result.Adapt<PaymentResponse>();
        }
    }
}

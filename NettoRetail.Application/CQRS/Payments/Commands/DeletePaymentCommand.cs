using Bogus.DataSets;
using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Repositories;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.DataAccess.Specifications.Payments;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;
using NettoRetail.Shared.Specifications;
using Stripe;
using Stripe.Forwarding;

namespace NettoRetail.Application.CQRS.Payments.Commands;

public record DeletePaymentCommand(Guid AccountId, string PaymentMethodId) : IRequest<PaymentMethodDeleteResponse>;

public class DeletePaymentCommandHandler(IPaymentRepository paymentRepository, PaymentMethodService paymentMethodService, IUnitOfWork unitOfWork) : IRequestHandler<DeletePaymentCommand, PaymentMethodDeleteResponse>
{
    public async Task<PaymentMethodDeleteResponse> Handle(DeletePaymentCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetPaymentMethodByIdSpecification(request.AccountId, request.PaymentMethodId);
        var paymentMethod = await paymentRepository.GetAsync(specification, cancellationToken)
                            ?? throw new NotFoundException("Payment method not found in DB");

        var isLast = paymentMethod.Account.PaymentMethods?.Count == 1;

        var transaction = unitOfWork.BeginTransaction();
        try
        {
            await paymentRepository.DeleteAsync(paymentMethod, cancellationToken);
            await paymentMethodService.DetachAsync(request.PaymentMethodId);

            transaction.Commit();

            var response = new PaymentMethodDeleteResponse
            {
                IsLastPaymentMethod = isLast
            };

            return response;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}
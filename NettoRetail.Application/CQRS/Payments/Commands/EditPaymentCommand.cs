using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Payments;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using System.Threading;
using NettoRetail.DataAccess.Specifications.Payment;
using Stripe;
using Mapster;

namespace NettoRetail.Application.CQRS.Payments.Commands
{
    public record EditPaymentCommand(Guid UserId, Guid Id, EditPaymentRequest Request) : IRequest<PaymentMethodEntity>;
    public class EditPaymentCommandHandler(IPaymentRepository paymentRepository, PaymentMethodService paymentMethodService)
        : IRequestHandler<EditPaymentCommand, PaymentMethodEntity>
    {
        public async Task<PaymentMethodEntity> Handle(EditPaymentCommand request, CancellationToken cancellationToken)
        {
            var specToken = new PaymentMethodByTokenSpecification(request.Request.StripePaymentMethodId);
            var existing = (await paymentRepository.GetAllBySpecAsync(specToken, cancellationToken)).FirstOrDefault();

            if (existing == null)
                throw new NotFoundException("Payment method not found.");


            var stripeOptions = new PaymentMethodUpdateOptions
            {
                BillingDetails = new PaymentMethodBillingDetailsOptions
                {
                    Name = request.Request.CardHolderName,
                }
            };

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(60));

            try
            {
                await paymentMethodService.UpdateAsync(
                    request.Request.StripePaymentMethodId,
                    stripeOptions,
                    cancellationToken: cts.Token);
            }
            catch (OperationCanceledException) when (cts.IsCancellationRequested)
            {
                throw new PaymentExceptions("The payment could not be completed. Try later");
            }
            catch (StripeException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.RequestTimeout ||
                                                                   ex.StripeError?.Type == "api_connection_error")
            {
                throw new PaymentExceptions("Unable to connect to the payment provider");
            }

            var specId = new ActivePaymentMethodsByUserIdSpecification(request.UserId);
            var activeMethods = await paymentRepository.GetAllBySpecAsync(specId, cancellationToken);

            foreach (var method in activeMethods)
            {
                method.IsActive = false;
                await paymentRepository.UpdateAsync(method, cancellationToken);
            }

            var paymentMethod = await paymentMethodService.GetAsync(request.Request.StripePaymentMethodId, cancellationToken: cancellationToken);

            existing.CardExpMonth = paymentMethod.Card?.ExpMonth ?? 0;
            existing.CardExpYear = paymentMethod.Card?.ExpYear ?? 0;
            existing.FourLastCardNumbers = paymentMethod.Card?.Last4 ?? string.Empty;

            await paymentRepository.UpdateAsync(existing, cancellationToken);

            return existing;
        }
    }
}

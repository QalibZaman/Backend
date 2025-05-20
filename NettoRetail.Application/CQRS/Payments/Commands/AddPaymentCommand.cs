using Mapster;
using MediatR;
using NettoRetail.Application.Models.Payments;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using Stripe;

namespace NettoRetail.Application.CQRS.Payments.Commands
{
    public record AddPaymentCommand(Guid AccountId, string PaymentMethodId) : IRequest;

    public class AddPaymentCommandHandler(IAccountRepository accountRepository, IPaymentRepository paymentRepository, CustomerService customerService, PaymentMethodService paymentMethodService) : IRequestHandler<AddPaymentCommand>
    {
        public async Task Handle(AddPaymentCommand request, CancellationToken cancellationToken)
        {
            var specification = new GetAccountByIdSpecification(request.AccountId);
            var account = await accountRepository.GetAsync(specification, cancellationToken);

            if (String.IsNullOrEmpty(account.StripeCustomerId))
            {
                var customerOptions = new CustomerCreateOptions
                {
                    Name = $"{account.FirstName} {account.LastName}",
                    Email = account.Email
                };

                var customer = await customerService.CreateAsync(customerOptions);
                account.StripeCustomerId = customer.Id;

                account = await accountRepository.UpdateAsync(account, cancellationToken);
            }

            var paymentMethod = await paymentMethodService.GetAsync(request.PaymentMethodId);

            await paymentMethodService.AttachAsync(request.PaymentMethodId, new PaymentMethodAttachOptions
            {
                Customer = account.StripeCustomerId
            });

            var paymentMethodRecord = new PaymentMethodEntity
            {
                AccountId = account.Id,
                StripePaymentMethodId = request.PaymentMethodId,
                CardBrand = paymentMethod.Card.Brand,
                CardExpMonth = paymentMethod.Card.ExpMonth,
                CardExpYear = paymentMethod.Card.ExpYear,
                FourLastCardNumbers = paymentMethod.Card.Last4,
                CreatedAt = DateTime.UtcNow,
            };

            await paymentRepository.AddAsync(paymentMethodRecord, cancellationToken);
        }
    }
}

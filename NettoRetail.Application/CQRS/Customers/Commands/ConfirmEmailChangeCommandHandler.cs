using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.DataAccess.Specifications.Customer;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using System.Security.Claims;

namespace NettoRetail.Application.Models.Request.Customer
{
    public record ConfirmEmailChangeCommand(string Token) : IRequest<Unit>;
    public class ConfirmEmailChangeCommandHandler(ICustomerRepository customerRepository, IJwtManager jwtManager) : IRequestHandler<ConfirmEmailChangeCommand, Unit>
    {
        public async Task<Unit> Handle(ConfirmEmailChangeCommand request, CancellationToken cancellationToken)
        {
            ClaimsPrincipal principal;
            try
            {
                principal = jwtManager.ValidateCustomerResetPasswordToken(request.Token);

            }
            catch (Exception ex)
            {
                throw new BadRequestException("The confirmation link is invalid or has expired.");
            }

            var newEmail = principal.FindFirstValue(ClaimTypes.Email)
                ?? throw new BadRequestException("Email claim is missing in token.");
            var customerId = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new BadRequestException("Customer ID claim is missing in token.");
            var tokenType = principal.FindFirstValue(AuthClaims.TokenType)
                ?? throw new BadRequestException("Token type claim is missing in token.");

            var specification = new GetCustomerByIdSpecification(Guid.Parse(customerId));

            var customer = await customerRepository.GetAsync(specification, cancellationToken)
                ?? throw new BadRequestException("Customer not found.");

            customer.Email = newEmail;
            await customerRepository.UpdateAsync(customer, cancellationToken);
            return Unit.Value;
        }
    }
}

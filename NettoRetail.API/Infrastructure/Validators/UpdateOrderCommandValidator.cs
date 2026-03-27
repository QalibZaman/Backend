using FluentValidation;
using NettoRetail.Application.CQRS.Orders.Commands;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Order;
using NettoRetail.DataAccess.Specifications.Customer;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.API.Infrastructure.Validators;

public class UpdateOrderCommandValidator : AbstractValidator<UpdateOrderCommand>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IValidator<UpdateOrderModel> _validator;
    public UpdateOrderCommandValidator(ICustomerRepository customerRepository, IValidator<UpdateOrderModel> validator)
    {
        _customerRepository = customerRepository;
        _validator = validator;
        ConfigureRules();
    }

    private void ConfigureRules()
    {
        RuleFor(x => x.Model)
            .SetValidator(_validator);

        RuleFor(x => x.Model.CustomerId)
            .CustomAsync(CustomerExistsAsync);
    }

    private async Task CustomerExistsAsync(Guid customerId, ValidationContext<UpdateOrderCommand> context, CancellationToken cancellationToken)
    {
        var customerSpecification = new GetCustomerByIdSpecification(customerId);
        var customerExists = await _customerRepository.Exists(customerSpecification, cancellationToken);
        if (!customerExists)
            throw new NotFoundException(ValidationErrorMessages.Customer.CustomerNotFoundErrorMessage);
    }
}

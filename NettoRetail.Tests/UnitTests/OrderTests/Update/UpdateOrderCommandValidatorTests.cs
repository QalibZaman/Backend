using FluentAssertions;
using FluentValidation;
using FluentValidation.TestHelper;
using NettoRetail.API.Infrastructure.Validators;
using NettoRetail.Application.CQRS.Orders.Commands;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Order;
using NettoRetail.DataAccess.Specifications.Customer;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.OrderTests.Update;

public class UpdateOrderCommandValidatorTests
{
    private readonly ICustomerRepository _customerRepository;
    private readonly UpdateOrderCommandValidator _validator;
    public UpdateOrderCommandValidatorTests()
    {
        var updateOrderModelValidator = new InlineValidator<UpdateOrderModel>();

        _customerRepository = Substitute.For<ICustomerRepository>();
        _validator = new UpdateOrderCommandValidator(_customerRepository, updateOrderModelValidator);
    }

    [Fact]
    public async Task Validate_Should_Not_Have_Error_When_OrderCommand_Is_Valid()
    {
        var command = SetupUpdateOrderCommand();

        var result = await _validator.TestValidateAsync(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_Should_Have_Error_When_Customer_Is_Invalid()
    {
        var command = SetupUpdateOrderCommand();

        _customerRepository.Exists(Arg.Any<GetCustomerByIdSpecification>(), Arg.Any<CancellationToken>()).Returns(false);

        var result = async () => await _validator.TestValidateAsync(command);

        await result.Should().ThrowAsync<NotFoundException>()
            .WithMessage(ValidationErrorMessages.Customer.CustomerNotFoundErrorMessage);
    }
    private UpdateOrderCommand SetupUpdateOrderCommand()
    {
        var command = new UpdateOrderCommand(
            new UpdateOrderModel
            {
                CustomerId = Guid.NewGuid(),
                ItemsToUpsert = new Dictionary<Guid, int> { { Guid.NewGuid(), 1 } },
                ItemsToDelete = new List<Guid> { Guid.NewGuid() }
            },
            21,
            Guid.NewGuid(),
            23);

        _customerRepository.Exists(Arg.Any<GetCustomerByIdSpecification>(), Arg.Any<CancellationToken>()).Returns(true);

        return command;
    }
}

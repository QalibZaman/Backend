using MediatR;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Controllers.V1;
using NettoRetail.Application.CQRS.Customers.Commands;
using NettoRetail.Application.Models.Request.Customer;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.CustomerManagementTests;

public class CustomersControllerTests
{
    [Fact]
    public async Task UpdateCustomer_ShouldSendCommandAndReturnNoContent()
    {
        var mediator = Substitute.For<IMediator>();
        var controller = new CustomersController(mediator);
        var request = new UpdateCustomerRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            CountryId = 21,
            PhoneNumber = "123 456 789",
            Remarks = "Valid remarks",
            IsActive = true
        };

        var result = await controller.UpdateCustomer("ABC123", request, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        await mediator.Received(1).Send(
            Arg.Is<UpdateCustomerCommand>(x =>
                x.LoyaltyId == "ABC123" &&
                ReferenceEquals(x.Model, request)),
            Arg.Any<CancellationToken>());
    }
}

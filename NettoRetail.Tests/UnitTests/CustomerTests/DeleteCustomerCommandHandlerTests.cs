using FluentAssertions;
using NettoRetail.Application.CQRS.Customers.Commands;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.DataAccess.Specifications.Customer;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NettoRetail.Tests.UnitTests.CustomerTests
{
    public class DeleteCustomerCommandHandlerTests
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ISessionTerminationService _sessionTerminationService;
        private readonly IUnitOfWork _unitOfWork;

        private readonly DeleteCustomerCommandHandler _handler;


        public DeleteCustomerCommandHandlerTests()
        {
            _customerRepository = Substitute.For<ICustomerRepository>();
            _sessionTerminationService = Substitute.For<ISessionTerminationService>();
            _unitOfWork = Substitute.For<IUnitOfWork>();

            _handler = new DeleteCustomerCommandHandler(_customerRepository, _sessionTerminationService, _unitOfWork);
        }


        [Fact]
        public async Task Handle_ShouldDeleteCustomer_WhenCustomerExists()
        {
            //Arrange
            var command = new DeleteCustomerCommand(Guid.NewGuid(), 1);
            var customer = new CustomerEntity
            {
                Id = command.Id,
                ShopId = command.ShopId,
                IsDeleted = false,
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com",
                PasswordHash = "hashed"
            };
            _customerRepository.GetAsync(Arg.Any<GetCustomerByIdSpecification>(), Arg.Any<CancellationToken>())
                .Returns(customer);
            //Act
            await _handler.Handle(command, CancellationToken.None);
            //Assert
            await _customerRepository.Received(1).UpdateAsync(Arg.Is<CustomerEntity>(c => c.IsDeleted), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_ShouldThrowNotFoundException_WhenCustomerDoesNotExist()
        {
            //Arrange
            var command = new DeleteCustomerCommand(Guid.NewGuid(), 1);
            _customerRepository.GetAsync(Arg.Any<GetCustomerByIdSpecification>(), Arg.Any<CancellationToken>())
                .Returns((CustomerEntity?)null);
            //Act
            var act = async () => await _handler.Handle(command, CancellationToken.None);
            //Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage(HandlerConstants.Customer.CustomerNotFoundMessage);
        }

        [Fact]
        public async Task Handle_ShouldThrowForbiddenException_WhenCustomerBelongsToAnotherShop()
        {
            var command = new DeleteCustomerCommand(Guid.NewGuid(), 1);
            _customerRepository.GetAsync(Arg.Any<GetCustomerByIdSpecification>(), Arg.Any<CancellationToken>())
                .Returns(new CustomerEntity
                {
                    Id = command.Id,
                    ShopId = 2,
                    IsDeleted = false,
                    FirstName = "Test",
                    LastName = "User",
                    Email = "test@example.com",
                    PasswordHash = "hashed"
                });

            var act = async () => await _handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ForbiddenException>()
                .WithMessage(HandlerConstants.Customer.CannotDeleteCustomerFromAnotherShopMessage);
        }


        [Fact]
        public async Task Handle_ShouldThrowBadRequestException_WhenCustomerHasActiveOrders()
        {
            var command = new DeleteCustomerCommand(Guid.NewGuid(), 1);
            _customerRepository.GetAsync(Arg.Any<GetCustomerByIdSpecification>(), Arg.Any<CancellationToken>())
                .Returns(new CustomerEntity
                {
                    Id = command.Id,
                    ShopId = command.ShopId,
                    IsDeleted = false,
                    FirstName = "Test",
                    LastName = "User",
                    Email = "test@example.com",
                    PasswordHash = "hashed",
                    Order = new List<OrderEntity>
                    {
                        new OrderEntity
                        {
                            CreationDate = DateTime.UtcNow,
                            Creator = "Example",
                            CustomerId = command.Id,
                            OrderStatus = OrderStatusEnum.InProgress,
                            DeliveryType = default(OrderDeliveryTypeEnum)
                        }
                    }
                });

            var act = async () => await _handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<BadRequestException>()
                .WithMessage(HandlerConstants.Customer.CustomerHasActiveOrdersMessage);
        }

    }

}

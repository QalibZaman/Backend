using MapsterMapper;
using NettoRetail.Application.Interfaces.Services.UserService;
using NettoRetail.Application.Models.Request;
using NettoRetail.Domain.Entities;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.UserTests
{
    public class UserServiceTests
    {
        private readonly IMapper _mapper;
        private readonly UserService _service;

        public UserServiceTests()
        {
            _mapper = Substitute.For<IMapper>();

            _service = new UserService(_mapper);
        }

        [Fact]
        public async Task UpdatePersonalInformationAsync_ShouldMapCustomerInformation()
        {
            // Arrange
            var customer = CreateCustomer();
            var request = CreateRequest();

            // Act
            await _service.UpdatePersonalInformationAsync(
                customer,
                request,
                CancellationToken.None);

            // Assert
            _mapper
                .Received(1)
                .Map(
                    request,
                    Arg.Is<BaseUserEntity>(x => x == customer));

            _mapper
                .Received(1)
                .Map(
                    request,
                    Arg.Is<AddressEntity>(x => x == customer.Address));
        }

        [Fact]
        public async Task UpdatePersonalInformationAsync_ShouldCreateCustomerAddress_WhenAddressDoesNotExist()
        {
            // Arrange
            var customer = CreateCustomer();
            var request = CreateRequest();

            Assert.Null(customer.Address);

            // Act
            await _service.UpdatePersonalInformationAsync(
                customer,
                request,
                CancellationToken.None);

            // Assert
            Assert.NotNull(customer.Address);
            Assert.Same(customer, customer.Address!.Customer);

            _mapper
                .Received(1)
                .Map(
                    request,
                    Arg.Is<BaseUserEntity>(x => x == customer));

            _mapper
                .Received(1)
                .Map(
                    request,
                    Arg.Is<AddressEntity>(x => x == customer.Address));
        }

        [Fact]
        public async Task UpdatePersonalInformationAsync_ShouldUseExistingCustomerAddress()
        {
            // Arrange
            var customer = CreateCustomer();

            var existingAddress = new AddressEntity
            {
                Customer = customer
            };

            customer.Address = existingAddress;

            var request = CreateRequest();

            // Act
            await _service.UpdatePersonalInformationAsync(
                customer,
                request,
                CancellationToken.None);

            // Assert
            Assert.NotNull(customer.Address);
            Assert.Same(existingAddress, customer.Address);

            _mapper
                .Received(1)
                .Map(
                    request,
                    Arg.Is<BaseUserEntity>(x => x == customer));

            _mapper
                .Received(1)
                .Map(
                    request,
                    Arg.Is<AddressEntity>(x => x == existingAddress));
        }

        [Fact]
        public async Task UpdatePersonalInformationAsync_ShouldMapStaffInformation()
        {
            // Arrange
            var staff = CreateStaff();
            var request = CreateRequest();

            // Act
            await _service.UpdatePersonalInformationAsync(
                staff,
                request,
                CancellationToken.None);

            // Assert
            _mapper
                .Received(1)
                .Map(
                    request,
                    Arg.Is<BaseUserEntity>(x => x == staff));

            _mapper
                .Received(1)
                .Map(
                    request,
                    Arg.Is<AddressEntity>(x => x == staff.Address));
        }

        [Fact]
        public async Task UpdatePersonalInformationAsync_ShouldCreateStaffAddress_WhenAddressDoesNotExist()
        {
            // Arrange
            var staff = CreateStaff();
            var request = CreateRequest();

            Assert.Null(staff.Address);

            // Act
            await _service.UpdatePersonalInformationAsync(
                staff,
                request,
                CancellationToken.None);

            // Assert
            Assert.NotNull(staff.Address);
            Assert.Same(staff, staff.Address!.Staff);

            _mapper
                .Received(1)
                .Map(
                    request,
                    Arg.Is<BaseUserEntity>(x => x == staff));

            _mapper
                .Received(1)
                .Map(
                    request,
                    Arg.Is<AddressEntity>(x => x == staff.Address));
        }

        [Fact]
        public async Task UpdatePersonalInformationAsync_ShouldUseExistingStaffAddress()
        {
            // Arrange
            var staff = CreateStaff();

            var existingAddress = new AddressEntity
            {
                Staff = staff
            };

            staff.Address = existingAddress;

            var request = CreateRequest();

            // Act
            await _service.UpdatePersonalInformationAsync(
                staff,
                request,
                CancellationToken.None);

            // Assert
            Assert.NotNull(staff.Address);
            Assert.Same(existingAddress, staff.Address);

            _mapper
                .Received(1)
                .Map(
                    request,
                    Arg.Is<BaseUserEntity>(x => x == staff));

            _mapper
                .Received(1)
                .Map(
                    request,
                    Arg.Is<AddressEntity>(x => x == existingAddress));
        }

        private static UpdatePersonalInformationRequest CreateRequest()
        {
            return new UpdatePersonalInformationRequest
            {
                FirstName = "Updated",
                LastName = "User",
                Email = "updated@example.com",
                PhoneNumber = "123456789",
                CountryId = 1,
                Street = "Main Street",
                HouseNumber = "10",
                Apartment = "5",
                City = "Baku",
                State = "Baku",
                ZipCode = "AZ1000"
            };
        }

        private static CustomerEntity CreateCustomer()
        {
            return new CustomerEntity
            {
                Id = Guid.NewGuid(),
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com"
            };
        }

        private static StaffEntity CreateStaff()
        {
            return new StaffEntity
            {
                Id = Guid.NewGuid(),
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com"
            };
        }
    }
}

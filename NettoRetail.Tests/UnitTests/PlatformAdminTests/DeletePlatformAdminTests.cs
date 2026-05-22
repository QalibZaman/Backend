using NettoRetail.Application.CQRS.Users.Commands.Staff;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using System.Data;

namespace NettoRetail.Tests.UnitTests.PlatformAdminTests
{
    public class DeletePlatformAdminTests
    {
        private readonly IStaffRepository _accRepo;
        private readonly IRoleRepository _roleRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDbTransaction _mockTransaction;

        private readonly StaffEntity _currentAdmin;


        public DeletePlatformAdminTests()
        {
            _accRepo = Substitute.For<IStaffRepository>();
            _roleRepo = Substitute.For<IRoleRepository>();
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _mockTransaction = Substitute.For<IDbTransaction>();
            _unitOfWork.BeginTransaction().Returns(_mockTransaction);

            _currentAdmin = CreateTestStaff(Guid.NewGuid(), StaffStatusEnum.Active);
        }

        [Fact]
        public async Task DeletePlatformAdmin_ShouldCallDeleteMethods_WhenAdminIsDeactivated()
        {
            // Arrange
            var inactiveAdmin = CreateTestStaff(Guid.NewGuid(), StaffStatusEnum.Deactivated, "PlatformAdmin");
            var handler = CreateHandler();

            _accRepo.GetAsync(Arg.Any<GetPlatformAdminByIdSpecification>(), Arg.Any<CancellationToken>())
                 .Returns(inactiveAdmin);

            var command = new DeletePlatformAdminCommand(inactiveAdmin.Id, _currentAdmin.Id);

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            await _accRepo.Received(1).DeleteAsync(inactiveAdmin, Arg.Any<CancellationToken>());
            await _roleRepo.Received(1).DeleteAsync(inactiveAdmin.Role, Arg.Any<CancellationToken>());
            _unitOfWork.Received(1).BeginTransaction();
            _mockTransaction.Received(1).Commit();
        }

        [Fact]
        public async Task DeletePlatformAdmin_ShouldCallDeleteMethods_WhenAdminHasPendingStatus()
        {
            // Arrange
            var pendingAdmin = CreateTestStaff(Guid.NewGuid(), StaffStatusEnum.Pending, "PlatformAdmin");
            var handler = CreateHandler();

            _accRepo.GetAsync(Arg.Any<GetPlatformAdminByIdSpecification>(), Arg.Any<CancellationToken>())
                 .Returns(pendingAdmin);

            var command = new DeletePlatformAdminCommand(pendingAdmin.Id, _currentAdmin.Id);

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            await _accRepo.Received(1).DeleteAsync(pendingAdmin, Arg.Any<CancellationToken>());
            await _roleRepo.Received(1).DeleteAsync(pendingAdmin.Role, Arg.Any<CancellationToken>());
            _unitOfWork.Received(1).BeginTransaction();
            _mockTransaction.Received(1).Commit();
        }

        [Fact]
        public async Task DeletePlatformAdmin_ShouldReturnNotFoundException_WhenAdminIsNotFound()
        {
            // Arrange
            var handler = CreateHandler();

            _accRepo.GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>())
                 .ReturnsNull();

            var command = new DeletePlatformAdminCommand(Guid.NewGuid(), _currentAdmin.Id);

            // Act
            var act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await Assert.ThrowsAsync<NotFoundException>(act);
        }

        [Fact]
        public async Task DeletePlatformAdmin_ShouldReturnNotFoundException_WhenAdminIsNotPlatformAdmin()
        {
            // Arrange

            var handler = CreateHandler();

            _accRepo.GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>())
                 .ReturnsNull();

            var command = new DeletePlatformAdminCommand(Guid.NewGuid(), _currentAdmin.Id);

            // Act
            var act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await Assert.ThrowsAsync<NotFoundException>(act);
        }

        [Fact]
        public async Task DeletePlatformAdmin_ShouldReturnBadRequestException_WhenAdminsIdsIsMatches()
        {
            // Arrange
            var handler = CreateHandler();
            var command = new DeletePlatformAdminCommand(_currentAdmin.Id, _currentAdmin.Id);

            // Act
            var act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await Assert.ThrowsAsync<BadRequestException>(act);

            await _accRepo.DidNotReceiveWithAnyArgs().GetAsync(default!, default);
        }

        [Fact]
        public async Task DeletePlatformAdmin_ShouldReturnBadRequestException_WhenAdminHasActiveStatus()
        {
            // Arrange
            var activeAdmin = CreateTestStaff(Guid.NewGuid(), StaffStatusEnum.Active);
            var handler = CreateHandler();

            _accRepo.GetAsync(Arg.Any<GetPlatformAdminByIdSpecification>(), Arg.Any<CancellationToken>())
                 .Returns(activeAdmin);

            var command = new DeletePlatformAdminCommand(activeAdmin.Id, _currentAdmin.Id);

            // Act
            var act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await Assert.ThrowsAsync<BadRequestException>(act);
        }

        [Fact]
        public async Task DeletePlatformAdmin_ShouldReturnBadRequestException_WhenAdminHasOnlineStatus()
        {
            // Arrange
            var onlineAdmin = CreateTestStaff(Guid.NewGuid(), StaffStatusEnum.Online);
            var handler = CreateHandler();

            _accRepo.GetAsync(Arg.Any<GetPlatformAdminByIdSpecification>(), Arg.Any<CancellationToken>())
                 .Returns(onlineAdmin);

            var command = new DeletePlatformAdminCommand(onlineAdmin.Id, _currentAdmin.Id);

            // Act
            var act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await Assert.ThrowsAsync<BadRequestException>(act);
        }

        private DeletePlatformAdminCommandHandler CreateHandler()
        {
            return new DeletePlatformAdminCommandHandler(_accRepo, _roleRepo, _unitOfWork);
        }

        private StaffEntity CreateTestStaff(Guid id, StaffStatusEnum status, string? roleName = null)
        {
            return new StaffEntity
            {
                Id = id,
                FirstName = "Test",
                LastName = "User",
                Email = $"{id}@gmail.com",
                PasswordHash = "hashedpassword",
                Status = status,
                Role = roleName == null ? null! : new RoleEntity
                {
                    Name = roleName,
                    Description = $"{roleName} Description"
                }
            };
        }
    }
}

using NettoRetail.Application.CQRS.Users.Commands.Staff;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace NettoRetail.Tests.UnitTests.PlatformAdminTests
{
    public class ActivateDeactivatePlatformAdminTests
    {
        private readonly IStaffRepository _accRepo;
        private readonly ISessionTerminationService _sessionTerminationService;

        private readonly StaffEntity admin;


        public ActivateDeactivatePlatformAdminTests()
        {
            _accRepo = Substitute.For<IStaffRepository>();
            _sessionTerminationService = Substitute.For<ISessionTerminationService>();

            admin = new StaffEntity
            {
                Id = Guid.NewGuid(),
                FirstName = "Platform",
                LastName = "Admin",
                Email = "platformadmin@gmail.com",
                PasswordHash = "hashedpassword",
                Status = StaffStatusEnum.Active
            };
        }

        [Fact]
        public async Task ActivateAdmin_ShouldUpdateStatusToActive_WhenAdminIsDeactivated()
        {
            // Arrange
            var inactiveAdmin = new StaffEntity
            {
                Id = Guid.NewGuid(),
                FirstName = "Platform",
                LastName = "Admin",
                Email = "platformadmin@gmail.com",
                PasswordHash = "hashedpassword",
                Status = StaffStatusEnum.Deactivated
            };

            var handler = new ActivatePlatformAdminCommandHandler(_accRepo);

            _accRepo.GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>())
                 .Returns(inactiveAdmin);

            var command = new ActivatePlatfromAdminCommand(inactiveAdmin.Id, admin.Id);

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            await _accRepo.Received(1).UpdateAsync(inactiveAdmin, Arg.Any<CancellationToken>());

            Assert.Equal(StaffStatusEnum.Active, inactiveAdmin.Status);
        }

        [Fact]
        public async Task ActivateAdmin_ShouldReturnNotFoundException_WhenAdminIsNotFound()
        {
            // Arrange
            var handler = new ActivatePlatformAdminCommandHandler(_accRepo);

            _accRepo.GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>())
                 .ReturnsNull();

            var command = new ActivatePlatfromAdminCommand(Guid.NewGuid(), admin.Id);

            // Act
            var act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await Assert.ThrowsAsync<NotFoundException>(act);
        }

        [Fact]
        public async Task ActivateAdmin_ShouldReturnNotFoundException_WhenAdminIsNotPlatformAdmin()
        {
            // Arrange
            var notAdmin = new StaffEntity
            {
                Id = Guid.NewGuid(),
                FirstName = "Not",
                LastName = "Admin",
                Email = "notadmin@gmail.com",
                PasswordHash = "hashedpassword",
                Status = StaffStatusEnum.Online,
                Role = new RoleEntity
                {
                    Name = "TestRole",
                    Description = "Test role description",
                }
            };

            var handler = new ActivatePlatformAdminCommandHandler(_accRepo);

            _accRepo.GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>())
                 .ReturnsNull();

            var command = new ActivatePlatfromAdminCommand(notAdmin.Id, admin.Id);

            // Act
            var act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await Assert.ThrowsAsync<NotFoundException>(act);
        }

        [Fact]
        public async Task ActivateAdmin_ShouldReturnBadRequestException_WhenAdminsIdsIsMatches()
        {
            // Arrange
            var handler = new ActivatePlatformAdminCommandHandler(_accRepo);

            _accRepo.GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>())
                 .Returns(admin);

            var command = new ActivatePlatfromAdminCommand(admin.Id, admin.Id);

            // Act
            var act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await Assert.ThrowsAsync<BadRequestException>(act);
        }

        [Fact]
        public async Task ActivateAdmin_ShouldReturnBadRequestException_WhenAdminHasPendingStatus()
        {
            // Arrange
            var pendingAdmin = new StaffEntity
            {
                Id = Guid.NewGuid(),
                FirstName = "Platform",
                LastName = "Admin",
                Email = "platformadmin@gmail.com",
                PasswordHash = "hashedpassword",
                Status = StaffStatusEnum.Pending
            };

            var handler = new ActivatePlatformAdminCommandHandler(_accRepo);

            _accRepo.GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>())
                 .Returns(pendingAdmin);

            var command = new ActivatePlatfromAdminCommand(pendingAdmin.Id, admin.Id);

            // Act
            var act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await Assert.ThrowsAsync<BadRequestException>(act);
        }

        [Fact]
        public async Task ActivateAdmin_ShouldReturnBadRequestException_WhenAdminHasActiveStatus()
        {
            // Arrange
            var activeAdmin = new StaffEntity
            {
                Id = Guid.NewGuid(),
                FirstName = "Platform",
                LastName = "Admin",
                Email = "platformadmin@gmail.com",
                PasswordHash = "hashedpassword",
                Status = StaffStatusEnum.Active
            };

            var handler = new ActivatePlatformAdminCommandHandler(_accRepo);

            _accRepo.GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>())
                 .Returns(activeAdmin);

            var command = new ActivatePlatfromAdminCommand(activeAdmin.Id, admin.Id);

            // Act
            var act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await Assert.ThrowsAsync<BadRequestException>(act);
        }

        [Fact]
        public async Task DeactivateAdmin_ShouldUpdateStatusToDeactivated_WhenAdminIsActive()
        {
            // Arrange
            var activeAdmin = new StaffEntity
            {
                Id = Guid.NewGuid(),
                FirstName = "Platform",
                LastName = "Admin",
                Email = "platformadmin@gmail.com",
                PasswordHash = "hashedpassword",
                Status = StaffStatusEnum.Active
            };

            var handler = new DeactivatePlatformAdminCommandHandler(_accRepo, _sessionTerminationService);

            _accRepo.GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>())
                 .Returns(activeAdmin);

            var command = new DeactivatePlatfromAdminCommand(activeAdmin.Id, admin.Id);

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            await _sessionTerminationService.Received(1).TerminateSessionsAsync(activeAdmin.Id, Arg.Any<CancellationToken>());
            await _accRepo.Received(1).UpdateAsync(activeAdmin, Arg.Any<CancellationToken>());

            Assert.Equal(StaffStatusEnum.Deactivated, activeAdmin.Status);
        }

        [Fact]
        public async Task DeactivateAdmin_ShouldReturnNotFoundException_WhenAdminIsNotFound()
        {
            // Arrange
            var handler = new DeactivatePlatformAdminCommandHandler(_accRepo, _sessionTerminationService);

            _accRepo.GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>())
                 .ReturnsNull();

            var command = new DeactivatePlatfromAdminCommand(Guid.NewGuid(), admin.Id);

            // Act
            var act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await Assert.ThrowsAsync<NotFoundException>(act);
        }

        [Fact]
        public async Task DeactivateAdmin_ShouldReturnNotFoundException_WhenAdminIsNotPlatformAdmin()
        {
            // Arrange
            var notAdmin = new StaffEntity
            {
                Id = Guid.NewGuid(),
                FirstName = "Not",
                LastName = "Admin",
                Email = "notadmin@gmail.com",
                PasswordHash = "hashedpassword",
                Status = StaffStatusEnum.Online,
                Role = new RoleEntity
                {
                    Name = "TestRole",
                    Description = "Test role description",
                }
            };

            var handler = new DeactivatePlatformAdminCommandHandler(_accRepo, _sessionTerminationService);

            _accRepo.GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>())
                 .ReturnsNull();

            var command = new DeactivatePlatfromAdminCommand(notAdmin.Id, admin.Id);

            // Act
            var act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await Assert.ThrowsAsync<NotFoundException>(act);
        }

        [Fact]
        public async Task DeactivateAdmin_ShouldReturnBadRequestException_WhenAdminsIdsIsMatches()
        {
            // Arrange
            var handler = new DeactivatePlatformAdminCommandHandler(_accRepo, _sessionTerminationService);

            _accRepo.GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>())
                 .Returns(admin);

            var command = new DeactivatePlatfromAdminCommand(admin.Id, admin.Id);

            // Act
            var act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await Assert.ThrowsAsync<BadRequestException>(act);
        }

        [Fact]
        public async Task DeactivateAdmin_ShouldReturnBadRequestException_WhenAdminHasPendingStatus()
        {
            // Arrange
            var pendingAdmin = new StaffEntity
            {
                Id = Guid.NewGuid(),
                FirstName = "Platform",
                LastName = "Admin",
                Email = "platformadmin@gmail.com",
                PasswordHash = "hashedpassword",
                Status = StaffStatusEnum.Pending
            };

            var handler = new DeactivatePlatformAdminCommandHandler(_accRepo, _sessionTerminationService);

            _accRepo.GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>())
                 .Returns(pendingAdmin);

            var command = new DeactivatePlatfromAdminCommand(pendingAdmin.Id, admin.Id);

            // Act
            var act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await Assert.ThrowsAsync<BadRequestException>(act);
        }

        [Fact]
        public async Task DeactivateAdmin_ShouldReturnBadRequestException_WhenAdminHasCreatePasswordStaffStatus()
        {
            // Arrange
            var inactiveAdmin = new StaffEntity
            {
                Id = Guid.NewGuid(),
                FirstName = "Platform",
                LastName = "Admin",
                Email = "platformadmin@gmail.com",
                PasswordHash = string.Empty,
                Status = StaffStatusEnum.CreatePassword
            };

            var handler = new DeactivatePlatformAdminCommandHandler(_accRepo, _sessionTerminationService);

            _accRepo.GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>())
                 .Returns(inactiveAdmin);

            var command = new DeactivatePlatfromAdminCommand(inactiveAdmin.Id, admin.Id);

            // Act
            var act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await Assert.ThrowsAsync<BadRequestException>(act);
        }

        [Fact]
        public async Task DeactivateAdmin_ShouldReturnBadRequestException_WhenAdminHasDeactivatedStatus()
        {
            // Arrange
            var inactiveAdmin = new StaffEntity
            {
                Id = Guid.NewGuid(),
                FirstName = "Platform",
                LastName = "Admin",
                Email = "platformadmin@gmail.com",
                PasswordHash = "hashedpassword",
                Status = StaffStatusEnum.Deactivated
            };

            var handler = new DeactivatePlatformAdminCommandHandler(_accRepo, _sessionTerminationService);

            _accRepo.GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>())
                 .Returns(inactiveAdmin);

            var command = new DeactivatePlatfromAdminCommand(inactiveAdmin.Id, admin.Id);

            // Act
            var act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await Assert.ThrowsAsync<BadRequestException>(act);
        }
    }
}

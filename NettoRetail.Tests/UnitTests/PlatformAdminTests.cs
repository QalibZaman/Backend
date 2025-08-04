using NettoRetail.Application.CQRS.Users.Commands.Staff;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.DataAccess.Specifications.PlatformAdmin;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace NettoRetail.Tests.UnitTests
{
    public class PlatformAdminTests
    {
        private readonly IPlatformAdminRepository _adminRepo;
        private readonly ISessionTerminationService _sessionTerminationService;

        public PlatformAdminTests()
        {
            _adminRepo = Substitute.For<IPlatformAdminRepository>();
            _sessionTerminationService = Substitute.For<ISessionTerminationService>();
        }

        [Fact]
        public async Task ActivateAdmin_ShouldUpdateStatusToActive_WhenAdminIsInactive()
        {
            // Arrange
            var admin = new PlatformAdminEntity
            {
                AccountId = Guid.NewGuid(),
                PlatformAdminStatus = PlatformAdminStatusEnum.Active
            };

            var inactiveAdmin = new PlatformAdminEntity
            {
                AccountId = Guid.NewGuid(),
                PlatformAdminStatus = PlatformAdminStatusEnum.Inactive
            };

            var handler = new ActivatePlatformAdminCommandHandler(_adminRepo);

            _adminRepo.GetAsync(Arg.Any<GetPlatformAdminByIdSpecification>(), Arg.Any<CancellationToken>())
                 .Returns(inactiveAdmin);

            var command = new ActivatePlatfromAdminCommand(inactiveAdmin.AccountId, admin.AccountId);

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            await _adminRepo.Received(1).UpdateAsync(inactiveAdmin, Arg.Any<CancellationToken>());

            Assert.Equal(PlatformAdminStatusEnum.Active, inactiveAdmin.PlatformAdminStatus);
        }

        [Fact]
        public async Task ActivateAdmin_ShouldReturnNotFoundException_WhenAdminIsNotFound()
        {
            // Arrange
            var admin = new PlatformAdminEntity
            {
                AccountId = Guid.NewGuid(),
                PlatformAdminStatus = PlatformAdminStatusEnum.Active
            };

            var handler = new ActivatePlatformAdminCommandHandler(_adminRepo);

            _adminRepo.GetAsync(Arg.Any<GetPlatformAdminByIdSpecification>(), Arg.Any<CancellationToken>())
                 .ReturnsNull();

            var command = new ActivatePlatfromAdminCommand(Guid.NewGuid(), admin.AccountId);

            // Act
            var act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await Assert.ThrowsAsync<NotFoundException>(act);
        }

        [Fact]
        public async Task ActivateAdmin_ShouldReturnBadRequestException_WhenAdminsIdsIsMatches()
        {
            // Arrange
            var admin = new PlatformAdminEntity
            {
                AccountId = Guid.NewGuid(),
                PlatformAdminStatus = PlatformAdminStatusEnum.Active
            };

            var handler = new ActivatePlatformAdminCommandHandler(_adminRepo);

            _adminRepo.GetAsync(Arg.Any<GetPlatformAdminByIdSpecification>(), Arg.Any<CancellationToken>())
                 .Returns(admin);

            var command = new ActivatePlatfromAdminCommand(admin.AccountId, admin.AccountId);

            // Act
            var act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await Assert.ThrowsAsync<BadRequestException>(act);
        }

        [Fact]
        public async Task ActivateAdmin_ShouldReturnBadRequestException_WhenAdminHasPendingStatus()
        {
            // Arrange
            var admin = new PlatformAdminEntity
            {
                AccountId = Guid.NewGuid(),
                PlatformAdminStatus = PlatformAdminStatusEnum.Active
            };

            var pendingAdmin = new PlatformAdminEntity
            {
                AccountId = Guid.NewGuid(),
                PlatformAdminStatus = PlatformAdminStatusEnum.Pending
            };

            var handler = new ActivatePlatformAdminCommandHandler(_adminRepo);

            _adminRepo.GetAsync(Arg.Any<GetPlatformAdminByIdSpecification>(), Arg.Any<CancellationToken>())
                 .Returns(pendingAdmin);

            var command = new ActivatePlatfromAdminCommand(pendingAdmin.AccountId, admin.AccountId);

            // Act
            var act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await Assert.ThrowsAsync<BadRequestException>(act);
        }

        [Fact]
        public async Task ActivateAdmin_ShouldReturnBadRequestException_WhenAdminHasActiveStatus()
        {
            // Arrange
            var admin = new PlatformAdminEntity
            {
                AccountId = Guid.NewGuid(),
                PlatformAdminStatus = PlatformAdminStatusEnum.Active
            };

            var activeAdmin = new PlatformAdminEntity
            {
                AccountId = Guid.NewGuid(),
                PlatformAdminStatus = PlatformAdminStatusEnum.Active
            };

            var handler = new ActivatePlatformAdminCommandHandler(_adminRepo);

            _adminRepo.GetAsync(Arg.Any<GetPlatformAdminByIdSpecification>(), Arg.Any<CancellationToken>())
                 .Returns(activeAdmin);

            var command = new ActivatePlatfromAdminCommand(activeAdmin.AccountId, admin.AccountId);

            // Act
            var act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await Assert.ThrowsAsync<BadRequestException>(act);
        }

        [Fact]
        public async Task DeactivateAdmin_ShouldUpdateStatusToInactive_WhenAdminIsActive()
        {
            // Arrange
            var admin = new PlatformAdminEntity
            {
                AccountId = Guid.NewGuid(),
                PlatformAdminStatus = PlatformAdminStatusEnum.Active
            };

            var account = new AccountEntity
            {
                Id = Guid.NewGuid(),
                FirstName = "Plartform",
                LastName = "Admin",
                Email = "platformadmin@gmail.com",
                PasswordHash = "hashedpassword",
                Status = StaffStatusEnum.Online
            };

            var activeAdmin = new PlatformAdminEntity
            {
                AccountId = account.Id,
                Account = account,
                PlatformAdminStatus = PlatformAdminStatusEnum.Active
            };

            var handler = new DeactivatePlatformAdminCommandHandler(_adminRepo, _sessionTerminationService);

            _adminRepo.GetAsync(Arg.Any<GetPlatformAdminByIdSpecification>(), Arg.Any<CancellationToken>())
                 .Returns(activeAdmin);

            var command = new DeactivatePlatfromAdminCommand(activeAdmin.AccountId, admin.AccountId);

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            await _sessionTerminationService.Received(1).TerminateSessionsAsync(activeAdmin.AccountId, Arg.Any<CancellationToken>());
            await _adminRepo.Received(1).UpdateAsync(activeAdmin, Arg.Any<CancellationToken>());

            Assert.Equal(PlatformAdminStatusEnum.Inactive, activeAdmin.PlatformAdminStatus);
        }

        [Fact]
        public async Task DeactivateAdmin_ShouldReturnNotFoundException_WhenAdminIsNotFound()
        {
            // Arrange
            var admin = new PlatformAdminEntity
            {
                AccountId = Guid.NewGuid(),
                PlatformAdminStatus = PlatformAdminStatusEnum.Active
            };

            var handler = new DeactivatePlatformAdminCommandHandler(_adminRepo, _sessionTerminationService);

            _adminRepo.GetAsync(Arg.Any<GetPlatformAdminByIdSpecification>(), Arg.Any<CancellationToken>())
                 .ReturnsNull();

            var command = new DeactivatePlatfromAdminCommand(Guid.NewGuid(), admin.AccountId);

            // Act
            var act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await Assert.ThrowsAsync<NotFoundException>(act);
        }

        [Fact]
        public async Task DeactivateAdmin_ShouldReturnBadRequestException_WhenAdminsIdsIsMatches()
        {
            // Arrange
            var admin = new PlatformAdminEntity
            {
                AccountId = Guid.NewGuid(),
                PlatformAdminStatus = PlatformAdminStatusEnum.Active
            };

            var handler = new DeactivatePlatformAdminCommandHandler(_adminRepo, _sessionTerminationService);

            _adminRepo.GetAsync(Arg.Any<GetPlatformAdminByIdSpecification>(), Arg.Any<CancellationToken>())
                 .Returns(admin);

            var command = new DeactivatePlatfromAdminCommand(admin.AccountId, admin.AccountId);

            // Act
            var act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await Assert.ThrowsAsync<BadRequestException>(act);
        }

        [Fact]
        public async Task DeactivateAdmin_ShouldReturnBadRequestException_WhenAdminHasPendingStatus()
        {
            // Arrange
            var admin = new PlatformAdminEntity
            {
                AccountId = Guid.NewGuid(),
                PlatformAdminStatus = PlatformAdminStatusEnum.Active
            };

            var pendingAdmin = new PlatformAdminEntity
            {
                AccountId = Guid.NewGuid(),
                PlatformAdminStatus = PlatformAdminStatusEnum.Pending
            };

            var handler = new DeactivatePlatformAdminCommandHandler(_adminRepo, _sessionTerminationService);

            _adminRepo.GetAsync(Arg.Any<GetPlatformAdminByIdSpecification>(), Arg.Any<CancellationToken>())
                 .Returns(pendingAdmin);

            var command = new DeactivatePlatfromAdminCommand(pendingAdmin.AccountId, admin.AccountId);

            // Act
            var act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await Assert.ThrowsAsync<BadRequestException>(act);
        }

        [Fact]
        public async Task DeactivateAdmin_ShouldReturnBadRequestException_WhenAdminHasCreatePasswordStaffStatus()
        {
            // Arrange
            var admin = new PlatformAdminEntity
            {
                AccountId = Guid.NewGuid(),
                PlatformAdminStatus = PlatformAdminStatusEnum.Active
            };

            var account = new AccountEntity
            {
                Id = Guid.NewGuid(),
                FirstName = "Plartform",
                LastName = "Admin",
                Email = "platformadmin@gmail.com",
                PasswordHash = string.Empty,
                Status = StaffStatusEnum.CreatePassword
            };

            var inactiveAdmin = new PlatformAdminEntity
            {
                AccountId = account.Id,
                Account = account,
                PlatformAdminStatus = PlatformAdminStatusEnum.Active
            };

            var handler = new DeactivatePlatformAdminCommandHandler(_adminRepo, _sessionTerminationService);

            _adminRepo.GetAsync(Arg.Any<GetPlatformAdminByIdSpecification>(), Arg.Any<CancellationToken>())
                 .Returns(inactiveAdmin);

            var command = new DeactivatePlatfromAdminCommand(inactiveAdmin.AccountId, admin.AccountId);

            // Act
            var act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await Assert.ThrowsAsync<BadRequestException>(act);
        }

        [Fact]
        public async Task DeactivateAdmin_ShouldReturnBadRequestException_WhenAdminHasInactiveStatus()
        {
            // Arrange
            var admin = new PlatformAdminEntity
            {
                AccountId = Guid.NewGuid(),
                PlatformAdminStatus = PlatformAdminStatusEnum.Active
            };

            var account = new AccountEntity
            {
                Id = Guid.NewGuid(),
                FirstName = "Plartform",
                LastName = "Admin",
                Email = "platformadmin@gmail.com",
                PasswordHash = "hashedpassword",
                Status = StaffStatusEnum.Online
            };

            var inactiveAdmin = new PlatformAdminEntity
            {
                AccountId = account.Id,
                Account = account,
                PlatformAdminStatus = PlatformAdminStatusEnum.Inactive
            };

            var handler = new DeactivatePlatformAdminCommandHandler(_adminRepo, _sessionTerminationService);

            _adminRepo.GetAsync(Arg.Any<GetPlatformAdminByIdSpecification>(), Arg.Any<CancellationToken>())
                 .Returns(inactiveAdmin);

            var command = new DeactivatePlatfromAdminCommand(inactiveAdmin.AccountId, admin.AccountId);

            // Act
            var act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await Assert.ThrowsAsync<BadRequestException>(act);
        }
    }
}

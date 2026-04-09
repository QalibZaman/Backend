using NettoRetail.Application.CQRS.Users.Commands.Staff;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace NettoRetail.Tests.UnitTests.PlatformAdminTests
{
    public class DeletePlatformAdminTests
    {
        private readonly IStaffRepository _accRepo;
        private readonly IRoleRepository _roleRepo;
        private readonly IUnitOfWork _unitOfWork;

        private readonly StaffEntity admin;


        public DeletePlatformAdminTests()
        {
            _accRepo = Substitute.For<IStaffRepository>();
            _roleRepo = Substitute.For<IRoleRepository>();
            _unitOfWork = Substitute.For<IUnitOfWork>();

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
        public async Task DeletePlatformAdmin_ShouldCallDeleteMethods_WhenAdminIsDeactivated()
        {
            // Arrange
            var inactiveAdmin = new StaffEntity
            {
                Id = Guid.NewGuid(),
                FirstName = "Platform",
                LastName = "Admin",
                Email = "platformadmin@gmail.com",
                PasswordHash = "hashedpassword",
                Status = StaffStatusEnum.Deactivated,
                Role = new RoleEntity
                {
                    Name = "PlatformAdmin",
                    Description = "Role description",
                }
            };

            var handler = new DeletePlatformAdminCommandHandler(_accRepo, _roleRepo, _unitOfWork);

            _accRepo.GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>())
                 .Returns(inactiveAdmin);

            var command = new DeletePlatformAdminCommand(inactiveAdmin.Id, admin.Id);

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            await _accRepo.Received(1).DeleteAsync(inactiveAdmin, Arg.Any<CancellationToken>());
            await _roleRepo.Received(1).DeleteAsync(inactiveAdmin.Role, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task DeletePlatformAdmin_ShouldCallDeleteMethods_WhenAdminHasPendingStatus()
        {
            // Arrange
            var inactiveAdmin = new StaffEntity
            {
                Id = Guid.NewGuid(),
                FirstName = "Platform",
                LastName = "Admin",
                Email = "platformadmin@gmail.com",
                PasswordHash = "hashedpassword",
                Status = StaffStatusEnum.Pending,
                Role = new RoleEntity
                {
                    Name = "PlatformAdmin",
                    Description = "Role description",
                }
            };

            var handler = new DeletePlatformAdminCommandHandler(_accRepo, _roleRepo, _unitOfWork);

            _accRepo.GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>())
                 .Returns(inactiveAdmin);

            var command = new DeletePlatformAdminCommand(inactiveAdmin.Id, admin.Id);

            // Act
            await handler.Handle(command, CancellationToken.None);

            // Assert
            await _accRepo.Received(1).DeleteAsync(inactiveAdmin, Arg.Any<CancellationToken>());
            await _roleRepo.Received(1).DeleteAsync(inactiveAdmin.Role, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task DeletePlatformAdmin_ShouldReturnNotFoundException_WhenAdminIsNotFound()
        {
            // Arrange
            var handler = new DeletePlatformAdminCommandHandler(_accRepo, _roleRepo, _unitOfWork);

            _accRepo.GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>())
                 .ReturnsNull();

            var command = new DeletePlatformAdminCommand(Guid.NewGuid(), admin.Id);

            // Act
            var act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await Assert.ThrowsAsync<NotFoundException>(act);
        }

        [Fact]
        public async Task DeletePlatformAdmin_ShouldReturnNotFoundException_WhenAdminIsNotPlatformAdmin()
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

            var handler = new DeletePlatformAdminCommandHandler(_accRepo, _roleRepo, _unitOfWork);

            _accRepo.GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>())
                 .ReturnsNull();

            var command = new DeletePlatformAdminCommand(notAdmin.Id, admin.Id);

            // Act
            var act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await Assert.ThrowsAsync<NotFoundException>(act);
        }

        [Fact]
        public async Task DeletePlatformAdmin_ShouldReturnBadRequestException_WhenAdminsIdsIsMatches()
        {
            // Arrange
            var handler = new DeletePlatformAdminCommandHandler(_accRepo, _roleRepo, _unitOfWork);

            _accRepo.GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>())
                 .Returns(admin);

            var command = new DeletePlatformAdminCommand(admin.Id, admin.Id);

            // Act
            var act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await Assert.ThrowsAsync<BadRequestException>(act);
        }

        [Fact]
        public async Task DeletePlatformAdmin_ShouldReturnBadRequestException_WhenAdminHasActiveStatus()
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

            var handler = new DeletePlatformAdminCommandHandler(_accRepo, _roleRepo, _unitOfWork);

            _accRepo.GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>())
                 .Returns(activeAdmin);

            var command = new DeletePlatformAdminCommand(activeAdmin.Id, admin.Id);

            // Act
            var act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await Assert.ThrowsAsync<BadRequestException>(act);
        }

        [Fact]
        public async Task DeletePlatformAdmin_ShouldReturnBadRequestException_WhenAdminHasOnlineStatus()
        {
            // Arrange
            var onlineAdmin = new StaffEntity
            {
                Id = Guid.NewGuid(),
                FirstName = "Platform",
                LastName = "Admin",
                Email = "platformadmin@gmail.com",
                PasswordHash = "hashedpassword",
                Status = StaffStatusEnum.Online
            };

            var handler = new DeletePlatformAdminCommandHandler(_accRepo, _roleRepo, _unitOfWork);

            _accRepo.GetAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>())
                 .Returns(onlineAdmin);

            var command = new DeletePlatformAdminCommand(onlineAdmin.Id, admin.Id);

            // Act
            var act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await Assert.ThrowsAsync<BadRequestException>(act);
        }
    }
}

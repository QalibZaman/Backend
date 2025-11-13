using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using NettoRetail.Application.CQRS.Users.Commands.Staff;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.DataAccess.Specifications.FavouriteList;
using NettoRetail.DataAccess.Specifications.Inventory;
using NettoRetail.Domain.Entities;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;
using NSubstitute;
using Xunit.Sdk;

namespace NettoRetail.Tests.UnitTests.UsersTests.StuffTests;

public class UploadStuffListCommandHandlerTests
{
    private readonly IAccountRepository _accountRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IEmailSender _emailSender;
    private readonly IEmailBuilderService _emailBuilderService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IShopRepository _shopRepository;

    public UploadStuffListCommandHandlerTests()
    {
        _accountRepository = Substitute.For<IAccountRepository>();
        _roleRepository = Substitute.For<IRoleRepository>();
        _emailSender = Substitute.For<IEmailSender>();
        _emailBuilderService = Substitute.For<IEmailBuilderService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _shopRepository = Substitute.For<IShopRepository>();
    }

    [Fact]
    public async Task Handle_ShouldThrownUploadStuffError_WhenFileIsNull()
    {
        //Arrange
        var handler = new UploadStaffListCommandHandler(_accountRepository, _roleRepository, _emailSender,
            _emailBuilderService, _unitOfWork, _shopRepository);
        var command = new UploadStaffListCommand(File: null, ShopId: 1);
        //Act 
        var result = await handler.Handle(command, CancellationToken.None);

        //Assert
        Assert.False(result.Success);
        Assert.Single(result.Errors);
        
    }
}

using FluentValidation;
using FluentValidation.TestHelper;
using NettoRetail.API.Infrastructure.Validators;
using NettoRetail.Application.CQRS.Payments.Queries;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.PaymentTests.GetPaymentHistory;

public class GetPaymentHistoryQueryValidatorTests
{
    private readonly IStaffRepository _staffRepository;
    private readonly InlineValidator<PaginationRequest> _paginationRequestValidator;
    private readonly GetPaymentHistoryQueryValidator _validator;
    public GetPaymentHistoryQueryValidatorTests()
    {
        _staffRepository = Substitute.For<IStaffRepository>();
        _paginationRequestValidator = new InlineValidator<PaginationRequest>();
        _validator = new GetPaymentHistoryQueryValidator(_staffRepository, _paginationRequestValidator);
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenAccountNotFound()
    {
        // Arrange
        var paginationRequest = new PaginationRequest { PageNumber = 1, PageSize = 10 };
        var query = new GetPaymentHistoryQuery(paginationRequest, Guid.NewGuid(), true);

        _staffRepository.ExistsAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>()).Returns(false);
        //Assert 
        await Assert.ThrowsAsync<NotFoundException>(() => _validator.TestValidateAsync(query));
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenDataIsValid()
    {
        //Arrange
        var paginationRequest = new PaginationRequest { PageNumber = 1, PageSize = 10 };
        var query = new GetPaymentHistoryQuery(paginationRequest, Guid.NewGuid(), true);

        _staffRepository.ExistsAsync(Arg.Any<GetStaffByIdSpecification>(), Arg.Any<CancellationToken>()).Returns(true);

        //Act
        var result = await _validator.TestValidateAsync(query);

        //Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}

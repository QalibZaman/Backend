using FluentAssertions;
using MapsterMapper;
using NettoRetail.Application.CQRS.Payments.Queries;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Payment;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.PaymentTests.GetPaymentHistory;

public class GetPaymentHistoryQueryHandlerTests
{
    private readonly IPaymentHistoryRepository _paymentHistoryRepository;
    private readonly IMapper _mapper;
    private readonly GetPaymentHistoryQueryHandler _handler;

    public GetPaymentHistoryQueryHandlerTests()
    {
        _paymentHistoryRepository = Substitute.For<IPaymentHistoryRepository>();
        _mapper = Substitute.For<IMapper>();
        _handler = new GetPaymentHistoryQueryHandler(_paymentHistoryRepository, _mapper);
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginatedResponse_WhenCalledWithValidRequest()
    {
        //Arrange
        var accountId = Guid.NewGuid();
        var pageModel = new PaginationRequest { PageNumber = 1, PageSize = 10 };
        var query = new GetPaymentHistoryQuery(pageModel, accountId, true);

        var paymentEntities = new List<PaymentHistoryEntity>
        {
            new PaymentHistoryEntity { Id = Guid.NewGuid(), AmountCharged = 100.00m },
            new PaymentHistoryEntity { Id = Guid.NewGuid(), AmountCharged = 50.00m }
        };

        var totalCount = 2;

        _paymentHistoryRepository.GetAllPaginatedAsync(Arg.Any<GetPaymentHistoriesByUserIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns((paymentEntities, totalCount));

        var expectedResponses = new List<PaymentHistoryResponse>
        {
            new() {Id = paymentEntities[0].Id, AmountCharged = paymentEntities[0].AmountCharged },
            new() {Id = paymentEntities[1].Id, AmountCharged = paymentEntities[1].AmountCharged }
        };

        _mapper.Map<ICollection<PaymentHistoryResponse>>(paymentEntities).Returns(expectedResponses);

        //Act
        var result = await _handler.Handle(query, CancellationToken.None);

        //Assert
        result.Should().NotBeNull();
        result.TotalAmount.Should().Be(totalCount);
        result.Items.Should().HaveCount(totalCount);
        result.Items.First().AmountCharged.Should().Be(100.00m);

        await _paymentHistoryRepository
            .Received(1)
            .GetAllPaginatedAsync(Arg.Any<GetPaymentHistoriesByUserIdSpecification>(), CancellationToken.None);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoPaymentExist()
    {
        //Arrange
        var query = new GetPaymentHistoryQuery(new PaginationRequest { PageNumber = 1, PageSize = 10 }, Guid.NewGuid(), true);

        _paymentHistoryRepository.GetAllPaginatedAsync(Arg.Any<GetPaymentHistoriesByUserIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns((new List<PaymentHistoryEntity>(), 0));

        _mapper.Map<ICollection<PaymentHistoryResponse>>(Arg.Any<List<PaymentHistoryEntity>>())
            .Returns(new List<PaymentHistoryResponse>());

        //Act
        var result = await _handler.Handle(query, CancellationToken.None);

        //Assert
        result.Items.Should().BeEmpty();
        result.TotalAmount.Should().Be(0);
    }
}
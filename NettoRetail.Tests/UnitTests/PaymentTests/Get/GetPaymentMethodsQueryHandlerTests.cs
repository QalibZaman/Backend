using MapsterMapper;
using NettoRetail.Application.CQRS.Payments.Queries;
using NettoRetail.Application.Models.Payments;
using NettoRetail.DataAccess.Specifications.Payment;
using NettoRetail.Domain.Entities;
using NettoRetail.Tests.Helpers;
using NettoRetail.Shared.Interfaces.Repositories;
using NSubstitute;

namespace NettoRetail.Tests.UnitTests.PaymentTests.Get;

public class GetPaymentMethodsQueryHandlerTests
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IMapper _mapper;

    public GetPaymentMethodsQueryHandlerTests()
    {
        _paymentRepository = Substitute.For<IPaymentRepository>();
        _mapper = MapperFactory.Create();
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_List_When_No_Payment_Methods_Found()
    {
        var query = new GetPaymentMethodsQuery(Guid.NewGuid(), 1);
        var handler = new GetPaymentMethodsQueryHandler(_paymentRepository, _mapper);

        _paymentRepository
            .GetAllBySpecAsync(Arg.Any<GetPaymentMethodsByAccountIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<PaymentMethodEntity>());

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_Should_Return_Mapped_Payment_Methods_When_They_Exist()
    {
        var query = new GetPaymentMethodsQuery(Guid.NewGuid(), 1);
        var paymentMethods = new[]
        {
            new PaymentMethodEntity
            {
                Id = 1,
                AccountId = query.UserId,
                StripePaymentMethodId = "pm_1",
                FourLastCardNumbers = "4242",
                CardBrand = "Visa",
                CardExpMonth = 12,
                CardExpYear = 2030,
                IsDefault = true,
            },
            new PaymentMethodEntity
            {
                Id = 2,
                AccountId = query.UserId,
                StripePaymentMethodId = "pm_2",
                FourLastCardNumbers = "1111",
                CardBrand = "Mastercard",
                CardExpMonth = 8,
                CardExpYear = 2032,
                IsDefault = false,
            },
        };

        var handler = new GetPaymentMethodsQueryHandler(_paymentRepository, _mapper);

        _paymentRepository
            .GetAllBySpecAsync(Arg.Any<GetPaymentMethodsByAccountIdSpecification>(), Arg.Any<CancellationToken>())
            .Returns(paymentMethods);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Collection(
            result,
            first =>
            {
                Assert.Equal(paymentMethods[0].Id, first.Id);
                Assert.Equal(paymentMethods[0].StripePaymentMethodId, first.StripePaymentMethodId);
                Assert.Equal(paymentMethods[0].IsDefault, first.IsDefault);
            },
            second =>
            {
                Assert.Equal(paymentMethods[1].Id, second.Id);
                Assert.Equal(paymentMethods[1].StripePaymentMethodId, second.StripePaymentMethodId);
                Assert.Equal(paymentMethods[1].IsDefault, second.IsDefault);
            });
    }
}

using MapsterMapper;
using MediatR;
using NettoRetail.Application.Models.Payments;
using NettoRetail.DataAccess.Specifications.Payment;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Payments.Queries;

public record GetPaymentMethodsQuery(Guid UserId, int ShopId)
    : IRequest<List<PaymentMethodsResponse>>;

public class GetPaymentMethodsQueryHandler(
    IPaymentRepository paymentRepository,
    IMapper mapper)
    : IRequestHandler<GetPaymentMethodsQuery, List<PaymentMethodsResponse>>
{
    public async Task<List<PaymentMethodsResponse>> Handle(GetPaymentMethodsQuery request, CancellationToken cancellationToken)
    {
        var targetPaymentMethods =
            await paymentRepository.GetAllBySpecAsync(
                new GetPaymentMethodsByAccountIdSpecification(request.UserId),
                cancellationToken);

        if (!targetPaymentMethods.Any())
        {
            return new List<PaymentMethodsResponse>();
        }

        return mapper.Map<List<PaymentMethodsResponse>>(targetPaymentMethods);
    }
}

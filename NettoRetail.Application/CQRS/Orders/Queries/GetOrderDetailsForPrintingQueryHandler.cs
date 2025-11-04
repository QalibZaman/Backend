using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Services.IOrderService;
using NettoRetail.Application.Services.IPdfGenerator;
using NettoRetail.DataAccess.Specifications.Order;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Orders.Queries;

public record GetOrderDetailsForPrintingQuery(int OrderId) : IRequest<OrderPrintingResponse>;

public class GetOrderDetailsForPrintingQueryHandler(
    IOrderRepository orderRepository,
    IOrderService orderService,
    IPdfGenerator pdfGenerator)
    : IRequestHandler<GetOrderDetailsForPrintingQuery, OrderPrintingResponse>
{
    public async Task<OrderPrintingResponse> Handle(GetOrderDetailsForPrintingQuery request, CancellationToken cancellationToken)
    {
        var spec = new SpecificationBuilder<OrderEntity>()
            .Add<GetOrderByIdSpecification>(request.OrderId)
            .AddInclude(x => x.Customer)
            .AddInclude(x => x.Customer.Account)
            .AddInclude(x => x.StatusHistory)
            .AddInclude(x => x.OrderItems)
            .Build();

        var order = await orderRepository.GetOrderForPrinting(spec, cancellationToken);
        if (order is null)
            throw new NotFoundException("Order can't be found");

        var model = await orderService.BuildPrintOrderDetailsModel(order);

        var pdfStream = await pdfGenerator.PrintOrderDetails(model);

        return new OrderPrintingResponse
        {
            Stream = pdfStream,
            FileName = $"order_{order.Id}.pdf"
        };
    }
}
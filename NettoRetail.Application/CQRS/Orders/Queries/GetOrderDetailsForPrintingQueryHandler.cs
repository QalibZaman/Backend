using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Order;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Services.IPdfGenerator;
using NettoRetail.DataAccess.Specifications.Order;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Orders.Queries;

public record GetOrderDetailsForPrintingQuery(int OrderId) : IRequest<OrderPrintingResponse>;

public class GetOrderDetailsForPrintingQueryHandler(
    IOrderRepository orderRepository,
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

        var model = order.Adapt<PrintOrderDetailsModel>();

        model.TotalProducts = order.OrderItems.Count();
        model.Subtotal = order.OrderItems.Sum(x => x.Quantity * x.InventoryItem.Price);
        model.TotalSale = order.OrderItems.Sum(x =>
            x.InventoryItem.PromotionPrice != null
                ? (x.InventoryItem.Price - x.InventoryItem.PromotionPrice.Value) * x.Quantity
                : 0
        );
        model.TotalAmount = model.Subtotal - model.TotalSale;

        Stream pdf = await pdfGenerator.PrintOrderDetails(model);

        return new OrderPrintingResponse
        {
            Stream = pdf,
            FileName = $"order_{order.Id}.pdf"
        };
    }
}
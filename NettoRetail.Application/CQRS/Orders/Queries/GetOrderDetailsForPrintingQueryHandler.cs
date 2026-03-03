using MediatR;
using Microsoft.Extensions.Logging;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Services.IOrderService;
using NettoRetail.Application.Services.IPdfGenerator;
using NettoRetail.DataAccess.Specifications.Order;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;
using Microsoft.EntityFrameworkCore;


namespace NettoRetail.Application.CQRS.Orders.Queries;

public record GetOrderDetailsForPrintingQuery(int OrderId) : IRequest<OrderPrintingResponse>;

public class GetOrderDetailsForPrintingQueryHandler(
    IOrderRepository orderRepository,
    IOrderService orderService,
    IPdfGenerator pdfGenerator,
    ILogger<GetOrderDetailsForPrintingQueryHandler> logger)
    : IRequestHandler<GetOrderDetailsForPrintingQuery, OrderPrintingResponse>
{
    public async Task<OrderPrintingResponse> Handle(GetOrderDetailsForPrintingQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching order {OrderId} for printing", request.OrderId);

        var spec = new SpecificationBuilder<OrderEntity>()
            .Add<GetOrderByIdSpecification>(request.OrderId)
            .AddInclude(q => q
                .Include(o => o.Customer))
            .AddInclude(q => q
                .Include(o => o.StatusHistory))
            .AddInclude(q => q
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.InventoryItem))
            .Build();

        var order = await orderRepository.GetOrderForPrinting(spec, cancellationToken);

        if (order is null)
        {
            logger.LogWarning("Order {OrderId} not found for printing", request.OrderId);
            throw new NotFoundException("Order can't be found");
        }

        logger.LogInformation("Building print model for order {OrderId}", request.OrderId);
        var model = await orderService.BuildPrintOrderDetailsModelAsync(order);

        try
        {
            logger.LogInformation("Generating PDF for order {OrderId}", request.OrderId);
            var pdfStream = await pdfGenerator.PrintOrderDetailsAsync(model);

            logger.LogInformation("Successfully generated PDF for order {OrderId}", request.OrderId);

            return new OrderPrintingResponse
            {
                Stream = pdfStream,
                FileName = $"order_{order.Id}.pdf"
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to generate PDF for order {OrderId}", request.OrderId);
            throw;
        }
    }
}
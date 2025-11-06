using NettoRetail.Application.Models.Order;
using NettoRetail.Domain.Entities;

namespace NettoRetail.Application.Services.IOrderService;

public interface IOrderService
{
    Task<PrintOrderDetailsModel> BuildPrintOrderDetailsModelAsync(OrderEntity order);
}
using NettoRetail.Application.Models.Order;
using NettoRetail.Application.Models.Request;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.Services.IOrderService;

public interface IFilteredOrderService
{
    Specification<OrderEntity> BuildFilteredOrdersSpecification(GetFilteredOrdersModel model, int shopId, string? searchTerm, PaginationRequest pagination);
}

using NettoRetail.Domain.Enums;

namespace NettoRetail.Application.Models.Customer;

public record CustomerOrderHistoryResponse(
    long OrderId,
    DateTime DateCreated,
    OrderStatusEnum OrderStatus,
    OrderDeliveryTypeEnum DeliveryType,
    string Creator
    );
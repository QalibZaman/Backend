using AngleSharp.Css;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Customer;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Brands;
using NettoRetail.DataAccess.Specifications.Customer;
using NettoRetail.DataAccess.Specifications.Order;
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.Application.CQRS.Customers.Queries;


public record GetOrdersForTheCustomeQuery(int shopId, Guid customerId, PaginationRequest PaginationRequest) : IRequest<PaginatedResponse<CustomerOrderHistoryResponse>>;

public class GetOrdersForTheCustomeQueryHandler(
    ICustomerRepository customerRepository,
    IOrderRepository orderRepository):IRequestHandler<GetOrdersForTheCustomeQuery,PaginatedResponse<CustomerOrderHistoryResponse>>
{
    public async Task<PaginatedResponse<CustomerOrderHistoryResponse>> Handle(GetOrdersForTheCustomeQuery request, CancellationToken cancellationToken)
    {
        var customer = await customerRepository.GetAsync(new GetCustomerByIdSpecification(request.customerId), cancellationToken)
                       ?? throw new NotFoundException("There is no such customer");
        var specificationBuilder = new SpecificationBuilder<OrderEntity>();
        var skip = (request.PaginationRequest.PageNumber - 1) * request.PaginationRequest.PageSize;

        specificationBuilder.Add<GetOrderByCustomerIdAndShopIdSpecification>(request.customerId,request.shopId);

        var specification = specificationBuilder.Skip(skip).Take(request.PaginationRequest.PageSize).Build();
        var (items, totalCount) = await orderRepository.GetAllPaginatedAsync(
                new GetOrderByCustomerIdAndShopIdSpecification(request.customerId, request.shopId),
                cancellationToken);
        var result = items.Select(o=>new CustomerOrderHistoryResponse(
            OrderId:o.Id,
            DateCreated:o.CreationDate,
            OrderStatus:o.OrderStatus,
            DeliveryType:o.DeliveryType,
            Creator:o.Creator
        )).ToList();
        
        return new PaginatedResponse<CustomerOrderHistoryResponse>
        {
            Items = result,
            TotalAmount = totalCount
        };

    }
}
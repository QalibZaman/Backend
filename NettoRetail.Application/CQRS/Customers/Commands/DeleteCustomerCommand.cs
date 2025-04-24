using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Customer;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Customers.Commands
{
    public record DeleteCustomerCommand(Guid id) : IRequest;

    public class DeleteCustomerCommandHandler(ICustomerRepository customerRepository) : IRequestHandler<DeleteCustomerCommand>
    {
        public async Task Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
        {
            var specification = new GetCustomerByIdSpecification(request.id);
            specification.AddInclude(model => model.Order);

            var customer = await customerRepository.GetAsync(specification, cancellationToken) ??
                throw new NotFoundException("Customer not found");
            if (customer.Order.Any(order =>
                                    order.OrderStatus != OrderStatusEnum.Done
                                    && order.OrderStatus != OrderStatusEnum.Cancelled))
            {
                throw new BadRequestException("Cannot delete, customer has active orders");
            }
            customer.IsActive = false;
            await customerRepository.UpdateAsync(customer, cancellationToken);
        }
    }
}


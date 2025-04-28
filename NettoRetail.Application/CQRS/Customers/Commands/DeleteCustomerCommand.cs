using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.DataAccess.Specifications.Customer;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.Customers.Commands
{
    public record DeleteCustomerCommand(Guid Id, int ShopId) : IRequest;

    public class DeleteCustomerCommandHandler(ICustomerRepository customerRepository,
    ISessionTerminationService sessionTerminationService,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteCustomerCommand>
    {
        public async Task Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
        {
            var specification = new GetCustomerByIdSpecification(request.Id);
            specification.AddInclude(x => x.Order);
            specification.AddInclude(x => x.Account);

            var customer = await customerRepository.GetAsync(specification, cancellationToken) ??
                throw new NotFoundException("Customer not found");

            if (customer.Account?.ShopId != request.ShopId)
                throw new ForbiddenException("Cannot delete customer from another shop");

            if (customer.Order.Any(order =>
                                    order.OrderStatus != OrderStatusEnum.Done
                                    && order.OrderStatus != OrderStatusEnum.Cancelled))
            {
                throw new BadRequestException("Cannot delete, customer has active orders");
            }

            var transaction = unitOfWork.BeginTransaction();
            try
            {
                customer.IsDeleted = true;
                await sessionTerminationService.TerminateSessionsAsync(customer.AccountId, cancellationToken);
                await customerRepository.UpdateAsync(customer, cancellationToken);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
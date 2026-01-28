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

            var customer = await customerRepository.GetAsync(specification, cancellationToken)
                ?? throw new NotFoundException("Customer not found");

            var account = customer.Account ?? throw new NotFoundException("Customer account not found");

            if (account.ShopId != request.ShopId)
                throw new ForbiddenException("Cannot delete customer from another shop");

            var hasActiveOrders = customer.Order?.Any(o =>
                o.OrderStatus != OrderStatusEnum.Done &&
                o.OrderStatus != OrderStatusEnum.Cancelled) == true;

            if (hasActiveOrders)
                throw new BadRequestException("Cannot delete, customer has active orders");

            using var transaction = unitOfWork.BeginTransaction();

            await sessionTerminationService.TerminateSessionsAsync(customer.AccountId, cancellationToken);
            await customerRepository.DeleteAsync(customer, cancellationToken);

            transaction.Commit();
        }
    }
}
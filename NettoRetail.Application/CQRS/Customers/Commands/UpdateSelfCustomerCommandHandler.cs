using System.Text.RegularExpressions;
using MapsterMapper;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Request.Customer;
using NettoRetail.Application.Services.ILinkService;
using NettoRetail.DataAccess.Specifications.Country;
using NettoRetail.DataAccess.Specifications.Customer;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Customers.Commands;

public record UpdateSelfCustomerCommand(Guid CustomerId, UpdateSelfCustomerRequest UpdateSelfCustomerRequest) : IRequest<Unit>;

public class UpdateSelfCustomerCommandHandler(
    ICountryRepository countryRepository,
    ICustomerRepository customerRepository,
    IEmailSender emailSender,
    IMapper mapper,
    ILinkService linkService,
    IEmailBuilderService emailBuilderService) : IRequestHandler<UpdateSelfCustomerCommand, Unit>
{
    public async Task<Unit> Handle(UpdateSelfCustomerCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetCustomerByIdSpecification(request.CustomerId);
        var customer = await customerRepository.GetAsync(specification, cancellationToken)
            ?? throw new NotFoundException("Customer not found");

        if (customer.CountryId == null)
            throw new BadRequestException("Customer does not have a country assigned.");

        var countrySpecification = new GetCountryByIdSpecification(customer.CountryId.Value);
        var country = await countryRepository.GetAsync(countrySpecification, cancellationToken)
            ?? throw new NotFoundException("Country not found");

        if (!Regex.IsMatch(request.UpdateSelfCustomerRequest.PhoneNumber, country.PhoneMask))
            throw new BadRequestException("Phone number does not match the required format for the customer's country.");

        bool isEmailChanging = customer.Email != request.UpdateSelfCustomerRequest.Email;
        if (isEmailChanging)
        {
            if (customer.ShopId == null)
                throw new BadRequestException("Customer does not belong to a shop.");

            var link = linkService.CreateCustomerEmailChangeLink(request.UpdateSelfCustomerRequest.Email, customer.ShopId.Value);
            var email = await emailBuilderService.BuildCustomerEmailChangeConfirmationEmailAsync(request.UpdateSelfCustomerRequest.Email, customer.FirstName, link, customer.Shop!.Name);
            await emailSender.SendAsync(email, request.CustomerId);
        }

        var oldEmail = customer.Email;
        var customerRequest = mapper.Map(request.UpdateSelfCustomerRequest, customer);
        if (isEmailChanging)
        {
            customer.Email = oldEmail;
        }

        await customerRepository.UpdateAsync(customerRequest, cancellationToken);
        return Unit.Value;
    }
}

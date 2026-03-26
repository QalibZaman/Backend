using MapsterMapper;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Country;
using NettoRetail.Application.Models.Request.Customer;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Services.ICountryValidationService;
using NettoRetail.Application.Services.ILinkService;
using NettoRetail.Application.Utilities;
using NettoRetail.DataAccess.Specifications.Country;
using NettoRetail.DataAccess.Specifications.Customer;
using NettoRetail.Domain.Entities;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.Application.CQRS.Customers.Commands;

public record SignUpCustomerCommand(int ShopId, SignUpCustomerRequest Model) : IRequest<SignUpCustomerResponse>;

public class SignUpCustomerCommandHandler(
    ICountryDetailsValidationService countryValidator,
    ICustomerRepository customerRepository,
    ICountryRepository countryRepository,
    ILinkService linkService,
    IEmailBuilderService emailBuilderService,
    IEmailSender emailSender,
    IMapper mapper)
    : IRequestHandler<SignUpCustomerCommand, SignUpCustomerResponse>
{
    public async Task<SignUpCustomerResponse> Handle(SignUpCustomerCommand request, CancellationToken cancellationToken)
    {
        var emailSpecification = new GetCustomerByEmailAndShopIdSpecification(request.Model.Email, request.ShopId);

        if (await customerRepository.Exists(emailSpecification, cancellationToken))
           throw new ConflictException("This email address is already signed up. Please Sign in");

        if (!string.IsNullOrEmpty(request.Model.PhoneNumber))
        {
            var countryValidationModel = new CountryValidationModel
            {
                CountryId = request.Model.CountryId,
                PhoneNumber = request.Model.PhoneNumber
            };

            var options = new CountryValidationOptions
            {
                CheckForPhoneNumber = true,
                CheckForZipCode = false
            };

            await countryValidator.ValidateAsync(countryValidationModel, cancellationToken, options);
        }

        var customer = mapper.Map<CustomerEntity>(request.Model);

        customer.RoleId = RoleConstants.Customer;
        customer.ShopId = request.ShopId;
        customer.IsActive = false;
        customer.LoyaltyId = UserUtility.GenerateLoyaltyId();

        var initialPassword = request.Model.Password;
        customer.PasswordHash = PasswordHasher.HashPassword(initialPassword);

        var countrySpecification = new GetCountryByIdSpecification(request.Model.CountryId);
        var country = await countryRepository.GetAsync(countrySpecification, cancellationToken);
        if (request.Model.PhoneNumber != null && country != null)
        {
            customer.PhoneNumber = $"{country.PhoneCode} {request.Model.PhoneNumber}";
        }

        await customerRepository.AddAsync(customer, cancellationToken);

        var link = linkService.CreateCustomerAccountVerificationLink(request.Model.Email, request.ShopId);

        var emailModel = await emailBuilderService.BuildCustomerAccountActivationEmailAsync(customer.Email, customer.FirstName, link);
        await emailSender.SendAsync(emailModel, customer.Id);

        return new SignUpCustomerResponse
        {
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email,
            IsActive = customer.IsActive,
            ActivationLink = link
        };
    }
}

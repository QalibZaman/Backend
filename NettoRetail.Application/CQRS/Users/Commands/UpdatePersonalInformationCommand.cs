using MapsterMapper;
using MediatR;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Country;
using NettoRetail.Application.Services.ICountryValidationService;
using NettoRetail.DataAccess.Specifications.Account;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Users.Commands;

public record UpdatePersonalInformationCommand(Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string ShippingAddress,
    string City,
    string? State,
    string ZipCode,
    int CountryId) : IRequest;

public class UpdatePersonalInformationCommandHandler(
    IStaffRepository accountRepository,
    ICountryDetailsValidationService countryDetailsValidator,
    IMapper mapper) : IRequestHandler<UpdatePersonalInformationCommand>
{
    public async Task Handle(UpdatePersonalInformationCommand request, CancellationToken cancellationToken)
    {
        var specification = new GetStaffByIdSpecification(request.Id);
        var account = await accountRepository.GetAsync(specification, cancellationToken) ?? throw new NotFoundException("No user with such id");

        if (request.Email != account.Email)
        {
            var emailSpecification = new GetStaffByEmailSpecification(request.Email);
            var existedAccount = await accountRepository.GetAsync(emailSpecification, cancellationToken);

            if(existedAccount != null)
            {
                throw new ConflictException("User with such email is already exists");
            }
        }

        var validationModel = mapper.Map<CountryValidationModel>(request);
        await countryDetailsValidator.ValidateAsync(validationModel, cancellationToken);

        mapper.Map(request, account);

        await accountRepository.UpdateAsync(account, cancellationToken);
    }
}

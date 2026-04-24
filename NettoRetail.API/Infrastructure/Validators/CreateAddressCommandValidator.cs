using System.Text.RegularExpressions;
using FluentValidation;
using NettoRetail.Application.CQRS.Address.Commands;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request.Address;
using NettoRetail.DataAccess.Specifications.Country;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.API.Infrastructure.Validators
{
    public class CreateAddressCommandValidator : AbstractValidator<CreateAddressCommand>
    {
        private readonly ICountryRepository _countryRepository;
        private readonly IValidator<CreateAddressRequest> _validator;
        private readonly IAddressRepository _addressRepository;
        public CreateAddressCommandValidator(
            ICountryRepository countryRepository,
            IValidator<CreateAddressRequest> validator,
            IAddressRepository addressRepository)
        {
            _validator = validator;
            _countryRepository = countryRepository;
            _addressRepository = addressRepository;
            ConfigureRules();
        }

        private void ConfigureRules()
        {
            RuleFor(x => x)
                .CustomAsync(ValidateAddressLimit);

            RuleFor(x => x.CreateAddressRequest)
                .SetValidator(_validator)
                .CustomAsync(IsCorrectZipCode);
        }

        private async Task IsCorrectZipCode(
            CreateAddressRequest createAddressRequest,
            ValidationContext<CreateAddressCommand> validationContext,
            CancellationToken cancellationToken)
        {
            var specification = new GetCountryByIdSpecification(createAddressRequest.CountryId);
            var country = await _countryRepository.GetAsync(specification, cancellationToken);

            if (country == null)
            {
                throw new NotFoundException(ValidationErrorMessages.Country.CountryNotFoundErrorMessage);
            }

            if (!Regex.IsMatch(createAddressRequest.ZipCode, country.ZipCodeMask))
            {
                throw new BadRequestException(ValidationErrorMessages.Country.InvalidZipCodeErrorMessage);
            }
        }

        private async Task ValidateAddressLimit(
            CreateAddressCommand createAddressCommand,
            ValidationContext<CreateAddressCommand> validationContext,
            CancellationToken cancellationToken)
        {
            var addressCount = await _addressRepository.CountByUserIdAsync(createAddressCommand.UserId, cancellationToken);
            if (addressCount >= 10)
            {
                throw new ConflictException(ValidationErrorMessages.Address.MaxAddressesLimitErrorMessage);
            }
        }
    }
}

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
    public class UpdateAddressCommandValidator : AbstractValidator<UpdateAddressCommand>
    {
        private readonly IValidator<CreateAddressRequest> _validator;
        private readonly ICountryRepository _countryRepository;
        private readonly IAddressRepository _addressRepository;
        public UpdateAddressCommandValidator(
            IValidator<CreateAddressRequest> validator,
            ICountryRepository countryRepository,
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

            RuleFor(x => x.UpdateAddress)
                .SetValidator(_validator);

            RuleFor(x => x.UpdateAddress)
                .CustomAsync(IsCorrectZipCode);

        }

        private async Task IsCorrectZipCode(
            UpdateAddressRequest updateAddressRequest,
            ValidationContext<UpdateAddressCommand> validationContext,
            CancellationToken cancellationToken)
        {
            var specification = new GetCountryByIdSpecification(updateAddressRequest.CountryId);
            var country = await _countryRepository.GetAsync(specification, cancellationToken);

            if (country == null)
            {
                throw new NotFoundException(ValidationErrorMessages.Country.CountryNotFoundErrorMessage);
            }

            if (!Regex.IsMatch(updateAddressRequest.ZipCode, country.ZipCodeMask))
            {
                throw new BadRequestException(ValidationErrorMessages.Country.InvalidZipCodeErrorMessage);
            }
        }

        private async Task ValidateAddressLimit(
            UpdateAddressCommand updateAddressCommand,
            ValidationContext<UpdateAddressCommand> validationContext,
            CancellationToken cancellationToken)
        {
            var addressCount = await _addressRepository.CountByUserIdAsync(updateAddressCommand.UserId, cancellationToken);
            if (addressCount > 10)
            {
                throw new ConflictException(ValidationErrorMessages.Address.MaxAddressesLimitErrorMessage);
            }
        }
    }
}

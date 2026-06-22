using FluentValidation;
using NettoRetail.Application.CQRS.Payments.Queries;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Request;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.API.Infrastructure.Validators;

public class GetPaymentHistoryQueryValidator : AbstractValidator<GetPaymentHistoryQuery>
{
    private readonly IStaffRepository _staffRepository;
    private readonly IValidator<PaginationRequest> _validator;
    public GetPaymentHistoryQueryValidator(IStaffRepository staffRepository, IValidator<PaginationRequest> validator)
    {
        _staffRepository = staffRepository;
        _validator = validator;
        ConfigureRules();
    }

    private void ConfigureRules()
    {
        RuleFor(x => x.AccountId)
            .CustomAsync(ValidateAccountExists)
            .When(x => x.AccountId != Guid.Empty);

        RuleFor(x => x.PageModel)
            .SetValidator(_validator);
    }

    private async Task ValidateAccountExists(Guid accountId, ValidationContext<GetPaymentHistoryQuery> context, CancellationToken cancellationToken)
    {
        var spec = new GetStaffByIdSpecification(accountId);
        if (!await _staffRepository.ExistsAsync(spec, cancellationToken))
        {
            throw new NotFoundException(ValidationErrorMessages.Account.AccountNotFoundErrorMessage);
        }
    }
}

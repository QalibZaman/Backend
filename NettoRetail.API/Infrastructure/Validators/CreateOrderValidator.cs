using FluentValidation;
using NettoRetail.Application.Models.Order;

namespace NettoRetail.API.Infrastructure.Validators;

public class CreateOrderValidator : AbstractValidator<CreateOrderModel>
{
    public CreateOrderValidator()
    {
        RuleFor(x => x.CustomerFirstName)
            .NotEmpty().WithMessage("Invalid First Name. Please check format or length.")
            .NotNull().WithMessage("Invalid First Name. Please check format or length.")
            .MinimumLength(2).WithMessage("Invalid First Name. Please check format or length.")
            .MaximumLength(30).WithMessage("Invalid First Name. Please check format or length.")
            .Matches("^[A-Za-z]+$").WithMessage("Invalid First Name. Please check format or length.")
            .Matches("^[A-Z][a-z]*$").WithMessage("Invalid First Name. Please check format or length.")
            .Matches(@"^\S.*\S$|^\S$").WithMessage("Invalid First Name. Please check format or length.");

        RuleFor(x => x.CustomerLastName)
            .NotEmpty().WithMessage("Invalid Last Name. Please check format or length.")
            .NotNull().WithMessage("Invalid Last Name. Please check format or length.")
            .MinimumLength(2).WithMessage("Invalid Last Name. Please check format or length.")
            .MaximumLength(30).WithMessage("Invalid Last Name. Please check format or length.")
            .Matches("^[A-Za-z]+$").WithMessage("Invalid Last Name. Please check format or length.")
            .Matches("^[A-Z][a-z]*$").WithMessage("Invalid Last Name. Please check format or length.")
            .Matches(@"^\S.*\S$|^\S$").WithMessage("Invalid Last Name. Please check format or length.");

        RuleFor(x => x.CustomerEmail)
            .NotEmpty().WithMessage("Email is required.")
            .NotNull().WithMessage("Email is required.")
            .MaximumLength(256).WithMessage("Email length must be less than 256 characters long.")
            .Matches(@"^(?!.*[_.]{2})[a-zA-Z0-9._%+-]+@[a-zA-Z0-9][a-zA-Z0-9.-]*[a-zA-Z0-9]+\.[a-zA-Z]{2,}$").WithMessage("Email must be in a valid format.");

        RuleFor(x => x.DeliveryType)
            .NotNull().WithMessage("DeliveryType is required.")
            .IsInEnum().WithMessage("DeliveryType must be a valid type.");

        RuleFor(x => x.Products)
            .NotEmpty().WithMessage("Products data is required.")
            .NotNull().WithMessage("Products data is required.")
            .Must(x => x.Keys.All(k => k != Guid.Empty)).WithMessage("Product IDs must not be empty.")
            .Must(x => x.Values.All(v => v > 0)).WithMessage("Product quantities must be greater than 0.");
    }
}
using FluentValidation;
using NettoRetail.Application.Models.Request.Lock;

namespace NettoRetail.API.Infrastructure.Validators
{
    public class LockRequestValidator : AbstractValidator<LockRequest>
    {
        public LockRequestValidator()
        {
            RuleFor(x => x.EntityToLock)
                .IsInEnum();

            RuleFor(x => x.EntityId)
                .NotEmpty().WithMessage("EntityId is required.")
                .Must(BeIntOrGuid).WithMessage("EntityId must be a valid integer or GUID.");
        }

        private bool BeIntOrGuid(string entityId)
        {
            if (string.IsNullOrWhiteSpace(entityId))
                return false;

            entityId = entityId.Trim();

            if (int.TryParse(entityId, out _))
                return true;

            if (Guid.TryParse(entityId, out _))
                return true;

            return false;
        }
    }
}
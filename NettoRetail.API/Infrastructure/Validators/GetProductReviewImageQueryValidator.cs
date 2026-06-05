using FluentValidation;
using NettoRetail.Application.CQRS.ProductReviews.Queries;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.ProductReview;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.API.Infrastructure.Validators;

public class GetProductReviewImageQueryValidator : AbstractValidator<GetProductReviewImageQuery>
{
    private readonly IProductReviewRepository _productReviewRepository;
    public GetProductReviewImageQueryValidator(IProductReviewRepository productReviewRepository)
    {
        _productReviewRepository = productReviewRepository;
        ConfigureRules();
    }

    private void ConfigureRules()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.CustomerId)
            .NotNull();

        RuleFor(x => x.ProductReviewImage)
            .NotNull();

        RuleFor(x => x)
            .CustomAsync(ValidateProductReviewImageExistence);
    }

    private async Task ValidateProductReviewImageExistence(
        GetProductReviewImageQuery query,
        ValidationContext<GetProductReviewImageQuery> context,
        CancellationToken cancellationToken)
    {
        var spec = new GetProductReviewByBlobNameAndCustomerIdSpecification(query.ProductReviewImage, query.CustomerId);
        var exists = await _productReviewRepository.Exists(spec, cancellationToken);
        if (!exists)
        {
            throw new NotFoundException("Product review image not found.");
        }
    }
}

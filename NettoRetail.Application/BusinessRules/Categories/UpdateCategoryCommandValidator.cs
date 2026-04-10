using FluentValidation;
using NettoRetail.Application.CQRS.Categories.Commands;
using NettoRetail.Application.Exceptions;
using NettoRetail.DataAccess.Specifications.Categories;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.BusinessRules.Categories;

public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoriesCommand>
{
    public UpdateCategoryCommandValidator(ICategoryRepository categoryRepository)
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required")
            .MinimumLength(2).WithMessage("Category name length must be greater than 2 characters long.")
            .MaximumLength(50).WithMessage("Category name length must be less than 50 characters long.");

        RuleFor(x => x.URL)
            .NotEmpty().WithMessage("URL is required")
            .Length(1, 50).WithMessage("The URL must be between 1 and 50 characters long.")
            .Matches("^[a-zA-Z0-9\\-._~]+$").WithMessage("Allowed characters: letters, digits, -, ., _, ~.")
            .Matches("^\\S.*\\S$").WithMessage("No leading or trailing spaces are allowed.");

        RuleFor(x => x.ParentId)
            .GreaterThan(0).WithMessage("The id of parent category must be greater than 0")
            .When(x => x.ParentId.HasValue);

        RuleFor(x => x)
            .CustomAsync(async (command, context, ct) =>
            {
                var category = await categoryRepository.GetAsync(
                    new GetCategoryByIdAndShopIdSpecification(command.Id, command.ShopId),
                    ct);

                if (category is null)
                {
                    throw new NotFoundException("Category not found");
                }

                if (command.ParentId.HasValue)
                {
                    if (command.ParentId.Value == command.Id)
                    {
                        throw new BadRequestException("Category cannot be its own parent.");
                    }

                    var parentExists = await categoryRepository.Exists(
                        new GetCategoryByIdAndShopIdSpecification(command.ParentId.Value, command.ShopId),
                        ct);

                    if (!parentExists)
                    {
                        throw new NotFoundException("Parent category not found");
                    }

                    var subtree = await categoryRepository.GetCategorySubtreeAsync(command.Id, ct);
                    if (subtree.Any(c => c.Id == command.ParentId.Value))
                    {
                        throw new BadRequestException("Category cannot be assigned to its own descendant.");
                    }
                }

                var normalizedName = command.Name.Trim();
                var normalizedUrl = command.URL?.Trim() ?? string.Empty;

                var nameChanged = !string.Equals(category.Name, normalizedName, StringComparison.OrdinalIgnoreCase);
                var urlChanged = !string.Equals(category.URL, normalizedUrl, StringComparison.OrdinalIgnoreCase);

                if (nameChanged || urlChanged)
                {
                    var conflictSpec = new GetCategoriesByNameOrUrlSpecification(
                        nameChanged ? normalizedName : null,
                        urlChanged ? normalizedUrl : null,
                        command.ShopId,
                        command.Id);

                    var conflicts = await categoryRepository.GetManyBySpecAsync(conflictSpec, ct);
                    var conflictList = conflicts as ICollection<NettoRetail.Domain.Entities.CategoryEntity> ?? conflicts.ToList();

                    var nameConflict = nameChanged && conflictList.Any(c => string.Equals(c.Name, normalizedName, StringComparison.OrdinalIgnoreCase));
                    var urlConflict = urlChanged && conflictList.Any(c => string.Equals(c.URL, normalizedUrl, StringComparison.OrdinalIgnoreCase));

                    if (nameConflict && urlConflict)
                    {
                        throw new ConflictException("Category name already exists and category URL already exists .");
                    }

                    if (nameConflict)
                    {
                        throw new ConflictException("Category name already exists .");
                    }

                    if (urlConflict)
                    {
                        throw new ConflictException("Category URL already exists .");
                    }
                }
            });
    }
}

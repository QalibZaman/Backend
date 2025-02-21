using System.Linq.Expressions;

namespace NettoRetail.Shared.Specifications;

public class SpecificationBuilder<TEntity>
    where TEntity : class
{
    private readonly Specification<TEntity> _specification;

    public SpecificationBuilder()
    {
        _specification = new Specification<TEntity>(Specification<TEntity>.Empty);
    }

    public SpecificationBuilder<TEntity> Skip(int skip)
    {
        _specification.Skip = skip;
        return this;
    }

    public SpecificationBuilder<TEntity> Take(int take)
    {
        _specification.Take = take;
        return this;
    }

    public SpecificationBuilder<TEntity> SetOrderBy(Expression<Func<TEntity, object>> expression)
    {
        _specification.OrderBy = expression;
        return this;
    }

    public SpecificationBuilder<TEntity> SetOrderByDescending(Expression<Func<TEntity, object>> expression)
    {
        _specification.OrderByDescending = expression;
        return this;
    }

    public SpecificationBuilder<TEntity> Add<TSpecification>(params object?[]? args)
        where TSpecification : Specification<TEntity>
    {
        if (args == null || args.All(a => a == null)) return this;

        // In case the specification can't be built,
        // which might happen due to incorrect parameter count or types, this will return false.
        var specificationIsValid = ValidateSpecificationIntegrity<TSpecification>(args);

        // If the check evaluates to false, that means we should throw an error due to incorrect implementation.
        // This means that the issue on the developer's side and the way the builder is being used should be checked.
        if (!specificationIsValid)
        {
            throw new ArgumentException("Specification could not be built due to incorrect argument count or types");
        }

        // Otherwise we create the instance of the generic specification dynamically
        // and then chain it to the previous specification.
        var newSpecification = (TSpecification)Activator.CreateInstance(typeof(TSpecification), args)!;
        _specification.And(newSpecification);
        return this;
    }

    public Specification<TEntity> Build() => _specification;

    private bool ValidateSpecificationIntegrity<TSpecification>(object?[] args)
        where TSpecification : Specification<TEntity>
    {
        var specType = typeof(TSpecification);
        var argsCount = args.Length;

        var matchingConstructor = specType.GetConstructors()
            .FirstOrDefault(c => c.GetParameters().Length == argsCount);

        if (matchingConstructor == null) return false;

        var matchingConstructorParameters = matchingConstructor.GetParameters();

        for (var i = 0; i < argsCount; i++)
        {
            var specParameter = matchingConstructorParameters[i].GetType();
            var argParam = args[i]!.GetType();

            if (!specParameter.IsAssignableFrom(argParam)) return false;
        }

        return true;
    }
}
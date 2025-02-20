using NettoHealth.Domain.Entities;
using NettoHealth.Shared.Specifications;

namespace NettoHealth.DataAccess.Specifications;

public class GetHealthcareProviderSpecification : Specification<HealthcareProviderEntity>
{
    public GetHealthcareProviderSpecification()
        : base(_ => true) { }
}
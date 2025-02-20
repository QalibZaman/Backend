using NettoHealth.Domain.Entities;
using NettoHealth.Shared.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NettoHealth.DataAccess.Specifications;

public class GetHealthcareProviderByEmailSpecification : Specification<HealthcareProviderEntity>
{
    public GetHealthcareProviderByEmailSpecification(string email)
    : base(x => x.Email == email) { }
}
using NettoRetail.Domain.Entities;
using NettoRetail.Shared.Specifications;

namespace NettoRetail.DataAccess.Specifications.Staff;

public class GetStaffByEmailsSpecification : Specification<StaffEntity>
{
    public GetStaffByEmailsSpecification(IEnumerable<string> emails)
        : this(emails.Select(email => email.ToLower()).ToArray())
    {
    }

    private GetStaffByEmailsSpecification(string[] normalizedEmails)
        : base(account => normalizedEmails.Contains(account.Email.ToLower()))
    {
    }
}

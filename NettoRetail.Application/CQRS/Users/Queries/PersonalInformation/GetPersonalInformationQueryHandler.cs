using System.Security.Claims;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Response;
using NettoRetail.DataAccess.Specifications.Customer;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Shared.Interfaces.Repositories;

namespace NettoRetail.Application.CQRS.Users.Queries.PersonalInformation
{
    public class GetPersonalInformationQueryHandler(
    ICustomerRepository customerRepository,
    IStaffRepository staffRepository,
    IHttpContextAccessor httpContextAccessor,
    IMapper mapper)
    : IRequestHandler<GetPersonalInformationQuery, GetPersonalInformationResponse>
    {
        public async Task<GetPersonalInformationResponse> Handle(
            GetPersonalInformationQuery request,
            CancellationToken cancellationToken)
        {
            var userId = httpContextAccessor.HttpContext?
                .User
                .FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(userId, out var id))
                throw new UnauthorizedAccessException("User is not authenticated.");

            var customer = await customerRepository.GetWithRelatedDataAsync(
                new GetCustomerByIdSpecification(id),
                cancellationToken);

            if (customer is not null)
            {
                return mapper.Map<GetPersonalInformationResponse>(customer);
            }

            var staff = await staffRepository.GetWithRelatedDataAsync(
                new GetStaffByIdSpecification(id),
                cancellationToken);

            if (staff is not null)
            {
                return mapper.Map<GetPersonalInformationResponse>(staff);
            }

            throw new NotFoundException("User not found.");
        }
    }
}

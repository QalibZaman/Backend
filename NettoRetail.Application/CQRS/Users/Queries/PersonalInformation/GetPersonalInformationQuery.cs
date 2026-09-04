using MediatR;
using NettoRetail.Application.Models.Response;

namespace NettoRetail.Application.CQRS.Users.Queries.PersonalInformation
{
    public record GetPersonalInformationQuery : IRequest<GetPersonalInformationResponse>;
}

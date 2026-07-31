using MediatR;
using NettoRetail.Application.Models.Request;

namespace NettoRetail.Application.CQRS.Users.Commands.PersonalInformation
{
    public record UpdatePersonalInformationCommand(
     UpdatePersonalInformationRequest Model)
     : IRequest;
}

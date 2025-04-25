using Hangfire;
using MediatR;
using NettoRetail.Application.Models.Email;
using NettoRetail.Application.Services.INewsletterService;

namespace NettoRetail.Application.CQRS.Newsletters.Commands;

public record PostNewsletterCommand(EmailModelRequest Model) : IRequest;

public class PostNewsletterCommandHandler : IRequestHandler<PostNewsletterCommand>
{
    public Task Handle(PostNewsletterCommand request, CancellationToken cancellationToken)
    {
        BackgroundJob.Enqueue<INewsletterService>(service => service.SendNewsletterAsync(request.Model));
        return Task.CompletedTask;
    }
}
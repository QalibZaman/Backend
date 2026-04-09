using MapsterMapper;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MediatR;
using Microsoft.Extensions.Options;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Interfaces;
using NettoRetail.Application.Models.Admin;
using NettoRetail.Application.Models.Request.Staff;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Services.IRoleService;
using NettoRetail.DataAccess.Specifications.Staff;
using NettoRetail.Domain.Entities;
using NettoRetail.Domain.Entities.JunctionEntities;
using NettoRetail.Domain.Enums;
using NettoRetail.Infrastructure.Interfaces;
using NettoRetail.Infrastructure.Options;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Interfaces.Repositories;
using NettoRetail.Shared.Interfaces.Utilities;

namespace NettoRetail.Application.CQRS.Users.Commands.Staff;

public record CreatePlatformAdminCommand(
    CreatePlatformAdminRequest Model,
    Guid SuperAdminId) : IRequest<CreatePlatformAdminResponse>;

public class CreatePlatformAdminCommandHandler (
    IStaffRepository accountRepo,
    IRoleRepository roleRepo,
    IUnitOfWork unitOfWork,
    IJwtManager jwtManager,
    IEmailBuilderService emailBuilderService,
    IEmailSender emailSender,
    IOptions<PlatformAdminActivationOptions> activationOptions,
    IRoleService roleService,
    IMapper mapper) : IRequestHandler<CreatePlatformAdminCommand, CreatePlatformAdminResponse>
{

        public async Task<CreatePlatformAdminResponse> Handle(
            CreatePlatformAdminCommand request,
            CancellationToken cancellationToken)
        {
            var activationLinkTemplate = activationOptions.Value.ActivationLink;
            if (string.IsNullOrWhiteSpace(activationLinkTemplate))
                throw new BadRequestException("PlatformAdminActivationOptions:ActivationLink is not configured.");

            if (await accountRepo.Exists(new GetStaffByEmailSpecification(request.Model.Email), cancellationToken))
                throw new ConflictException("A platform admin with this email already exists");

            var model = new CreatePlatformAdminModel(request.Model);

            var role = await roleService.CreatePlatformAdminRole(model, cancellationToken);

            var account = mapper.Map<StaffEntity>(model);
            if (account.Id == Guid.Empty)
                account.Id = Guid.NewGuid();

            account.Role = role;
            if (role.Id != 0)
                account.RoleId = role.Id;
            account.Status = StaffStatusEnum.Pending;
            var now = DateTime.UtcNow;
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, account.Id.ToString()),
                new(AuthClaims.TokenType, AuthTokenTypes.PlatformAdminActivation),
                new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
            };

            var rawToken = jwtManager.Generate(
                claims,
                TimeSpan.FromHours(activationOptions.Value.TokenExpirationHours));

            using var tx = unitOfWork.BeginTransaction();

            if (role.Id == 0)
                await roleRepo.AddAsync(role, cancellationToken);

            account.InvitationTokenIssuedAt = now;
            await accountRepo.AddAsync(account, cancellationToken);

            tx.Commit();

            var link = string.Format(activationLinkTemplate, Uri.EscapeDataString(rawToken));

            var emailModel = await emailBuilderService.BuildPlatformAdminActivationEmailAsync(
                account.Email,
                account.FirstName,
                link);

            await emailSender.SendAsync(emailModel, account.Id);

            return new CreatePlatformAdminResponse
            {
                AccountId = account.Id,
                Email = account.Email,
                Status = account.Status,
                Permissions = model.Permissions
            };
        }
    }

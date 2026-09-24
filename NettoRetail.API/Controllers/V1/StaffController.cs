using Asp.Versioning;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.Newsletters.Commands;
using NettoRetail.Application.CQRS.Sessions.Commands;
using NettoRetail.Application.CQRS.Users.Commands.Admin;
using NettoRetail.Application.CQRS.Users.Commands.Auth;
using NettoRetail.Application.CQRS.Users.Commands.Password;
using NettoRetail.Application.CQRS.Users.Queries.Password;
using NettoRetail.Application.CQRS.Users.Queries.PersonalInformation;
using NettoRetail.Application.Models.Admin;
using NettoRetail.Application.Models.Authentication;
using NettoRetail.Application.Models.Email;
using NettoRetail.Application.Models.Request.Password;
using NettoRetail.Application.Models.Response;
using NettoRetail.Domain.Enums;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.API.Controllers.V1;

[ApiController]
[ApiVersion(1)]
[Route("/api/v{version:ApiVersion}/[controller]")]
public class StaffController(
    IMediator mediator,
    IMapper mapper) : ControllerBase
{
    [Authorize]
    [HttpGet("personal-information")]
    public async Task<IActionResult> GetPersonalInformation(
        CancellationToken cancellationToken)
    {
        var query = new GetPersonalInformationQuery();

        var data = await mediator.Send(query, cancellationToken);

        var response = new ApiResponse<GetPersonalInformationResponse>(data);

        return Ok(response);
    }

    [HttpPost("log-in")]
    public async Task<IActionResult> LogIn([FromBody] LoginModel model)
    {
        var loginCommand = new LoginCommand(model);
        var data = await mediator.Send(loginCommand);
        var response = new ApiResponse<TokenResponse>(data);

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
        var sessionCreateCommand = new CreateSessionCommand(
            ipAddress,
            Request.Headers["User-Agent"].ToString(),
            data.AccessToken,
            data.RefreshToken,
            UserTypeEnum.Admin);

        await mediator.Send(sessionCreateCommand);

        return Ok(response);
    }

    [HttpPost("locked-account-activation")]
    public async Task<IActionResult> ResendActivationEmail(
        [FromBody] ActivationEmailAttemptRequest model)
    {
        var emailCommand = new ResendActivationEmailCommand(model);
        await mediator.Send(emailCommand);

        return Ok(new
        {
            message = "If an account with that email exists, an activation email has been sent.",
        });
    }

    [HttpPost("log-out")]
    [Authorize]
    public async Task<IActionResult> LogOut()
    {
        var token = JwtUtils.ExtractTokenFromAuthorizationHeader(HttpContext);
        var userId = HttpContext.GetUserId();

        var logoutCommand = new LogoutCommand(token!, userId);
        await mediator.Send(logoutCommand);

        return NoContent();
    }

    [HttpGet("activate")]
    public async Task<IActionResult> Activate([FromQuery] string token)
    {
        var activateLockedAccountCommand = new ActivateLockedAccountCommand(token);
        await mediator.Send(activateLockedAccountCommand);

        return Ok();
    }

    [HttpPost("subscribe-to-newsletter")]
    [Authorize]
    public async Task<IActionResult> SubscribeToNewsletter(
        CancellationToken cancellationToken)
    {
        var accountId = HttpContext.GetUserId();

        var command = new SubscribeToNewsletterCommand(accountId);
        await mediator.Send(command, cancellationToken);

        return Ok();
    }

    [HttpPost("unsubscribe-to-newsletter")]
    [Authorize]
    public async Task<IActionResult> UnsubscribeToNewsletter(
        CancellationToken cancellationToken)
    {
        var accountId = HttpContext.GetUserId();

        var command = new UnsubscribeToNewsletterCommand(accountId);
        await mediator.Send(command, cancellationToken);

        return Ok();
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request)
    {
        var command = new ForgotPasswordCommand(request.Email);
        await mediator.Send(command);

        return NoContent();
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequest model)
    {
        var resetPasswordCommand = new ResetPasswordCommand(model);
        await mediator.Send(resetPasswordCommand);

        return NoContent();
    }

    [Authorize]
    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = HttpContext.GetUserId();
        var changePasswordCommand = new ChangePasswordCommand(userId, request);

        await mediator.Send(changePasswordCommand);

        return NoContent();
    }

    [HttpGet("validate-reset-password-token")]
    public async Task<IActionResult> ValidateResetPasswordToken(
        [FromQuery] string token)
    {
        var validateResetPasswordTokenQuery = new ValidateResetPasswordTokenQuery(token);
        await mediator.Send(validateResetPasswordTokenQuery);
        return Ok();
    }
    
    [ProducesResponseType(typeof(ApiResponse<ValidateResetPasswordResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost("validate-reset-password")]
    public async Task<IActionResult> ValidateResetPassword([FromBody] ResetPasswordRequest model)
    {
        var validateResetPasswordQuery = new ValidateResetPasswordQuery(model);
        var response = await mediator.Send(validateResetPasswordQuery);
        return Ok(response);
    }


    [HttpPost("sign-up")]
    // [Authorize(Policy = PolicyNames.SuperAdminOnlyPolicy)]
    [ProducesResponseType(typeof(ApiResponse<TokenResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Produces("application/json")]
    public async Task<IActionResult> SignUp(AdminSignUpModel model)
    {
        var adminCreationCommand = new CreateAdminCommand(model);
        var data = await mediator.Send(adminCreationCommand);
        var response = new ApiResponse<TokenResponse>(data);

        // for session
        string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
        var sessionCreateCommand = new CreateSessionCommand(ipAddress, Request.Headers["User-Agent"].ToString(),
            data.AccessToken, data.RefreshToken, UserTypeEnum.Admin);
        await mediator.Send(sessionCreateCommand);

        return Created("", response);
    }
}
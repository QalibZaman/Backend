using Asp.Versioning;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NettoRetail.API.Infrastructure.Extensions;
using NettoRetail.API.Models.Response;
using NettoRetail.Application.CQRS.InvitationToken;
using NettoRetail.Application.CQRS.Newsletters.Commands;
using NettoRetail.Application.CQRS.Sessions.Commands;
using NettoRetail.Application.CQRS.Users.Commands;
using NettoRetail.Application.CQRS.Users.Commands.Auth;
using NettoRetail.Application.CQRS.Users.Commands.Password;
using NettoRetail.Application.CQRS.Users.Commands.Staff;
using NettoRetail.Application.CQRS.Users.Queries.Account;
using NettoRetail.Application.CQRS.Users.Queries.Staff;
using NettoRetail.Application.Exceptions;
using NettoRetail.Application.Models.Authentication;
using NettoRetail.Application.Models.Request;
using NettoRetail.Application.Models.Request.Password;
using NettoRetail.Application.Models.Request.Staff;
using NettoRetail.Application.Models.Request.Token;
using NettoRetail.Application.Models.Response;
using NettoRetail.Application.Models.Response.Account;
using NettoRetail.Application.Models.Session;
using NettoRetail.Application.Models.TwoFactorAuth;
using NettoRetail.Domain.Enums;
using NettoRetail.Infrastructure.Extensions;
using NettoRetail.Shared.Constants;
using NettoRetail.Shared.Utilities;

namespace NettoRetail.API.Controllers.V1;

[ApiController]
[ApiVersion(1)]
[Route("/api/v{version:ApiVersion}/[controller]")]
public class StaffController(
    IMediator mediator) : ControllerBase
{
    [HttpPost("log-in")]
    public async Task<IActionResult> LogIn([FromBody] LoginModel model)
    {
        var loginCommand = new LoginCommand(model);
        var data = await mediator.Send(loginCommand);

        if (!string.IsNullOrEmpty(data.TempToken))
        {
            return Ok(new ApiResponse<TokenResponse>(data));
        }

        string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
        var sessionCreateCommand = new CreateSessionCommand(ipAddress, Request.Headers["User-Agent"].ToString(), data.AccessToken, data.RefreshToken, UserTypeEnum.Admin);
        await mediator.Send(sessionCreateCommand);

        var response = new ApiResponse<TokenResponse>(data);

        return Ok(response);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var command = new RefreshTokenCommand(request.RefreshToken);
        var data = await mediator.Send(command);

        var sessionUpdateCommand = new UpdateSessionCommand(new SessionUpdateModel
        {
            CurrentJwt = data.AccessToken,
            LastActivityTime = DateTime.UtcNow,
            CurrentRefreshToken = data.RefreshToken,
            PreviousRefreshToken = request.RefreshToken,
        });
        await mediator.Send(sessionUpdateCommand);

        var response = new ApiResponse<TokenResponse>(data);

        return Ok(response);
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

    [Authorize]
    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = HttpContext.GetUserId();
        var changePasswordCommand = new ChangePasswordCommand(userId, request);

        await mediator.Send(changePasswordCommand);

        return NoContent();
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var command = new ForgotPasswordCommand(request.Email);
        await mediator.Send(command);

        return NoContent();
    }

    [HttpPost("verify-reset-token")]
    public async Task<IActionResult> VerifyResetToken([FromBody] VerifyResetTokenRequest request)
    {
        var command = new VerifyResetTokenCommand(request.Token);
        await mediator.Send(command);

        return NoContent();
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> RecoverPassword([FromBody] ResetPasswordRequest model)
    {
        var resetPasswordCommand = new ResetPasswordCommand(model);
        await mediator.Send(resetPasswordCommand);

        return NoContent();
    }

    [Authorize]
    [HttpPost("enable-2fa")]
    [ProducesResponseType(typeof(ApiResponse<EnableTwoFactorAuthResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> EnableTwoFactorAuth(
        [FromBody] EnableTwoFactorAuthRequest request)
    {
        var userId = HttpContext.GetUserId();
        var accessToken = JwtUtils.ExtractTokenFromAuthorizationHeader(HttpContext);

        var command = new EnableTwoFactorAuthCommand(userId, request, accessToken!);
        var response = await mediator.Send(command);

        return Ok(new ApiResponse<EnableTwoFactorAuthResponse>(response));
    }

    [Authorize]
    [HttpPost("confirm-2fa")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ConfirmTwoFactor(
        [FromBody] VerifyTotpRequest request)
    {
        var userId = HttpContext.GetUserId();
        var command = new ConfirmTwoFactorAuthCommand(userId, request.TotpCode);

        await mediator.Send(command);

        return NoContent();
    }

    [Authorize(
        AuthenticationSchemes = AuthSchemes.TempToken,
        Policy = PolicyNames.TwoFactorAuthPolicy)]
    [HttpPost("verify-2fa")]
    [ProducesResponseType(typeof(ApiResponse<TokenResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> VerifyTwoFactor(
        [FromBody] VerifyTotpRequest request)
    {
        var userId = HttpContext.GetUserId();
        var command = new VerifyTwoFactorAuthCommand(userId, request.TotpCode);

        var tokenResponse = await mediator.Send(command);

        string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
        var sessionCreateCommand = new CreateSessionCommand(ipAddress, Request.Headers["User-Agent"].ToString(), tokenResponse.AccessToken, tokenResponse.RefreshToken, UserTypeEnum.Admin);
        await mediator.Send(sessionCreateCommand);

        return Ok(new ApiResponse<TokenResponse>(tokenResponse));
    }

    [Authorize]
    [HttpPost("disable-2fa")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DisableTwoFactorAuth(
        [FromBody] DisableTwoFactorAuthRequest request)
    {
        var userId = HttpContext.GetUserId();
        var accessToken = JwtUtils.ExtractTokenFromAuthorizationHeader(HttpContext);

        var command = new DisableTwoFactorAuthCommand(userId, request, accessToken!);
        await mediator.Send(command);

        return NoContent();
    }

    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<StaffDetailsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetStaffById(Guid id, CancellationToken cancellationToken)
    {
        var shopId = HttpContext.GetUserShopId();

        var query = new GetStaffByIdQuery(id, shopId);
        var data = await mediator.Send(query, cancellationToken);

        var response = new ApiResponse<StaffDetailsResponse>(data);
        return Ok(response);
    }

    [Authorize]
    [HttpGet("personal-information")]
    public async Task<IActionResult> GetPersonalInformation(CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();

        var query = new GetPersonalInfoQuery(userId);
        var data = await mediator.Send(query, cancellationToken);

        var response = new ApiResponse<AccountDetailsResponse>(data);
        return Ok(response);
    }

    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpPost]
    public async Task<IActionResult> CreateStaff([FromBody] CreateStaffRequest request, CancellationToken cancellationToken)
    {
        var shopId = HttpContext.GetUserShopId();

        var command = new CreateStaffCommand(request, shopId);
        var data = await mediator.Send(command, cancellationToken);

        var response = new ApiResponse<CreateStaffResponse>(data);

        return Ok(response);
    }

    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> EditStaff(Guid id, [FromBody] UpdateStaffRequest request, CancellationToken cancellationToken)
    {
        var shopId = HttpContext.GetUserShopId();

        var command = new UpdateStaffCommand(id, request, shopId);

        await mediator.Send(command, cancellationToken);

        return NoContent();
    }

    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpPost("{id:guid}/reset-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ResetStaffPassword(Guid id, CancellationToken cancellationToken)
    {
        var shopId = HttpContext.GetUserShopId();

        var command = new ResetStaffPasswordCommand(id, shopId);
        await mediator.Send(command, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Deletes a staff member from the current shop.
    /// </summary>
    /// <remarks>
    /// US-6.4 Delete staff member.
    /// <para>Request: <c>DELETE /api/v1/staff/{id}</c></para>
    /// <para>Auth: Bearer token with shop claim is required.</para>
    /// <para>Request body: none.</para>
    /// <para>Success response: 204 No Content (empty body).</para>
    /// <para>Error response shape:</para>
    /// <para>
    /// <code>
    /// { "message": "Error message", "errors": { "field": [ "details" ] } }
    /// </code>
    /// </para>
    /// <para>Notes:</para>
    /// <para>- <c>id</c> must be a valid GUID of a staff member in the current shop.</para>
    /// </remarks>
    /// <param name="id">Staff member identifier (GUID) from the route.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    /// <returns>204 on success.</returns>
    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteStaff(Guid id, CancellationToken cancellationToken)
    {
        var shopId = HttpContext.GetUserShopId();

        var command = new DeleteStaffCommand(id, shopId);
        await mediator.Send(command, cancellationToken);

        return NoContent();
    }

    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpGet]
    public async Task<IActionResult> GetStaffList([FromQuery] PaginationRequest paginationRequest, [FromQuery] GetFilteredStaffRequest filteredStaffRequest, CancellationToken cancellationToken)
    {
        var shopId = HttpContext.GetUserShopId();

        var query = new GetStaffListQuery(paginationRequest, filteredStaffRequest, shopId);
        var result = await mediator.Send(query, cancellationToken);

        return Ok(result);
    }

    [Authorize]
    [HttpPost("subscribe-to-newsletter")]
    public async Task<IActionResult> SubscribeToNewsletter(CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var command = new SubscribeToNewsletterCommand(userId);
        await mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [Authorize]
    [HttpPost("unsubscribe-to-newsletter")]
    public async Task<IActionResult> UnsubscribeToNewsletter(CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();

        var command = new UnsubscribeToNewsletterCommand(userId);

        await mediator.Send(command, cancellationToken);

        return NoContent();
    }

    [Authorize]
    [HttpPut("{id:guid}/personal-info")]
    public async Task<IActionResult> UpdatePersonalInformation(Guid id, UpdatePersonalInformationRequest model, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        if (userId != id) throw new BadRequestException("You are not able to change data of another user");

        var command = model.Adapt<UpdatePersonalInformationCommand>();
        command = command with { Id = id };

        await mediator.Send(command, cancellationToken);

        return NoContent();
    }

    [Authorize]
    [HttpPut("{id:guid}/profile-image")]
    public async Task<IActionResult> UpdateProfileImage([FromRoute] Guid id, IFormFile image, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        if (userId != id) throw new BadRequestException("You are not able to change data of another user");

        var command = new UpdateProfileImageCommand(id, image);

        await mediator.Send(command, cancellationToken);

        return NoContent();
    }

    [Authorize]
    [HttpDelete("{id:guid}/profile-image")]
    public async Task<IActionResult> DeleteProfileImage([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        if (userId != id) throw new BadRequestException("You are not able to change data of another user");

        var command = new DeleteProfileImageCommand(id);

        await mediator.Send(command, cancellationToken);

        return NoContent();
    }

    [Authorize(Policy = PolicyNames.HasShopClaimPolicy)]
    [HttpGet("csv-file")]
    public async Task<IActionResult> DownloadStaffList([FromQuery] GetFilteredStaffCsvRequest getFilteredStaffCsvRequest, CancellationToken cancellationToken)
    {
        var userShopId = HttpContext.GetUserShopId();

        var staffListDownloadQuery = new DownloadStaffListQuery(userShopId, getFilteredStaffCsvRequest);

        var response = await mediator.Send(staffListDownloadQuery, cancellationToken);

        return File(response, "text/csv", $"Staff-{DateTime.UtcNow}.csv");
    }

    [Authorize]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [HttpPost("upload")]
    public async Task<IActionResult> UploadStaffList(IFormFile file, int id, CancellationToken cancellationToken)
    {
        var command = new UploadStaffListCommand(file, id);
        await mediator.Send(command, cancellationToken);

        return NoContent();
    }

    [Authorize]
    [HttpPatch("toggle-onboarding-status")]
    public async Task<IActionResult> ToggleOnboardingStatus([FromQuery] ToggleOnboardingStatusRequest request, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();

        var command = new ToggleOnboardingStatusCommand(userId, request.IsOnboardingCompleted);

        await mediator.Send(command, cancellationToken);

        return NoContent();
    }

    [Authorize]
    [HttpGet("communication-preference")]
    public async Task<IActionResult> GetCommunicationPreferences(CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();

        var command = new GetCommunicationPreferencesByAccountIdQuery(userId);

        var data = await mediator.Send(command, cancellationToken);
        if (data is null)
            return NotFound();

        var response = new ApiResponse<CommunicationPreferencesResponse>(data);

        return Ok(response);
    }

    [Authorize]
    [HttpPut("update-communication-preference")]
    public async Task ChangeCommunicationPreference([FromQuery] UpdateCommunicationPreferencesRequest request, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();

        var command = new UpdateCommunicationPreferencesCommand(userId, request);

        await mediator.Send(command, cancellationToken);
    }

    [HttpPost("activate-by-invitation")]
    [AllowAnonymous]
    public async Task<IActionResult> ActivateByInvitation([FromBody] SetPasswordByInvitationRequest model)
    {
        var tokenResponse = await mediator.Send(
            new SetPasswordByInvitationTokenCommand(model));

        string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;

        var sessionCreateCommand = new CreateSessionCommand(
            ipAddress,
            Request.Headers.GetUserAgent(),
            tokenResponse.AccessToken,
            tokenResponse.RefreshToken,
            UserTypeEnum.Admin);

        await mediator.Send(sessionCreateCommand);

        return Ok(new ApiResponse<TokenResponse>(tokenResponse));
    }
    [HttpPost("validate-invitation")]
    [AllowAnonymous]
    public async Task<IActionResult> ValidateInvitation([FromBody] string Token)
    {
        await mediator.Send(new ValidateInvitationTokenCommand(Token));
        return NoContent();
    }

    [HttpPost("resend-invitation")]
    [AllowAnonymous]
    public async Task<IActionResult> ResendInvitation([FromBody] ResendInvitationRequest request)
    {
        await mediator.Send(new ResendInvitationByTokenCommand(request.Token));
        return Ok(new ApiResponse("A new invitation link was sent"));
    }
}

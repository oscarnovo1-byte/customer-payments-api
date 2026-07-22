using CustomerPayments.Api.Application.Interfaces;
using CustomerPayments.Api.DTOs;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;
using Microsoft.AspNetCore.RateLimiting;

namespace CustomerPayments.Api.Controllers;

[ApiController]
[AllowAnonymous]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IValidator<LoginRequest> _validator;

    public AuthController(
        IAuthenticationService authenticationService,
        IValidator<LoginRequest> validator)
    {
        _authenticationService = authenticationService;
        _validator = validator;
    }

    [HttpPost("login")]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<ActionResult<LoginResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(
            request,
            cancellationToken);

        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }

            return ValidationProblem(ModelState);
        }

        var response = await _authenticationService.LoginAsync(
            request,
            cancellationToken);

        if (response is null)
        {
            return Unauthorized();
        }

        return Ok(response);
    }

    [HttpPost("refresh")]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<ActionResult<LoginResponse>> Refresh(
        RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var response =
            await _authenticationService.RefreshAsync(
                request,
                cancellationToken);

        if (response is null)
        {
            return Unauthorized();
        }

        return Ok(response);
    }

    [HttpPost("revoke")]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<IActionResult> Revoke(
        RevokeRefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var revoked =
            await _authenticationService.RevokeAsync(
                request,
                cancellationToken);

        if (!revoked)
        {
            return Unauthorized();
        }

        return NoContent();
    }    
}
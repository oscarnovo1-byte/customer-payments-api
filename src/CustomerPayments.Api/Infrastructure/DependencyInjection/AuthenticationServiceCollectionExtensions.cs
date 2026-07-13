using System.Text;
using CustomerPayments.Api.Application.Interfaces;
using CustomerPayments.Api.Infrastructure.Security;
using CustomerPayments.Api.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace CustomerPayments.Api.Infrastructure.DependencyInjection;

public static class AuthenticationServiceCollectionExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtOptions = configuration
            .GetSection(JwtOptions.SectionName)
            .Get<JwtOptions>()
            ?? throw new InvalidOperationException(
                $"Configuration section '{JwtOptions.SectionName}' is missing.");

        ValidateJwtOptions(jwtOptions);

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Disabled only for local development and demo purposes.
                options.RequireHttpsMetadata = false;
                options.SaveToken = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,

                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();

        services.AddScoped<IJwtTokenService, JwtTokenService>();

        return services;
    }

    private static void ValidateJwtOptions(JwtOptions jwtOptions)
    {
        if (string.IsNullOrWhiteSpace(jwtOptions.SecretKey))
        {
            throw new InvalidOperationException(
                "JWT SecretKey is not configured. " +
                "Configure it using User Secrets or an environment variable.");
        }

        if (jwtOptions.SecretKey.Length < 32)
        {
            throw new InvalidOperationException(
                "JWT SecretKey must contain at least 32 characters.");
        }

        if (string.IsNullOrWhiteSpace(jwtOptions.Issuer))
        {
            throw new InvalidOperationException(
                "JWT Issuer is not configured.");
        }

        if (string.IsNullOrWhiteSpace(jwtOptions.Audience))
        {
            throw new InvalidOperationException(
                "JWT Audience is not configured.");
        }

        if (jwtOptions.ExpirationMinutes <= 0)
        {
            throw new InvalidOperationException(
                "JWT ExpirationMinutes must be greater than zero.");
        }
    }
}
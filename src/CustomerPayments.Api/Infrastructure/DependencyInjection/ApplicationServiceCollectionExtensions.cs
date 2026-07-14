using Asp.Versioning;
using CustomerPayments.Api.Application.Interfaces;
using CustomerPayments.Api.Application.Services;
using CustomerPayments.Api.Interfaces.Caching;
using CustomerPayments.Api.Interfaces.Services;
using CustomerPayments.Api.Mappings;
using CustomerPayments.Api.Options;
using CustomerPayments.Api.Services;
using CustomerPayments.Api.Validators;
using FluentValidation;

namespace CustomerPayments.Api.Infrastructure.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<ApplicationOptions>()
            .Bind(configuration.GetSection(ApplicationOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        services
            .AddOptions<DemoUserOptions>()
            .Bind(configuration.GetSection(DemoUserOptions.SectionName))
            .ValidateDataAnnotations()
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.Password),
                "Demo user password is required.")
            .ValidateOnStart();

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddControllers();

        services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddMvc()
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<ICacheService, OutputCacheService>();  

        services.AddAutoMapper(config =>
        {
            config.AddProfile<MappingProfile>();
        });
        
        services.AddValidatorsFromAssemblyContaining<CreateCustomerRequestValidator>();

        return services;
    }
}
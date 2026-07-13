using Asp.Versioning;
using CustomerPayments.Api.DTOs;
using CustomerPayments.Api.Interfaces.Catching;
using CustomerPayments.Api.Interfaces.Services;
using CustomerPayments.Api.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;

namespace CustomerPayments.Api.Controllers;

[ApiController]
[Authorize]
[ApiVersion(1.0)]
[EnableRateLimiting("FixedWindowPolicy")]
[Route("api/v{version:apiVersion}/customers")]
public sealed class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly IValidator<CreateCustomerRequest> _createCustomerValidator;
    private readonly IValidator<UpdateCustomerRequest> _updateCustomerValidator;
    private readonly IValidator<DeactivateCustomerRequest> _deactivateCustomerValidator;
    private readonly IOutputCacheStore _outputCacheStore;
    private readonly ICacheService _cacheService;

    public CustomersController(
        ICustomerService customerService,
        IValidator<CreateCustomerRequest> createCustomerValidator,
        IValidator<UpdateCustomerRequest> updateCustomerValidator,
        IValidator<DeactivateCustomerRequest> deactivateCustomerValidator,
        IOutputCacheStore outputCacheStore,
        ICacheService cacheService)
    {
        _customerService = customerService;
        _createCustomerValidator = createCustomerValidator;
        _updateCustomerValidator = updateCustomerValidator;
        _deactivateCustomerValidator = deactivateCustomerValidator;
        _cacheService = cacheService;
        _outputCacheStore = outputCacheStore;
    }

    [HttpGet]
    [OutputCache(PolicyName = "CustomersReadPolicy")]
    public async Task<ActionResult<PagedResponse<CustomerDto>>> GetAllAsync(
        [FromQuery] PagedRequest request,
        CancellationToken cancellationToken)
    {
        var customers = await _customerService.GetAllAsync(
            request,
            cancellationToken);

        return Ok(customers);
    }

    [HttpGet("{id:guid}")]
    [OutputCache(PolicyName = "CustomerByIdReadPolicy")]
    public async Task<ActionResult<CustomerDto>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var customer = await _customerService.GetByIdAsync(
            id,
            cancellationToken);

        return Ok(customer);
    }

    [HttpPost]
    public async Task<ActionResult<CustomerDto>> CreateAsync(
        CreateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _createCustomerValidator.ValidateAsync(
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

        var createdCustomer = await _customerService.CreateAsync(
            request,
            cancellationToken);

        await _outputCacheStore.EvictByTagAsync(
            "customers-list",
            cancellationToken);

        await _outputCacheStore.EvictByTagAsync(
            "customers-by-id",
            cancellationToken);

        return Created(
            $"/api/v1/customers/{createdCustomer.Id}",
            createdCustomer);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CustomerDto>> UpdateAsync(
        Guid id,
        UpdateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _updateCustomerValidator.ValidateAsync(
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

        var updatedCustomer = await _customerService.UpdateAsync(
            id,
            request,
            cancellationToken);

        await _cacheService.InvalidateCustomersAsync(
            cancellationToken);

        return Ok(updatedCustomer);
    }

    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> DeactivateAsync(
        Guid id,
        [FromBody] DeactivateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _deactivateCustomerValidator.ValidateAsync(
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

        await _customerService.DeactivateAsync(
            id,
            request,
            cancellationToken);

        await _cacheService.InvalidateCustomersAsync(
            cancellationToken);

        return NoContent();
    }
}
using Asp.Versioning;
using CustomerPayments.Api.DTOs;
using CustomerPayments.Api.Interfaces.Services;
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
[Route("api/v{version:apiVersion}/customers/{customerId:guid}/payments")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly IValidator<CreatePaymentRequest> _validator;
    private readonly IOutputCacheStore _outputCacheStore;

    public PaymentsController(
        IPaymentService paymentService,
        IValidator<CreatePaymentRequest> validator,
        IOutputCacheStore outputCacheStore)
    {
        _paymentService = paymentService;
        _validator = validator;
        _outputCacheStore = outputCacheStore;
    }

    [HttpGet]
    [OutputCache(PolicyName = "PaymentsReadPolicy")]
    public async Task<ActionResult<IReadOnlyList<PaymentDto>>> GetByCustomerId(
        Guid customerId,
        CancellationToken cancellationToken)
    {
        var payments = await _paymentService.GetByCustomerIdAsync(
            customerId,
            cancellationToken);

        return Ok(payments);
    }

    [HttpPost]
    public async Task<ActionResult<PaymentDto>> Create(
        Guid customerId,
        CreatePaymentRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }

            return ValidationProblem(ModelState);
        }

        var createdPayment = await _paymentService.CreateAsync(
            customerId,
            request,
            cancellationToken);

        await _outputCacheStore.EvictByTagAsync(
            "payments",
            cancellationToken);

        return Created(
            $"/api/v1/customers/{customerId}/payments/{createdPayment.Id}",
            createdPayment);
    }
}
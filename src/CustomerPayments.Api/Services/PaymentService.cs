using AutoMapper;
using CustomerPayments.Api.Application.Interfaces;
using CustomerPayments.Api.Domain.Entities;
using CustomerPayments.Api.Domain.Enums;
using CustomerPayments.Api.DTOs;
using CustomerPayments.Api.Exceptions;
using CustomerPayments.Api.Interfaces.Repositories;
using CustomerPayments.Api.Interfaces.Services;

namespace CustomerPayments.Api.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<PaymentService> _logger;
    private readonly ICurrentUserContext _currentUserContext;

    public PaymentService(
        IPaymentRepository paymentRepository,
        ICustomerRepository customerRepository,
        IMapper mapper,
        ILogger<PaymentService> logger,
        ICurrentUserContext currentUserContext)
    {
        _paymentRepository = paymentRepository;
        _customerRepository = customerRepository;
        _mapper = mapper;
        _logger = logger;
        _currentUserContext = currentUserContext;
    }

    public async Task<IReadOnlyList<PaymentDto>> GetByCustomerIdAsync(
        Guid customerId,
        CancellationToken cancellationToken)
    {
        await EnsureCustomerExistsAsync(customerId, cancellationToken);

        var payments = await _paymentRepository.GetByCustomerIdAsync(
            customerId,
            cancellationToken);

        return _mapper.Map<IReadOnlyList<PaymentDto>>(payments);
    }

    public async Task<PaymentDto> CreateAsync(
        Guid customerId,
        CreatePaymentRequest request,
        CancellationToken cancellationToken)
    {
        await EnsureCustomerExistsAsync(customerId, cancellationToken);

        var payment = Payment.Create(
            customerId,
            request.PaymentDate,
            request.Amount,
            request.Method,
            request.Description,
            request.ExternalReference,
            _currentUserContext.Email
            );

        await _paymentRepository.AddAsync(payment, cancellationToken);

        _logger.LogInformation(
            "Payment {PaymentId} was created successfully for customer {CustomerId}.",
            payment.Id,
            customerId);

        return _mapper.Map<PaymentDto>(payment);
    }

    private async Task EnsureCustomerExistsAsync(
        Guid customerId,
        CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId, cancellationToken);

        if (customer is null)
        {
            _logger.LogWarning(
                "Customer {CustomerId} was not found while processing a payment operation.",
                customerId);

            throw new NotFoundException($"Customer with id {customerId} was not found.");
        }
    }
}

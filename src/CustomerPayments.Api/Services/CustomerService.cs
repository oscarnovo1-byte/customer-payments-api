using AutoMapper;
using CustomerPayments.Api.Application.Interfaces;
using CustomerPayments.Api.Domain.Entities;
using CustomerPayments.Api.DTOs;
using CustomerPayments.Api.Exceptions;
using CustomerPayments.Api.Interfaces.Repositories;
using CustomerPayments.Api.Interfaces.Services;

namespace CustomerPayments.Api.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CustomerService> _logger;
    private readonly ICurrentRequestContext _currentRequestContext;
    private readonly ICurrentUserContext _currentUserContext;

    public CustomerService(
        ICustomerRepository customerRepository,
        IMapper mapper,
        ILogger<CustomerService> logger,
        ICurrentRequestContext currentRequestContext,
        ICurrentUserContext currentUserContext)
    {
        _customerRepository = customerRepository;
        _mapper = mapper;
        _logger = logger;
        _currentRequestContext = currentRequestContext;
        _currentUserContext = currentUserContext;
    }

    public async Task<PagedResponse<CustomerDto>> GetAllAsync(
        PagedRequest request,
        CancellationToken cancellationToken)
    {
        var pageNumber = request.ValidPageNumber;
        var pageSize = request.ValidPageSize;

        var customers = await _customerRepository.GetPagedAsync(
            pageNumber,
            pageSize,
            request.Search,
            request.SortBy,
            request.SortDescending,
            request.IncludeInactive,
            cancellationToken);

        var totalCount = await _customerRepository.CountAsync(
            request.Search,
            request.IncludeInactive,
            cancellationToken);

        var customerDtos = _mapper.Map<IReadOnlyList<CustomerDto>>(customers);

        return new PagedResponse<CustomerDto>(
            customerDtos,
            pageNumber,
            pageSize,
            totalCount);
    }

    public async Task<CustomerDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(
            id,
            cancellationToken);

        if (customer is null)
        {
            throw new NotFoundException($"Customer with id '{id}' was not found.");
        }

        return _mapper.Map<CustomerDto>(customer);
    }

    public async Task<CustomerDto> CreateAsync(
        CreateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var emailExists = await _customerRepository.ExistsByEmailAsync(
            request.Email,
            cancellationToken);

        if (emailExists)
        {
            _logger.LogWarning(
                "Customer creation rejected because email {Email} already exists.",
                request.Email);

            throw new ConflictException("A customer with the same email already exists.");
        }

        var customer = Customer.Create(
            request.FirstName,
            request.LastName,
            request.Email,
            request.PhoneNumber,
            request.DocumentNumber,
            _currentUserContext.Email);

        await _customerRepository.AddAsync(customer, cancellationToken);

        _logger.LogInformation(
            "Customer {CustomerId} was created successfully.",
            customer.Id);

        _logger.LogInformation(
            "Creating customer. CorrelationId: {CorrelationId}",
            _currentRequestContext.CorrelationId);

        _logger.LogInformation(
            "Creating customer. UserId: {UserId}, Email: {Email}, Role: {Role}",
            _currentUserContext.UserId,
            _currentUserContext.Email,
            _currentUserContext.Role);

        return _mapper.Map<CustomerDto>(customer);
    }

    public async Task<CustomerDto> UpdateAsync(
        Guid id,
        UpdateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetTrackedByIdAsync(
            id,
            cancellationToken);

        if (customer is null)
        {
            throw new NotFoundException($"Customer with id '{id}' was not found.");
        }

        customer.UpdateContactInfo(
            request.FirstName,
            request.LastName,
            request.Email,
            request.PhoneNumber,
            request.DocumentNumber,
            request.Version,
            _currentUserContext.Email);

        await _customerRepository.UpdateAsync(
            customer,
            cancellationToken);

        return _mapper.Map<CustomerDto>(customer);
    }

    public async Task DeactivateAsync(
        Guid id,
        DeactivateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetTrackedByIdAsync(
            id,
            cancellationToken);

        if (customer is null)
        {
            throw new NotFoundException($"Customer with id '{id}' was not found.");
        }

        customer.Deactivate(
            request.Version,
            _currentUserContext.Email);

        await _customerRepository.UpdateAsync(
            customer,
            cancellationToken);
    }
}

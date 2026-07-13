using AutoMapper;
using CustomerPayments.Api.Application.Interfaces;
using CustomerPayments.Api.Domain.Entities;
using CustomerPayments.Api.DTOs;
using CustomerPayments.Api.Exceptions;
using CustomerPayments.Api.Interfaces.Repositories;
using CustomerPayments.Api.Mappings;
using CustomerPayments.Api.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CustomerPayments.UnitTests;

public class CustomerServiceTests
{
    private const string CurrentUserEmail = "admin@customerpayments.com";

    private readonly Mock<ICustomerRepository> _repositoryMock;
    private readonly Mock<ICurrentRequestContext> _currentRequestContextMock;
    private readonly Mock<ICurrentUserContext> _currentUserContextMock;
    private readonly CustomerService _service;

    public CustomerServiceTests()
    {
        _repositoryMock = new Mock<ICustomerRepository>();
        _currentRequestContextMock = new Mock<ICurrentRequestContext>();
        _currentUserContextMock = new Mock<ICurrentUserContext>();

        _currentUserContextMock
            .Setup(x => x.Email)
            .Returns(CurrentUserEmail);

        var mapperConfiguration = new MapperConfiguration(config =>
        {
            config.AddProfile<MappingProfile>();
        }, NullLoggerFactory.Instance);

        var mapper = mapperConfiguration.CreateMapper();

        _service = new CustomerService(
            _repositoryMock.Object,
            mapper,
            NullLogger<CustomerService>.Instance,
            _currentRequestContextMock.Object,
            _currentUserContextMock.Object);
    }

    #region Create customer tests

    [Fact]
    public async Task CreateAsync_ShouldCreateCustomer_WhenEmailDoesNotExist()
    {
        var request = CreateCustomerRequest();

        _repositoryMock
            .Setup(x => x.ExistsByEmailAsync(
                request.Email,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _service.CreateAsync(
            request,
            CancellationToken.None);

        result.FirstName.Should().Be(request.FirstName);
        result.LastName.Should().Be(request.LastName);
        result.Email.Should().Be(request.Email);
        result.PhoneNumber.Should().Be(request.PhoneNumber);
        result.DocumentNumber.Should().Be(request.DocumentNumber);
        result.IsActive.Should().BeTrue();
        result.CreatedBy.Should().Be(CurrentUserEmail);

        _repositoryMock.Verify(
            x => x.AddAsync(
                It.Is<Customer>(customer =>
                    customer.FirstName == request.FirstName &&
                    customer.LastName == request.LastName &&
                    customer.Email == request.Email &&
                    customer.PhoneNumber == request.PhoneNumber &&
                    customer.DocumentNumber == request.DocumentNumber &&
                    customer.CreatedBy == CurrentUserEmail),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowConflictException_WhenEmailAlreadyExists()
    {
        var request = CreateCustomerRequest(
            phoneNumber: null,
            documentNumber: null);

        _repositoryMock
            .Setup(x => x.ExistsByEmailAsync(
                request.Email,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var act = async () => await _service.CreateAsync(
            request,
            CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();

        _repositoryMock.Verify(
            x => x.AddAsync(
                It.IsAny<Customer>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region Get all customers tests

    [Fact]
    public async Task GetAllAsync_ShouldReturnPagedResponse()
    {
        var request = new PagedRequest
        {
            PageNumber = 1,
            PageSize = 2,
            Search = "john",
            SortBy = "firstName",
            SortDirection = "desc",
            IncludeInactive = true
        };

        var customers = new List<Customer>
        {
            CreateCustomer(),
            CreateCustomer(
                firstName: "Johnny",
                lastName: "Smith",
                email: "johnny.smith@test.com")
        };

        _repositoryMock
            .Setup(x => x.GetPagedAsync(
                request.PageNumber,
                request.PageSize,
                request.Search,
                request.SortBy,
                true,
                request.IncludeInactive,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(customers);

        _repositoryMock
            .Setup(x => x.CountAsync(
                request.Search,
                request.IncludeInactive,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        var result = await _service.GetAllAsync(
            request,
            CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(2);
        result.TotalCount.Should().Be(5);
        result.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task GetAllAsync_ShouldNormalizeInvalidPaginationValues()
    {
        var request = new PagedRequest
        {
            PageNumber = -1,
            PageSize = 500
        };

        _repositoryMock
            .Setup(x => x.GetPagedAsync(
                1,
                100,
                null,
                null,
                false,
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        _repositoryMock
            .Setup(x => x.CountAsync(
                null,
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var result = await _service.GetAllAsync(
            request,
            CancellationToken.None);

        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(100);
    }

    #endregion

    #region Get customer by id tests

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCustomer_WhenCustomerExists()
    {
        var customer = CreateCustomer();

        _repositoryMock
            .Setup(x => x.GetByIdAsync(
                customer.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        var result = await _service.GetByIdAsync(
            customer.Id,
            CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(customer.Id);
        result.FirstName.Should().Be(customer.FirstName);
        result.LastName.Should().Be(customer.LastName);
        result.Email.Should().Be(customer.Email);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrowNotFoundException_WhenCustomerDoesNotExist()
    {
        var customerId = Guid.NewGuid();

        _repositoryMock
            .Setup(x => x.GetByIdAsync(
                customerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        var act = async () => await _service.GetByIdAsync(
            customerId,
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region Update customer tests

    [Fact]
    public async Task UpdateAsync_ShouldUpdateCustomer_WhenCustomerExistsAndVersionMatches()
    {
        var customer = CreateCustomer();
        var request = UpdateCustomerRequest(customer.Version);

        _repositoryMock
            .Setup(x => x.GetTrackedByIdAsync(
                customer.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        var result = await _service.UpdateAsync(
            customer.Id,
            request,
            CancellationToken.None);

        result.FirstName.Should().Be(request.FirstName);
        result.LastName.Should().Be(request.LastName);
        result.Email.Should().Be(request.Email);
        result.PhoneNumber.Should().Be(request.PhoneNumber);
        result.DocumentNumber.Should().Be(request.DocumentNumber);
        result.Version.Should().Be(2);
        result.UpdatedBy.Should().Be(CurrentUserEmail);

        _repositoryMock.Verify(
            x => x.UpdateAsync(
                customer,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowNotFoundException_WhenCustomerDoesNotExist()
    {
        var customerId = Guid.NewGuid();
        var request = UpdateCustomerRequest(version: 1);

        _repositoryMock
            .Setup(x => x.GetTrackedByIdAsync(
                customerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        var act = async () => await _service.UpdateAsync(
            customerId,
            request,
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();

        _repositoryMock.Verify(
            x => x.UpdateAsync(
                It.IsAny<Customer>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowConflictException_WhenVersionDoesNotMatch()
    {
        var customer = CreateCustomer();
        var request = UpdateCustomerRequest(version: 99);

        _repositoryMock
            .Setup(x => x.GetTrackedByIdAsync(
                customer.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        var act = async () => await _service.UpdateAsync(
            customer.Id,
            request,
            CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();

        _repositoryMock.Verify(
            x => x.UpdateAsync(
                It.IsAny<Customer>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region Deactivate customer tests

    [Fact]
    public async Task DeactivateAsync_ShouldDeactivateCustomer_WhenCustomerExistsAndVersionMatches()
    {
        var customer = CreateCustomer();
        var request = DeactivateCustomerRequest(version: customer.Version);

        _repositoryMock
            .Setup(x => x.GetTrackedByIdAsync(
                customer.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        await _service.DeactivateAsync(
            customer.Id,
            request,
            CancellationToken.None);

        customer.IsActive.Should().BeFalse();
        customer.Version.Should().Be(2);
        customer.UpdatedBy.Should().Be(CurrentUserEmail);

        _repositoryMock.Verify(
            x => x.UpdateAsync(
                customer,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowNotFoundException_WhenCustomerDoesNotExist()
    {
        var customerId = Guid.NewGuid();
        var request = DeactivateCustomerRequest(version: 1);

        _repositoryMock
            .Setup(x => x.GetTrackedByIdAsync(
                customerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        var act = async () => await _service.DeactivateAsync(
            customerId,
            request,
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();

        _repositoryMock.Verify(
            x => x.UpdateAsync(
                It.IsAny<Customer>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowConflictException_WhenVersionDoesNotMatch()
    {
        var customer = CreateCustomer();
        var request = DeactivateCustomerRequest(version: 99);

        _repositoryMock
            .Setup(x => x.GetTrackedByIdAsync(
                customer.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        var act = async () => await _service.DeactivateAsync(
            customer.Id,
            request,
            CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();

        _repositoryMock.Verify(
            x => x.UpdateAsync(
                It.IsAny<Customer>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region Private methods

    private static CreateCustomerRequest CreateCustomerRequest(
        string firstName = "John",
        string lastName = "Doe",
        string email = "john.doe@test.com",
        string? phoneNumber = "123456789",
        string? documentNumber = "DNI123")
    {
        return new CreateCustomerRequest(
            firstName,
            lastName,
            email,
            phoneNumber,
            documentNumber);
    }

    private static UpdateCustomerRequest UpdateCustomerRequest(
        int version,
        string firstName = "Jane",
        string lastName = "Smith",
        string email = "jane.smith@test.com",
        string? phoneNumber = "987654321",
        string? documentNumber = "DOC999")
    {
        return new UpdateCustomerRequest(
            firstName,
            lastName,
            email,
            phoneNumber,
            documentNumber,
            version);
    }

    private static DeactivateCustomerRequest DeactivateCustomerRequest(
        int version = 1)
    {
        return new DeactivateCustomerRequest(version);
    }

    private static Customer CreateCustomer(
        string firstName = "John",
        string lastName = "Doe",
        string email = "john.doe@test.com",
        string? phoneNumber = "123456789",
        string? documentNumber = "DNI123",
        string createdBy = "testuser")
    {
        return Customer.Create(
            firstName,
            lastName,
            email,
            phoneNumber,
            documentNumber,
            createdBy);
    }

    #endregion
}
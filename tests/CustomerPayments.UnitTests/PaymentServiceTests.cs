using AutoMapper;
using CustomerPayments.Api.Application.Interfaces;
using CustomerPayments.Api.Domain.Entities;
using CustomerPayments.Api.Domain.Enums;
using CustomerPayments.Api.DTOs;
using CustomerPayments.Api.Exceptions;
using CustomerPayments.Api.Interfaces.Repositories;
using CustomerPayments.Api.Mappings;
using CustomerPayments.Api.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CustomerPayments.UnitTests;

public class PaymentServiceTests
{
    private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
    private readonly Mock<ICustomerRepository> _customerRepositoryMock;
    private readonly PaymentService _service;
    private readonly Mock<ICurrentUserContext> _currentUserContextMock;

    public PaymentServiceTests()
    {
        _paymentRepositoryMock = new Mock<IPaymentRepository>();
        _customerRepositoryMock = new Mock<ICustomerRepository>();
        _currentUserContextMock = new Mock<ICurrentUserContext>();

        var mapperConfiguration = new MapperConfiguration(config =>
        {
            config.AddProfile<MappingProfile>();
        }, NullLoggerFactory.Instance);

        var mapper = mapperConfiguration.CreateMapper();

        _currentUserContextMock
            .Setup(x => x.Email)
            .Returns("admin@customerpayments.com");

        _service = new PaymentService(
            _paymentRepositoryMock.Object,
            _customerRepositoryMock.Object,
            mapper,
            NullLogger<PaymentService>.Instance,
            _currentUserContextMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreatePayment_WhenCustomerExists()
    {
        var customerId = Guid.NewGuid();

        var customer = Customer.Create(
            "John",
            "Doe",
            "john.doe@test.com",
            "541234567890",
            "1234567890",
            "testuser"
        );

        var request = new CreatePaymentRequest(
            DateOnly.FromDateTime(DateTime.UtcNow),
            100,
            PaymentMethod.BankTransfer,
            "Test payment",
            "EXT-123");

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(
                customerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        var result = await _service.CreateAsync(
            customerId,
            request,
            CancellationToken.None);

        result.CustomerId.Should().Be(customerId);
        result.Amount.Should().Be(100);
        result.Method.Should().Be(PaymentMethod.BankTransfer);
        result.Description.Should().Be("Test payment");
        result.ExternalReference.Should().Be("EXT-123");

        _paymentRepositoryMock.Verify(
            x => x.AddAsync(
                It.Is<Payment>(payment =>
                    payment.CustomerId == customerId &&
                    payment.Amount == request.Amount &&
                    payment.Method == request.Method &&
                    payment.Description == request.Description &&
                    payment.ExternalReference == request.ExternalReference),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowNotFoundException_WhenCustomerDoesNotExist()
    {
        var customerId = Guid.NewGuid();

        var request = new CreatePaymentRequest(
            DateOnly.FromDateTime(DateTime.UtcNow),
            100,
            PaymentMethod.Cash,
            null,
            null);

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(
                customerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        var act = async () => await _service.CreateAsync(
            customerId,
            request,
            CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();

        _paymentRepositoryMock.Verify(
            x => x.AddAsync(
                It.IsAny<Payment>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
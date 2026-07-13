namespace CustomerPayments.Api.Application.Interfaces;

public interface ICurrentRequestContext
{
    string? CorrelationId { get; }
}
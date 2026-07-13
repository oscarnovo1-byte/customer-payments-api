namespace CustomerPayments.Api.DTOs;

public sealed record PagedRequest
{
    private const int MaxPageSize = 100;

    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 10;

    public string? Search { get; init; }

    public int ValidPageNumber => PageNumber < 1 ? 1 : PageNumber;

    public int ValidPageSize => PageSize switch
    {
        < 1 => 10,
        > MaxPageSize => MaxPageSize,
        _ => PageSize
    };

    public string? SortBy { get; init; }

    public string? SortDirection { get; init; }

    public bool SortDescending =>
        SortDirection?.Equals("desc", StringComparison.OrdinalIgnoreCase) == true;

    public bool IncludeInactive { get; init; } = false;
}
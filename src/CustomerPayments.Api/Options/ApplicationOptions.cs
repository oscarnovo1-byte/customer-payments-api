namespace CustomerPayments.Api.Options;

public sealed class ApplicationOptions
{
    public const string SectionName = "Application";

    public required string Name { get; init; }

    public required string Version { get; init; }

    public required string Description { get; init; }
}

public abstract class BaseEntity
{
    public Guid Id { get; protected set; }

    public DateTime CreatedAtUtc { get; protected set; }

    public DateTime? UpdatedAtUtc { get; protected set; }

    public string? CreatedBy { get; protected set; }

    public string? UpdatedBy { get; protected set; }

    public int Version { get; protected set; } = 1;
}
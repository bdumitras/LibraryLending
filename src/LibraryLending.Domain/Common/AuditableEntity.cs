namespace LibraryLending.Domain.Common;

public abstract class AuditableEntity : Entity
{
    public DateTime CreatedAtUtc { get; protected set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; protected set; } = DateTime.UtcNow;

    protected void Touch()
    {
        UpdatedAtUtc = DateTime.UtcNow;
    }
}

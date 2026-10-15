namespace NOQTA.Invoice.Domain.Common;

public abstract class Entity
{
    public Guid Id { get; private set; }

    protected Entity()
    {
        // Required by EF Core materialization.
    }

    protected Entity(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException(
                "Entity ID cannot be empty.",
                nameof(id));

        Id = id;
    }
}
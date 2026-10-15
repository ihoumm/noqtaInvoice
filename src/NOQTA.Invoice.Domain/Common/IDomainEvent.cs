namespace NOQTA.Invoice.Domain.Common;

public interface IDomainEvent
{
    DateTimeOffset OccurredOn { get; }
}
namespace HorizonSurvey.Domain.Errors;

public enum InventoryErrorKind
{
    InvalidInput,
    NotFound,
    Conflict
}

public sealed class InventoryException(string message, InventoryErrorKind kind) : Exception(message)
{
    public InventoryErrorKind Kind { get; } = kind;
}

using HorizonSurvey.Domain.Errors;

namespace HorizonSurvey.Domain.Models;

public sealed class PooledStock
{
    private PooledStock() { }

    public PooledStock(string name, int totalQuantity)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InventoryException("Stock name is required.", InventoryErrorKind.InvalidInput);
        if (totalQuantity < 0)
            throw new InventoryException("Total quantity cannot be negative.", InventoryErrorKind.InvalidInput);

        Id = Guid.NewGuid();
        Name = name.Trim();
        TotalQuantity = totalQuantity;
        AvailableQuantity = totalQuantity;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public int TotalQuantity { get; private set; }
    public int AvailableQuantity { get; private set; }

    public void Checkout(int quantity)
    {
        if (quantity <= 0)
            throw new InventoryException("Quantity must be greater than zero.", InventoryErrorKind.InvalidInput);
        if (quantity > AvailableQuantity)
            throw new InventoryException("Requested quantity is not available.", InventoryErrorKind.Conflict);

        AvailableQuantity -= quantity;
    }

    public void Return(int quantity)
    {
        if (quantity <= 0)
            throw new InventoryException("Return quantity must be greater than zero.", InventoryErrorKind.InvalidInput);
        if (AvailableQuantity + quantity > TotalQuantity)
            throw new InventoryException("Return quantity exceeds the outstanding checkout quantity.", InventoryErrorKind.Conflict);

        AvailableQuantity += quantity;
    }
}

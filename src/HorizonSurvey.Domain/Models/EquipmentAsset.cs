namespace HorizonSurvey.Domain.Models;

public sealed class EquipmentAsset
{
    private EquipmentAsset() { }

    public EquipmentAsset(string name, Guid categoryId)
    {
        Id = Guid.NewGuid();
        Name = name;
        CategoryId = categoryId;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public Guid? CategoryId { get; private set; }
    public EquipmentCategory? Category { get; private set; }
}

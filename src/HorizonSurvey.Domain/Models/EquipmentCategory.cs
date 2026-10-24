namespace HorizonSurvey.Domain.Models;

public sealed class EquipmentCategory
{
    private EquipmentCategory() { }

    public EquipmentCategory(string name)
    {
        Id = Guid.NewGuid();
        Name = name;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;

    public void Rename(string name) => Name = name;
    public bool RequiresApproval => string.Equals(Name, "Drone", StringComparison.OrdinalIgnoreCase);
}

namespace CampusEquipment.Core.DTOs;

public class EquipmentDto
{
    public int EquipmentId { get; set; }

    public string AssetCode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string? Brand { get; set; }

    public string? Model { get; set; }

    public DateOnly? PurchaseDate { get; set; }

    public string Status { get; set; } = string.Empty;

    public int DepartmentId { get; set; }

    public string DepartmentName { get; set; } = string.Empty;
}

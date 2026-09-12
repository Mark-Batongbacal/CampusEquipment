using System.ComponentModel.DataAnnotations;

namespace CampusEquipment.Core.DTOs;

public class CreateEquipmentDto
{
    [Required]
    [StringLength(50)]
    public string AssetCode { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Category { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Brand { get; set; }

    [StringLength(100)]
    public string? Model { get; set; }

    [DataType(DataType.Date)]
    public DateOnly? PurchaseDate { get; set; }

    [Required]
    [StringLength(50)]
    [RegularExpression("^(Available|Assigned|UnderMaintenance|Retired)$",
        ErrorMessage = "Status must be Available, Assigned, UnderMaintenance, or Retired.")]
    public string Status { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Select a valid department.")]
    public int DepartmentId { get; set; }
}

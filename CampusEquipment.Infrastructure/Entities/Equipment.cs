using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CampusEquipment.Infrastructure.Entities;

[Index("AssetCode", Name = "UQ_Equipment_AssetCode", IsUnique = true)]
public partial class Equipment
{
    [Key]
    public int EquipmentId { get; set; }

    [StringLength(50)]
    public string AssetCode { get; set; } = null!;

    [StringLength(100)]
    public string Name { get; set; } = null!;

    [StringLength(100)]
    public string Category { get; set; } = null!;

    [StringLength(100)]
    public string? Brand { get; set; }

    [StringLength(100)]
    public string? Model { get; set; }

    public DateOnly? PurchaseDate { get; set; }

    [StringLength(50)]
    public string Status { get; set; } = null!;

    public int DepartmentId { get; set; }

    [ForeignKey("DepartmentId")]
    [InverseProperty("Equipment")]
    public virtual Department Department { get; set; } = null!;
}

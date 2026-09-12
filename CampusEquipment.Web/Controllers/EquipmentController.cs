using System.ComponentModel.DataAnnotations;
using CampusEquipment.Core.DTOs;
using CampusEquipment.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CampusEquipment.Web.Controllers;

public class EquipmentController : Controller
{
    private readonly IEquipmentService _equipmentService;
    private readonly IDepartmentService _departmentService;

    public EquipmentController(IEquipmentService equipmentService,
        IDepartmentService departmentService)
    {
        _equipmentService = equipmentService;
        _departmentService = departmentService;
    }

    public async Task<IActionResult> Index(string? search, string? category,
        string? status, int? departmentId)
    {
        ViewBag.Search = search;
        ViewBag.Category = category;
        ViewBag.Status = status;
        ViewBag.DepartmentId = departmentId;
        await LoadDepartmentsAsync(departmentId);

        var equipment = await _equipmentService.GetAllAsync(search, category, status, departmentId);
        return View(equipment);
    }

    public async Task<IActionResult> Details(int id)
    {
        var equipment = await _equipmentService.GetByIdAsync(id);
        return equipment is null ? NotFound() : View(equipment);
    }

    public async Task<IActionResult> Create()
    {
        await LoadDepartmentsAsync();
        return View(new CreateEquipmentDto { Status = "Available" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateEquipmentDto dto)
    {
        if (!ModelState.IsValid)
        {
            await LoadDepartmentsAsync(dto.DepartmentId);
            return View(dto);
        }

        try
        {
            await _equipmentService.CreateAsync(dto);
            TempData["SuccessMessage"] = "Equipment created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (ValidationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            await LoadDepartmentsAsync(dto.DepartmentId);
            return View(dto);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        var equipment = await _equipmentService.GetByIdAsync(id);
        if (equipment is null)
            return NotFound();

        await LoadDepartmentsAsync(equipment.DepartmentId);
        return View(new UpdateEquipmentDto
        {
            EquipmentId = equipment.EquipmentId,
            AssetCode = equipment.AssetCode,
            Name = equipment.Name,
            Category = equipment.Category,
            Brand = equipment.Brand,
            Model = equipment.Model,
            PurchaseDate = equipment.PurchaseDate,
            Status = equipment.Status,
            DepartmentId = equipment.DepartmentId
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateEquipmentDto dto)
    {
        if (id != dto.EquipmentId)
            return BadRequest();

        if (!ModelState.IsValid)
        {
            await LoadDepartmentsAsync(dto.DepartmentId);
            return View(dto);
        }

        try
        {
            if (!await _equipmentService.UpdateAsync(id, dto))
                return NotFound();

            TempData["SuccessMessage"] = "Equipment updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (ValidationException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            await LoadDepartmentsAsync(dto.DepartmentId);
            return View(dto);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Retire(int id)
    {
        if (!await _equipmentService.RetireAsync(id))
            return NotFound();

        TempData["SuccessMessage"] = "Equipment retired successfully.";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadDepartmentsAsync(int? selectedId = null)
    {
        var departments = await _departmentService.GetAllAsync();
        ViewBag.Departments = new SelectList(departments, "DepartmentId", "Name", selectedId);
    }
}

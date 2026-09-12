namespace CampusEquipment.Api.Models;

public record ApiResponse<T>(bool Success, string Message, T? Data);

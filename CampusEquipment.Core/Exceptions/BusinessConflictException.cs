namespace CampusEquipment.Core.Exceptions;

// Services can throw this for conflicts such as duplicate asset codes or forbidden status transitions.
public class BusinessConflictException(string message) : Exception(message);

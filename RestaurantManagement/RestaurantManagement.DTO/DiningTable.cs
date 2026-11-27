namespace RestaurantManagement.DTO;

public sealed record DiningTable(
    string Code,
    string Area,
    int Capacity,
    string Status);

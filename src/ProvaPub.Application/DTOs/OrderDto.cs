using ProvaPub.Domain.Entities;

namespace ProvaPub.Application.DTOs;

public sealed record OrderDto(
    int Id,
    decimal Value,
    int? CustomerId,
    DateTime OrderDate)
{
    public static implicit operator OrderDto?(Order? order)
    {
        if (order is null)
            return null;

        var brZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
        var horaBrasilia = TimeZoneInfo.ConvertTimeFromUtc(order.OrderDate, brZone);

        return new OrderDto(
            order.Id,
            order.Value,
            order.CustomerId,
            horaBrasilia);
    }
}
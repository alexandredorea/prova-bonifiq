namespace ProvaPub.Application.Commons.Middlewares;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
    DateTime LocalNow { get; }
    TimeZoneInfo TimeZone { get; }
}

internal sealed class BrazilDateTimeProvider(TimeProvider timeProvider) : IDateTimeProvider
{
    private readonly TimeZoneInfo _timeZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");

    public DateTime UtcNow => timeProvider.GetUtcNow().UtcDateTime;

    public DateTime LocalNow => TimeZoneInfo.ConvertTimeFromUtc(UtcNow, _timeZone);

    public TimeZoneInfo TimeZone => _timeZone;
}
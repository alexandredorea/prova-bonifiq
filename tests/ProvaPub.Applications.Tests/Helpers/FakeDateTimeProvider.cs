using ProvaPub.Application.Commons.Middlewares;

namespace ProvaPub.Applications.Tests.Helpers;

public sealed class FakeDateTimeProvider(FakeTimeProvider timeProvider, TimeZoneInfo timeZone) : IDateTimeProvider
{
    public DateTime UtcNow => timeProvider.GetUtcNow().UtcDateTime;

    public DateTime LocalNow => TimeZoneInfo.ConvertTimeFromUtc(UtcNow, timeZone);

    public TimeZoneInfo TimeZone => timeZone;
}
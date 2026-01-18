namespace ProvaPub.Applications.Tests.Helpers;

public sealed class FakeTimeProvider : TimeProvider
{
    private DateTimeOffset _utcNow;

    public FakeTimeProvider(DateTime utcNow)
    {
        if (utcNow.Kind != DateTimeKind.Utc)
            throw new ArgumentException("DateTime precisa ser UTC", nameof(utcNow));

        _utcNow = new DateTimeOffset(utcNow, TimeSpan.Zero);
    }

    public override DateTimeOffset GetUtcNow() => _utcNow;

    public void SetUtcNow(DateTime utcNow)
    {
        if (utcNow.Kind != DateTimeKind.Utc)
            throw new ArgumentException("DateTime precisa ser UTC", nameof(utcNow));

        _utcNow = new DateTimeOffset(utcNow, TimeSpan.Zero);
    }

    public void AdvanceBy(TimeSpan timeSpan)
    {
        _utcNow = _utcNow.Add(timeSpan);
    }
}
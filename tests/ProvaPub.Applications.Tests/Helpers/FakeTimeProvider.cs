namespace ProvaPub.Applications.Tests.Helpers;

public sealed class FakeTimeProvider(DateTime utcNow) : TimeProvider
{
    private DateTimeOffset _utcNow = new DateTimeOffset(utcNow, TimeSpan.Zero);

    public override DateTimeOffset GetUtcNow() => _utcNow;

    public void SetUtcNow(DateTime utcNow)
    {
        _utcNow = new DateTimeOffset(utcNow, TimeSpan.Zero);
    }

    public void AdvanceBy(TimeSpan timeSpan)
    {
        _utcNow = _utcNow.Add(timeSpan);
    }
}
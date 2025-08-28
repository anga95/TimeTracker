using TimeTracker.Services;

namespace TimeTracker.Tests.Services;

[TestFixture]
public class AiSummaryStateServiceTests
{
    private AiSummaryStateService _service;
    private bool _eventFired;

    [SetUp]
    public void Setup()
    {
        _service = new AiSummaryStateService();
        _eventFired = false;
        _service.OnChange += () => _eventFired = true;
    }

    [Test]
    public void SetAiSummary_UpdatesValueAndTriggersEvent()
    {
        var summary = "Test summary";

        _service.SetAiSummary(summary);

        Assert.That(_service.AiSummary, Is.EqualTo(summary));
        Assert.That(_eventFired, Is.True);
    }

    [Test]
    public void ClearAiSummary_ClearsValueAndTriggersEvent()
    {
        _service.SetAiSummary("something");
        _eventFired = false;

        _service.ClearAiSummary();

        Assert.That(_service.AiSummary, Is.Empty);
        Assert.That(_eventFired, Is.True);
    }
}

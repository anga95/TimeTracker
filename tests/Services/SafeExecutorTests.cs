using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Net;
using Moq;
using TimeTracker.Services;

namespace TimeTracker.Tests.Services;

[TestFixture]
public class SafeExecutorTests
{
    private Mock<IErrorHandlingService> _errorHandlerMock = null!;
    private SafeExecutor _executor = null!;

    [SetUp]
    public void Setup()
    {
        _errorHandlerMock = new Mock<IErrorHandlingService>();
        _executor = new SafeExecutor(_errorHandlerMock.Object);
    }

    [Test]
    public async Task ExecuteAsync_NoException_RunsAction()
    {
        bool ran = false;

        await _executor.ExecuteAsync(() => { ran = true; return Task.CompletedTask; });

        Assert.That(ran, Is.True);
        _errorHandlerMock.VerifyNoOtherCalls();
    }

    private class TestDbException : DbException
    {
        public TestDbException(string message) : base(message) { }
    }

    [Test]
    public async Task ExecuteAsync_DbException_CallsDatabaseHandler()
    {
        var ex = new TestDbException("db fail");

        await _executor.ExecuteAsync(() => throw ex);

        _errorHandlerMock.Verify(e => e.HandleDatabaseErrorAsync(ex, It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_HttpRequestException_CallsApiHandler()
    {
        HttpResponseMessage? captured = null;
        _errorHandlerMock
            .Setup(e => e.HandleApiErrorAsync(It.IsAny<HttpResponseMessage>(), It.IsAny<string>()))
            .Callback<HttpResponseMessage, string>((resp, _) => captured = resp)
            .Returns(Task.CompletedTask);

        var httpEx = new HttpRequestException("not found", null, HttpStatusCode.NotFound);

        await _executor.ExecuteAsync(() => throw httpEx);

        Assert.That(captured, Is.Not.Null);
        Assert.That(captured!.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        Assert.That(captured.ReasonPhrase, Is.EqualTo("not found"));
    }

    [Test]
    public async Task ExecuteAsync_ValidationException_CallsValidationHandler()
    {
        var ex = new ValidationException("bad");

        await _executor.ExecuteAsync(() => throw ex);

        _errorHandlerMock.Verify(e => e.HandleValidationErrorAsync("bad", It.IsAny<string>(), It.IsAny<IDictionary<string, string[]>>()), Times.Once);
    }

    [Test]
    public async Task ExecuteAsync_GeneralException_CallsExceptionHandler()
    {
        var ex = new InvalidOperationException("oops");

        await _executor.ExecuteAsync(() => throw ex);

        _errorHandlerMock.Verify(e => e.HandleExceptionAsync(ex, It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task ExecuteAsyncT_OnException_ReturnsFallback()
    {
        var result = await _executor.ExecuteAsync<int>(() => throw new Exception(), () => 42);

        Assert.That(result, Is.EqualTo(42));
        _errorHandlerMock.Verify(e => e.HandleExceptionAsync(It.IsAny<Exception>(), It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task ExecuteListAsync_OnException_ReturnsEmptyList()
    {
        var list = await _executor.ExecuteListAsync<int>(() => throw new Exception());

        Assert.That(list, Is.Empty);
        _errorHandlerMock.Verify(e => e.HandleExceptionAsync(It.IsAny<Exception>(), It.IsAny<string>()), Times.Once);
    }
}

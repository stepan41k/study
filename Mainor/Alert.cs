using Moq;
using Xunit;

public interface IMessageService
{
    void Send(string text);
}

public class AlertService
{
    private readonly IMessageService _messageService;

    public AlertService(IMessageService messageService)
    {
        _messageService = messageService;
    }

    public void SendAlert(string message)
    {
        _messageService.Send(message);
    }
}

public class AlertServiceTests
{
    [Fact]
    public void SendAlert_HappyPath()
    {
        var mockMessageService = new Mock<IMessageService>();

        var alertService = new AlertService(mockMessageService.Object);

        string testMessage = "Some test message";

        alertService.SendAlert(testMessage);

        mockMessageService.Verify(
            service => service.Send(testMessage),
            Times.Once());
    }
}

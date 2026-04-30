using Moq;
using Xunit;

public interface ISmsSender
{
    void SendSms(string phone, string message);
}

public class SmsNotificationService
{
    private readonly ISmsSender _smsSender;

    public SmsNotificationService(ISmsSender smsSender)
    {
        _smsSender = smsSender;
    }

    public void Notify(string phone)
    {
        _smsSender.SendSms(phone, "Your OTP 1234");
    }
}

public class SmsNotificationServiceTests
{
    [Fact]
    public void SendSms_WithCorrectPhoneAndMessageOnce()
    {
        var mockSmsSender = new Mock<ISmsSender>();
        var service = new SmsNotificationService(mockSmsSender.Object);

        string testPhone = "+79990000000";
        string expectedMessage = "Your OTP 1234";

        service.Notify(testPhone);

        mockSmsSender.Verify(
            sender => sender.SendSms(testPhone, expectedMessage),
            Times.Once());
    }
}

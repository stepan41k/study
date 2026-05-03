using Moq;
using Xunit;

public interface IAccessService
{
    bool HasAccess(string user);
}

public class SecureService
{
    private readonly IAccessService _accessService;

    public SecureService(IAccessService accessService)
    {
        _accessService = accessService;
    }

    public bool Execute(string user)
    {
        if (!_accessService.HasAccess(user))
            return false;

        return true;
    }
}

public class SecureServiceTests
{
    [Fact]
    public void Execute_UserHasAccess_ReturnsTrue()
    {
        var mockAccess = new Mock<IAccessService>();
        var user = "admin";

        mockAccess.Setup(s => s.HasAccess(user)).Returns(true);

        var service = new SecureService(mockAccess.Object);

        var result = service.Execute(user);

        Assert.True(result, "Method Execute must return true if access allowed");
    }

    [Fact]
    public void Execute_UserHasNoAccess_ReturnsFalse()
    {
        var mockAccess = new Mock<IAccessService>();
        var user = "guest";

        mockAccess.Setup(s => s.HasAccess(user)).Returns(false);

        var service = new SecureService(mockAccess.Object);

        var result = service.Execute(user);

        Assert.False(result, "Method Execute must return true if access allowed");
    }

    [Fact]
    public void Execute_UserHasNoAccess_NoAdditionalActions()
    {
        var mockAccess = new Mock<IAccessService>();
        var user = "restricted_user";
        mockAccess.Setup(s => s.HasAccess(user)).Returns(false);
        var service = new SecureService(mockAccess.Object);

        service.Execute(user);

        mockAccess.Verify(s => s.HasAccess(user), Times.Once());

        mockAccess.Verify(s => s.HasAccess(It.Is<string>(u => u != user)), Times.Never());

        mockAccess.VerifyNoOtherCalls();
    }
}

using Moq;
using Xunit;

public interface IInventoryService { bool IsAvailable(int productId); }
public interface IPaymentService { bool Pay(decimal amount); }
public interface IOrderRepository { void Save(int productId, decimal amount); }
public interface INotificationService { void Send(string message); }

public class OrderCheckoutService
{
    private readonly IInventoryService _inventory;
    private readonly IPaymentService _payment;
    private readonly IOrderRepository _repository;
    private readonly INotificationService _notification;

    public OrderCheckoutService(IInventoryService inventory, IPaymentService payment, 
                               IOrderRepository repository, INotificationService notification)
    {
        _inventory = inventory;
        _payment = payment;
        _repository = repository;
        _notification = notification;
    }

    public bool Checkout(int productId, decimal amount)
    {
        if (!_inventory.IsAvailable(productId)) return false;
        if (!_payment.Pay(amount)) return false;

        _repository.Save(productId, amount);
        _notification.Send("Order completed");
        return true;
    }
}

public class OrderCheckoutServiceTests
{
    private readonly Mock<IInventoryService> _inventoryMock = new();
    private readonly Mock<IPaymentService> _paymentMock = new();
    private readonly Mock<IOrderRepository> _repositoryMock = new();
    private readonly Mock<INotificationService> _notificationMock = new();
    private readonly OrderCheckoutService _service;

    public OrderCheckoutServiceTests()
    {
        _service = new OrderCheckoutService(
            _inventoryMock.Object, 
            _paymentMock.Object, 
            _repositoryMock.Object, 
            _notificationMock.Object);
    }

    [Fact]
    public void Checkout_Success_ReturnsTrueAndCallsAllMethods()
    {
        int productId = 1;
        decimal amount = 100m;
        _inventoryMock.Setup(x => x.IsAvailable(productId)).Returns(true);
        _paymentMock.Setup(x => x.Pay(amount)).Returns(true);

        var result = _service.Checkout(productId, amount);

        Assert.True(result);
        _inventoryMock.Verify(x => x.IsAvailable(productId), Times.Once);
        _paymentMock.Verify(x => x.Pay(amount), Times.Once);
        _repositoryMock.Verify(x => x.Save(productId, amount), Times.Once);
        _notificationMock.Verify(x => x.Send("Order completed"), Times.Once);
    }

    [Fact]
    public void Checkout_InventoryNotAvailable_ReturnsFalseAndStopsExecution()
    {
        int productId = 1;
        decimal amount = 100m;
        _inventoryMock.Setup(x => x.IsAvailable(productId)).Returns(false);

        var result = _service.Checkout(productId, amount);

        Assert.False(result);
        _paymentMock.Verify(x => x.Pay(It.IsAny<decimal>()), Times.Never);
        _repositoryMock.Verify(x => x.Save(It.IsAny<int>(), It.IsAny<decimal>()), Times.Never);
        _notificationMock.Verify(x => x.Send(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Checkout_PaymentFails_ReturnsFalseAndDoesNotSaveOrNotify()
    {
        int productId = 1;
        decimal amount = 100m;
        _inventoryMock.Setup(x => x.IsAvailable(productId)).Returns(true);
        _paymentMock.Setup(x => x.Pay(amount)).Returns(false);

        var result = _service.Checkout(productId, amount);

        Assert.False(result);
        _inventoryMock.Verify(x => x.IsAvailable(productId), Times.Once);
        _paymentMock.Verify(x => x.Pay(amount), Times.Once);
        
        _repositoryMock.Verify(x => x.Save(It.IsAny<int>(), It.IsAny<decimal>()), Times.Never);
        _notificationMock.Verify(x => x.Send(It.IsAny<string>()), Times.Never);
    }
}
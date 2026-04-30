using Moq;
using Xunit;

public interface IApiService
{
    string GetData();
}

public class DataService
{
    private readonly IApiService _apiService;

    public DataService(IApiService apiService)
    {
        _apiService = apiService;
    }

    public string LoadData()
    {
        try
        {
            return _apiService.GetData();
        }
        catch
        {
            return "Error";
        }
    }
}

public class DataServiceTests
{
    [Fact]
    public void LoadData_ReturnsCorrectString()
    {
        var mockApi = new Mock<IApiService>();
        string expectedData = "Some Secret Data";
        mockApi.Setup(s => s.GetData()).Returns(expectedData);
        
        var service = new DataService(mockApi.Object);

        var result = service.LoadData();

        Assert.Equal(expectedData, result);
    }

    [Fact]
    public void LoadData_ReturnsErrorMessage()
    {
        var mockApi = new Mock<IApiService>();
        
        mockApi.Setup(s => s.GetData()).Throws(new System.Exception("API Offline"));
        
        var service = new DataService(mockApi.Object);

        var result = service.LoadData();

        Assert.Equal("Error", result);
    }
}
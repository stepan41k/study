namespace lab3.Domain;

// Разделение класса на два partial-файла.
// Демонстрирует C# 13 partial property и partial method.
public partial class ExpenseTransaction
{
    private string _comment = string.Empty;

    // 2.3.8: Реализация partial-свойства (объявление может быть в другом файле/части)
    public partial string UserComment
    {
        get => _comment;
        set => _comment = value?.Trim() ?? string.Empty;
    }

    // 2.3.8: Определение и реализация partial-метода
    partial void OnStateChanged(string oldStatus, string newStatus)
    {
        // Метод вызывается при изменении статуса
        LastStatusMessage = $"Статус изменен с '{oldStatus}' на '{newStatus}'.";
    }

    public string LastStatusMessage { get; private set; } = "OK";
}

// Заголовочная декларация partial property внутри того же типа
public partial class ExpenseTransaction
{
    public partial string UserComment { get; set; }
    partial void OnStateChanged(string oldStatus, string newStatus);
}
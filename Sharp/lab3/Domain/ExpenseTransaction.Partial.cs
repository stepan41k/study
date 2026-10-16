namespace lab3.Domain;

public partial class ExpenseTransaction
{
    private string _comment = string.Empty;

    public partial string UserComment
    {
        get => _comment;
        set => _comment = value?.Trim() ?? string.Empty;
    }

    partial void OnStateChanged(string oldStatus, string newStatus)
    {
        LastStatusMessage = $"Статус изменен с '{oldStatus}' на '{newStatus}'.";
    }

    public string LastStatusMessage { get; private set; } = "OK";
}

public partial class ExpenseTransaction
{
    public partial string UserComment { get; set; }
    partial void OnStateChanged(string oldStatus, string newStatus);
}
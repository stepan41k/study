namespace lab3.Domain;

public abstract class FinancialTransaction : IIdentifiable
{
    public required string Id { get; init; }
    public required DateTime CreatedAt { get; init; }

    private Money _amount;
    public Money Amount
    {
        get => _amount;
        set             
        {
            if (value.Cents <= 0)
                throw new ArgumentException("Сумма транзакции должна быть строго больше нуля.");
            _amount = value;
        }
    }

    public string Status { get; protected set; } = "Completed";

    public bool IsFresh => (DateTime.Now - CreatedAt).TotalDays < 7;

    /*
     * Демонстрация ключевого слова 'field':
     * public string Tag
     * {
     *     get => field;
     *     set => field = string.IsNullOrWhiteSpace(value) ? "General" : value.Trim();
     * }
     */
    private string _tag = "General";
    public string Tag
    {
        get => _tag;
        set => _tag = string.IsNullOrWhiteSpace(value) ? "General" : value.Trim();
    }

    protected FinancialTransaction(Money amount)
    {
        Amount = amount;
    }

    public abstract string GetOperationType();

    public virtual string GetAuditLog() => $"{CreatedAt:yyyy-MM-dd HH:mm}: {GetOperationType()} на сумму {Amount}";

    public string PrintHeader() => $"[BASE-HEADER] Транзакция #{Id}";

    public override string ToString() => $"#{Id} | {GetOperationType()} | {Amount} | Статус: {Status}";
}
namespace lab3.Domain;

/// <summary>
/// Именованный record для параметров фильтрации транзакций (Пункт 2.3.9).
/// </summary>
public record TransactionFilter
{
    public string? OperationType { get; init; }
    public decimal MinAmount { get; init; }
    public decimal MaxAmount { get; init; }

    public TransactionFilter(string? operationType, decimal minAmount, decimal maxAmount)
    {
        if (minAmount < 0 || maxAmount < 0)
            throw new ArgumentOutOfRangeException(nameof(minAmount), "Суммы фильтра не могут быть отрицательными.");
        if (maxAmount < minAmount)
            throw new ArgumentException("Максимальная граница не может быть меньше минимальной.");

        OperationType = operationType;
        MinAmount = minAmount;
        MaxAmount = maxAmount;
    }
}
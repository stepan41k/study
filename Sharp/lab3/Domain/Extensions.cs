namespace lab3.Domain;

/// <summary>
/// Классический класс методов расширения
/// </summary>
public static class FinancialExtensions
{
    public static string ToShortFormat(this FinancialTransaction tx) =>
        $"[{tx.Id}] {tx.GetOperationType()}: {tx.Amount}";

    public static bool IsLargeOperation(this FinancialTransaction tx, Money threshold) =>
        tx.Amount > threshold;
}

/*
 * public implicit extension TransactionFeature for FinancialTransaction
 * {
 *     public string ShortBadge => $"TX#{this.Id}:{this.Amount}";
 * }
 */
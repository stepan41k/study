namespace lab3.Domain;

/// <summary>
/// Наследник 1: Расходная операция.
/// Реализует ICancellable и ISummarizable.
/// </summary>
public partial class ExpenseTransaction : FinancialTransaction, ICancellable, ISummarizable
{
    public ExpenseCategory Category { get; init; }
    public bool IsCancelled { get; private set; }
    public string? CancellationReason { get; private set; }

    public bool IsTaxDeductible { get; init; }

    public ExpenseTransaction(Money amount, ExpenseCategory category) : base(amount)
    {
        Category = category ?? throw new ArgumentNullException(nameof(category));
    }

    public override string GetOperationType() => "Расход";

    public override string GetAuditLog() =>
        $"{base.GetAuditLog()} (Категория: {Category.Title}, Отменена: {IsCancelled})";

    public new string PrintHeader() => $"[EXPENSE-HEADER] Расходная операция #{Id} ({Category.Code})";

    public void Cancel(string reason)
    {
        if (IsCancelled)
            throw new InvalidOperationException($"Транзакция #{Id} уже отменена ранее.");

        IsCancelled = true;
        CancellationReason = string.IsNullOrWhiteSpace(reason) ? "Без причины" : reason;
        Status = "Cancelled";
        OnStateChanged("Completed", "Cancelled");
    }

    string ISummarizable.GetBriefSummary() =>
        $"Расход {Amount} в категории '{Category.Title}' [Статус: {Status}]";
}
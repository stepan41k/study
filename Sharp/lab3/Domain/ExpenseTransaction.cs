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

    // Специфическое свойство наследника для проверки через pattern matching
    public bool IsTaxDeductible { get; init; }

    public ExpenseTransaction(Money amount, ExpenseCategory category) : base(amount)
    {
        Category = category ?? throw new ArgumentNullException(nameof(category));
    }

    // 2.3.2: override абстрактной операции
    public override string GetOperationType() => "Расход";

    // 2.3.2: override виртуальной операции
    public override string GetAuditLog() =>
        $"{base.GetAuditLog()} (Категория: {Category.Title}, Отменена: {IsCancelled})";

    // 2.3.2: Метод new (скрытие базового метода)
    public new string PrintHeader() => $"[EXPENSE-HEADER] Расходная операция #{Id} ({Category.Code})";

    // 2.3.3: Обычная реализация первого интерфейса ICancellable
    public void Cancel(string reason)
    {
        if (IsCancelled)
            throw new InvalidOperationException($"Транзакция #{Id} уже отменена ранее.");

        IsCancelled = true;
        CancellationReason = string.IsNullOrWhiteSpace(reason) ? "Без причины" : reason;
        Status = "Cancelled";
        OnStateChanged("Completed", "Cancelled");
    }

    // 2.3.3: Явная реализация второго интерфейса ISummarizable
    string ISummarizable.GetBriefSummary() =>
        $"Расход {Amount} в категории '{Category.Title}' [Статус: {Status}]";
}
namespace lab3.Domain;

/// <summary>
/// Наследник 2: Доходная операция.
/// </summary>
public class IncomeTransaction : FinancialTransaction, ISummarizable
{
    public string SourceName { get; init; }

    public IncomeTransaction(Money amount, string sourceName) : base(amount)
    {
        SourceName = string.IsNullOrWhiteSpace(sourceName) ? "Основной доход" : sourceName;
    }

    public override string GetOperationType() => "Доход";

    public override string GetAuditLog() => $"{base.GetAuditLog()} (Источник поступления: {SourceName})";

    public string GetBriefSummary() => $"Доход {Amount} от '{SourceName}'";
}
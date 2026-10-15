namespace lab3.Domain;

/// <summary>
/// Классический класс методов расширения
/// </summary>
public static class FinancialExtensions
{
    // Классический extension-метод
    public static string ToShortFormat(this FinancialTransaction tx) =>
        $"[{tx.Id}] {tx.GetOperationType()}: {tx.Amount}";

    // Вспомогательный метод вычисления "крупной операции"
    public static bool IsLargeOperation(this FinancialTransaction tx, Money threshold) =>
        tx.Amount > threshold;
}

/*
 * Демонстрация синтаксиса блока расширений C# 14 (extension declarations):
 * Позволяет объявлять extension-свойства напрямую на типе без статического this-метода.
 *
 * public implicit extension TransactionFeature for FinancialTransaction
 * {
 *     public string ShortBadge => $"TX#{this.Id}:{this.Amount}";
 * }
 *
 * Отличие от обычного члена класса:
 * Расширения не могут иметь собственное состояние экземпляра (backing fields),
 * не могут обращаться к private/protected членам класса и разрешаются статически
 * во время компиляции, а не динамически через таблицу виртуальных методов.
 */
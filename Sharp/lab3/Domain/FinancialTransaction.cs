namespace lab3.Domain;

public abstract class FinancialTransaction : IIdentifiable
{
    // required и init-свойства
    public required string Id { get; init; }
    public required DateTime CreatedAt { get; init; }

    // Изменяемое свойство с проверкой в блочном аксессоре
    private Money _amount;
    public Money Amount
    {
        get => _amount; // Аксессор-выражение =>
        set             // Аксессор блоком
        {
            if (value.Cents <= 0)
                throw new ArgumentException("Сумма транзакции должна быть строго больше нуля.");
            _amount = value;
        }
    }

    // Свойство с ограниченным set (protected set)
    public string Status { get; protected set; } = "Completed";

    // Вычисляемое свойство (expression-bodied)
    public bool IsFresh => (DateTime.Now - CreatedAt).TotalDays < 7;

    /*
     * Демонстрация ключевого слова 'field' (C# 13/14 preview feature):
     * При использовании semi-auto property значение сохраняется в неявное поле 'field':
     *
     * public string Tag
     * {
     *     get => field;
     *     set => field = string.IsNullOrWhiteSpace(value) ? "General" : value.Trim();
     * }
     *
     * Эквивалент с явно объявленным полем (приведен ниже):
     */
    private string _tag = "General";
    public string Tag
    {
        get => _tag;
        set => _tag = string.IsNullOrWhiteSpace(value) ? "General" : value.Trim();
    }

    // Конструктор
    protected FinancialTransaction(Money amount)
    {
        Amount = amount;
    }

    // 2.3.2: Абстрактная операция
    public abstract string GetOperationType();

    // 2.3.2: Виртуальная операция с базовой реализацией
    public virtual string GetAuditLog() => $"{CreatedAt:yyyy-MM-dd HH:mm}: {GetOperationType()} на сумму {Amount}";

    // Метод скрытия (для демонстрации new в 2.3.2)
    public string PrintHeader() => $"[BASE-HEADER] Транзакция #{Id}";

    // Переопределение ToString (2.3.6)
    public override string ToString() => $"#{Id} | {GetOperationType()} | {Amount} | Статус: {Status}";
}
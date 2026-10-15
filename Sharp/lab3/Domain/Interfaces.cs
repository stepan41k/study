namespace lab3.Domain;

// 2.3.3: Первая независимая возможность — отмена операции
public interface ICancellable
{
    bool IsCancelled { get; }
    void Cancel(string reason);
}

// 2.3.3: Вторая независимая возможность — сводка с реализацией по умолчанию
public interface ISummarizable
{
    string GetBriefSummary();

    // Метод с готовым телом в интерфейсе (Default Interface Method)
    string GetDetailedBadge() => $"[Сводка]: {GetBriefSummary()}";
}

// 2.3.5: Ковариантный интерфейс-источник (out T)
public interface ISource<out T>
{
    T GetItem();
}

// 2.3.5: Контрвариантный интерфейс-обработчик (in T)
public interface IHandler<in T>
{
    void Handle(T item);
}
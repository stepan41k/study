namespace lab3.Domain;

public interface ICancellable
{
    bool IsCancelled { get; }
    void Cancel(string reason);
}

public interface ISummarizable
{
    string GetBriefSummary();

    string GetDetailedBadge() => $"[Сводка]: {GetBriefSummary()}";
}

public interface ISource<out T>
{
    T GetItem();
}

public interface IHandler<in T>
{
    void Handle(T item);
}
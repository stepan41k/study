namespace lab3.Domain;

/// <summary>
/// Sealed-класс категории расходов с предметным критерием равенства по Code.
/// </summary>
public sealed class ExpenseCategory : IEquatable<ExpenseCategory>
{
    public string Code { get; }
    public string Title { get; }

    public ExpenseCategory(string code, string title)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Код категории не может быть пустым.", nameof(code));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Название категории не может быть пустым.", nameof(title));

        Code = code.Trim().ToUpperInvariant();
        Title = title.Trim();
    }

    // Предметное равенство строго по коду категории
    public bool Equals(ExpenseCategory? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return string.Equals(Code, other.Code, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj) => Equals(obj as ExpenseCategory);

    public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Code);

    public static bool operator ==(ExpenseCategory? left, ExpenseCategory? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(ExpenseCategory? left, ExpenseCategory? right) => !(left == right);

    public override string ToString() => $"[{Code}] {Title}";
}
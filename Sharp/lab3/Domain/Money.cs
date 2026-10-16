namespace lab3.Domain;

/// <summary>
/// Неизменяемый тип-значение для денежных сумм (в копейках/центах для точности).
/// </summary>
public readonly struct Money : IEquatable<Money>, IComparable<Money>
{
    public long Cents { get; }

    public Money(decimal amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Сумма не может быть отрицательной.");

        checked
        {
            Cents = (long)Math.Round(amount * 100m, MidpointRounding.AwayFromZero);
        }
    }

    private Money(long cents) => Cents = cents;

    public decimal ToDecimal() => Cents / 100m;

    public static Money operator +(Money left, Money right)
    {
        checked
        {
            return new Money(left.Cents + right.Cents);
        }
    }

    public static bool operator <(Money left, Money right) => left.Cents < right.Cents;
    public static bool operator >(Money left, Money right) => left.Cents > right.Cents;

    public static implicit operator decimal(Money m) => m.ToDecimal();

    public static explicit operator Money(decimal value)
    {
        if (value < 0)
            throw new InvalidCastException($"Невозможно преобразовать отрицательное число {value} в Money.");
        return new Money(value);
    }

    public bool Equals(Money other) => Cents == other.Cents;
    public override bool Equals(object? obj) => obj is Money other && Equals(other);
    public override int GetHashCode() => Cents.GetHashCode();
    public int CompareTo(Money other) => Cents.CompareTo(other.Cents);
    public static bool operator ==(Money left, Money right) => left.Equals(right);
    public static bool operator !=(Money left, Money right) => !left.Equals(right);
    public override string ToString() => $"{ToDecimal():F2} руб.";
}
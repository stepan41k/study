using System;

namespace WarehouseModel
{
    #region Категориальные данные (Перечисления)

    public enum ProductCategory
    {
        Electronics,
        Groceries,
        Apparel,
        Household,
        Books
    }

    public enum ProductStatus
    {
        InStock,
        OutOfStock,
        Discontinued,
        AwaitingShipment
    }

    #endregion

    public abstract class Product
    {
        private decimal[] _priceHistory;

        public string Sku { get; protected set; }
        public string Name { get; set; }
        public string Description { get; set; } = string.Empty;

        public decimal BasePrice { get; set; }
        public int StockQuantity { get; set; }
        public double WeightKg { get; set; }

        public DateTime CreatedAt { get; }
        public DateTime LastModifiedAt { get; protected set; }
        public DateOnly ManufactureDate { get; set; }

        public ProductCategory Category { get; set; }
        public ProductStatus Status { get; set; }

        public string[] Tags { get; set; }
        public decimal[] PriceHistory => _priceHistory;

        protected Product(
            string sku,
            string name,
            decimal basePrice,
            int stockQuantity,
            ProductCategory category,
            DateOnly manufactureDate,
            string[]? tags = null)
        {
            if (string.IsNullOrWhiteSpace(sku))
                throw new ArgumentException("Артикул не может быть пустым.", nameof(sku));

            Sku = sku;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            BasePrice = basePrice >= 0 ? basePrice : throw new ArgumentOutOfRangeException(nameof(basePrice));
            StockQuantity = stockQuantity >= 0 ? stockQuantity : 0;
            Category = category;
            ManufactureDate = manufactureDate;

            CreatedAt = DateTime.UtcNow;
            LastModifiedAt = DateTime.UtcNow;

            Status = StockQuantity > 0 ? ProductStatus.InStock : ProductStatus.OutOfStock;

            Tags = tags ?? Array.Empty<string>();
            _priceHistory = new decimal[] { basePrice };
        }

        public virtual void UpdatePrice(decimal newPrice)
        {
            if (newPrice < 0)
                throw new ArgumentOutOfRangeException(nameof(newPrice), "Цена не может быть отрицательной.");

            BasePrice = newPrice;
            LastModifiedAt = DateTime.UtcNow;


            Array.Resize(ref _priceHistory, _priceHistory.Length + 1);
            _priceHistory[^1] = newPrice;
        }

        public abstract decimal CalculateFinalPrice();

        public virtual string GetProductSummary()
        {
            return $"[{Sku}] {Name} | Категория: {Category} | Цена: {BasePrice:C} | Остаток: {StockQuantity} шт.";
        }
    }

    public class PerishableProduct : Product
    {
        public DateOnly ExpirationDate { get; set; }

        public PerishableProduct(
            string sku,
            string name,
            decimal basePrice,
            int stockQuantity,
            DateOnly manufactureDate,
            DateOnly expirationDate,
            string[]? tags = null)
            : base(sku, name, basePrice, stockQuantity, ProductCategory.Groceries, manufactureDate, tags)
        {
            ExpirationDate = expirationDate;
        }

        public override decimal CalculateFinalPrice()
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            if (ExpirationDate <= today.AddDays(2))
            {
                return BasePrice * 0.5m;
            }
            return BasePrice;
        }

        public override string GetProductSummary()
        {
            return $"{base.GetProductSummary()} | Годен до: {ExpirationDate:yyyy-MM-dd}";
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            var milk = new PerishableProduct(
                sku: "MILK-001",
                name: "Молоко пастеризованное 3.2%",
                basePrice: 85.50m,
                stockQuantity: 20,
                manufactureDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-3)),
                expirationDate: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), // Истекает завтра
                tags: new[] { "молочка", "свежее", "акция" }
            );

            milk.Description = "Свежее фермерское молоко";

            Console.WriteLine(milk.GetProductSummary());
            Console.WriteLine($"Итоговая цена со скидкой за срок: {milk.CalculateFinalPrice():C}");

            // Обновляем цену и проверяем историю
            milk.UpdatePrice(90.00m);
            Console.WriteLine($"История цен: {string.Join(" -> ", milk.PriceHistory)}");
        }
    }
}
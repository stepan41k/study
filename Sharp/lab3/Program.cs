using lab3.Domain;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;
Console.WriteLine("================================================================================");
Console.WriteLine("  ПРАКТИЧЕСКАЯ РАБОТА: ПРИКЛАДНАЯ МОДЕЛЬ «УЧЁТ ЛИЧНЫХ ФИНАНСОВ» (C#)");
Console.WriteLine("================================================================================\n");

// Выполнение обязательных демонстраций
DemoPropertiesAndCreation();      // 2.3.1
DemoInheritanceAndPolymorphism(); // 2.3.2
DemoInterfaces();                 // 2.3.3
DemoCatalogAndIndexers();         // 2.3.4
DemoVariance();                   // 2.3.5
DemoEquality();                   // 2.3.6
DemoOperatorsAndConversions();    // 2.3.7
DemoExtensionsAndPartial();       // 2.3.8
DemoRecordAndAnonymousSummary();  // 2.3.9
RunFinalEndToEndScenario();       // 2.4

Console.WriteLine("\n[✔] Все демонстрации успешно завершены без прерывания программы!");
return;

// ================================================================================================
// МЕТОДЫ ДЕМОНСТРАЦИИ ПО ПУНКТАМ
// ================================================================================================

// --- 2.3.1 Свойства и создание объектов ---
static void DemoPropertiesAndCreation()
{
    PrintHeader("2.3.1 Свойства и создание объектов");

    var foodCat = new ExpenseCategory("FOOD", "Продукты питания");

    var tx = new ExpenseTransaction((Money)1500m, foodCat)
    {
        Id = "TX-101",
        CreatedAt = DateTime.Now.AddDays(-2),
        Tag = "Супермаркет"
    };

    Console.WriteLine($"[OK] Создан объект: Id={tx.Id}, Сумма={tx.Amount}, Дата={tx.CreatedAt:yyyy-MM-dd}, IsFresh={tx.IsFresh}");

    tx.Amount = (Money)1850.50m;
    Console.WriteLine($"[OK] Допустимое изменение Amount: {tx.Amount}");

    try
    {
        Console.WriteLine("[ТЕСТ ОШИБКИ] Попытка установить некорректную сумму (0 руб.)...");
        tx.Amount = (Money)0m;
    }
    catch (ArgumentException ex)
    {
        Console.WriteLine($"  -> Ошибка перехвачена: {ex.Message}");
        Console.WriteLine($"  -> Состояние не испорчено, текущая сумма: {tx.Amount}");
    }

    /*
     * Закомментированные примеры диагностик компилятора:
     * 1. Не задан обязательный required-член:
     * var invalid = new ExpenseTransaction((Money)100m, foodCat);
     * // ОШИБКА CS9035: Required member 'FinancialTransaction.Id' must be set in the object initializer.
     *
     * 2. Изменение init-свойства после создания:
     * tx.CreatedAt = DateTime.Now;
     * // ОШИБКА CS8852: Init-only property 'FinancialTransaction.CreatedAt' can only be assigned in an object initializer.
     */
}

// --- 2.3.2 Наследование и полиморфизм ---
static void DemoInheritanceAndPolymorphism()
{
    PrintHeader("2.3.2 Наследование и полиморфизм");

    var cat = new ExpenseCategory("TECH", "Электроника");
    var transactions = new List<FinancialTransaction>
    {
        new ExpenseTransaction((Money)45000m, cat) { Id = "TX-E1", CreatedAt = DateTime.Now, IsTaxDeductible = true },
        new IncomeTransaction((Money)120000m, "Зарплатный проект") { Id = "TX-I1", CreatedAt = DateTime.Now }
    };

    Console.WriteLine("--- Вызов override-методов в едином цикле по базовому типу ---");
    foreach (var tx in transactions)
    {
        Console.WriteLine($"Тип: {tx.GetOperationType()} | Лог: {tx.GetAuditLog()}");

        if (tx is ExpenseTransaction { IsTaxDeductible: true } expense)
        {
            Console.WriteLine($"  -> [Pattern Matching]: Найден расход с налоговым вычетом (Сумма: {expense.Amount})");
        }
    }

    Console.WriteLine("\n--- Эксперимент с сокрытием методов (new vs override) ---");
    var specificExpense = new ExpenseTransaction((Money)200m, cat) { Id = "TX-EXP", CreatedAt = DateTime.Now };
    FinancialTransaction baseRef = specificExpense;

    Console.WriteLine($"Вызов через ссылку наследника : {specificExpense.PrintHeader()}");
    Console.WriteLine($"Вызов через базовую ссылку   : {baseRef.PrintHeader()}");
}

// --- 2.3.3 Интерфейсы ---
static void DemoInterfaces()
{
    PrintHeader("2.3.3 Интерфейсы (Явная реализация и Default Interface Methods)");

    var food = new ExpenseCategory("CAFE", "Кафе и рестораны");
    var expense = new ExpenseTransaction((Money)850m, food) { Id = "TX-201", CreatedAt = DateTime.Now };
    var income = new IncomeTransaction((Money)5000m, "Фриланс") { Id = "TX-202", CreatedAt = DateTime.Now };

    PrintAnySummary(expense);
    PrintAnySummary(income);

    ISummarizable summarizable = expense;
    Console.WriteLine($"Явная реализация: {summarizable.GetBriefSummary()}");
    Console.WriteLine($"Метод по умолчанию (DIM): {summarizable.GetDetailedBadge()}");

    expense.Cancel("Ошибочное списание официантом");
    Console.WriteLine($"Результат отмены: IsCancelled={expense.IsCancelled}, Причина='{expense.CancellationReason}', Заметка='{expense.LastStatusMessage}'");

    try
    {
        Console.WriteLine("[ТЕСТ ОШИБКИ] Повторная отмена транзакции...");
        expense.Cancel("Повторная отмена");
    }
    catch (InvalidOperationException ex)
    {
        Console.WriteLine($"  -> Ошибка перехвачена: {ex.Message}");
    }

    static void PrintAnySummary(ISummarizable item) =>
        Console.WriteLine($"[PrintAnySummary]: {item.GetBriefSummary()}");
}

// --- 2.3.4 Обобщённый контейнер и индексаторы ---
static void DemoCatalogAndIndexers()
{
    PrintHeader("2.3.4 Обобщённый контейнер и индексаторы");

    var catalog = new Catalog<FinancialTransaction>();
    var cat = new ExpenseCategory("TRANS", "Транспорт");

    catalog.Add(new ExpenseTransaction((Money)60m, cat) { Id = "T1", CreatedAt = DateTime.Now });
    catalog.Add(new ExpenseTransaction((Money)1200m, cat) { Id = "T2", CreatedAt = DateTime.Now });
    catalog.Add(new IncomeTransaction((Money)25000m, "Аванс") { Id = "T3", CreatedAt = DateTime.Now });
    catalog.Add(new IncomeTransaction((Money)7000m, "Дивиденды") { Id = "T4", CreatedAt = DateTime.Now });

    Console.WriteLine($"В каталог добавлено {catalog.Count} объектов.");
    Console.WriteLine($"Индексатор по номеру [0]: {catalog[0]}");
    Console.WriteLine($"Индексатор по Id [\"T3\"]: {catalog["T3"]}");

    bool addedAgain = catalog.Add(new ExpenseTransaction((Money)10m, cat) { Id = "T1", CreatedAt = DateTime.Now });
    Console.WriteLine($"Попытка добавить дубликат Id 'T1': {(addedAgain ? "Успех" : "Отклонено (Корректно)")}");

    var notFound = catalog["NON_EXISTENT"];
    Console.WriteLine($"Поиск отсутствующего Id: {(notFound is null ? "Вернул null (Корректно)" : "Найден")}");

    try
    {
        Console.WriteLine("[ТЕСТ ОШИБКИ] Обращение по индексу 99...");
        var _ = catalog[99];
    }
    catch (ArgumentOutOfRangeException ex)
    {
        Console.WriteLine($"  -> Ошибка индексатора перехвачена: {ex.ParamName ?? "Параметр вне диапазона"}");
    }
}

// --- 2.3.5 Ковариантность и контрвариантность ---
static void DemoVariance()
{
    PrintHeader("2.3.5 Ковариантность (out) и контрвариантность (in)");

    var expenseSource = new SpecificExpenseSource();
    var baseHandler = new BaseTransactionHandler();

    // out T (Ковариантность)
    ISource<FinancialTransaction> baseSource = expenseSource;
    FinancialTransaction item = baseSource.GetItem();
    Console.WriteLine($"[out T Ковариантность]: Получен элемент: {item.GetOperationType()} на сумму {item.Amount}");

    // in T (Контрвариантность)
    IHandler<ExpenseTransaction> expenseHandler = baseHandler;
    expenseHandler.Handle((ExpenseTransaction)item);
}

// --- 2.3.6 Равенство и строковое представление ---
static void DemoEquality()
{
    PrintHeader("2.3.6 Равенство и строковое представление");

    var cat1 = new ExpenseCategory("GROC", "Продукты питания");
    var cat2 = new ExpenseCategory("groc", "Продуктовая корзина");
    var cat3 = new ExpenseCategory("RENT", "Аренда жилья");

    Console.WriteLine($"Сравнение cat1 и cat2: Equals={cat1.Equals(cat2)}, ==: {cat1 == cat2}");
    Console.WriteLine($"Сравнение cat1 и cat3: Equals={cat1.Equals(cat3)}, ==: {cat1 == cat3}");
    Console.WriteLine($"Сравнение с null: Equals={cat1.Equals(null)}, ==: {cat1 == null}");

    Console.WriteLine($"Хэш-код cat1: {cat1.GetHashCode()}");
    Console.WriteLine($"Хэш-код cat2: {cat2.GetHashCode()}");

    var set = new HashSet<ExpenseCategory> { cat1, cat2, cat3 };
    Console.WriteLine($"Количество элементов в HashSet (ожидается 2): {set.Count}");
}

// --- 2.3.7 Операторы и преобразования ---
static void DemoOperatorsAndConversions()
{
    PrintHeader("2.3.7 Операторы и пользовательские преобразования");

    Money m1 = (Money)150.50m;
    Money m2 = (Money)49.50m;

    Money sum = m1 + m2;
    Console.WriteLine($"Сложение: {m1} + {m2} = {sum} (исходный m1: {m1})");
    Console.WriteLine($"Сравнение: {m1} > {m2}: {m1 > m2}; {m1} < {m2}: {m1 < m2}");

    decimal decimalValue = sum;
    Console.WriteLine($"Неявное преобразование в decimal: {decimalValue:F2}");

    try
    {
        Console.WriteLine("[ТЕСТ ОШИБКИ] Попытка преобразовать отрицательное число в Money...");
        var _ = (Money)(-25m);
    }
    catch (InvalidCastException ex)
    {
        Console.WriteLine($"  -> Ошибка явного преобразования перехвачена: {ex.Message}");
    }
}

// --- 2.3.8 Расширения и partial ---
static void DemoExtensionsAndPartial()
{
    PrintHeader("2.3.8 Методы расширения и Partial-классы");

    var cat = new ExpenseCategory("BOOK", "Книги");
    var tx = new ExpenseTransaction((Money)1200m, cat)
    {
        Id = "TX-PARTIAL",
        CreatedAt = DateTime.Now,
        UserComment = "Купил учебник по архитектуре ПО"
    };

    Console.WriteLine($"Классический extension: {tx.ToShortFormat()}");
    Console.WriteLine($"Проверка порога: {tx.IsLargeOperation((Money)1000m)}");
    Console.WriteLine($"Partial-свойство (UserComment): '{tx.UserComment}'");

    tx.Cancel("Возврат в магазин");
    Console.WriteLine($"Partial-метод отработал (LastStatusMessage): '{tx.LastStatusMessage}'");
}

// --- 2.3.9 Record и анонимная сводка ---
static void DemoRecordAndAnonymousSummary()
{
    PrintHeader("2.3.9 Record, неразрушающая мутация и анонимная сводка");

    var filter1 = new TransactionFilter("Расход", 100m, 5000m);
    var filter2 = new TransactionFilter("Расход", 100m, 5000m);

    Console.WriteLine($"Равенство record по значениям: {filter1 == filter2} (Equals: {filter1.Equals(filter2)})");

    var filterModified = filter1 with { MaxAmount = 10000m };
    Console.WriteLine($"Исходный record : Min={filter1.MinAmount}, Max={filter1.MaxAmount}");
    Console.WriteLine($"Модифицированный: Min={filterModified.MinAmount}, Max={filterModified.MaxAmount}");

    var cat = new ExpenseCategory("MISC", "Разное");
    var list = new List<FinancialTransaction>
    {
        new ExpenseTransaction((Money)500m, cat) { Id = "S1", CreatedAt = DateTime.Now },
        new ExpenseTransaction((Money)15000m, cat) { Id = "S2", CreatedAt = DateTime.Now },
        new IncomeTransaction((Money)40000m, "Премия") { Id = "S3", CreatedAt = DateTime.Now },
        new IncomeTransaction((Money)1200m, "Кэшбэк") { Id = "S4", CreatedAt = DateTime.Now }
    };

    var summary = list.Select(tx => new
    {
        Title = tx.Id,
        Kind = tx.GetOperationType(),
        IsSubstantial = tx.Amount > (Money)5000m,
        FormattedTotal = tx.Amount.ToString()
    });

    Console.WriteLine("\n--- Анонимная сводка «Название — Статус — Итог» ---");
    foreach (var row in summary)
    {
        Console.WriteLine($"{row.Title} — {row.Kind} — Крупная: {row.IsSubstantial} — Итог: {row.FormattedTotal}");
    }
}

// --- 2.4 Итоговый сквозной сценарий ---
static void RunFinalEndToEndScenario()
{
    PrintHeader("2.4 Итоговый сквозной сценарий работы пользователя");

    var catalog = new Catalog<FinancialTransaction>();

    var catGroceries = new ExpenseCategory("FOOD", "Продукты питания");
    var catTransport = new ExpenseCategory("TRIP", "Транспорт");
    var catTech = new ExpenseCategory("ELEC", "Гаджеты");

    catalog.Add(new IncomeTransaction((Money)95000m, "Основная зарплата") { Id = "TX-01", CreatedAt = DateTime.Now.AddDays(-3) });
    catalog.Add(new ExpenseTransaction((Money)4200m, catGroceries) { Id = "TX-02", CreatedAt = DateTime.Now.AddDays(-2) });
    catalog.Add(new ExpenseTransaction((Money)850m, catTransport) { Id = "TX-03", CreatedAt = DateTime.Now.AddDays(-1) });

    var techTx = new ExpenseTransaction((Money)29990m, catTech) { Id = "TX-04", CreatedAt = DateTime.Now };
    catalog.Add(techTx);

    Console.WriteLine($"Всего зарегистрировано операций: {catalog.Count}");

    techTx.Cancel("Случайный заказ в интернет-магазине");
    Console.WriteLine($"Статус TX-04 изменен: {techTx.Status}, Причина: {techTx.CancellationReason}");

    Money totalIncome = (Money)0m;
    Money totalExpense = (Money)0m;

    foreach (var tx in catalog)
    {
        if (tx is ExpenseTransaction { IsCancelled: true })
            continue;

        if (tx is IncomeTransaction inc)
            totalIncome += inc.Amount;
        else if (tx is ExpenseTransaction exp)
            totalExpense += exp.Amount;
    }

    Console.WriteLine($"  Всего доходов  : {totalIncome}");
    Console.WriteLine($"  Всего расходов : {totalExpense}");
    Console.WriteLine($"  Итоговый баланс: {totalIncome.ToDecimal() - totalExpense.ToDecimal():F2} руб.");
}

static void PrintHeader(string title)
{
    Console.WriteLine($"\n--------------------------------------------------------------------------------");
    Console.WriteLine($" >>> {title}");
    Console.WriteLine($"--------------------------------------------------------------------------------");
}

// ================================================================================================
// ВСПОМОГАТЕЛЬНЫЕ КЛАССЫ ДЛЯ ПУНКТА 2.3.5 (ДОЛЖНЫ БЫТЬ СТРОГО В КОНЦЕ ФАЙЛА)
// ================================================================================================

class SpecificExpenseSource : ISource<ExpenseTransaction>
{
    public ExpenseTransaction GetItem() =>
        new((Money)3200m, new ExpenseCategory("FIT", "Фитнес")) { Id = "SRC-1", CreatedAt = DateTime.Now };
}

class BaseTransactionHandler : IHandler<FinancialTransaction>
{
    public void Handle(FinancialTransaction item) =>
        Console.WriteLine($"[in T Контрвариантность]: Обработчик базового типа обработал #{item.Id} ({item.Amount})");
}
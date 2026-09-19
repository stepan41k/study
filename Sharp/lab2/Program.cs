using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace MultithreadingLab;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Console.WriteLine("=== Задание 1 ===");
        Task1_TwoThreads();

        Console.WriteLine("\n=== Задание 2 ===");
        await Task2_AsyncSourcesAndPrimes();

        Console.WriteLine("\n=== Задание 3 ===");
        await Task3_ErrorsAndCancellation();

        Console.WriteLine("\n=== Задание 4 ===");
        await Task4_SemaphoreSlim();

        Console.WriteLine("\n=== Задание 5 ===");
        await Task5_WorkQueueAndStats();

        Console.WriteLine("\n=== Задание 6 ===");
        Task6_ParallelCalculations();

        Console.WriteLine("\n=== Задание 7 ===");
        await Task7_AsyncEnumerable();

        Console.WriteLine("\n=== Задание 8 ===");
        await Task8_ValueTaskAndTcs();

        Console.WriteLine("\n=== Задание 9 ===");
        await Task9_SynchronizationContext();

        Console.WriteLine("\nВсе задания завершены.");
    }

    #region Задание 1: Два независимых потока (Вариант 3)
    // Входная строка: «C# async 2026». Буквы (6) и цифры (4).
    private static void Task1_TwoThreads()
    {
        const string input = "C# async 2026";
        int letterCount = 0;
        int digitCount = 0;

        // Поток подсчета букв
        Thread letterThread = new Thread(() =>
        {
            Console.WriteLine("[Буквы] Начало работы.");
            Thread.Sleep(100);
            letterCount = input.Count(char.IsLetter);
            Console.WriteLine("[Буквы] Конец работы.");
        });

        // Поток подсчета цифр
        Thread digitThread = new Thread(() =>
        {
            Console.WriteLine("[Цифры] Начало работы.");
            Thread.Sleep(100);
            digitCount = input.Count(char.IsDigit);
            Console.WriteLine("[Цифры] Конец работы.");
        });

        // Запуск обоих потоков до ожидания Join
        letterThread.Start();
        digitThread.Start();

        letterThread.Join();
        digitThread.Join();

        Console.WriteLine($"Результат: Букв = {letterCount}, Цифр = {digitCount}");
    }
    #endregion

    #region Задание 2: Получение данных и вычисление результата (Вариант 3)
    // Источники {2, 3}, {4, 5}, {6, 7} с задержками 300, 100, 200 мс. 
    // Найти простые числа через Task.Run.
    private static async Task Task2_AsyncSourcesAndPrimes()
    {
        async Task<int[]> SourceAsync(int delayMs, int[] data)
        {
            await Task.Delay(delayMs);
            return data;
        }

        var task1 = SourceAsync(300, new[] { 2, 3 });
        var task2 = SourceAsync(100, new[] { 4, 5 });
        var task3 = SourceAsync(200, new[] { 6, 7 });

        var allTasks = new[] { task1, task2, task3 };

        // 1. Ожидаем первый завершившийся через WhenAny
        Task<int[]> firstTask = await Task.WhenAny(allTasks);
        int[] firstResult = await firstTask;
        Console.WriteLine($"Первый полученный набор: [{string.Join(", ", firstResult)}]");

        // 2. Ожидаем все наборы через WhenAll
        int[][] allResults = await Task.WhenAll(allTasks);
        int[] mergedData = allResults.SelectMany(x => x).ToArray();

        // 3. Вычисление простых чисел через Task.Run (проверка делителями)
        var primes = await Task.Run(() =>
        {
            bool IsPrime(int n)
            {
                if (n < 2) return false;
                for (int i = 2; i * i <= n; i++)
                {
                    if (n % i == 0) return false;
                }
                return true;
            }

            return mergedData.Where(IsPrime).ToList();
        });

        Console.WriteLine($"Простые числа: {string.Join(", ", primes)}; количество: {primes.Count}");
    }
    #endregion

    #region Задание 3: Ошибки и отмена группы операций (Вариант 2)
    // 100/d для d in {5, 0, 4}. Задержка Delay(100, token).
    // Без отмены, с заранее отмененным токеном, с CancelAfter(50).
    private static async Task Task3_ErrorsAndCancellation()
    {
        async Task<int> DivideOpAsync(int d, CancellationToken token)
        {
            await Task.Delay(100, token);
            return 100 / d; // При d == 0 вызовет DivideByZeroException
        }

        async Task RunExperiment(string label, CancellationToken token)
        {
            Console.WriteLine($"\n--- {label} ---");
            int[] divisors = { 5, 0, 4 };
            var tasks = divisors.Select(d => DivideOpAsync(d, token)).ToArray();

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (Exception)
            {
                // Исключение поглощается для дальнейшего подробного анализа каждого таска
            }

            for (int i = 0; i < tasks.Length; i++)
            {
                var t = tasks[i];
                Console.Write($"Задача #{i + 1} (d={divisors[i]}): Статус = {t.Status}");
                if (t.IsCompletedSuccessfully)
                {
                    Console.WriteLine($", Результат = {t.Result}");
                }
                else if (t.IsFaulted)
                {
                    Console.WriteLine($", Ошибка = {t.Exception?.InnerException?.GetType().Name}: {t.Exception?.InnerException?.Message}");
                }
                else if (t.IsCanceled)
                {
                    Console.WriteLine(", Отменена (Canceled)");
                }
            }

            // Получение полного набора ошибок через aggregate task
            var aggregateTask = Task.WhenAll(tasks);
            if (aggregateTask.Exception != null)
            {
                Console.WriteLine("Ошибки из InnerExceptions:");
                foreach (var ex in aggregateTask.Exception.InnerExceptions)
                {
                    Console.WriteLine($" - {ex.GetType().Name}: {ex.Message}");
                }
            }
        }

        // 1. Запуск без отмены
        await RunExperiment("Запуск 1: Без отмены", CancellationToken.None);

        // 2. Запуск с заранее отмененным токеном
        using var preCanceledCts = new CancellationTokenSource();
        preCanceledCts.Cancel();
        await RunExperiment("Запуск 2: С заранее отмененным токеном", preCanceledCts.Token);

        // 3. Запуск с CancelAfter(50)
        using var timeoutCts = new CancellationTokenSource();
        timeoutCts.CancelAfter(50);
        await RunExperiment("Запуск 3: С CancelAfter(50)", timeoutCts.Token);
    }
    #endregion

    #region Задание 4: Доступ к ограниченному ресурсу (Вариант 3)
    // SemaphoreSlim(2), 8 запросов, Delay(100), отслеживание через Interlocked.
    private static async Task Task4_SemaphoreSlim()
    {
        using var semaphore = new SemaphoreSlim(2, 2);
        int activeCount = 0;
        int maxActiveCount = 0;

        Task[] requests = new Task[8];

        for (int i = 0; i < 8; i++)
        {
            int requestId = i + 1;
            requests[i] = Task.Run(async () =>
            {
                await semaphore.WaitAsync();
                try
                {
                    // Увеличиваем счетчик и фиксируем максимум
                    int current = Interlocked.Increment(ref activeCount);

                    int initialMax, newMax;
                    do
                    {
                        initialMax = maxActiveCount;
                        newMax = Math.Max(initialMax, current);
                    } while (Interlocked.CompareExchange(ref maxActiveCount, newMax, initialMax) != initialMax);

                    Console.WriteLine($"[Запрос {requestId}] Начало обслуживания (активно: {current})");
                    await Task.Delay(100);
                    Console.WriteLine($"[Запрос {requestId}] Окончание обслуживания");
                }
                finally
                {
                    Interlocked.Decrement(ref activeCount);
                    semaphore.Release();
                }
            });
        }

        await Task.WhenAll(requests);
        Console.WriteLine($"Все 8 запросов выполнены. Максимальное одновременное количество: {maxActiveCount}");
    }
    #endregion

    #region Задание 5: Очередь работы и общая статистика (Вариант 3)
    // ConcurrentQueue (1..100), 3 воркера, ConcurrentDictionary (остатки 0, 1, 2 от деления на 3).
    private static async Task Task5_WorkQueueAndStats()
    {
        var queue = new ConcurrentQueue<int>(Enumerable.Range(1, 100));
        var stats = new ConcurrentDictionary<string, int>();
        int totalProcessed = 0;

        Task[] workers = new Task[3];

        for (int w = 0; w < 3; w++)
        {
            workers[w] = Task.Run(() =>
            {
                while (queue.TryDequeue(out int item))
                {
                    Interlocked.Increment(ref totalProcessed);

                    string key = (item % 3).ToString();
                    stats.AddOrUpdate(key, 1, (_, count) => count + 1);
                }
            });
        }

        await Task.WhenAll(workers);

        Console.WriteLine($"Общий счётчик обработанных элементов: {totalProcessed}");
        Console.WriteLine("Отсортированные категории (остаток : количество):");
        foreach (var kvp in stats.OrderBy(x => x.Key))
        {
            Console.WriteLine($"  Остаток {kvp.Key} => {kvp.Value}");
        }
    }
    #endregion

    #region Задание 6: Независимые вычисления через Parallel (Вариант 1)
    // Квадраты чисел 1..100 в long[]. Parallel.For, Parallel.ForEach + Interlocked.Add. Сумма = 338350.
    private static void Task6_ParallelCalculations()
    {
        long[] seqArray = new long[100];
        long[] parArray = new long[100];

        // 1. Обычный цикл
        for (int i = 0; i < 100; i++)
        {
            long n = i + 1;
            seqArray[i] = n * n;
        }

        // 2. Parallel.For
        Parallel.For(0, 100, i =>
        {
            long n = i + 1;
            parArray[i] = n * n;
        });

        // 3. Сравнение массивов
        bool areEqual = seqArray.SequenceEqual(parArray);
        Console.WriteLine($"Массивы совпадают: {areEqual}");

        // 4. Подсчет суммы через Parallel.ForEach и Interlocked.Add
        long totalSum = 0;
        Parallel.ForEach(parArray, val =>
        {
            Interlocked.Add(ref totalSum, val);
        });

        Console.WriteLine($"Итоговая сумма: {totalSum} (Ожидается: 338350)");
    }
    #endregion

    #region Задание 7: Данные по мере поступления (Вариант 1)
    // IAsyncEnumerable<int> выдает 1..5 с задержкой 100 мс.
    // 1) Полный проход (сумма 15); 2) Break после 3 (сумма 6); 3) CancelAfter(250).
    private static async Task Task7_AsyncEnumerable()
    {
        static async IAsyncEnumerable<int> GenerateNumbersAsync([EnumeratorCancellation] CancellationToken token = default)
        {
            for (int i = 1; i <= 5; i++)
            {
                await Task.Delay(100, token);
                yield return i;
            }
        }

        // 1. Полный проход
        Console.WriteLine("1. Полный проход:");
        int sumFull = 0;
        await foreach (var item in GenerateNumbersAsync())
        {
            sumFull += item;
            Console.WriteLine($"   Получено: {item}, Текущая сумма: {sumFull}");
        }
        Console.WriteLine($"   Результат полного прохода: сумма = {sumFull}");

        // 2. Проход с break после третьего элемента
        Console.WriteLine("\n2. Проход с прерыванием (break после 3-го элемента):");
        int sumBreak = 0;
        await foreach (var item in GenerateNumbersAsync())
        {
            sumBreak += item;
            Console.WriteLine($"   Получено: {item}, Текущая сумма: {sumBreak}");
            if (item == 3)
            {
                Console.WriteLine("   Прерывание цикла по break.");
                break;
            }
        }
        Console.WriteLine($"   Результат: сумма = {sumBreak}");

        // 3. Проход с CancelAfter(250)
        Console.WriteLine("\n3. Проход с CancelAfter(250):");
        int sumCancel = 0;
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(250);

        try
        {
            await foreach (var item in GenerateNumbersAsync().WithCancellation(cts.Token))
            {
                sumCancel += item;
                Console.WriteLine($"   Получено: {item}, Текущая сумма: {sumCancel}");
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"   [Отмена] Операция была отменена токеном. Частичная сумма: {sumCancel}");
        }
    }
    #endregion

    #region Задание 8: Готовый результат и внешний сигнал (Вариант 3)
    // T = double. ValueTask<double>, ValueTask без результата (CompletedTask). 
    // TaskCompletionSource<double> в отдельном потоке.
    private static async Task Task8_ValueTaskAndTcs()
    {
        // Часть А: ValueTask<double>
        static ValueTask<double> GetDoubleAsync(bool isReady, double value)
        {
            if (isReady)
            {
                // Возврат готового значения сразу без выделения Task в куче
                return new ValueTask<double>(value);
            }

            // Асинхронное выполнение
            async Task<double> DelayedAsync()
            {
                await Task.Delay(100);
                return value;
            }

            return new ValueTask<double>(DelayedAsync());
        }

        static ValueTask DoEmptyWorkAsync()
        {
            // Возврат ValueTask без результата
            return ValueTask.CompletedTask;
        }

        // Вызовы и однократные await
        double res1 = await GetDoubleAsync(true, 3.14);
        double res2 = await GetDoubleAsync(false, 2.71);
        await DoEmptyWorkAsync();

        Console.WriteLine($"Часть А: res1 = {res1}, res2 = {res2}");

        // Часть Б: TaskCompletionSource<double> и отдельный поток
        var tcs = new TaskCompletionSource<double>();
        var thread = new Thread(() =>
        {
            Thread.Sleep(100);
            tcs.SetResult(42.5); // Сигнал и передача результата
        });

        thread.Start();

        double externalResult = await tcs.Task;
        Console.WriteLine($"Часть Б: Результат из TaskCompletionSource = {externalResult}");
    }
    #endregion

    #region Задание 9: Контекст продолжения (Вариант 1)
    // Метод возвращает Task<int> с задержкой 100 мс и результатом 42.
    // ConfigureAwait(true) и ConfigureAwait(false). Логирование Environment.CurrentManagedThreadId.
    private static async Task Task9_SynchronizationContext()
    {
        static async Task<int> Compute42Async()
        {
            await Task.Delay(100);
            return 42;
        }

        Console.WriteLine("--- Консольный контекст по умолчанию (SynchronizationContext.Current == null) ---");

        // Режим ConfigureAwait(true)
        Console.WriteLine($"[ConfigureAwait(true)]  До await: Thread ID = {Environment.CurrentManagedThreadId}");
        int resTrue = await Compute42Async().ConfigureAwait(true);
        Console.WriteLine($"[ConfigureAwait(true)]  После await: Thread ID = {Environment.CurrentManagedThreadId}, Результат = {resTrue}");

        // Режим ConfigureAwait(false)
        Console.WriteLine($"[ConfigureAwait(false)] До await: Thread ID = {Environment.CurrentManagedThreadId}");
        int resFalse = await Compute42Async().ConfigureAwait(false);
        Console.WriteLine($"[ConfigureAwait(false)] После await: Thread ID = {Environment.CurrentManagedThreadId}, Результат = {resFalse}");

        // Демонстрация с установленным SynchronizationContext (как в UI/WPF/WinForms)
        Console.WriteLine("\n--- Эксперимент с кастомным SynchronizationContext ---");
        var customContext = new SingleThreadSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(customContext);

        try
        {
            Console.WriteLine($"[С Контекстом] До await: Thread ID = {Environment.CurrentManagedThreadId}");

            // С true контекст восстанавливается (пост в очередь контекста)
            int resCtx = await Compute42Async().ConfigureAwait(true);
            Console.WriteLine($"[С Контекстом (true)] После await: Thread ID = {Environment.CurrentManagedThreadId}, Результат = {resCtx}");

            // С false контекст игнорируется и продолжение выполняется в потоке пула
            int resNoCtx = await Compute42Async().ConfigureAwait(false);
            Console.WriteLine($"[С Контекстом (false)] После await: Thread ID = {Environment.CurrentManagedThreadId}, Результат = {resNoCtx}");
        }
        finally
        {
            customContext.Complete();
            SynchronizationContext.SetSynchronizationContext(null);
        }
    }

    // Вспомогательный контекст для наглядной демонстрации влияния ConfigureAwait
    private class SingleThreadSynchronizationContext : SynchronizationContext
    {
        private readonly BlockingCollection<(SendOrPostCallback Callback, object? State)> _queue = new();
        private readonly Thread _thread;

        public SingleThreadSynchronizationContext()
        {
            _thread = new Thread(Run) { IsBackground = true };
            _thread.Start();
        }

        public override void Post(SendOrPostCallback d, object? state) => _queue.Add((d, state));

        private void Run()
        {
            SetSynchronizationContext(this);
            foreach (var (cb, state) in _queue.GetConsumingEnumerable())
            {
                cb(state);
            }
        }

        public void Complete() => _queue.CompleteAdding();
    }
    #endregion
}
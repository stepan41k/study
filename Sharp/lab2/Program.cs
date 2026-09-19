using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace MultithreadingLab;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Console.WriteLine("Task 1");
        Task1_TwoThreads();

        Console.WriteLine("\nTask 2");
        await Task2_AsyncSourcesAndPrimes();

        Console.WriteLine("\nTask 3");
        await Task3_ErrorsAndCancellation();

        Console.WriteLine("\nTask 4");
        await Task4_SemaphoreSlim();

        Console.WriteLine("\nTask 5");
        await Task5_WorkQueueAndStats();

        Console.WriteLine("\nTask 6");
        Task6_ParallelCalculations();

        Console.WriteLine("\nTask 7");
        await Task7_AsyncEnumerable();

        Console.WriteLine("\nTask 8");
        await Task8_ValueTaskAndTcs();

        Console.WriteLine("\nTask 9");
        await Task9_SynchronizationContext();

        Console.WriteLine("\nCompleted.");
    }

    #region Task1 1 (Variant 3)
    private static void Task1_TwoThreads()
    {
        const string input = "C# async 2026";
        int letterCount = 0;
        int digitCount = 0;

        Thread letterThread = new Thread(() =>
        {
            Console.WriteLine("[Буквы] Начало работы.");
            Thread.Sleep(100);
            letterCount = input.Count(char.IsLetter);
            Console.WriteLine("[Буквы] Конец работы.");
        });

        Thread digitThread = new Thread(() =>
        {
            Console.WriteLine("[Цифры] Начало работы.");
            Thread.Sleep(100);
            digitCount = input.Count(char.IsDigit);
            Console.WriteLine("[Цифры] Конец работы.");
        });

        letterThread.Start();
        digitThread.Start();

        letterThread.Join();
        digitThread.Join();

        Console.WriteLine($"Результат: Букв = {letterCount}, Цифр = {digitCount}");
    }
    #endregion

    #region Task 2 (Variant 3)

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

        Task<int[]> firstTask = await Task.WhenAny(allTasks);
        int[] firstResult = await firstTask;
        Console.WriteLine($"Первый полученный набор: [{string.Join(", ", firstResult)}]");

        int[][] allResults = await Task.WhenAll(allTasks);
        int[] mergedData = allResults.SelectMany(x => x).ToArray();

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

    #region Task 3 (Variant 2)
    private static async Task Task3_ErrorsAndCancellation()
    {
        async Task<int> DivideOpAsync(int d, CancellationToken token)
        {
            await Task.Delay(100, token);
            return 100 / d;
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
                // Exception
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

        await RunExperiment("Запуск 1: Без отмены", CancellationToken.None);

        using var preCanceledCts = new CancellationTokenSource();
        preCanceledCts.Cancel();
        await RunExperiment("Запуск 2: С заранее отмененным токеном", preCanceledCts.Token);

        using var timeoutCts = new CancellationTokenSource();
        timeoutCts.CancelAfter(50);
        await RunExperiment("Запуск 3: С CancelAfter(50)", timeoutCts.Token);
    }
    #endregion

    #region Task 4 (Variant 3)
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

    #region Task 5 (Variant 3)
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

    #region Task 6 (Variant 1)
    private static void Task6_ParallelCalculations()
    {
        long[] seqArray = new long[100];
        long[] parArray = new long[100];

        for (int i = 0; i < 100; i++)
        {
            long n = i + 1;
            seqArray[i] = n * n;
        }

        Parallel.For(0, 100, i =>
        {
            long n = i + 1;
            parArray[i] = n * n;
        });

        bool areEqual = seqArray.SequenceEqual(parArray);
        Console.WriteLine($"Массивы совпадают: {areEqual}");

        long totalSum = 0;
        Parallel.ForEach(parArray, val =>
        {
            Interlocked.Add(ref totalSum, val);
        });

        Console.WriteLine($"Итоговая сумма: {totalSum} (Ожидается: 338350)");
    }
    #endregion

    #region Task 7 (Variant 1)
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

        Console.WriteLine("1. Полный проход:");
        int sumFull = 0;
        await foreach (var item in GenerateNumbersAsync())
        {
            sumFull += item;
            Console.WriteLine($"   Получено: {item}, Текущая сумма: {sumFull}");
        }
        Console.WriteLine($"   Результат полного прохода: сумма = {sumFull}");

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

    #region Task 8 (Variant 3)
    private static async Task Task8_ValueTaskAndTcs()
    {
        static ValueTask<double> GetDoubleAsync(bool isReady, double value)
        {
            if (isReady)
            {
                return new ValueTask<double>(value);
            }

            async Task<double> DelayedAsync()
            {
                await Task.Delay(100);
                return value;
            }

            return new ValueTask<double>(DelayedAsync());
        }

        static ValueTask DoEmptyWorkAsync()
        {
            return ValueTask.CompletedTask;
        }

        double res1 = await GetDoubleAsync(true, 3.14);
        double res2 = await GetDoubleAsync(false, 2.71);
        await DoEmptyWorkAsync();

        Console.WriteLine($"Часть А: res1 = {res1}, res2 = {res2}");

        var tcs = new TaskCompletionSource<double>();
        var thread = new Thread(() =>
        {
            Thread.Sleep(100);
            tcs.SetResult(42.5);
        });

        thread.Start();

        double externalResult = await tcs.Task;
        Console.WriteLine($"Часть Б: Результат из TaskCompletionSource = {externalResult}");
    }
    #endregion

    #region Task 9 (Variant 1)
    private static async Task Task9_SynchronizationContext()
    {
        static async Task<int> Compute42Async()
        {
            await Task.Delay(100);
            return 42;
        }

        Console.WriteLine("--- Консольный контекст по умолчанию (SynchronizationContext.Current == null) ---");

        Console.WriteLine($"[ConfigureAwait(true)]  До await: Thread ID = {Environment.CurrentManagedThreadId}");
        int resTrue = await Compute42Async().ConfigureAwait(true);
        Console.WriteLine($"[ConfigureAwait(true)]  После await: Thread ID = {Environment.CurrentManagedThreadId}, Результат = {resTrue}");

        Console.WriteLine($"[ConfigureAwait(false)] До await: Thread ID = {Environment.CurrentManagedThreadId}");
        int resFalse = await Compute42Async().ConfigureAwait(false);
        Console.WriteLine($"[ConfigureAwait(false)] После await: Thread ID = {Environment.CurrentManagedThreadId}, Результат = {resFalse}");

        Console.WriteLine("\n--- Эксперимент с кастомным SynchronizationContext ---");
        var customContext = new SingleThreadSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(customContext);

        try
        {
            Console.WriteLine($"[С Контекстом] До await: Thread ID = {Environment.CurrentManagedThreadId}");

            int resCtx = await Compute42Async().ConfigureAwait(true);
            Console.WriteLine($"[С Контекстом (true)] После await: Thread ID = {Environment.CurrentManagedThreadId}, Результат = {resCtx}");

            int resNoCtx = await Compute42Async().ConfigureAwait(false);
            Console.WriteLine($"[С Контекстом (false)] После await: Thread ID = {Environment.CurrentManagedThreadId}, Результат = {resNoCtx}");
        }
        finally
        {
            customContext.Complete();
            SynchronizationContext.SetSynchronizationContext(null);
        }
    }

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
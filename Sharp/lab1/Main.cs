using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using lab1;

List<ProjectTask> taskList = new();

while (true)
{
    Console.Clear();
    Console.WriteLine(" 1. Task 2 & 4: Ввести задачу с консоли вручную и вывести");
    Console.WriteLine(" 2. Task 6-8: Add, Insert, Remove");
    Console.WriteLine(" 3. Task 9-10: HashSet (Объединение, Пересечение, Разность)");
    Console.WriteLine(" 4. Task 11: History (Undo через Stack)");
    Console.WriteLine(" 5. Task 12-15: Иерархия (BFS, DFS, stat)");
    Console.WriteLine(" 6. Task 16: LINQ (Where, OrderBy, Min/Max/Avg, GroupBy)");
    Console.WriteLine(" 7. Task 17: LINQ");
    Console.WriteLine(" 8. Task 18: LINQ");
    Console.WriteLine(" 9. Task 19 & 20: LINQ");
    Console.WriteLine(" 0. Exit");
    Console.Write("Select menu item (0-9): ");

    string? choice = Console.ReadLine()?.Trim();
    Console.WriteLine();

    switch (choice)
    {
        case "1":
            RunTask2And4();
            break;

        case "2":
            RunTask8();
            break;

        case "3":
            RunTask9And10();
            break;

        case "4":
            RunTask11();
            break;

        case "5":
            RunTask12To15();
            break;

        case "6":
            RunTask16();
            break;

        case "7":
            RunTask17Demo();
            break;

        case "8":
            RunTask18Demo();
            break;

        case "9":
            RunTask19And20Demo();
            break;

        case "0":
            Console.WriteLine("Exit...");
            return;

        default:
            Console.WriteLine("Invalid input! Try again.");
            break;
    }

    Console.WriteLine("\nPress Enter to return to the main menu...");
    Console.ReadLine();
}


void RunTask2And4()
{
    Console.WriteLine("Task 2 and 4: Creating object throw the console");
    ProjectTask task = TaskManager.ReadFromConsole();
    taskList.Add(task);

    Console.WriteLine("\nResult for ToString()");
    Console.WriteLine(task);
    Console.WriteLine($"\nTask successfully created and added to the working list. Total tasks: {taskList.Count}");
}

void RunTask8()
{
    Console.WriteLine("Tasks 6-8: Generating and working with the list");
    Console.Write("How many random tasks to generate? (e.g., 8): ");
    if (!int.TryParse(Console.ReadLine(), out int count) || count <= 0) count = 8;

    taskList = TaskManager.GenerateStream().Take(count).ToList();
    Console.WriteLine($"\nGenerated {taskList.Count} tasks:");
    PrintTaskList(taskList);

    Console.WriteLine("\nAdd:");
    var newTask = TaskManager.GenerateRandomTask();
    taskList.Add(newTask);
    Console.WriteLine($"Added: {newTask}");

    Console.WriteLine("\nInsert:");
    
    int insertIndex;
    while (true)
    {
        Console.Write($"Input index for insert (from 0 to {taskList.Count}): ");
        if (int.TryParse(Console.ReadLine(), out insertIndex) && insertIndex >= 0 && insertIndex <= taskList.Count)
        {
            break;
        }
        Console.WriteLine($"Err: index must be an integer in the range from 0 to {taskList.Count}.");
    }
    
    var insertedTask = TaskManager.GenerateRandomTask();
    taskList.Insert(insertIndex, insertedTask);
    Console.WriteLine($"Inserted (at position {insertIndex}): {insertedTask}");

    Console.WriteLine("\nRemoveAt:");
    
    if (taskList.Count == 0)
    {
        Console.WriteLine("List empty, deletion not possible");
    }
    else
    {
        int removeIndex;
        while (true)
        {
            Console.Write($"Input index for deletion (from 0 to {taskList.Count - 1}): ");
            if (int.TryParse(Console.ReadLine(), out removeIndex) && removeIndex >= 0 && removeIndex < taskList.Count)
            {
                break;
            }
            Console.WriteLine($"Error: index must be an integer in the range from 0 to {taskList.Count - 1}.");
        }
    
        var taskToRemove = taskList[removeIndex];
        taskList.RemoveAt(removeIndex);
        Console.WriteLine($"Deleted task (index {removeIndex}): \"{taskToRemove.Title}\" (ID: {taskToRemove.Id})");
    }

    Console.WriteLine($"\nList size: {taskList.Count}");
}

void RunTask9And10()
{
    Console.WriteLine("Tasks 9-10: HashSet<T>");
    if (taskList.Count < 5)
    {
        Console.WriteLine("Too few tasks, generating 10 new tasks...");
        taskList = TaskManager.GenerateStream().Take(10).ToList();
    }

    var set1 = new HashSet<ProjectTask>(taskList.Where(t => t.EstimatedHours > 15));
    var set2 = new HashSet<ProjectTask>(taskList.Where(t => t.Status is TaskProgress.InProgress or TaskProgress.Review));

    Console.WriteLine($"Set 1 (Часы > 15h): {set1.Count} шт.");
    foreach (var t in set1) Console.WriteLine($"   {t}");

    Console.WriteLine($"\nSet 2 (В работе / На ревью): {set2.Count} шт.");
    foreach (var t in set2) Console.WriteLine($"   {t}");

    Console.WriteLine("\n--- Операции над множествами ---");

    var union = new HashSet<ProjectTask>(set1);
    union.UnionWith(set2);
    Console.WriteLine($"Union (UnionWith) -> elements: {union.Count}");

    var intersect = new HashSet<ProjectTask>(set1);
    intersect.IntersectWith(set2);
    Console.WriteLine($"Intersect (IntersectWith) -> elements: {intersect.Count}");

    var diff = new HashSet<ProjectTask>(set1);
    diff.ExceptWith(set2);
    Console.WriteLine($"Exception (ExceptWith: Set1 \\ Set2) -> elements: {diff.Count}");
}

void RunTask11()
{
    Console.WriteLine("Task 11: History (Stack<T> + Undo)");
    var task = taskList.FirstOrDefault() ?? TaskManager.GenerateRandomTask();

    var history = new Dictionary<ProjectTask, Stack<double>>
    {
        [task] = new Stack<double>()
    };

    history[task].Push(task.EstimatedHours);
    Console.WriteLine($"Selected task: #{task.Id} '{task.Title}'");
    Console.WriteLine($"Initial estimated hours: {task.EstimatedHours} ч.");

    for (int i = 1; i <= 2; i++)
    {
        Console.Write($"\nInput new value for change #{i} (h): ");
        if (double.TryParse(Console.ReadLine()?.Replace('.', ','), out double newHours))
        {
            task.EstimatedHours = newHours;
            history[task].Push(newHours);
            Console.WriteLine($"Property changed to: {task.EstimatedHours} h. Value saved to stack");
        }
    }

    Console.WriteLine($"\nCurrent value of property: {task.EstimatedHours} h.");
    Console.WriteLine("Press Enter to undo the last change...");
    Console.ReadLine();

    if (history[task].Count > 1)
    {
        double removed = history[task].Pop();
        task.EstimatedHours = history[task].Peek();
        Console.WriteLine($"Undo value: {removed} h.");
        Console.WriteLine($"Property restored to: {task.EstimatedHours} h.");
    }
}

void RunTask12To15()
{
    Console.WriteLine("Tasks 12-15:");
    var root = TaskManager.GenerateRandomTask();
    root.Title = "Project root";

    var sub1 = TaskManager.GenerateRandomTask();
    sub1.Title = "Authorization module";

    var sub2 = TaskManager.GenerateRandomTask();
    sub2.Title = "Reporting module";

    var sub1_1 = TaskManager.GenerateRandomTask();
    sub1_1.Title = "OAuth integration";

    root.AddSubTask(sub1);
    root.AddSubTask(sub2);
    sub1.AddSubTask(sub1_1);

    TaskManager.BreadthFirstSearch(root);
    Console.WriteLine("\nDFS:");
    var dfsList = TaskManager.DepthFirstSearch(root).ToList();
    foreach (var item in dfsList)
    {
        Console.WriteLine($"  -> {item}");
    }

    Console.WriteLine("\nStatistics:");
    Console.WriteLine($"- Number of nodes: {dfsList.Count}");
    Console.WriteLine($"- Node with max date: {dfsList.MaxBy(x => x.CreatedDate)?.CreatedDate:yyyy-MM-dd}");
    Console.WriteLine($"- Node with min date: {dfsList.MinBy(x => x.CreatedDate)?.CreatedDate:yyyy-MM-dd}");
    Console.WriteLine($"- Total estimated hours: {dfsList.Sum(x => x.EstimatedHours):F1} h.");
}

void RunTask16()
{
    Console.WriteLine("Task 16: LINQ-запросы");
    if (taskList.Count < 10)
    {
        taskList = TaskManager.GenerateStream().Take(15).ToList();
    }

    Console.WriteLine($"Current dataset ({taskList.Count} tasks):");
    PrintTaskList(taskList);

    Console.WriteLine("\n1. Where:");
    var filtered = taskList.Where(t => t.EstimatedHours >= 12).ToList();
    PrintTaskList(filtered);

    Console.WriteLine("\n2. OrderBy, ThenBy:");
    var sorted = taskList.OrderBy(t => t.Category).ThenByDescending(t => t.EstimatedHours).ToList();
    PrintTaskList(sorted.Take(5));
    Console.WriteLine("   ... (showed first 5 for brevity)");

    Console.WriteLine("\n3. Select, Distinct:");
    var assignees = taskList.Select(t => t.Assignee).Distinct();
    Console.WriteLine("   " + string.Join(", ", assignees));

    Console.WriteLine("\n4. First / Single:");
    var firstTesting = taskList.FirstOrDefault(t => t.Category == TaskCategory.Testing);
    Console.WriteLine($"   First testing task: {(firstTesting != null ? firstTesting.Title : "not found")}");
FirstOrDefault
    Console.WriteLine("\n5. Statistics: Count, Min, Max, Average, Sum:");
    Console.WriteLine($"   Total tasks (Count): {taskList.Count}");
    Console.WriteLine($"   Minimum hours (Min): {taskList.Min(t => t.EstimatedHours):F1} ч.");
    Console.WriteLine($"   Maximum hours (Max): {taskList.Max(t => t.EstimatedHours):F1} ч.");
    Console.WriteLine($"   Average time (Average): {taskList.Average(t => t.EstimatedHours):F1} ч.");
    Console.WriteLine($"   Sum of all hours (Sum): {taskList.Sum(t => t.EstimatedHours):F1} ч.");

    Console.WriteLine("\n6. GroupBy:");
    var groups = taskList.GroupBy(t => t.Category);
    foreach (var g in groups)
    {
        Console.WriteLine($"\n* Category: {g.Key} (Elements: {g.Count()})");
        Console.WriteLine($"  Min date: {g.Min(x => x.CreatedDate):yyyy-MM-dd} | Max date: {g.Max(x => x.CreatedDate):yyyy-MM-dd}");
        Console.WriteLine($"  Average time: {g.Average(x => x.EstimatedHours):F1} ч.");
        foreach (var item in g)
        {
            Console.WriteLine($"     -> ID: {item.Id}, '{item.Title}' ({item.EstimatedHours}h)");
        }
    }
}

void RunTask17Demo()
{
    Console.WriteLine("Task 17:");
    var grades = new List<StudentGrade>
    {
        new() { Form = 9, LastName = "Иванов", Initials = "И.И.", Subject = "Алгебра", Grade = 5 },
        new() { Form = 9, LastName = "Иванов", Initials = "И.И.", Subject = "Геометрия", Grade = 5 },
        new() { Form = 9, LastName = "Иванов", Initials = "И.И.", Subject = "Информатика", Grade = 4 },
        new() { Form = 9, LastName = "Петров", Initials = "П.П.", Subject = "Алгебра", Grade = 5 },
        new() { Form = 9, LastName = "Петров", Initials = "П.П.", Subject = "Информатика", Grade = 3 },
        new() { Form = 10, LastName = "Сидоров", Initials = "С.С.", Subject = "Алгебра", Grade = 5 },
        new() { Form = 10, LastName = "Сидоров", Initials = "С.С.", Subject = "Геометрия", Grade = 5 },
        new() { Form = 11, LastName = "Алексеев", Initials = "А.А.", Subject = "Информатика", Grade = 5 }
    };

    string file = "result17.txt";
    Task17.Run(grades, file);

    Console.WriteLine($"File '{file}' generated with the following content:\n");
    Console.WriteLine(File.ReadAllText(file));
}

void RunTask18Demo()
{
    Console.WriteLine("Task 18: Consumers and discounts by streets");
    var consumers = new List<ConsumerA>
    {
        new() { ConsumerCode = 101, BirthYear = 1995, Street = "Ленина" },
        new() { ConsumerCode = 102, BirthYear = 1992, Street = "Ленина" },
        new() { ConsumerCode = 103, BirthYear = 2000, Street = "Мира" }
    };

    var discounts = new List<DiscountC>
    {
        new() { ConsumerCode = 101, StoreName = "Магнит", DiscountPercent = 10 },
        new() { ConsumerCode = 102, StoreName = "Магнит", DiscountPercent = 5 },
        new() { ConsumerCode = 103, StoreName = "Пятерочка", DiscountPercent = 15 }
    };

    Task18.Run(consumers, discounts);
}

void RunTask19And20Demo()
{
    Console.WriteLine("Task 19 and 20: Sales and total purchase cost");
    var consumers = new List<ConsumerA>
    {
        new() { ConsumerCode = 101, BirthYear = 1995, Street = "Ленина" },
        new() { ConsumerCode = 102, BirthYear = 1995, Street = "Ленина" },
        new() { ConsumerCode = 103, BirthYear = 2000, Street = "Мира" }
    };

    var discounts = new List<DiscountC>
    {
        new() { ConsumerCode = 101, StoreName = "Магнит", DiscountPercent = 10 },
        new() { ConsumerCode = 102, StoreName = "Магнит", DiscountPercent = 5 }
    };

    var prices = new List<PriceD>
    {
        new() { Article = "A-1", Price = 150.75m, StoreName = "Магнит" },
        new() { Article = "A-2", Price = 300.00m, StoreName = "Магнит" },
        new() { Article = "B-1", Price = 50.00m, StoreName = "Пятерочка" }
    };

    var purchases = new List<PurchaseE>
    {
        new() { Article = "A-1", ConsumerCode = 101, StoreName = "Магнит" },
        new() { Article = "A-2", ConsumerCode = 101, StoreName = "Магнит" },
        new() { Article = "A-1", ConsumerCode = 102, StoreName = "Магнит" },
        new() { Article = "B-1", ConsumerCode = 103, StoreName = "Пятерочка" }
    };

    Console.WriteLine("\nTask 19: Sum of discounts by consumer-store pairs:");
    Task19.Run(discounts, prices, purchases, "result19.txt");
    Console.WriteLine(File.ReadAllText("result19.txt"));

    Console.WriteLine("Task 20:");
    Task20.Run(consumers, discounts, prices, purchases);
}

void PrintTaskList(IEnumerable<ProjectTask> list)
{
    foreach (var item in list)
    {
        Console.WriteLine("  " + item);
    }
}
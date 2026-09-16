using System;
using System.Collections.Generic;
using System.Linq;

namespace lab1;

public enum TaskCategory { Development, Design, Testing, Documentation, Management }
public enum TaskProgress { Todo, InProgress, Review, Done }

public class ProjectTask : IEquatable<ProjectTask>
{
    public int Id { get; set; }
    public double EstimatedHours { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Assignee { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; }

    public TaskCategory Category { get; set; }
    public TaskProgress Status { get; set; }

    public string[] Tags { get; set; } = Array.Empty<string>();

    public ProjectTask? Parent { get; set; }
    public List<ProjectTask> SubTasks { get; set; } = new();

    public void AddSubTask(ProjectTask child)
    {
        child.Parent = this;
        SubTasks.Add(child);
    }

    public override string ToString()
    {
        string tagsStr = Tags.Length > 0 ? string.Join(", ", Tags) : "no";
        return $"Task #{Id} '{Title}' | Assignee: {Assignee} | Hours: {EstimatedHours:F1}h | " +
               $"Date: {CreatedDate:yyyy-MM-dd} | Category: {Category} | Status: {Status} | Tags: [{tagsStr}]";
    }

    public bool Equals(ProjectTask? other) => other is not null && Id == other.Id;
    public override bool Equals(object? obj) => Equals(obj as ProjectTask);
    public override int GetHashCode() => Id.GetHashCode();
}

public static class TaskManager
{
    public static ProjectTask ReadFromConsole()
    {
        Console.WriteLine("Input:");
        Console.Write("Input ID: ");
        int id = int.Parse(Console.ReadLine() ?? "1");

        Console.Write("Input title: ");
        string title = Console.ReadLine() ?? "New Task";

        Console.Write("Input assignee: ");
        string assignee = Console.ReadLine() ?? "Ivanov I.I.";

        Console.Write("Input hours: ");
        double hours = double.Parse(Console.ReadLine() ?? "8");

        Console.Write("Input date (yyyy-MM-dd) or Enter for current: ");
        string? dateStr = Console.ReadLine();
        DateTime date = string.IsNullOrWhiteSpace(dateStr) ? DateTime.Now : DateTime.Parse(dateStr);

        Console.WriteLine("Input category (0-Development, 1-Design, 2-Testing, 3-Documentation, 4-Management): ");
        TaskCategory category = (TaskCategory)int.Parse(Console.ReadLine() ?? "0");

        Console.WriteLine("Input status (0-Todo, 1-InProgress, 2-Review, 3-Done): ");
        TaskProgress status = (TaskProgress)int.Parse(Console.ReadLine() ?? "0");

        Console.Write("Input tags: ");
        string[] tags = (Console.ReadLine() ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return new ProjectTask
        {
            Id = id,
            Title = title,
            Assignee = assignee,
            EstimatedHours = hours,
            CreatedDate = date,
            Category = category,
            Status = status,
            Tags = tags
        };
    }

    private static readonly Random Rnd = new();
    private static readonly string[] Titles = { "Верстка UI", "Рефакторинг API", "Написание тестов", "Развертывание в облаке", "Анализ требований", "Оптимизация SQL" };
    private static readonly string[] People = { "Ivanov", "Petrov", "Sidorov", "Kuznecov", "Smirnov" };
    private static readonly string[][] TagsList = {
        new[] { "Backend", "C#" },
        new[] { "Frontend", "CSS" },
        new[] { "Database" },
        new[] { "Security", "Auth" },
        new[] { "Urgent", "Bug" }
    };

    private static int _nextId = 1;

    public static ProjectTask GenerateRandomTask()
    {
        return new ProjectTask
        {
            Id = _nextId++,
            Title = Titles[Rnd.Next(Titles.Length)],
            Assignee = People[Rnd.Next(People.Length)],
            EstimatedHours = Math.Round(Rnd.NextDouble() * 38 + 2, 1),
            CreatedDate = DateTime.Today.AddDays(-Rnd.Next(0, 60)),
            Category = (TaskCategory)Rnd.Next(Enum.GetValues<TaskCategory>().Length),
            Status = (TaskProgress)Rnd.Next(Enum.GetValues<TaskProgress>().Length),
            Tags = TagsList[Rnd.Next(TagsList.Length)]
        };
    }

    public static IEnumerable<ProjectTask> GenerateStream()
    {
        while (true)
        {
            yield return GenerateRandomTask();
        }
    }

    public static void BreadthFirstSearch(ProjectTask root)
    {
        var queue = new Queue<ProjectTask>();
        queue.Enqueue(root);

        Console.WriteLine("\nBFS:");
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            Console.WriteLine($"Node #{current.Id} ('{current.Title}'), Children: {current.SubTasks.Count}");
            foreach (var child in current.SubTasks)
            {
                queue.Enqueue(child);
            }
        }
    }

    public static IEnumerable<ProjectTask> DepthFirstSearch(ProjectTask node)
    {
        yield return node;
        foreach (var child in node.SubTasks)
        {
            foreach (var desc in DepthFirstSearch(child))
            {
                yield return desc;
            }
        }
    }
}

public static class TasksDemo
{
    public static void Run()
    {
        Console.WriteLine("Console input/output");

        int n = 10;
        List<ProjectTask> list = TaskManager.GenerateStream().Take(n).ToList();

        Console.WriteLine($"\nList из {n} elements");
        list.Add(TaskManager.GenerateRandomTask());       
        list.Insert(2, TaskManager.GenerateRandomTask());  
        list.RemoveAt(0);                                  

        Console.WriteLine($"Result list length: {list.Count}");

        var set1 = new HashSet<ProjectTask>(list.Where(t => t.Category == TaskCategory.Development));
        var set2 = new HashSet<ProjectTask>(list.Where(t => t.EstimatedHours > 15));

        Console.WriteLine("\nSets");
        var union = new HashSet<ProjectTask>(set1);
        union.UnionWith(set2);
        Console.WriteLine($"Union: {union.Count} elements");

        var intersect = new HashSet<ProjectTask>(set1);
        intersect.IntersectWith(set2);
        Console.WriteLine($"Intersect: {intersect.Count} elements");

        var diff = new HashSet<ProjectTask>(set1);
        diff.ExceptWith(set2);
        Console.WriteLine($"Diff (Set1 \\ Set2): {diff.Count} elements");

        Console.WriteLine("\nHistory Undo");
        var history = new Dictionary<ProjectTask, Stack<double>>();
        var sampleTask = list.First();

        history[sampleTask] = new Stack<double>();
        history[sampleTask].Push(sampleTask.EstimatedHours);
        Console.WriteLine($"Start time: {sampleTask.EstimatedHours}h");

        sampleTask.EstimatedHours = 25.0;
        history[sampleTask].Push(sampleTask.EstimatedHours);
        Console.WriteLine($"Changed to: {sampleTask.EstimatedHours}h");

        sampleTask.EstimatedHours = 40.0;
        history[sampleTask].Push(sampleTask.EstimatedHours);
        Console.WriteLine($"Changed to: {sampleTask.EstimatedHours}h");

        history[sampleTask].Pop();
        sampleTask.EstimatedHours = history[sampleTask].Peek();
        Console.WriteLine($"After Undo (Pop): {sampleTask.EstimatedHours}h");

        Console.WriteLine("\nИерархия задач");
        var root = TaskManager.GenerateRandomTask();
        var child1 = TaskManager.GenerateRandomTask();
        var child2 = TaskManager.GenerateRandomTask();
        var subChild1 = TaskManager.GenerateRandomTask();

        root.AddSubTask(child1);
        root.AddSubTask(child2);
        child1.AddSubTask(subChild1);

        TaskManager.BreadthFirstSearch(root);
        var dfsSequence = TaskManager.DepthFirstSearch(root).ToList();

        Console.WriteLine("\nTree stat");
        Console.WriteLine($"Count: {dfsSequence.Count}");
        Console.WriteLine($"Max date: {dfsSequence.MaxBy(t => t.CreatedDate)}");
        Console.WriteLine($"Min date: {dfsSequence.MinBy(t => t.CreatedDate)}");
        Console.WriteLine($"Sum of hours: {dfsSequence.Sum(t => t.EstimatedHours)}");

        Console.WriteLine("\nLINQ demo");
        var bigList = TaskManager.GenerateStream().Take(20).ToList();

        var query = bigList
            .Where(t => t.EstimatedHours >= 10)
            .OrderBy(t => t.Category)
            .ThenByDescending(t => t.EstimatedHours)
            .Select(t => t.Assignee)
            .Distinct();
        Console.WriteLine($"Unique executors of big tasks: {string.Join(", ", query)}");

        var firstDev = bigList.FirstOrDefault(t => t.Category == TaskCategory.Development);
        Console.WriteLine($"First task of development: {(firstDev != null ? firstDev.Title : "not found")}");

        Console.WriteLine($"Count: {bigList.Count(t => t.Status == TaskProgress.Done)} done");
        Console.WriteLine($"Min hours: {bigList.Min(t => t.EstimatedHours)}");
        Console.WriteLine($"Max hours: {bigList.Max(t => t.EstimatedHours)}");
        Console.WriteLine($"Average hours: {bigList.Average(t => t.EstimatedHours):F2}");
        Console.WriteLine($"Sum hours: {bigList.Sum(t => t.EstimatedHours):F1}");

        Console.WriteLine("\nGrouping by category");
        var groups = bigList.GroupBy(t => t.Category);
        foreach (var g in groups)
        {
            Console.WriteLine($"Group: {g.Key} | Tasks: {g.Count()} | " +
                              $"Min date: {g.Min(x => x.CreatedDate):yyyy-MM-dd} | " +
                              $"Max date: {g.Max(x => x.CreatedDate):yyyy-MM-dd} | " +
                              $"Average hours: {g.Average(x => x.EstimatedHours):F1}");
        }
    }
}

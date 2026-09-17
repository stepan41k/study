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
        return $"ID: {Id,2} \"{Title,-20}\" | Executor: {Assignee,-10} | {EstimatedHours,4:F1} h. | " +
               $"{CreatedDate:yyyy-MM-dd} | {Category,-13} | {Status,-10} | Tags: [{tagsStr}]";
    }

    public bool Equals(ProjectTask? other) => other is not null && Id == other.Id;
    public override bool Equals(object? obj) => Equals(obj as ProjectTask);
    public override int GetHashCode() => Id.GetHashCode();
}

public static class TaskManager
{
    public static ProjectTask ReadFromConsole()
    {
        Console.WriteLine("\nData input");

        int id;
        while (true)
        {
            Console.Write("Input ID: ");
            if (int.TryParse(Console.ReadLine(), out id)) break;
            Console.WriteLine("Error: input valid number");
        }

        Console.Write("Input title: ");
        string title = Console.ReadLine()?.Trim() ?? "";
        if (string.IsNullOrEmpty(title)) title = "Nameless";

        Console.Write("Input executor: ");
        string assignee = Console.ReadLine()?.Trim() ?? "";
        if (string.IsNullOrEmpty(assignee)) assignee = "Not assigned";

        double hours;
        while (true)
        {
            Console.Write("Input estimated hours: ");
            if (double.TryParse(Console.ReadLine()?.Replace('.', ','), out hours) && hours >= 0) break;
            Console.WriteLine("Error: input valid number");
        }

        DateTime date;
        while (true)
        {
            Console.Write("Input date (yyyy-mm-dd) or press Enter for today: ");
            string? input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input))
            {
                date = DateTime.Today;
                break;
            }
            if (DateTime.TryParse(input, out date)) break;
            Console.WriteLine("Error: invalid date format");
        }

        Console.WriteLine("Select category:");
        var categories = Enum.GetValues<TaskCategory>();
        for (int i = 0; i < categories.Length; i++)
            Console.WriteLine($"  {i} - {categories[i]}");
        int catIdx;
        while (true)
        {
            Console.Write("Your choice: ");
            if (int.TryParse(Console.ReadLine(), out catIdx) && catIdx >= 0 && catIdx < categories.Length) break;
            Console.WriteLine("Error: select number from list");
        }

        Console.WriteLine("Select status:");
        var statuses = Enum.GetValues<TaskProgress>();
        for (int i = 0; i < statuses.Length; i++)
            Console.WriteLine($"  {i} - {statuses[i]}");
        int statusIdx;
        while (true)
        {
            Console.Write("Your choice: ");
            if (int.TryParse(Console.ReadLine(), out statusIdx) && statusIdx >= 0 && statusIdx < statuses.Length) break;
            Console.WriteLine("Error: select number from list");
        }

        Console.Write("Input tags (comma-separated, f.e. Backend, Design, Interface): ");
        string[] tags = (Console.ReadLine() ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return new ProjectTask
        {
            Id = id,
            Title = title,
            Assignee = assignee,
            EstimatedHours = hours,
            CreatedDate = date,
            Category = (TaskCategory)catIdx,
            Status = (TaskProgress)statusIdx,
            Tags = tags
        };
    }

    private static readonly Random Rnd = new();
    private static readonly string[] Titles = { "Верстка UI", "Рефакторинг API", "Написание тестов", "CI/CD пайплайн", "Оптимизация БД", "Аналитика фичи", "Исправление бага" };
    private static readonly string[] People = { "Иванов", "Петров", "Сидоров", "Кузнецов", "Смирнов", "Попов" };
    private static readonly string[][] TagsList = {
        new[] { "Backend", "C#" },
        new[] { "Frontend", "CSS" },
        new[] { "Database", "SQL" },
        new[] { "DevOps", "Docker" },
        new[] { "Bug", "Urgent" }
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
            CreatedDate = DateTime.Today.AddDays(-Rnd.Next(0, 45)),
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
            Console.WriteLine($"Node #{current.Id} \"{current.Title}\" | Childrens: {current.SubTasks.Count}");
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
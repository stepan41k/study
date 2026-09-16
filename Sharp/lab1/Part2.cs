using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace lab1;

public class StudentGrade
{
    public int Grade { get; set; }
    public int Form { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string Initials { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
}

public static class Task17
{
    public static void Run(IEnumerable<StudentGrade> source, string outputPath)
    {
        var students = source.GroupBy(g => new { g.Form, g.LastName, g.Initials })
            .Select(g => new
            {
                g.Key.Form,
                g.Key.LastName,
                g.Key.Initials,
                HasBadGrades = g.Any(x => x.Grade == 2 || x.Grade == 3),
                FivesCount = g.Count(x => x.Grade == 5)
            })
            .Where(s => !s.HasBadGrades && s.FivesCount > 0);

        var result = students
            .GroupBy(s => s.Form)
            .SelectMany(classGroup =>
            {
                int maxFives = classGroup.Max(s => s.FivesCount);
                return classGroup.Where(s => s.FivesCount == maxFives);
            })
            .OrderBy(s => s.Form)
            .ThenBy(s => s.LastName)
            .ThenBy(s => s.Initials)
            .ToList();

        using var sw = new StreamWriter(outputPath);
        if (result.Count == 0)
        {
            sw.WriteLine("Students not found");
        }
        else
        {
            foreach (var item in result)
            {
                sw.WriteLine($"{item.Form} {item.LastName} {item.Initials} {item.FivesCount}");
            }
        }
    }
}

public class ConsumerA
{
    public int BirthYear { get; set; }
    public int ConsumerCode { get; set; }
    public string Street { get; set; } = string.Empty;
}

public class DiscountC
{
    public int ConsumerCode { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public double DiscountPercent { get; set; }
}

public class PriceD
{
    public string Article { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string StoreName { get; set; } = string.Empty;
}

public class PurchaseE
{
    public string Article { get; set; } = string.Empty;
    public int ConsumerCode { get; set; }
    public string StoreName { get; set; } = string.Empty;
}

public static class Task18
{
    public static void Run(IEnumerable<ConsumerA> seqA, IEnumerable<DiscountC> seqC)
    {
        var query = seqA.Join(seqC,
                a => a.ConsumerCode,
                c => c.ConsumerCode,
                (a, c) => new { a.Street, c.StoreName, a.ConsumerCode })
            .Distinct()
            .GroupBy(x => new { x.StoreName, x.Street })
            .Select(g => new
            {
                g.Key.StoreName,
                g.Key.Street,
                ConsumerCount = g.Count()
            })
            .OrderBy(x => x.StoreName)
            .ThenBy(x => x.Street);

        Console.WriteLine("\nConsumers with discount by store and street:");
        foreach (var item in query)
        {
            Console.WriteLine($"{item.StoreName} | {item.Street} | {item.ConsumerCount}");
        }
    }
}

public static class Task19
{
    public static void Run(IEnumerable<DiscountC> seqC, IEnumerable<PriceD> seqD, IEnumerable<PurchaseE> seqE, string outputPath)
    {
        var purchasesWithPrice = seqE.Join(seqD,
            e => new { e.Article, e.StoreName },
            d => new { d.Article, d.StoreName },
            (e, d) => new { e.ConsumerCode, e.StoreName, d.Price });

        var query = purchasesWithPrice.Join(seqC,
            p => new { p.ConsumerCode, p.StoreName },
            c => new { c.ConsumerCode, c.StoreName },
            (p, c) => new
            {
                p.ConsumerCode,
                p.StoreName,
                DiscountAmount = Math.Truncate(p.Price * (decimal)c.DiscountPercent / 100m)
            })
            .GroupBy(x => new { x.ConsumerCode, x.StoreName })
            .Select(g => new
            {
                g.Key.ConsumerCode,
                g.Key.StoreName,
                TotalDiscount = g.Sum(x => x.DiscountAmount)
            })
            .OrderBy(x => x.ConsumerCode)
            .ThenBy(x => x.StoreName)
            .ToList();

        using var sw = new StreamWriter(outputPath);
        if (query.Count == 0)
        {
            sw.WriteLine("Data not found");
        }
        else
        {
            foreach (var item in query)
            {
                sw.WriteLine($"{item.ConsumerCode} {item.StoreName} {item.TotalDiscount}");
            }
        }
    }
}

public static class Task20
{
    public static void Run(IEnumerable<ConsumerA> seqA, IEnumerable<DiscountC> seqC, IEnumerable<PriceD> seqD, IEnumerable<PurchaseE> seqE)
    {
        var purchasesWithPrice = seqE.Join(seqD,
            e => new { e.Article, e.StoreName },
            d => new { d.Article, d.StoreName },
            (e, d) => new { e.ConsumerCode, e.StoreName, d.Price });

        var purchasesWithDiscount = from p in purchasesWithPrice
                                    join c in seqC on new { p.ConsumerCode, p.StoreName } equals new { c.ConsumerCode, c.StoreName } into discountGroup
                                    from dg in discountGroup.DefaultIfEmpty()
                                    let discPercent = dg != null ? dg.DiscountPercent : 0.0
                                    let discRubles = Math.Truncate(p.Price * (decimal)discPercent / 100m)
                                    select new
                                    {
                                        p.ConsumerCode,
                                        p.StoreName,
                                        FinalPrice = p.Price - discRubles
                                    };

        var query = purchasesWithDiscount.Join(seqA,
                p => p.ConsumerCode,
                a => a.ConsumerCode,
                (p, a) => new { a.BirthYear, p.StoreName, p.FinalPrice })
            .GroupBy(x => new { x.BirthYear, x.StoreName })
            .Select(g => new
            {
                g.Key.BirthYear,
                g.Key.StoreName,
                TotalPrice = g.Sum(x => x.FinalPrice)
            })
            .OrderBy(x => x.BirthYear)
            .ThenBy(x => x.StoreName);

        Console.WriteLine("\nCost of purchases by birth year and store:");
        foreach (var item in query)
        {
            Console.WriteLine($"{item.BirthYear} | {item.StoreName} | {item.TotalPrice} rub.");
        }
    }
}

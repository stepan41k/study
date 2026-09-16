using System;
using System.Collections.Generic;
using lab1;

// 1-16
TasksDemo.Run();

// 17
var grades = new List<StudentGrade>
{
    new() { Form = 9, LastName = "Иванов", Initials = "И.И.", Subject = "Алгебра", Grade = 5 },
    new() { Form = 9, LastName = "Иванов", Initials = "И.И.", Subject = "Геометрия", Grade = 5 },
    new() { Form = 9, LastName = "Иванов", Initials = "И.И.", Subject = "Информатика", Grade = 4 },
    new() { Form = 9, LastName = "Петров", Initials = "П.П.", Subject = "Алгебра", Grade = 5 },
    new() { Form = 9, LastName = "Петров", Initials = "П.П.", Subject = "Информатика", Grade = 3 },
    new() { Form = 10, LastName = "Сидоров", Initials = "С.С.", Subject = "Алгебра", Grade = 5 },
    new() { Form = 10, LastName = "Сидоров", Initials = "С.С.", Subject = "Геометрия", Grade = 5 }
};
Task17.Run(grades, "result17.txt");
Console.WriteLine("\nResult saved in result17.txt");

// 18-20
var consumers = new List<ConsumerA>
{
    new() { ConsumerCode = 101, BirthYear = 1995, Street = "Ленина" },
    new() { ConsumerCode = 102, BirthYear = 1995, Street = "Ленина" },
    new() { ConsumerCode = 103, BirthYear = 2000, Street = "Мира" }
};

var discounts = new List<DiscountC>
{
    new() { ConsumerCode = 101, StoreName = "Магнит", DiscountPercent = 10 },
    new() { ConsumerCode = 102, StoreName = "Магнит", DiscountPercent = 5 },
    new() { ConsumerCode = 103, StoreName = "Пятерочка", DiscountPercent = 15 }
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

// 18, 19, 20
Task18.Run(consumers, discounts);
Task19.Run(discounts, prices, purchases, "result19.txt");
Console.WriteLine("Result saved in result19.txt");
Task20.Run(consumers, discounts, prices, purchases);
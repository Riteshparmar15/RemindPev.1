using RemindPe.Models;

namespace RemindPe.Data;

public static class DbInitializer
{
    public static void Seed(KhataDbContext context)
    {
        if (context.Customers.Any())
        {
            return; // DB has already been seeded
        }

        var customers = new List<Customer>
        {
            new Customer
            {
                Name = "Ramesh Kumar (Kirana)",
                Phone = "9876543210",
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                Transactions = new List<Transaction>
                {
                    new Transaction
                    {
                        Amount = 1500m,
                        Type = "Credit",
                        Note = "Monthly ration & spices",
                        Date = DateTime.UtcNow.AddDays(-15)
                    },
                    new Transaction
                    {
                        Amount = 1000m,
                        Type = "Debit",
                        Note = "UPI GPay Payment",
                        Date = DateTime.UtcNow.AddDays(-7)
                    },
                    new Transaction
                    {
                        Amount = 750m,
                        Type = "Credit",
                        Note = "Cooking oil & wheat flour",
                        Date = DateTime.UtcNow.AddDays(-2)
                    }
                }
            },
            new Customer
            {
                Name = "Priya Verma",
                Phone = "9123456789",
                CreatedAt = DateTime.UtcNow.AddDays(-25),
                Transactions = new List<Transaction>
                {
                    new Transaction
                    {
                        Amount = 2400m,
                        Type = "Credit",
                        Note = "Festival gift hampers",
                        Date = DateTime.UtcNow.AddDays(-10)
                    },
                    new Transaction
                    {
                        Amount = 1200m,
                        Type = "Debit",
                        Note = "Cash payment",
                        Date = DateTime.UtcNow.AddDays(-3)
                    }
                }
            },
            new Customer
            {
                Name = "Rahul Sharma",
                Phone = "9988776655",
                CreatedAt = DateTime.UtcNow.AddDays(-20),
                Transactions = new List<Transaction>
                {
                    new Transaction
                    {
                        Amount = 850m,
                        Type = "Credit",
                        Note = "Stationery items & notebook bundle",
                        Date = DateTime.UtcNow.AddDays(-12)
                    },
                    new Transaction
                    {
                        Amount = 850m,
                        Type = "Debit",
                        Note = "Paytm full settlement",
                        Date = DateTime.UtcNow.AddDays(-1)
                    }
                }
            },
            new Customer
            {
                Name = "Amit Patel (Dairy)",
                Phone = "9822334455",
                CreatedAt = DateTime.UtcNow.AddDays(-14),
                Transactions = new List<Transaction>
                {
                    new Transaction
                    {
                        Amount = 3200m,
                        Type = "Credit",
                        Note = "Weekly milk & paneer supply",
                        Date = DateTime.UtcNow.AddDays(-5)
                    },
                    new Transaction
                    {
                        Amount = 1500m,
                        Type = "Debit",
                        Note = "PhonePe transfer",
                        Date = DateTime.UtcNow.AddDays(-1)
                    }
                }
            },
            new Customer
            {
                Name = "Sunita Devi",
                Phone = "9711223344",
                CreatedAt = DateTime.UtcNow.AddDays(-8),
                Transactions = new List<Transaction>
                {
                    new Transaction
                    {
                        Amount = 450m,
                        Type = "Credit",
                        Note = "Household cleaning supplies",
                        Date = DateTime.UtcNow.AddDays(-4)
                    }
                }
            }
        };

        context.Customers.AddRange(customers);
        context.SaveChanges();
    }
}

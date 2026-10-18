using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RemindPe.Models;

public class Transaction
{
    public int Id { get; set; }

    [Required]
    public int CustomerId { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Type of transaction:
    /// - "Credit": Goods/money given to customer on udhar (Customer owes merchant -> increases pending balance)
    /// - "Debit": Payment received from customer (Jama/Paid -> reduces pending balance)
    /// </summary>
    [Required]
    [MaxLength(10)]
    public string Type { get; set; } = "Credit";

    [MaxLength(300)]
    public string? Note { get; set; }

    public DateTime Date { get; set; } = DateTime.UtcNow;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public Customer? Customer { get; set; }
}

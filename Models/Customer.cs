using System.ComponentModel.DataAnnotations;

namespace RemindPe.Models;

public class Customer
{
    public int Id { get; set; }

    [Required]
    [MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public List<Transaction> Transactions { get; set; } = new();
}

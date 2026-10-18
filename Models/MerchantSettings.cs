using System.ComponentModel.DataAnnotations;

namespace RemindPe.Models;

public class MerchantSettings
{
    public int Id { get; set; } = 1;

    [Required]
    [MaxLength(120)]
    public string StoreName { get; set; } = "Apex Trading & Services";

    [MaxLength(120)]
    public string OwnerName { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(100)]
    public string UpiId { get; set; } = "merchant@upi";

    [MaxLength(10)]
    public string Currency { get; set; } = "₹";

    [MaxLength(50)]
    public string DefaultReminderTemplate { get; set; } = "hinglish";

    [MaxLength(200)]
    public string? ReceiptFooter { get; set; } = "Thank you for your business!";

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

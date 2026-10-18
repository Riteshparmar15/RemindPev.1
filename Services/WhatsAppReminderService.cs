using System.Globalization;
using RemindPe.Common;
using RemindPe.Models.DTOs;
using RemindPe.Repositories;

namespace RemindPe.Services;

public class WhatsAppReminderService : IWhatsAppReminderService
{
    private readonly ICustomerRepository _customerRepository;

    public WhatsAppReminderService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<WhatsAppReminderDto> GenerateReminderAsync(int customerId, string? templateKey = null, string? customNote = null)
    {
        var customer = await _customerRepository.GetByIdWithTransactionsAsync(customerId);
        if (customer == null)
        {
            throw new NotFoundException($"Customer with ID {customerId} was not found.");
        }

        var totalCredit = customer.Transactions.Where(t => t.Type == "Credit").Sum(t => t.Amount);
        var totalDebit = customer.Transactions.Where(t => t.Type == "Debit").Sum(t => t.Amount);
        var balance = totalCredit - totalDebit;

        if (balance <= 0)
        {
            throw new ValidationException($"Customer '{customer.Name}' has no outstanding balance (Current Balance: ₹{balance:N2}). Reminder cannot be generated.");
        }

        var indianCulture = new CultureInfo("en-IN");
        var formattedBalance = balance % 1 == 0
            ? "₹" + balance.ToString("N0", indianCulture)
            : "₹" + balance.ToString("N2", indianCulture);

        var key = string.IsNullOrWhiteSpace(templateKey) ? "standard" : templateKey.ToLower();
        var message = BuildMessage(key, customer.Name, formattedBalance, customNote);

        var digits = new string(customer.Phone.Where(char.IsDigit).ToArray());
        if (digits.StartsWith("91") && digits.Length == 12)
        {
            digits = digits[2..];
        }
        else if (digits.StartsWith("0") && digits.Length == 11)
        {
            digits = digits[1..];
        }

        var cleanPhone = "91" + digits;

        // WhatsApp Click to Chat URL standard: https://wa.me/[CountryCode][Phone]?text=[EncodedMessage]
        var encodedMessage = Uri.EscapeDataString(message);
        var waUrl = $"https://wa.me/{cleanPhone}?text={encodedMessage}";

        return new WhatsAppReminderDto(
            customer.Id,
            customer.Name,
            customer.Phone,
            balance,
            formattedBalance,
            key,
            message,
            waUrl
        );
    }

    public IEnumerable<WhatsAppTemplateOptionDto> GetAvailableTemplates()
    {
        return new List<WhatsAppTemplateOptionDto>
        {
            new WhatsAppTemplateOptionDto(
                "standard",
                "Standard English Reminder",
                "Direct, clear, and professional reminder.",
                "Hello [Name], your pending balance is ₹[Balance]. Please clear it soon."
            ),
            new WhatsAppTemplateOptionDto(
                "hinglish",
                "Polite Hinglish (Vyapar Standard)",
                "Respectful Hindi-English blend popular with local customers.",
                "Namaste [Name] ji, aapka VyaparKhata mein pending balance ₹[Balance] hai. Kripya samay par bhuqtan karein. Dhanyawad!"
            ),
            new WhatsAppTemplateOptionDto(
                "formal",
                "Formal Store Invoice Notice",
                "Formal business reminder with store settlement reference.",
                "Dear [Name], gentle reminder from our store regarding your outstanding balance of ₹[Balance]. Kindly settle at the earliest. Thank you."
            )
        };
    }

    private static string BuildMessage(string key, string customerName, string formattedBalance, string? customNote)
    {
        var baseMsg = key switch
        {
            "hinglish" => $"Namaste {customerName} ji, aapka VyaparKhata mein pending balance {formattedBalance} hai. Kripya samay par bhuqtan karein. Dhanyawad!",
            "formal" => $"Dear {customerName}, gentle reminder from our store regarding your outstanding balance of {formattedBalance}. Kindly settle at the earliest. Thank you.",
            _ => $"Hello {customerName}, your pending balance is {formattedBalance}. Please clear it soon."
        };

        if (!string.IsNullOrWhiteSpace(customNote))
        {
            baseMsg += $"\nNote: {customNote.Trim()}";
        }

        return baseMsg;
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaymentApp.Models;

public class Payment
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }
    
    [Required]
    public DateTime DueDate { get; set; }
    
    public DateTime? PaidDate { get; set; }
    
    public bool IsRecurring { get; set; }
    
    public RecurrenceType RecurrenceType { get; set; }
    
    [StringLength(500)]
    public string? Notes { get; set; }
    
    [NotMapped]
    public bool IsPaid => PaidDate.HasValue;
    
    [NotMapped]
    public bool IsOverdue => !IsPaid && DueDate < DateTime.Today;
    
    [NotMapped]
    public int DaysUntilDue => (DueDate - DateTime.Today).Days;
    
    [NotMapped]
    public string Status
    {
        get
        {
            if (IsPaid) return "Paid";
            if (IsOverdue) return "Overdue";
            if (DaysUntilDue <= 7) return "Due Soon";
            return "Upcoming";
        }
    }
}

public enum RecurrenceType
{
    None = 0,
    Monthly = 1,
    Quarterly = 2,
    Yearly = 3
}
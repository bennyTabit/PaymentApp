using System.ComponentModel.DataAnnotations;

namespace PaymentApp.Models.DTOs;

public class CreatePaymentDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }
    
    [Required]
    public DateTime DueDate { get; set; }
    
    public bool IsRecurring { get; set; }
    
    public RecurrenceType RecurrenceType { get; set; }
    
    [StringLength(500)]
    public string? Notes { get; set; }
}

public class UpdatePaymentDto
{
    [StringLength(100)]
    public string? Name { get; set; }
    
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal? Amount { get; set; }
    
    public DateTime? DueDate { get; set; }
    
    public bool? IsRecurring { get; set; }
    
    public RecurrenceType? RecurrenceType { get; set; }
    
    [StringLength(500)]
    public string? Notes { get; set; }
}

public class PaymentResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public bool IsRecurring { get; set; }
    public RecurrenceType RecurrenceType { get; set; }
    public string? Notes { get; set; }
    public bool IsPaid { get; set; }
    public bool IsOverdue { get; set; }
    public int DaysUntilDue { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class PaymentSummaryDto
{
    public int TotalPayments { get; set; }
    public int PaidCount { get; set; }
    public int UnpaidCount { get; set; }
    public int OverdueCount { get; set; }
    public int DueSoonCount { get; set; }
    public decimal TotalAmountPaid { get; set; }
    public decimal TotalAmountDue { get; set; }
    public decimal TotalOverdueAmount { get; set; }
}

public class MarkAsPaidDto
{
    public DateTime? PaidDate { get; set; }
}
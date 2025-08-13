using Microsoft.EntityFrameworkCore;
using PaymentApp.Data;
using PaymentApp.Models;
using PaymentApp.Models.DTOs;

namespace PaymentApp.Services;

public class PaymentService : IPaymentService
{
    private readonly PaymentDbContext _context;
    
    public PaymentService(PaymentDbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<PaymentResponseDto>> GetAllPaymentsAsync()
    {
        var payments = await _context.Payments
            .OrderBy(p => p.DueDate)
            .ToListAsync();
            
        return payments.Select(MapToResponseDto);
    }
    
    public async Task<IEnumerable<PaymentResponseDto>> GetUpcomingPaymentsAsync(int days = 30)
    {
        var cutoffDate = DateTime.Today.AddDays(days);
        var payments = await _context.Payments
            .Where(p => !p.PaidDate.HasValue && p.DueDate <= cutoffDate)
            .OrderBy(p => p.DueDate)
            .ToListAsync();
            
        return payments.Select(MapToResponseDto);
    }
    
    public async Task<IEnumerable<PaymentResponseDto>> GetOverduePaymentsAsync()
    {
        var payments = await _context.Payments
            .Where(p => !p.PaidDate.HasValue && p.DueDate < DateTime.Today)
            .OrderBy(p => p.DueDate)
            .ToListAsync();
            
        return payments.Select(MapToResponseDto);
    }
    
    public async Task<IEnumerable<PaymentResponseDto>> GetDueSoonPaymentsAsync(int days = 7)
    {
        var cutoffDate = DateTime.Today.AddDays(days);
        var payments = await _context.Payments
            .Where(p => !p.PaidDate.HasValue && p.DueDate <= cutoffDate && p.DueDate >= DateTime.Today)
            .OrderBy(p => p.DueDate)
            .ToListAsync();
            
        return payments.Select(MapToResponseDto);
    }
    
    public async Task<IEnumerable<PaymentResponseDto>> GetRemindersAsync()
    {
        var reminders = new List<PaymentResponseDto>();
        
        // Overdue payments
        var overduePayments = await GetOverduePaymentsAsync();
        reminders.AddRange(overduePayments);
        
        // Due in next 3 days
        var dueSoonPayments = await GetDueSoonPaymentsAsync(3);
        reminders.AddRange(dueSoonPayments);
        
        return reminders.OrderBy(p => p.DueDate);
    }
    
    public async Task<PaymentResponseDto?> GetPaymentByIdAsync(int id)
    {
        var payment = await _context.Payments.FindAsync(id);
        return payment != null ? MapToResponseDto(payment) : null;
    }
    
    public async Task<PaymentResponseDto> CreatePaymentAsync(CreatePaymentDto createDto)
    {
        var payment = new Payment
        {
            Name = createDto.Name,
            Amount = createDto.Amount,
            DueDate = createDto.DueDate,
            IsRecurring = createDto.IsRecurring,
            RecurrenceType = createDto.RecurrenceType,
            Notes = createDto.Notes
        };
        
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();
        
        return MapToResponseDto(payment);
    }
    
    public async Task<PaymentResponseDto?> UpdatePaymentAsync(int id, UpdatePaymentDto updateDto)
    {
        var payment = await _context.Payments.FindAsync(id);
        if (payment == null) return null;
        
        if (updateDto.Name != null)
            payment.Name = updateDto.Name;
        if (updateDto.Amount.HasValue)
            payment.Amount = updateDto.Amount.Value;
        if (updateDto.DueDate.HasValue)
            payment.DueDate = updateDto.DueDate.Value;
        if (updateDto.IsRecurring.HasValue)
            payment.IsRecurring = updateDto.IsRecurring.Value;
        if (updateDto.RecurrenceType.HasValue)
            payment.RecurrenceType = updateDto.RecurrenceType.Value;
        if (updateDto.Notes != null)
            payment.Notes = updateDto.Notes;
        
        await _context.SaveChangesAsync();
        return MapToResponseDto(payment);
    }
    
    public async Task<bool> DeletePaymentAsync(int id)
    {
        var payment = await _context.Payments.FindAsync(id);
        if (payment == null) return false;
        
        _context.Payments.Remove(payment);
        await _context.SaveChangesAsync();
        return true;
    }
    
    public async Task<bool> MarkAsPaidAsync(int id, MarkAsPaidDto markAsPaidDto)
    {
        var payment = await _context.Payments.FindAsync(id);
        if (payment == null) return false;
        
        payment.PaidDate = markAsPaidDto.PaidDate ?? DateTime.Today;
        
        // If recurring, create next payment
        if (payment.IsRecurring && payment.RecurrenceType != RecurrenceType.None)
        {
            var nextPayment = CreateNextRecurringPayment(payment);
            _context.Payments.Add(nextPayment);
        }
        
        await _context.SaveChangesAsync();
        return true;
    }
    
    public async Task<PaymentSummaryDto> GetPaymentSummaryAsync()
    {
        var allPayments = await _context.Payments.ToListAsync();
        var paidPayments = allPayments.Where(p => p.PaidDate.HasValue);
        var unpaidPayments = allPayments.Where(p => !p.PaidDate.HasValue);
        var overduePayments = allPayments.Where(p => !p.PaidDate.HasValue && p.DueDate < DateTime.Today);
        var dueSoonPayments = allPayments.Where(p => !p.PaidDate.HasValue && (p.DueDate - DateTime.Today).Days <= 7);
        
        return new PaymentSummaryDto
        {
            TotalPayments = allPayments.Count,
            PaidCount = paidPayments.Count(),
            UnpaidCount = unpaidPayments.Count(),
            OverdueCount = overduePayments.Count(),
            DueSoonCount = dueSoonPayments.Count(),
            TotalAmountPaid = paidPayments.Sum(p => p.Amount),
            TotalAmountDue = unpaidPayments.Sum(p => p.Amount),
            TotalOverdueAmount = overduePayments.Sum(p => p.Amount)
        };
    }
    
    private Payment CreateNextRecurringPayment(Payment originalPayment)
    {
        var nextDueDate = originalPayment.RecurrenceType switch
        {
            RecurrenceType.Monthly => originalPayment.DueDate.AddMonths(1),
            RecurrenceType.Quarterly => originalPayment.DueDate.AddMonths(3),
            RecurrenceType.Yearly => originalPayment.DueDate.AddYears(1),
            _ => originalPayment.DueDate
        };
        
        return new Payment
        {
            Name = originalPayment.Name,
            Amount = originalPayment.Amount,
            DueDate = nextDueDate,
            IsRecurring = originalPayment.IsRecurring,
            RecurrenceType = originalPayment.RecurrenceType,
            Notes = originalPayment.Notes
        };
    }
    
    private static PaymentResponseDto MapToResponseDto(Payment payment)
    {
        return new PaymentResponseDto
        {
            Id = payment.Id,
            Name = payment.Name,
            Amount = payment.Amount,
            DueDate = payment.DueDate,
            PaidDate = payment.PaidDate,
            IsRecurring = payment.IsRecurring,
            RecurrenceType = payment.RecurrenceType,
            Notes = payment.Notes,
            IsPaid = payment.IsPaid,
            IsOverdue = payment.IsOverdue,
            DaysUntilDue = payment.DaysUntilDue,
            Status = payment.Status
        };
    }
}
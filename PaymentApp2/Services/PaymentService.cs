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
            .AsNoTracking()
            .OrderBy(p => p.DueDate)
            .ToListAsync();
            
        return payments.Select(MapToResponseDto);
    }
    
    public async Task<IEnumerable<PaymentResponseDto>> GetUpcomingPaymentsAsync(int days = 30)
    {
        var today = DateTime.Today;
        var cutoffDate = today.AddDays(days);
        var payments = await _context.Payments
            .AsNoTracking()
            .Where(p => !p.PaidDate.HasValue && p.DueDate >= today && p.DueDate <= cutoffDate)
            .OrderBy(p => p.DueDate)
            .ToListAsync();
            
        return payments.Select(MapToResponseDto);
    }
    
    public async Task<IEnumerable<PaymentResponseDto>> GetOverduePaymentsAsync()
    {
        var payments = await _context.Payments
            .AsNoTracking()
            .Where(p => !p.PaidDate.HasValue && p.DueDate < DateTime.Today)
            .OrderBy(p => p.DueDate)
            .ToListAsync();
            
        return payments.Select(MapToResponseDto);
    }
    
    public async Task<IEnumerable<PaymentResponseDto>> GetDueSoonPaymentsAsync(int days = 7)
    {
        var today = DateTime.Today;
        var cutoffDate = today.AddDays(days);
        var payments = await _context.Payments
            .AsNoTracking()
            .Where(p => !p.PaidDate.HasValue && p.DueDate <= cutoffDate && p.DueDate >= today)
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
        var payment = await _context.Payments
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
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
        var today = DateTime.Today;
        var soonCutoff = today.AddDays(7);

        var totalPayments = await _context.Payments.AsNoTracking().CountAsync();

        var paidQuery = _context.Payments.AsNoTracking().Where(p => p.PaidDate.HasValue);
        var unpaidQuery = _context.Payments.AsNoTracking().Where(p => !p.PaidDate.HasValue);
        var overdueQuery = _context.Payments.AsNoTracking().Where(p => !p.PaidDate.HasValue && p.DueDate < today);
        var dueSoonQuery = _context.Payments.AsNoTracking().Where(p => !p.PaidDate.HasValue && p.DueDate >= today && p.DueDate <= soonCutoff);
        
        var paidCount = await paidQuery.CountAsync();
        var unpaidCount = await unpaidQuery.CountAsync();
        var overdueCount = await overdueQuery.CountAsync();
        var dueSoonCount = await dueSoonQuery.CountAsync();

        var totalAmountPaid = await paidQuery.SumAsync(p => (decimal?)p.Amount) ?? 0m;
        var totalAmountDue = await unpaidQuery.SumAsync(p => (decimal?)p.Amount) ?? 0m;
        var totalOverdueAmount = await overdueQuery.SumAsync(p => (decimal?)p.Amount) ?? 0m;
        
        return new PaymentSummaryDto
        {
            TotalPayments = totalPayments,
            PaidCount = paidCount,
            UnpaidCount = unpaidCount,
            OverdueCount = overdueCount,
            DueSoonCount = dueSoonCount,
            TotalAmountPaid = totalAmountPaid,
            TotalAmountDue = totalAmountDue,
            TotalOverdueAmount = totalOverdueAmount
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
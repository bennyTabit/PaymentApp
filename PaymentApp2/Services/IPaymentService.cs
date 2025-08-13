using PaymentApp.Models;
using PaymentApp.Models.DTOs;

namespace PaymentApp.Services;

public interface IPaymentService
{
    Task<IEnumerable<PaymentResponseDto>> GetAllPaymentsAsync();
    Task<IEnumerable<PaymentResponseDto>> GetUpcomingPaymentsAsync(int days = 30);
    Task<IEnumerable<PaymentResponseDto>> GetOverduePaymentsAsync();
    Task<IEnumerable<PaymentResponseDto>> GetDueSoonPaymentsAsync(int days = 7);
    Task<IEnumerable<PaymentResponseDto>> GetRemindersAsync();
    Task<PaymentResponseDto?> GetPaymentByIdAsync(int id);
    Task<PaymentResponseDto> CreatePaymentAsync(CreatePaymentDto createDto);
    Task<PaymentResponseDto?> UpdatePaymentAsync(int id, UpdatePaymentDto updateDto);
    Task<bool> DeletePaymentAsync(int id);
    Task<bool> MarkAsPaidAsync(int id, MarkAsPaidDto markAsPaidDto);
    Task<PaymentSummaryDto> GetPaymentSummaryAsync();
}
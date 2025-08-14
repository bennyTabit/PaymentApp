using PaymentApp.Models;
using PaymentApp.Models.DTOs;

namespace PaymentApp.Services;

public interface IPaymentService
{
    Task<IEnumerable<PaymentResponseDto>> GetAllPaymentsAsync(int userId);
    Task<IEnumerable<PaymentResponseDto>> GetUpcomingPaymentsAsync(int userId, int days = 30);
    Task<IEnumerable<PaymentResponseDto>> GetOverduePaymentsAsync(int userId);
    Task<IEnumerable<PaymentResponseDto>> GetDueSoonPaymentsAsync(int userId, int days = 7);
    Task<IEnumerable<PaymentResponseDto>> GetRemindersAsync(int userId);
    Task<PaymentResponseDto?> GetPaymentByIdAsync(int userId, int id);
    Task<PaymentResponseDto> CreatePaymentAsync(int userId, CreatePaymentDto createDto);
    Task<PaymentResponseDto?> UpdatePaymentAsync(int userId, int id, UpdatePaymentDto updateDto);
    Task<bool> DeletePaymentAsync(int userId, int id);
    Task<bool> MarkAsPaidAsync(int userId, int id, MarkAsPaidDto markAsPaidDto);
    Task<PaymentSummaryDto> GetPaymentSummaryAsync(int userId);
}
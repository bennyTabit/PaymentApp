using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentApp.Models.DTOs;
using PaymentApp.Services;
using System.Security.Claims;

namespace PaymentApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        /// <summary>
        /// Get all payments
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaymentResponseDto>>> GetAllPayments()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var payments = await _paymentService.GetAllPaymentsAsync();
            return Ok(payments);
        }

        /// <summary>
        /// Get payment by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentResponseDto>> GetPayment(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var payment = await _paymentService.GetPaymentByIdAsync(id);
            if (payment == null)
                return NotFound($"Payment with ID {id} not found");

            return Ok(payment);
        }

        /// <summary>
        /// Create a new payment
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<PaymentResponseDto>> CreatePayment(CreatePaymentDto createDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var payment = await _paymentService.CreatePaymentAsync(createDto);
            return CreatedAtAction(nameof(GetPayment), new { id = payment.Id }, payment);
        }

        /// <summary>
        /// Update an existing payment
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<PaymentResponseDto>> UpdatePayment(int id, UpdatePaymentDto updateDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var payment = await _paymentService.UpdatePaymentAsync(id, updateDto);
            if (payment == null)
                return NotFound($"Payment with ID {id} not found");

            return Ok(payment);
        }

        /// <summary>
        /// Delete a payment
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeletePayment(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var success = await _paymentService.DeletePaymentAsync(id);
            if (!success)
                return NotFound($"Payment with ID {id} not found");

            return NoContent();
        }

        /// <summary>
        /// Mark a payment as paid
        /// </summary>
        [HttpPost("{id}/mark-as-paid")]
        public async Task<ActionResult> MarkAsPaid(int id, MarkAsPaidDto markAsPaidDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var success = await _paymentService.MarkAsPaidAsync(id, markAsPaidDto);
            if (!success)
                return NotFound($"Payment with ID {id} not found");

            return Ok(new { message = "Payment marked as paid successfully" });
        }

        /// <summary>
        /// Get upcoming payments (next 30 days by default)
        /// </summary>
        [HttpGet("upcoming")]
        public async Task<ActionResult<IEnumerable<PaymentResponseDto>>> GetUpcomingPayments([FromQuery] int days = 30)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var payments = await _paymentService.GetUpcomingPaymentsAsync(days);
            return Ok(payments);
        }

        /// <summary>
        /// Get overdue payments
        /// </summary>
        [HttpGet("overdue")]
        public async Task<ActionResult<IEnumerable<PaymentResponseDto>>> GetOverduePayments()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var payments = await _paymentService.GetOverduePaymentsAsync();
            return Ok(payments);
        }

        /// <summary>
        /// Get payments due soon (next 7 days by default)
        /// </summary>
        [HttpGet("due-soon")]
        public async Task<ActionResult<IEnumerable<PaymentResponseDto>>> GetDueSoonPayments([FromQuery] int days = 7)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var payments = await _paymentService.GetDueSoonPaymentsAsync(days);
            return Ok(payments);
        }

        /// <summary>
        /// Get payment reminders (overdue + due soon)
        /// </summary>
        [HttpGet("reminders")]
        public async Task<ActionResult<IEnumerable<PaymentResponseDto>>> GetReminders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var payments = await _paymentService.GetRemindersAsync();
            return Ok(payments);
        }

        /// <summary>
        /// Get payment summary statistics
        /// </summary>
        [HttpGet("summary")]
        public async Task<ActionResult<PaymentSummaryDto>> GetPaymentSummary()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var summary = await _paymentService.GetPaymentSummaryAsync();
            return Ok(summary);
        }
    }
}

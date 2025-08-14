using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using PaymentApp.Models.DTOs;
using PaymentApp.Services;

namespace PaymentApp.Controllers
{
    [ApiController]
    [Route("api/{userId?}/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        private int? GetUserId()
        {
            if (RouteData.Values.TryGetValue("userId", out var routeUser) && int.TryParse(routeUser?.ToString(), out var rid))
                return rid;
            if (Request.Query.TryGetValue("userId", out var queryUser) && int.TryParse(queryUser, out var qid))
                return qid;
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(claim, out var cid))
                return cid;
            return null;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaymentResponseDto>>> GetAllPayments()
        {
            var userId = GetUserId();
            if (userId == null)
                return BadRequest("UserId is required.");

            var payments = await _paymentService.GetAllPaymentsAsync(userId.Value);
            return Ok(payments);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentResponseDto>> GetPayment(int id)
        {
            var userId = GetUserId();
            if (userId == null)
                return BadRequest("UserId is required.");

            var payment = await _paymentService.GetPaymentByIdAsync(userId.Value, id);
            if (payment == null)
                return NotFound($"Payment with ID {id} not found");

            return Ok(payment);
        }

        [HttpPost]
        public async Task<ActionResult<PaymentResponseDto>> CreatePayment(CreatePaymentDto createDto)
        {
            var userId = GetUserId();
            if (userId == null)
                return BadRequest("UserId is required.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var payment = await _paymentService.CreatePaymentAsync(userId.Value, createDto);
            return CreatedAtAction(nameof(GetPayment), new { id = payment.Id, userId = userId.Value }, payment);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<PaymentResponseDto>> UpdatePayment(int id, UpdatePaymentDto updateDto)
        {
            var userId = GetUserId();
            if (userId == null)
                return BadRequest("UserId is required.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var payment = await _paymentService.UpdatePaymentAsync(userId.Value, id, updateDto);
            if (payment == null)
                return NotFound($"Payment with ID {id} not found");

            return Ok(payment);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeletePayment(int id)
        {
            var userId = GetUserId();
            if (userId == null)
                return BadRequest("UserId is required.");

            var success = await _paymentService.DeletePaymentAsync(userId.Value, id);
            if (!success)
                return NotFound($"Payment with ID {id} not found");

            return NoContent();
        }

        [HttpPost("{id}/mark-as-paid")]
        public async Task<ActionResult> MarkAsPaid(int id, MarkAsPaidDto markAsPaidDto)
        {
            var userId = GetUserId();
            if (userId == null)
                return BadRequest("UserId is required.");

            var success = await _paymentService.MarkAsPaidAsync(userId.Value, id, markAsPaidDto);
            if (!success)
                return NotFound($"Payment with ID {id} not found");

            return Ok(new { message = "Payment marked as paid successfully" });
        }

        [HttpGet("upcoming")]
        public async Task<ActionResult<IEnumerable<PaymentResponseDto>>> GetUpcomingPayments([FromQuery] int days = 30)
        {
            var userId = GetUserId();
            if (userId == null)
                return BadRequest("UserId is required.");

            var payments = await _paymentService.GetUpcomingPaymentsAsync(userId.Value, days);
            return Ok(payments);
        }

        [HttpGet("overdue")]
        public async Task<ActionResult<IEnumerable<PaymentResponseDto>>> GetOverduePayments()
        {
            var userId = GetUserId();
            if (userId == null)
                return BadRequest("UserId is required.");

            var payments = await _paymentService.GetOverduePaymentsAsync(userId.Value);
            return Ok(payments);
        }

        [HttpGet("due-soon")]
        public async Task<ActionResult<IEnumerable<PaymentResponseDto>>> GetDueSoonPayments([FromQuery] int days = 7)
        {
            var userId = GetUserId();
            if (userId == null)
                return BadRequest("UserId is required.");

            var payments = await _paymentService.GetDueSoonPaymentsAsync(userId.Value, days);
            return Ok(payments);
        }

        [HttpGet("reminders")]
        public async Task<ActionResult<IEnumerable<PaymentResponseDto>>> GetReminders()
        {
            var userId = GetUserId();
            if (userId == null)
                return BadRequest("UserId is required.");

            var payments = await _paymentService.GetRemindersAsync(userId.Value);
            return Ok(payments);
        }

        [HttpGet("summary")]
        public async Task<ActionResult<PaymentSummaryDto>> GetPaymentSummary()
        {
            var userId = GetUserId();
            if (userId == null)
                return BadRequest("UserId is required.");

            var summary = await _paymentService.GetPaymentSummaryAsync(userId.Value);
            return Ok(summary);
        }
    }
}

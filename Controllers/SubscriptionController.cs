using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiTaller.Attributes;
using MiTaller.Data;
using MiTaller.DTO.Subscription;
using MiTaller.Models.Auth;
using MiTaller.Models.Subscription;
using MiTaller.Services;

namespace MiTaller.Controllers
{
    [Route("api/subscription")]
    [ApiController]
    [Authorize]
    public class SubscriptionController : ControllerBase
    {
        private readonly SubscriptionService _subscriptions;
        private readonly DataContext _context;

        public SubscriptionController(SubscriptionService subscriptions, DataContext context)
        {
            _subscriptions = subscriptions;
            _context = context;
        }

        // Paying and the payment history belong to the workshop account itself, never to its employees.
        private bool CallerIsOwner => User.FindFirst("UserType")?.Value == nameof(UserType.Workshop);

        // Owner and employees: employees need the plan to know which features are locked.
        [HttpGet("workshop/{workshopId}")]
        [OwnWorkshop]
        public async Task<ActionResult<SubscriptionStatusDto>> GetStatus(Guid workshopId) =>
            Ok(await _subscriptions.GetStatusAsync(workshopId));

        /// <summary>Channels for the "where did you get this code" dropdown.</summary>
        [HttpGet("sources")]
        public async Task<ActionResult<List<DiscountSourceDto>>> GetSources() =>
            Ok(await _context.DiscountSources.Where(s => s.IsActive).OrderBy(s => s.Name)
                .Select(s => new DiscountSourceDto { Id = s.Id, Name = s.Name, IsActive = true })
                .ToListAsync());

        [HttpPost("workshop/{workshopId}/quote")]
        [OwnWorkshop]
        public async Task<ActionResult<SubscriptionQuoteDto>> Quote(Guid workshopId, [FromBody] SubscriptionQuoteRequestDto model)
        {
            if (!CallerIsOwner) return Forbid();
            var (quote, error) = await _subscriptions.QuoteAsync(workshopId, model);
            return error != null ? BadRequest(error) : Ok(quote);
        }

        /// <summary>One-time payment with Checkout Pro: returns the Mercado Pago URL to open.</summary>
        [HttpPost("workshop/{workshopId}/checkout")]
        [OwnWorkshop]
        public async Task<ActionResult<CheckoutResponseDto>> Checkout(
            Guid workshopId, [FromBody] SubscriptionQuoteRequestDto model, [FromServices] MercadoPagoClient mp)
        {
            if (!CallerIsOwner) return Forbid();
            var (url, paymentId, error) = await _subscriptions.CreateCheckoutAsync(workshopId, model, mp);
            return error != null ? BadRequest(error) : Ok(new CheckoutResponseDto { CheckoutUrl = url!, PaymentId = paymentId!.Value });
        }

        /// <summary>
        /// Called when the owner comes back from Mercado Pago: asks MP for the order so the plan
        /// activates even if the webhook hasn't arrived yet.
        /// </summary>
        [HttpPost("workshop/{workshopId}/payments/{paymentId:int}/sync")]
        [OwnWorkshop]
        public async Task<ActionResult<SubscriptionPaymentDto>> SyncPayment(
            Guid workshopId, int paymentId, [FromServices] MercadoPagoClient mp)
        {
            if (!CallerIsOwner) return Forbid();
            var payment = await _context.SubscriptionPayments.FirstOrDefaultAsync(p => p.Id == paymentId && p.WorkshopId == workshopId);
            if (payment == null) return NotFound();
            if (payment.Status == PaymentStatus.Pending && payment.MpOrderId != null && mp.IsConfigured)
            {
                var order = await mp.GetOrderAsync(payment.MpOrderId);
                if (order != null) payment = await _subscriptions.ApplyOrderAsync(order) ?? payment;
            }
            return Ok(SubscriptionService.ToPaymentDto(payment));
        }

        /// <summary>The workshop's own payments, newest first, optionally by year and status.</summary>
        [HttpGet("workshop/{workshopId}/payments")]
        [OwnWorkshop]
        public async Task<ActionResult<List<SubscriptionPaymentDto>>> GetPayments(
            Guid workshopId, [FromQuery] int? year, [FromQuery] PaymentStatus? status)
        {
            if (!CallerIsOwner) return Forbid();
            var query = _context.SubscriptionPayments.Where(p => p.WorkshopId == workshopId);
            if (year != null) query = query.Where(p => p.CreatedAt.Year == year);
            if (status != null) query = query.Where(p => p.Status == status);
            var payments = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
            return Ok(payments.Select(p => SubscriptionService.ToPaymentDto(p)).ToList());
        }
    }
}

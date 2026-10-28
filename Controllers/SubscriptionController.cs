using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiTaller.Attributes;
using MiTaller.DTO.Subscription;
using MiTaller.Services;

namespace MiTaller.Controllers
{
    [Route("api/subscription")]
    [ApiController]
    [Authorize]
    public class SubscriptionController : ControllerBase
    {
        private readonly SubscriptionService _subscriptions;

        public SubscriptionController(SubscriptionService subscriptions)
        {
            _subscriptions = subscriptions;
        }

        // Owner and employees: employees need the plan to know which features are locked.
        [HttpGet("workshop/{workshopId}")]
        [OwnWorkshop]
        public async Task<ActionResult<SubscriptionStatusDto>> GetStatus(Guid workshopId) =>
            Ok(await _subscriptions.GetStatusAsync(workshopId));
    }
}

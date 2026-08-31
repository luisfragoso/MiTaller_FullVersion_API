using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiTaller.Data;
using MiTaller.DTO.Pager;
using MiTaller.DTO.Review;
using MiTaller.Models.Reviews;
using MiTaller.Models.Workshop;


namespace MiTaller.Controllers
{
    [Route("api/reviews")]
    [ApiController]
    [Authorize]
    public class ReviewController : ControllerBase
    {
        private readonly DataContext _context;

        public ReviewController(DataContext context)
        {
            _context = context;
        }

        [HttpGet("workshop/{workshopId}")]
        [AllowAnonymous]
        public async Task<ActionResult<ReviewResponseDto>> GetReviewsByWorkshop(Guid workshopId)
        {
            try 
            {
                var reviews = await _context.Reviews
                    .Where(r => r.WorkshopId == workshopId)
                    .Include(r => r.Customer)
                    .Select(r => new WorkshopReviewDto
                    {
                        Id = r.Id,
                        CustomerId = r.CustomerId,
                        CustomerName = r.Customer.FullName,
                        Comment = r.Comment,
                        Rate = r.Rate,
                        Date = r.Date.ToString("yyyy-MM-dd")
                    })
                    .ToListAsync();

                if (!reviews.Any()) return NotFound("not-found");

                var averageRate = reviews.Average(r => r.Rate);

                // Contador de veces que se ha calificado cada estrella
                var starCounts = Enumerable.Range(1, 5)
                    .ToDictionary(
                        i => i,
                        i => reviews.Count(r => (int)Math.Truncate(r.Rate) == i)
                    );

                var responseDto = new ReviewResponseDto
                {
                    AverageRate = (float)Math.Round(averageRate, 1), // Redondear a 1 decimal
                    TotalReviews = reviews.Count,
                    StarCounts = starCounts,
                    WorkshopReviews = reviews
                };

                return Ok(responseDto);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPost("workshop-paged/{workshopId}")]
        [AllowAnonymous]
        public async Task<ActionResult<ReviewPagedResponseDto>> GetReviewsByWorkshopPaged(Guid workshopId, [FromBody] PagerBodyDto pager)
        {
            try
            {
                var baseQuery = _context.Reviews
                    .Where(r => r.WorkshopId == workshopId)
                    .Include(r => r.Customer);

                var totalCount = await baseQuery.CountAsync();

                if (totalCount == 0) return NotFound("not-found");

                var averageRate = await baseQuery.AverageAsync(r => r.Rate);

                var starCounts = Enumerable.Range(1, 5)
                    .ToDictionary(
                        i => i,
                        i => baseQuery.Count(r => (int)Math.Truncate(r.Rate) == i)
                    );

                var now = DateTime.Now;
                var thisMonthReviews = await baseQuery.CountAsync(
                    r => r.Date.Year == now.Year && r.Date.Month == now.Month);

                var pagedReviews = await baseQuery
                    .OrderByDescending(r => r.Date)
                    .Skip((pager.PageNumber - 1) * pager.PageSize)
                    .Take(pager.PageSize)
                    .Select(r => new WorkshopReviewDto
                    {
                        Id = r.Id,
                        CustomerId = r.CustomerId,
                        CustomerName = r.Customer.FullName,
                        Comment = r.Comment,
                        Rate = r.Rate,
                        Date = r.Date.ToString("yyyy-MM-dd")
                    })
                    .ToListAsync();

                var totalPages = (int)Math.Ceiling((double)totalCount / pager.PageSize);

                var response = new ReviewPagedResponseDto
                {
                    AverageRate = (float)Math.Round(averageRate, 1),
                    TotalReviews = totalCount,
                    ThisMonthReviews = thisMonthReviews,
                    StarCounts = starCounts,
                    CurrentPage = pager.PageNumber,
                    MaxPage = totalPages,
                    WorkshopReviews = pagedReviews
                };

                return Ok(response);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }


        [HttpPost]
        public async Task<ActionResult> CreateReview([FromBody] PostReviewDto model)
        {
            try
            {
                var customerExists = await _context.Customers.AnyAsync(c => c.Id == model.CustomerId);
                var workshopExists = await _context.Workshops.AnyAsync(w => w.Id == model.WorkshopId);

                if (!customerExists) return NotFound("not-found");
                if (!workshopExists) return NotFound("not-found");

                bool relationCustomerWorkshopExists = 
                    await _context.Appointments.AnyAsync(a => a.WorkshopId == model.WorkshopId) ||
                    await _context.Quotations.AnyAsync(q => q.WorkshopId == model.WorkshopId) ||
                    await _context.WorkshopVehicleInspections.AnyAsync(v => v.WorkshopId == model.WorkshopId) ||
                    await _context.WorkshopMotocycleInspections.AnyAsync(m => m.WorkshopId == model.WorkshopId);

                if (!relationCustomerWorkshopExists) return NotFound("no-relation-customer-workshop");

                var existingReview = await _context.Reviews
                    .FirstOrDefaultAsync(r => r.CustomerId == model.CustomerId && r.WorkshopId == model.WorkshopId);

                if (existingReview != null)
                {
                    existingReview.Comment = model.Comment;
                    existingReview.Rate = model.Rate;
                    _context.Reviews.Update(existingReview);
                }
                else
                {
                    var review = new Review
                    {
                        CustomerId = model.CustomerId,
                        WorkshopId = model.WorkshopId,
                        Comment = model.Comment,
                        Rate = model.Rate
                    };
                    _context.Reviews.Add(review);
                }

                await _context.SaveChangesAsync();

                return Ok("review-deleted");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateReview(int id, [FromBody] PostReviewDto model)
        {
            try
            {
                var review = await _context.Reviews.FindAsync(id);
                if (review == null) return NotFound("not-found");

                review.Comment = model.Comment;
                review.Rate = model.Rate;
                review.Date = DateTime.Now;

                await _context.SaveChangesAsync();

                return Ok("review-updated");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteReview(int id)
        {
            try 
            {
                var review = await _context.Reviews.FindAsync(id);
                if (review == null) return NotFound("not-found");

                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();

                return Ok("review-deleted");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }
    }
}

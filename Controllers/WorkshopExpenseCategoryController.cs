using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiTaller.Attributes;
using MiTaller.Data;
using MiTaller.DTO.Workshop.Bill;
using MiTaller.Models.Workshop;
using MiTaller.Services;

namespace MiTaller.Controllers
{
    [Route("api/[controller]")]
    [RequiresProPlan]
    [ApiController]
    [Authorize]
    public class WorkshopExpenseCategoryController : ControllerBase
    {
        private readonly DataContext _context;

        public WorkshopExpenseCategoryController(DataContext context)
        {
            _context = context;
        }

        [HttpGet("workshop/{workshopId}")]
        [AuthorizeEmployee("Administrador")]
        [OwnWorkshop]
        public async Task<ActionResult<List<ExpenseCategoryResponseDto>>> GetByWorkshop(Guid workshopId)
        {
            var categories = await ExpenseCategoryService.EnsureForWorkshopAsync(_context, workshopId);
            var counts = await _context.WorkshopBills
                .Where(b => b.WorkshopId == workshopId)
                .GroupBy(b => b.Category)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count);

            return Ok(categories.Select(c => new ExpenseCategoryResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                IsHidden = c.IsHidden,
                BillCount = counts.TryGetValue(c.Name, out var n) ? n : 0,
            }));
        }

        [HttpPost]
        [CallerMatches("WorkshopId")]
        [AuthorizeEmployee("Administrador")]
        public async Task<ActionResult> Create([FromBody] PostExpenseCategoryDto model)
        {
            var name = model.Name?.Trim() ?? string.Empty;
            if (name.Length == 0 || name.Length > ExpenseCategoryService.MaxNameLength)
                return BadRequest("invalid-category-name");

            var categories = await ExpenseCategoryService.EnsureForWorkshopAsync(_context, model.WorkshopId);
            var existing = categories.FirstOrDefault(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                // Re-adding a hidden one just brings it back
                if (!existing.IsHidden) return Conflict("duplicate-category");
                existing.IsHidden = false;
            }
            else
            {
                _context.WorkshopExpenseCategories.Add(new WorkshopExpenseCategory
                {
                    WorkshopId = model.WorkshopId,
                    Name = name,
                    SortOrder = categories.Count == 0 ? 0 : categories.Max(c => c.SortOrder) + 1,
                });
            }

            await _context.SaveChangesAsync();
            return Ok("category-created");
        }

        [HttpPut("{id}")]
        [OwnsResource(ResourceKind.WorkshopExpenseCategory, "id")]
        [AuthorizeEmployee("Administrador")]
        public async Task<ActionResult> Update(int id, [FromBody] PutExpenseCategoryDto model)
        {
            var category = await _context.WorkshopExpenseCategories.FindAsync(id);
            if (category == null) return NotFound("not-found");

            var name = model.Name?.Trim() ?? string.Empty;
            if (name.Length == 0 || name.Length > ExpenseCategoryService.MaxNameLength)
                return BadRequest("invalid-category-name");

            var siblings = await _context.WorkshopExpenseCategories
                .Where(c => c.WorkshopId == category.WorkshopId && c.Id != id)
                .ToListAsync();
            if (siblings.Any(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase)))
                return Conflict("duplicate-category");
            if (model.IsHidden && !siblings.Any(c => !c.IsHidden))
                return BadRequest("last-visible-category");

            if (name != category.Name)
            {
                // Bills store the name, so a rename carries them along
                var bills = await _context.WorkshopBills
                    .Where(b => b.WorkshopId == category.WorkshopId && b.Category == category.Name)
                    .ToListAsync();
                bills.ForEach(b => b.Category = name);
                category.Name = name;
            }
            category.IsHidden = model.IsHidden;

            await _context.SaveChangesAsync();
            return Ok("category-updated");
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiTaller.Attributes;
using MiTaller.Data;
using MiTaller.DTO;
using MiTaller.DTO.Workshop.Bill;
using MiTaller.Models.Workshop;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MiTaller.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WorkshopBillController : ControllerBase
    {
        private readonly DataContext _context;

        private static readonly string[] ValidCategories =
            { "Renta", "Servicios", "Refacciones", "Salarios", "Equipo", "Otro" };

        private static string NormalizeCategory(string? category) =>
            category != null && ValidCategories.Contains(category) ? category : "Otro";

        public WorkshopBillController(DataContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<WorkshopBillResponseDto>> GetWorkshopBillById(int id)
        {
            try 
            {
                var bill = await _context.WorkshopBills
                    .Where(b => b.Id == id)
                    .Select(b => new WorkshopBillResponseDto
                    {
                        Id = b.Id,
                        WorkshopId = b.WorkshopId,
                        Description = b.Description,
                        Category = b.Category,
                        Amount = b.Amount
                    })
                    .FirstOrDefaultAsync();

                if (bill == null) return NotFound("not-found");

                return Ok(bill);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpGet("workshop/{workshopId}")]
        public async Task<ActionResult<ICollection<WorkshopBillResponseDto>>> GetWorkshopBillsByWorkshop(Guid workshopId)
        {
            try
            {
                var bills = await _context.WorkshopBills
                    .Where(b => b.WorkshopId == workshopId)
                    .Select(b => new WorkshopBillResponseDto
                    {
                        Id = b.Id,
                        Description = b.Description,
                        Category = b.Category,
                        Amount = b.Amount
                    })
                    .ToListAsync();

                if (!bills.Any()) return NotFound("not-found");

                return Ok(bills);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPost]
        [AuthorizeEmployee("Administrador")]
        public async Task<ActionResult> CreateWorkshopBill([FromBody] PostWorkshopBillDto model)
        {
            try 
            {
                var workshopExists = await _context.Workshops.AnyAsync(w => w.Id == model.WorkshopId);
                if (!workshopExists) return NotFound("not-found");

                var bill = new WorkshopBill
                {
                    WorkshopId = model.WorkshopId,
                    Description = model.Description,
                    Category = NormalizeCategory(model.Category),
                    Amount = model.Amount
                };

                _context.WorkshopBills.Add(bill);
                await _context.SaveChangesAsync();

                return Ok("bill-created");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPut("{id}")]
        [AuthorizeEmployee("Administrador")]
        public async Task<ActionResult> UpdateWorkshopBill(int id, [FromBody] PostWorkshopBillDto model)
        {
            try 
            {
                var bill = await _context.WorkshopBills.FindAsync(id);
                if (bill == null) return NotFound("not-found");

                bill.Description = model.Description;
                bill.Category = NormalizeCategory(model.Category);
                bill.Amount = model.Amount;

                await _context.SaveChangesAsync();
                return Ok("bill-updated");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpDelete("{id}")]
        [AuthorizeEmployee("Administrador")]
        public async Task<ActionResult> DeleteWorkshopBill(int id)
        {
            try 
            {
                var bill = await _context.WorkshopBills.FindAsync(id);
                if (bill == null) return NotFound("not-found");

                _context.WorkshopBills.Remove(bill);
                await _context.SaveChangesAsync();

                return Ok("bill-deleted");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }
    }
}

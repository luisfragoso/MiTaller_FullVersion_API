using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiTaller.Attributes;
using MiTaller.Data;
using MiTaller.DTO;
using MiTaller.DTO.Workshop.Income;
using MiTaller.Models.Workshop;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MiTaller.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WorkshopIncomeController : ControllerBase
    {
        private readonly DataContext _context;

        public WorkshopIncomeController(DataContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<WorkshopIncomeResponseDto>> GetWorkshopIncomeById(int id)
        {
            try 
            {
                var income = await _context.WorkshopIncomes
                    .Where(b => b.Id == id)
                    .Include(b => b.WorkshopServices)
                    .ThenInclude(b => b.Service)
                    .Select(b => new 
                    {
                        WorkshopId = b.WorkshopId,
                        Name = b.WorkshopServices.Service.Name,
                        Amount = b.Amount
                    })
                    .FirstOrDefaultAsync();

                if (income == null) return NotFound("not-found");

                return Ok(income);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpGet("workshop/{workshopId}/{monthYear}")]
        public async Task<ActionResult<ICollection<WorkshopIncomeResponseDto>>> GetWorkshopIncomesByWorkshop(Guid workshopId, string monthYear)
        {
            try
            {
                if (!DateTime.TryParseExact(monthYear, "MM-yyyy", null, System.Globalization.DateTimeStyles.None, out var parsedDate))
                {
                    return BadRequest("invalid-date-format");
                }

                var startDate = new DateTime(parsedDate.Year, parsedDate.Month, 1);
                var endDate = startDate.AddMonths(1).AddTicks(-1);


                var incomes = await _context.WorkshopIncomes
                    .Where(b => b.WorkshopId == workshopId && b.CreatedAt >= startDate && b.CreatedAt <= endDate)
                    .Include(b => b.WorkshopServices)
                    .ThenInclude(b => b.Service)
                    .Select(b => new WorkshopIncomeResponseDto
                    {
                        WorkshopServiceId = b.WorkshopServices.Id,
                        Name = b.WorkshopServices.Service.Name,
                        Amount = b.Amount
                    })
                    .ToListAsync();

                if (!incomes.Any()) return NotFound("not-found");

                return Ok(incomes);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPost]
        [AuthorizeEmployee("Administrador")]
        public async Task<ActionResult> CreateWorkshopIncome([FromBody] PostWorkshopIncomeDto model)
        {
            try 
            {
                var workshop = await _context.Workshops
                    .Where(w => w.Id == model.WorkshopId && !w.IsDeleted)
                    .FirstOrDefaultAsync();
                if (workshop == null) return NotFound("not-found");

                var workshopService = await _context.WorkshopServices
                    .Where(w => w.Id == model.WorkshopServiceId)
                    .FirstOrDefaultAsync();
                if (workshopService == null) return NotFound("not-found");

                if (workshop.Id != workshopService.WorkshopId) return BadRequest("wrong-workshop-service");

                var income = new WorkshopIncomes
                {
                    WorkshopId = model.WorkshopId,
                    WorkshopServiceId = model.WorkshopServiceId,
                    Amount = model.Amount
                };

                _context.WorkshopIncomes.Add(income);
                await _context.SaveChangesAsync();

                return Ok("income-created");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateWorkshopIncome(int id, [FromBody] PostWorkshopIncomeDto model)
        {
            try 
            {
                var income = await _context.WorkshopIncomes.FindAsync(id);
                if (income == null) return NotFound("not-found");

                income.Amount = model.Amount;

                await _context.SaveChangesAsync();
                return Ok("income-updated");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteWorkshopIncome(int id)
        {
            try 
            {
                var Income = await _context.WorkshopIncomes.FindAsync(id);
                if (Income == null) return NotFound("not-found");

                _context.WorkshopIncomes.Remove(Income);
                await _context.SaveChangesAsync();

                return Ok("income-deleted");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }
    }
}

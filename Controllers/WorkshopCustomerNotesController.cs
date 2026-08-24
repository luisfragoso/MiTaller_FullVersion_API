using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiTaller.Attributes;
using MiTaller.Data;
using MiTaller.DTO;
using MiTaller.DTO.Workshop.WorkshopCustomerNotes;
using MiTaller.Models.Workshop;

namespace MiTaller.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WorkshopCustomerNotesController : ControllerBase
    {
        private readonly DataContext _context;

        public WorkshopCustomerNotesController(DataContext context)
        {
            _context = context;
        }


        [HttpPost("register")]
        [AuthorizeEmployee("Administrador")]
        public async Task<ActionResult> RegisterNote(PostWorkshopCustomerNoteDto model)
        {
            try
            {
                var workshop = await _context.Workshops
                    .Where(w => w.Id == model.WorkshopId && !w.IsDeleted)
                    .FirstOrDefaultAsync();

                var customer = await _context.Customers
                    .Where(w => w.Id == model.CustomerId && !w.IsDeleted)
                    .FirstOrDefaultAsync();

                if (workshop == null || customer == null)
                {
                    return NotFound("not-found");
                }

                var workshopCustomerNote = await _context.WorkshopCustomerNotes
                    .Where(w => w.WorkshopId == model.WorkshopId && w.CustomerId == model.CustomerId)
                    .FirstOrDefaultAsync();

                if (workshopCustomerNote == null)
                {
                    var newWorkshopCustomerNote = new WorkshopCustomerNotes
                    {
                        WorkshopId = model.WorkshopId,
                        CustomerId = model.CustomerId,
                        Note = model.Note,
                        CreatedAt = DateTime.Now
                    };
                    await _context.WorkshopCustomerNotes.AddAsync(newWorkshopCustomerNote);

                } 
                else
                {
                    workshopCustomerNote.Note = model.Note;
                    workshopCustomerNote.UpdatedAt = DateTime.Now;
                }

                await _context.SaveChangesAsync();

                return Ok("note-created");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPost("get-workshop-customer-note")]
        public async Task<ActionResult<ICollection<WorkshopCustomerNotes>>> GetNotesByWorkshop(CustomerWorkshopBodyDto model)
        {
            try
            {
                var workshopCustomerNote = await _context.WorkshopCustomerNotes
                    .Where(w => w.WorkshopId == model.WorkshopId && w.CustomerId == model.CustomerId)
                    .FirstOrDefaultAsync();

                if (workshopCustomerNote == null)
                {
                    return NotFound("not-found");
                }

                return Ok(workshopCustomerNote);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpDelete("note/{noteId}")]
        [AuthorizeEmployee("Administrador")]
        public async Task<ActionResult> DeleteNote(int noteId)
        {
            try
            {
                var note = await _context.WorkshopNotes
                    .Where(e => e.Id == noteId)
                    .FirstOrDefaultAsync();

                if (note == null)
                {
                    return NotFound("not-found");
                }

                _context.WorkshopNotes.Remove(note);

                await _context.SaveChangesAsync();

                return Ok("note-deleted");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }
    }
}

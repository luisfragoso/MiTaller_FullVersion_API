using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiTaller.Attributes;
using MiTaller.Data;
using MiTaller.DTO.Workshop.Note;
using MiTaller.Models.Auth;
using MiTaller.Models.Notification;
using MiTaller.Models.Workshop;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiTaller.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WorkshopNoteController : ControllerBase
    {
        private readonly UserManager<BaseIdentityUser> _userManager;
        private readonly DataContext _context;

        public WorkshopNoteController(UserManager<BaseIdentityUser> userManager, DataContext context)
        {
            _userManager = userManager;
            _context = context;
        }
        [HttpPost("register")]
        [AuthorizeEmployee("Administrador")]
        public async Task<ActionResult> RegisterNote(PostWorkshopNoteDto model)
        {
            try
            {
                var workshop = await _context.Workshops
                    .Where(w => w.Id == model.WorkshopId && !w.IsDeleted)
                    .FirstOrDefaultAsync();

                if (workshop == null)
                {
                    return NotFound("not-found");
                }

                var workshopNote = new WorkshopNote
                {
                    WorkshopId = workshop.Id,
                    Title = model.Title,
                    Description = model.Description,
                };

                await _context.WorkshopNotes.AddAsync(workshopNote);

                await _context.SaveChangesAsync();

                return Ok("note-created");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpGet("get-notes-by/{workshopId}")]
        public async Task<ActionResult<ICollection<WorkshopNoteResponseDto>>> GetNotesByWorkshop(Guid workshopId)
        {
            try
            {
                var workshop = await _context.Workshops
                    .Where(w => w.Id == workshopId && !w.IsDeleted)
                    .FirstOrDefaultAsync();

                if (workshop == null)
                {
                    return NotFound("not-found");
                }

                var workshopNotesDto = new List<WorkshopNoteResponseDto>();

                var workshopNotes = await _context.WorkshopNotes
                    .Where(e => e.WorkshopId == workshopId)
                    .ToListAsync();

                if (workshopNotes == null)
                {
                    return NotFound("not-found");
                }

                foreach (var model in workshopNotes)
                {
                    var note = new WorkshopNoteResponseDto
                    {
                        Id = model.Id,
                        Title = model.Title,
                        Description = model.Description,
                        Date = model.Date,
                    };
                    workshopNotesDto.Add(note);
                }

                return Ok(workshopNotesDto);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPut("update-note/{noteId}")]
        [AuthorizeEmployee("Administrador")]
        public async Task<ActionResult> UpdateNote(int noteId, PutWorkshopNoteDto model)
        {
            try
            {
                var workshopNote = await _context.WorkshopNotes
                    .Where(w => w.Id == noteId)
                    .FirstOrDefaultAsync();

                if (workshopNote == null)
                {
                    return NotFound("not-found");
                }

                workshopNote.Title = model.Title;
                workshopNote.Description = model.Description;
                workshopNote.Date = DateTime.Now;

                await _context.SaveChangesAsync();

                return Ok("note-updated");
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

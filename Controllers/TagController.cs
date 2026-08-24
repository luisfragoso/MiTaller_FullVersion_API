using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiTaller.Attributes;
using MiTaller.Data;
using MiTaller.DTO;
using MiTaller.DTO.Tag;
using MiTaller.Models.Tags;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TagController : ControllerBase
{
    private readonly DataContext _context;

    public TagController(DataContext context)
    {
        _context = context;
    }

    [HttpGet("workshop/{workshopId}")]
    public async Task<ActionResult<IEnumerable<WorkshopTagsResponseDto>>> GetTagsByWorkshop(Guid workshopId)
    {
        try 
        {
            var tags = await _context.Tags
                .Where(t => t.WorkshopId == workshopId)
                .Select(t => new WorkshopTagsResponseDto
                {
                    Id = t.Id,
                    Value = t.Value,
                    Description = t.Description,
                    HexColor = t.HexColor
                })
                .ToListAsync();

            if (!tags.Any()) return NotFound("not-found");

            return Ok(tags);
        }
        catch (Exception)
        {
            return BadRequest("unkwnown-error");
        }
    }

    [HttpGet("workshop/{workshopId}/with-count")]
    public async Task<ActionResult<IEnumerable<WorkshopTagsResponseDto>>> GetTagsWithCount(Guid workshopId)
    {
        try 
        {
            var tagsWithCount = await _context.Tags
                .Where(t => t.WorkshopId == workshopId)
                .Select(t => new WorkshopTagsResponseDto
                {
                    Id = t.Id,
                    Value = t.Value,
                    Description = t.Description,
                    HexColor = t.HexColor,
                    AssignedCount = _context.CustomerAssociatedTags.Count(cat => cat.TagId == t.Id)
                })
                .ToListAsync();

            if (!tagsWithCount.Any()) return NotFound("not-found");

            return Ok(tagsWithCount);
        }
        catch (Exception)
        {
            return BadRequest("unkwnown-error");
        }
    }


    [HttpPost("workshop/{workshopId}")]
    [AuthorizeEmployee("Administrador")]
    public async Task<ActionResult> CreateTag(Guid workshopId, [FromBody] PostTagDto model)
    {
        try 
        {
            var workshopExists = await _context.Workshops.AnyAsync(w => w.Id == workshopId);
            if (!workshopExists) return NotFound("not-found");

            var tag = new Tag
            {
                WorkshopId = workshopId,
                Value = model.Value,
                Description = model.Description,
                HexColor = model.HexColor
            };

            _context.Tags.Add(tag);
            await _context.SaveChangesAsync();

            return Ok("tag-created");
        }
        catch (Exception)
        {
            return BadRequest("unkwnown-error");
        }
    }

    [HttpDelete("{tagId}")]
    [AuthorizeEmployee("Administrador")]
    public async Task<ActionResult> DeleteTag(int tagId)
    {
        try 
        {
            var tag = await _context.Tags.FindAsync(tagId);
            if (tag == null) return NotFound("not-found");

            var isTagAssigned = await _context.CustomerAssociatedTags.AnyAsync(cat => cat.TagId == tagId);
            if (isTagAssigned)
                return BadRequest("cannot-delete-tag-assigned");

            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();

            return Ok("tag-deleted");
        }
        catch (Exception)
        {
            return BadRequest("unkwnown-error");
        }
    }

    [HttpPost("get-customer-tags")]
    public async Task<ActionResult<IEnumerable<WorkshopTagsResponseDto>>> GetTagsByCustomer(CustomerWorkshopBodyDto model)
    {
        try 
        {
            var customerTags = await _context.CustomerAssociatedTags
                .Where(cat => cat.CustomerId == model.CustomerId && cat.WorkshopId == model.WorkshopId)
                .Select(cat => new WorkshopTagsResponseDto
                {
                    Id = cat.Id,
                    Value = cat.Tag.Value,
                    Description = cat.Tag.Description,
                    HexColor = cat.Tag.HexColor
                })
                .ToListAsync();

            if (!customerTags.Any()) return NotFound("not-found");

            return Ok(customerTags);
        }
        catch (Exception)
        {
            return BadRequest("unkwnown-error");
        }
    }

    [HttpPost("assign")]
    [AuthorizeEmployee("Administrador")]
    public async Task<ActionResult> AssignTagToCustomer([FromBody] AssignTagDto model)
    {
        try 
        {
            var workshopExists = await _context.Workshops.AnyAsync(w => w.Id == model.WorkshopId);
            var customerExists = await _context.Customers.AnyAsync(c => c.Id == model.CustomerId);
            var tagExists = await _context.Tags.AnyAsync(t => t.Id == model.TagId && t.WorkshopId == model.WorkshopId);

            if (!workshopExists) return NotFound("not-found");
            if (!customerExists) return NotFound("not-found");
            if (!tagExists) return NotFound("not-found");

            // Verificar si el tag ya está asignado
            var tagAlreadyAssigned = await _context.CustomerAssociatedTags.AnyAsync(cat =>
                cat.CustomerId == model.CustomerId &&
                cat.TagId == model.TagId &&
                cat.WorkshopId == model.WorkshopId
            );

            if (tagAlreadyAssigned)
                return BadRequest("tag-already-assigned");

            var customerTag = new CustomerAssociatedTag
            {
                WorkshopId = model.WorkshopId,
                CustomerId = model.CustomerId,
                TagId = model.TagId
            };

            _context.CustomerAssociatedTags.Add(customerTag);
            await _context.SaveChangesAsync();

            return Ok("tag-assinged");
        }
        catch (Exception)
        {
            return BadRequest("unkwnown-error");
        }
    }

    [HttpPut("update-tag/{tagId}")]
    [AuthorizeEmployee("Administrador")]
    public async Task<ActionResult> UpdateTag(int tagId, [FromBody] PostTagDto model)
    {
        try 
        {
            var tag = await _context.Tags
                .Where(t => t.Id == tagId)
                .FirstOrDefaultAsync();

            if (tag == null) return NotFound("El tag no existe.");

            tag.Value = model.Value;
            tag.Description = model.Description;
            tag.HexColor = model.HexColor;

            await _context.SaveChangesAsync();

            return Ok("tag-updated");
        }
        catch (Exception)
        {
            return BadRequest("unkwnown-error");
        }
    }

    [HttpDelete("unassign/{customerTagId}")]
    [AuthorizeEmployee("Administrador")]
    public async Task<ActionResult> UnassignTagFromCustomer(int customerTagId)
    {
        try 
        {
            var customerTag = await _context.CustomerAssociatedTags.FindAsync(customerTagId);
            if (customerTag == null) return NotFound("La asignación del tag no existe.");

            _context.CustomerAssociatedTags.Remove(customerTag);
            await _context.SaveChangesAsync();

            return Ok("tag-unassigned");
        }
        catch (Exception)
        {
            return BadRequest("unkwnown-error");
        }
    }
}

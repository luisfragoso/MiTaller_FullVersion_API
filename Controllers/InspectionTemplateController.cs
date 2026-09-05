using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiTaller.Data;
using MiTaller.DTO.Inspections;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MiTaller.Controllers
{
    [Route("api/inspection-templates")]
    [ApiController]
    [Authorize]
    public class InspectionTemplateController : ControllerBase
    {
        private readonly DataContext _context;

        public InspectionTemplateController(DataContext context)
        {
            _context = context;
        }

        [HttpGet("{vehicleFormat}")]
        public async Task<ActionResult<InspectionTemplateResponseDto>> GetTemplateByVehicleFormat(string vehicleFormat)
        {
            try
            {
                var template = await _context.InspectionTemplates
                    .Where(t => t.IsActive && _context.InspectionTemplateFormats
                        .Any(f => f.TemplateId == t.Id && f.VehicleFormat.ToLower() == vehicleFormat.ToLower()))
                    .Include(t => t.Sections)
                        .ThenInclude(s => s.Items)
                    .FirstOrDefaultAsync();

                if (template == null) return NotFound("not-found");

                var responseDto = new InspectionTemplateResponseDto
                {
                    Id = template.Id,
                    VehicleFormat = template.VehicleFormat,
                    Name = template.Name,
                    Sections = template.Sections
                        .OrderBy(s => s.Order)
                        .Select(s => new InspectionTemplateSectionResponseDto
                        {
                            Id = s.Id,
                            Name = s.Name,
                            Order = s.Order,
                            Items = s.Items
                                .OrderBy(i => i.Order)
                                .Select(i => new InspectionTemplateItemResponseDto
                                {
                                    Id = i.Id,
                                    Name = i.Name,
                                    Order = i.Order,
                                    AllowNotApplicable = i.AllowNotApplicable
                                }).ToList()
                        }).ToList()
                };

                return Ok(responseDto);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }
    }
}

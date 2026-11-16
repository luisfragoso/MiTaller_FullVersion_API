using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiTaller.Data;
using MiTaller.DTO.Inspections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MiTaller.Services.Localization;

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

        [HttpGet("specialties")]
        [AllowAnonymous]
        public async Task<ActionResult<List<string>>> GetSpecialties()
        {
            try
            {
                var specialties = await _context.InspectionTemplateSpecialties
                    .Where(s => s.Template != null && s.Template.IsActive)
                    .Select(s => s.Specialty)
                    .Distinct()
                    .ToListAsync();

                return Ok(specialties);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpGet("{vehicleFormat}")]
        public async Task<ActionResult<InspectionTemplateResponseDto>> GetTemplateByVehicleFormat(string vehicleFormat)
        {
            try
            {
                // Matches either a real vehicle format (Yate, Avión, ...) or a
                // "taller especializado" specialty key (Radiadores, ...) - both
                // resolve to a template the same way, the caller doesn't need to
                // know which kind of key it's passing.
                var template = await _context.InspectionTemplates
                    .Where(t => t.IsActive && (_context.InspectionTemplateFormats
                        .Any(f => f.TemplateId == t.Id && f.VehicleFormat.ToLower() == vehicleFormat.ToLower())
                        || _context.InspectionTemplateSpecialties
                        .Any(s => s.TemplateId == t.Id && s.Specialty.ToLower() == vehicleFormat.ToLower())))
                    .Include(t => t.Sections)
                        .ThenInclude(s => s.Items)
                    .FirstOrDefaultAsync();

                if (template == null) return NotFound("not-found");
                var lang = Lang.FromRequest(Request);

                var responseDto = new InspectionTemplateResponseDto
                {
                    Id = template.Id,
                    VehicleFormat = template.VehicleFormat,
                    Name = template.Name.Localized(template.NameEn, lang),
                    Sections = template.Sections
                        .OrderBy(s => s.Order)
                        .Select(s => new InspectionTemplateSectionResponseDto
                        {
                            Id = s.Id,
                            Key = s.Name,
                            Name = s.Name.Localized(s.NameEn, lang),
                            Order = s.Order,
                            Items = s.Items
                                .OrderBy(i => i.Order)
                                .Select(i => new InspectionTemplateItemResponseDto
                                {
                                    Id = i.Id,
                                    Key = i.Name,
                                    Name = i.Name.Localized(i.NameEn, lang),
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

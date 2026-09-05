using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiTaller.Data;
using MiTaller.DTO.Vehicle;
using MiTaller.Models.Domain;
using MiTaller.Models.Vehicle;

namespace MiTaller.Controllers
{
    /// <summary>
    /// Write endpoints for the Brand -> VehicleModel -> VehicleVersion -> VehicleType
    /// catalog (mirrors the read-only routes already exposed by VehicleController).
    /// This catalog is global to the platform (not scoped to a single workshop), so
    /// it's protected the same way as the rest of the Portal Admin backend
    /// (AdminController): [Authorize(Policy = "PlatformAdmin")] + the "AdminPortal"
    /// CORS policy, rather than AuthorizeEmployee which is for per-workshop staff.
    /// </summary>
    [Route("api/vehicle")]
    [ApiController]
    [Authorize(Policy = "PlatformAdmin")]
    [EnableCors("AdminPortal")]
    public class VehicleCatalogController : ControllerBase
    {
        private readonly DataContext _context;

        public VehicleCatalogController(DataContext context)
        {
            _context = context;
        }

        // Brands //

        [HttpPost("brands")]
        public async Task<ActionResult> CreateBrand([FromBody] BrandRequestDto model)
        {
            try
            {
                var brand = new Brand
                {
                    Type = model.Type,
                    Name = model.Name,
                };

                _context.Brands.Add(brand);
                await _context.SaveChangesAsync();

                return Ok(new { brand.Id, brand.Type, brand.Name });
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPut("brands/{id}")]
        public async Task<ActionResult> UpdateBrand(int id, [FromBody] BrandRequestDto model)
        {
            try
            {
                if (id == -1) return BadRequest("cannot-edit-other");

                var brand = await _context.Brands.FindAsync(id);
                if (brand == null) return NotFound("not-found");

                brand.Type = model.Type;
                brand.Name = model.Name;
                await _context.SaveChangesAsync();

                return Ok("brand-updated");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpDelete("brands/{id}")]
        public async Task<ActionResult> DeleteBrand(int id)
        {
            try
            {
                if (id == -1) return BadRequest("cannot-delete-other");

                var brand = await _context.Brands.FindAsync(id);
                if (brand == null) return NotFound("not-found");

                var hasModels = await _context.VehicleModels.AnyAsync(m => m.BrandId == id);
                if (hasModels) return BadRequest("No se puede eliminar la marca: tiene modelos registrados.");

                var hasVehicles = await _context.Vehicles.AnyAsync(v => v.BrandId == id);
                if (hasVehicles) return BadRequest("No se puede eliminar la marca: hay vehículos registrados con ella.");

                _context.Brands.Remove(brand);
                await _context.SaveChangesAsync();

                return Ok("brand-deleted");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        // Models //

        [HttpPost("models")]
        public async Task<ActionResult> CreateModel([FromBody] VehicleModelRequestDto model)
        {
            try
            {
                var brandExists = await _context.Brands.AnyAsync(b => b.Id == model.BrandId);
                if (!brandExists) return NotFound("not-found");

                var vehicleModel = new VehicleModel
                {
                    BrandId = model.BrandId,
                    Model = model.Model,
                };

                _context.VehicleModels.Add(vehicleModel);
                await _context.SaveChangesAsync();

                return Ok(new { vehicleModel.Id, vehicleModel.BrandId, vehicleModel.Model });
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPut("models/{id}")]
        public async Task<ActionResult> UpdateModel(int id, [FromBody] VehicleModelRequestDto model)
        {
            try
            {
                if (id == -1) return BadRequest("cannot-edit-other");

                var vehicleModel = await _context.VehicleModels.FindAsync(id);
                if (vehicleModel == null) return NotFound("not-found");

                var brandExists = await _context.Brands.AnyAsync(b => b.Id == model.BrandId);
                if (!brandExists) return NotFound("not-found");

                vehicleModel.BrandId = model.BrandId;
                vehicleModel.Model = model.Model;
                await _context.SaveChangesAsync();

                return Ok("model-updated");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpDelete("models/{id}")]
        public async Task<ActionResult> DeleteModel(int id)
        {
            try
            {
                if (id == -1) return BadRequest("cannot-delete-other");

                var vehicleModel = await _context.VehicleModels.FindAsync(id);
                if (vehicleModel == null) return NotFound("not-found");

                var hasVersions = await _context.VehicleVersions.AnyAsync(v => v.VehicleModelId == id);
                if (hasVersions) return BadRequest("No se puede eliminar el modelo: tiene versiones registradas.");

                var hasVehicles = await _context.Vehicles.AnyAsync(v => v.VehicleModelId == id);
                if (hasVehicles) return BadRequest("No se puede eliminar el modelo: hay vehículos registrados con él.");

                _context.VehicleModels.Remove(vehicleModel);
                await _context.SaveChangesAsync();

                return Ok("model-deleted");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        // Versions //

        [HttpPost("versions")]
        public async Task<ActionResult> CreateVersion([FromBody] VehicleVersionRequestDto model)
        {
            try
            {
                var modelExists = await _context.VehicleModels.AnyAsync(m => m.Id == model.VehicleModelId);
                if (!modelExists) return NotFound("not-found");

                var version = new VehicleVersion
                {
                    VehicleModelId = model.VehicleModelId,
                    Version = model.Version,
                };

                _context.VehicleVersions.Add(version);
                await _context.SaveChangesAsync();

                return Ok(new { version.Id, version.VehicleModelId, version.Version });
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPut("versions/{id}")]
        public async Task<ActionResult> UpdateVersion(int id, [FromBody] VehicleVersionRequestDto model)
        {
            try
            {
                if (id == -1) return BadRequest("cannot-edit-other");

                var version = await _context.VehicleVersions.FindAsync(id);
                if (version == null) return NotFound("not-found");

                var modelExists = await _context.VehicleModels.AnyAsync(m => m.Id == model.VehicleModelId);
                if (!modelExists) return NotFound("not-found");

                version.VehicleModelId = model.VehicleModelId;
                version.Version = model.Version;
                await _context.SaveChangesAsync();

                return Ok("version-updated");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpDelete("versions/{id}")]
        public async Task<ActionResult> DeleteVersion(int id)
        {
            try
            {
                if (id == -1) return BadRequest("cannot-delete-other");

                var version = await _context.VehicleVersions.FindAsync(id);
                if (version == null) return NotFound("not-found");

                var hasTypes = await _context.VehicleTypes.AnyAsync(t => t.VehicleVersionId == id);
                if (hasTypes) return BadRequest("No se puede eliminar la versión: tiene tipos registrados.");

                var hasVehicles = await _context.Vehicles.AnyAsync(v => v.VehicleVersionId == id);
                if (hasVehicles) return BadRequest("No se puede eliminar la versión: hay vehículos registrados con ella.");

                _context.VehicleVersions.Remove(version);
                await _context.SaveChangesAsync();

                return Ok("version-deleted");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        // Types //

        [HttpPost("types")]
        public async Task<ActionResult> CreateType([FromBody] VehicleTypeRequestDto model)
        {
            try
            {
                var versionExists = await _context.VehicleVersions.AnyAsync(v => v.Id == model.VehicleVersionId);
                if (!versionExists) return NotFound("not-found");

                var type = new VehicleType
                {
                    VehicleVersionId = model.VehicleVersionId,
                    Type = model.Type,
                };

                _context.VehicleTypes.Add(type);
                await _context.SaveChangesAsync();

                return Ok(new { type.Id, type.VehicleVersionId, type.Type });
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPut("types/{id}")]
        public async Task<ActionResult> UpdateType(int id, [FromBody] VehicleTypeRequestDto model)
        {
            try
            {
                if (id == -1) return BadRequest("cannot-edit-other");

                var type = await _context.VehicleTypes.FindAsync(id);
                if (type == null) return NotFound("not-found");

                var versionExists = await _context.VehicleVersions.AnyAsync(v => v.Id == model.VehicleVersionId);
                if (!versionExists) return NotFound("not-found");

                type.VehicleVersionId = model.VehicleVersionId;
                type.Type = model.Type;
                await _context.SaveChangesAsync();

                return Ok("type-updated");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpDelete("types/{id}")]
        public async Task<ActionResult> DeleteType(int id)
        {
            try
            {
                if (id == -1) return BadRequest("cannot-delete-other");

                var type = await _context.VehicleTypes.FindAsync(id);
                if (type == null) return NotFound("not-found");

                var hasVehicles = await _context.Vehicles.AnyAsync(v => v.VehicleTypeId == id);
                if (hasVehicles) return BadRequest("No se puede eliminar el tipo: hay vehículos registrados con él.");

                _context.VehicleTypes.Remove(type);
                await _context.SaveChangesAsync();

                return Ok("type-deleted");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiTaller.Data;
using MiTaller.DTO.Customer;
using MiTaller.DTO.Pager;
using MiTaller.DTO.Vehicle;
using MiTaller.Models.Vehicle;

namespace MiTaller.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class VehicleController : ControllerBase
    {
        private readonly DataContext _context;

        public VehicleController(DataContext context)
        {
            _context = context;
        }

        [HttpGet("vehicle-types")]
        public async Task<ActionResult> GetVehicleTypes()
        {
            try 
            {
                var types = await _context.Brands
                    .Select(b => b.Type)
                    .Where(type => type != "Generic")
                    .Distinct()
                    .ToListAsync();

                return Ok(types);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpGet("brands/{type}")]
        public async Task<ActionResult> GetBrandsByType(string type)
        {
            try 
            {
                var brands = await _context.Brands
                    .Where(b => b.Type == type)
                    .Select(b => new { b.Id, b.Name })
                    .ToListAsync();

                brands.Add(new { Id = -1, Name = "Other" });
                return Ok(brands);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpGet("models/{brandId}")]
        public async Task<ActionResult> GetModelsByBrand(int brandId)
        {
            try 
            {
                var models = await _context.VehicleModels
                    .Where(vm => vm.BrandId == brandId)
                    .Select(vm => new { vm.Id, vm.Model })
                    .ToListAsync();

                models.Add(new { Id = -1, Model = "Other" });
                return Ok(models);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpGet("versions/{modelId}")]
        public async Task<ActionResult> GetVersionsByModel(int modelId)
        {
            try 
            {
                var versions = await _context.VehicleVersions
                    .Where(vv => vv.VehicleModelId == modelId)
                    .Select(vv => new { vv.Id, vv.Version })
                    .ToListAsync();

                versions.Add(new { Id = -1, Version = "Other" });
                return Ok(versions);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpGet("types/{versionId}")]
        public async Task<ActionResult> GetTypesByVersion(int versionId)
        {
            try 
            {
                var types = await _context.VehicleTypes
                    .Where(vt => vt.VehicleVersionId == versionId)
                    .Select(vt => new { vt.Id, vt.Type })
                    .ToListAsync();

                types.Add(new { Id = -1, Type = "Other" });
                return Ok(types);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }


        [HttpGet("{vehicleId}")]
        public async Task<ActionResult<IEnumerable<VehicleResponseDto>>> GetVehicleById(int vehicleId)
        {
            try 
            {
                var vehicle = await _context.Vehicles
                    .Where(v => v.Id == vehicleId && !v.IsDeleted)
                    .Select(v => new VehicleResponseDto
                    {
                        Id = v.Id,
                        Brand = v.BrandId == -1 ? v.OtherBrand : v.Brand.Name,
                        Model = v.VehicleModelId == -1 ? v.OtherVehicleModel : v.VehicleModel.Model,
                        Version = v.VehicleVersionId == -1 ? v.OtherVehicleVersion : v.VehicleVersion.Version,
                        Type = v.VehicleTypeId == -1 ? v.OtherVehicleType : v.VehicleType.Type,
                        Year = v.Year,
                        SerialNumber = v.SerialNumber,
                        Color = v.Color,
                        Plates = v.Plates,
                        RimRubber = v.RimRubber,
                        Kms = v.Kms,
                        VehicleFormat = v.VehicleFormat,
                        Image = v.Image
                    })
                    .FirstOrDefaultAsync();

                if (vehicle == null) return NotFound("not-found");

                return Ok(vehicle);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpGet("customer/{customerId}")]
        public async Task<ActionResult<IEnumerable<VehicleByCustomerResponseDto>>> GetVehiclesByCustomer(Guid customerId)
        {
            try 
            {
                var vehicles = await _context.Vehicles
                    .Where(v => v.CustomerId == customerId && !v.IsDeleted)
                    .Select(v => new VehicleByCustomerResponseDto
                    {
                        Id = v.Id,
                        Brand = v.BrandId == -1 ? v.OtherBrand : v.Brand.Name,
                        Model = v.VehicleModelId == -1 ? v.OtherVehicleModel : v.VehicleModel.Model,
                        Version = v.VehicleVersionId == -1 ? v.OtherVehicleVersion : v.VehicleVersion.Version,
                        Type = v.VehicleTypeId == -1 ? v.OtherVehicleType : v.VehicleType.Type,
                        Year = v.Year,
                        SerialNumber = v.SerialNumber,
                        Color = v.Color,
                        Plates = v.Plates,
                        RimRubber = v.RimRubber,
                        Kms = v.Kms,
                        VehicleFormat = v.VehicleFormat,
                        Image = v.Image
                    })
                    .ToListAsync();

                if (!vehicles.Any()) return NotFound("not-found");

                return Ok(vehicles);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPost("customer-pager/{customerId}")]
        public async Task<ActionResult<PagerResponseDto<VehicleByCustomerResponseDto>>> GetVehiclesByCustomerPaged(Guid customerId, [FromBody] PagerBodyDto pager)
        {
            try
            {
                var query = _context.Vehicles
                    .Where(v => v.CustomerId == customerId && !v.IsDeleted)
                    .Include(v => v.Brand)
                    .Include(v => v.VehicleModel)
                    .Include(v => v.VehicleVersion)
                    .Include(v => v.VehicleType);

                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalCount / pager.PageSize);

                var pagedVehicles = await query
                    .OrderByDescending(v => v.Year) // o por CreatedAt si lo tienes
                    .Skip((pager.PageNumber - 1) * pager.PageSize)
                    .Take(pager.PageSize)
                    .Select(v => new VehicleByCustomerResponseDto
                    {
                        Id = v.Id,
                        Brand = v.BrandId == -1 ? v.OtherBrand : v.Brand.Name,
                        Model = v.VehicleModelId == -1 ? v.OtherVehicleModel : v.VehicleModel.Model,
                        Version = v.VehicleVersionId == -1 ? v.OtherVehicleVersion : v.VehicleVersion.Version,
                        Type = v.VehicleTypeId == -1 ? v.OtherVehicleType : v.VehicleType.Type,
                        Year = v.Year,
                        SerialNumber = v.SerialNumber,
                        Color = v.Color,
                        Plates = v.Plates,
                        RimRubber = v.RimRubber,
                        Kms = v.Kms,
                        VehicleFormat = v.VehicleFormat,
                        Image = v.Image
                    })
                    .ToListAsync();

                if (!pagedVehicles.Any()) return NotFound("not-found");

                var response = new PagerResponseDto<VehicleByCustomerResponseDto>
                {
                    CurrentPage = pager.PageNumber,
                    TotalPages = totalPages,
                    Elements = pagedVehicles
                };

                return Ok(response);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPost]
        public async Task<ActionResult> CreateVehicle(PostVehicleDto model)
        {
            try 
            {
                var customerExists = await _context.Customers.AnyAsync(c => c.Id == model.CustomerId && !c.IsDeleted);
                if (!customerExists)
                    return NotFound("not-found");

                // "Other" Validations
                if (model.BrandId == -1 && string.IsNullOrEmpty(model.OtherBrand))
                    return BadRequest("You must provide a brand if you selected 'Other'.");

                if (model.VehicleModelId == -1 && string.IsNullOrEmpty(model.OtherVehicleModel))
                    return BadRequest("You must provide a model if you selected 'Other'.");

                if (model.VehicleVersionId == -1 && string.IsNullOrEmpty(model.OtherVehicleVersion))
                    return BadRequest("You must provide a version if you selected 'Other'.");

                if (model.VehicleTypeId == -1 && string.IsNullOrEmpty(model.OtherVehicleType))
                    return BadRequest("You must provide a type if you selected 'Other'.");

                var vehicle = new Vehicle
                {
                    CustomerId = model.CustomerId,
                    Year = model.Year,
                    BrandId = model.BrandId,
                    OtherBrand = model.OtherBrand,
                    VehicleModelId = model.VehicleModelId,
                    OtherVehicleModel = model.OtherVehicleModel,
                    VehicleVersionId = model.VehicleVersionId,
                    OtherVehicleVersion = model.OtherVehicleVersion,
                    VehicleTypeId = model.VehicleTypeId,
                    OtherVehicleType = model.OtherVehicleType,
                    SerialNumber = model.SerialNumber,
                    Color = model.Color,
                    Plates = model.Plates,
                    RimRubber = model.RimRubber != null ? model.RimRubber : null,
                    Kms = model.Kms,
                    VehicleFormat = model.VehicleFormat,
                };

                if (model.Image != null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await model.Image.CopyToAsync(memoryStream);
                        vehicle.Image = memoryStream.ToArray();
                    }
                }

                _context.Vehicles.Add(vehicle);
                await _context.SaveChangesAsync();

                return Ok("vehicle-created");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPut("{vehicleId}")]
        public async Task<ActionResult> UpdateVehicle(int vehicleId, [FromBody] Vehicle vehicle)
        {
            try 
            {
                var existingVehicle = await _context.Vehicles.FindAsync(vehicleId);
                if (existingVehicle == null || existingVehicle.IsDeleted)
                    return NotFound("not-found");

                existingVehicle.Year = vehicle.Year;
                existingVehicle.BrandId = vehicle.BrandId;
                existingVehicle.OtherBrand = vehicle.OtherBrand;
                existingVehicle.VehicleModelId = vehicle.VehicleModelId;
                existingVehicle.OtherVehicleModel = vehicle.OtherVehicleModel;
                existingVehicle.VehicleVersionId = vehicle.VehicleVersionId;
                existingVehicle.OtherVehicleVersion = vehicle.OtherVehicleVersion;
                existingVehicle.VehicleTypeId = vehicle.VehicleTypeId;
                existingVehicle.OtherVehicleType = vehicle.OtherVehicleType;
                existingVehicle.SerialNumber = vehicle.SerialNumber;
                existingVehicle.Color = vehicle.Color;
                existingVehicle.Plates = vehicle.Plates;
                existingVehicle.RimRubber = vehicle.RimRubber;
                existingVehicle.Kms = vehicle.Kms;
                existingVehicle.VehicleFormat = vehicle.VehicleFormat;
                existingVehicle.Image = vehicle.Image;

                await _context.SaveChangesAsync();

                return Ok("vehicle-updated");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpDelete("{vehicleId}")]
        public async Task<ActionResult> DeleteVehicle(int vehicleId)
        {
            try 
            {
                var vehicle = await _context.Vehicles.FindAsync(vehicleId);
                if (vehicle == null || vehicle.IsDeleted)
                    return NotFound("not-found");

                vehicle.IsDeleted = true;
                await _context.SaveChangesAsync();

                return Ok("vehicle-deleted");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpPost("upload-image/{vehicleId}")]
        public async Task<ActionResult> UploadVehicleImage(int vehicleId, IFormFile file)
        {
            try 
            {
                var vehicle = await _context.Vehicles.FindAsync(vehicleId);
                if (vehicle == null || vehicle.IsDeleted)
                    return NotFound("not-found");

                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    vehicle.Image = memoryStream.ToArray();
                }

                await _context.SaveChangesAsync();
                return Ok("image-saved");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpGet("view-image/{vehicleId}")]
        public async Task<ActionResult> ViewVehicleImage(int vehicleId)
        {
            try 
            {
                var vehicle = await _context.Vehicles.FindAsync(vehicleId);
                if (vehicle == null || vehicle.IsDeleted)
                    return NotFound("not-found");

                if (vehicle.Image == null)
                    return NotFound("not-found");

                return File(vehicle.Image, "image/jpeg");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }
    }
}

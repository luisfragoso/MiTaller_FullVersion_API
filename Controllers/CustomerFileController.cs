using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiTaller.Data;
using MiTaller.DTO.Customer;
using MiTaller.Models.Customer;
using MiTaller.Models.Domain;

namespace MiTaller.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerFileController : ControllerBase
    {
        private readonly DataContext _context;

        public CustomerFileController(DataContext context)
        {
            _context = context;
        }

        [HttpPost("upload-files")]
        public async Task<ActionResult> UploadFiles(PostCustomerFilesDto model)
        {
            try
            {
                var customer = await _context.Customers.FindAsync(model.CustomerId);
                if (customer == null)
                {
                    return NotFound("not-found");
                }

                if (model.Files.Count != model.FileDescriptions.Count)
                {
                    return BadRequest("files-and-descriptions-do-not-match");
                }

                var customerFilesToAdd = new List<CustomerFile>();
                var customerFilesToUpdate = new List<CustomerFile>();


                int index = 0;
                foreach (var file in model.Files)
                {
                    if (file.Length > 0) // Validar que no esté vacío
                    {
                        using var memoryStream = new MemoryStream();
                        await file.CopyToAsync(memoryStream);

                        var existingFile = await _context.CustomerFiles
                        .FirstOrDefaultAsync(cf => cf.CustomerId == model.CustomerId && cf.File.FileDescription == model.FileDescriptions[index]);

                        if (existingFile != null)
                        {
                            existingFile.File.FileName = file.FileName;
                            existingFile.File.FileType = file.ContentType;
                            existingFile.File.FileData = memoryStream.ToArray();
                            existingFile.UploadedAt = DateTime.Now;

                            customerFilesToUpdate.Add(existingFile);
                        }
                        else
                        {
                            var customerFile = new CustomerFile
                            {
                                CustomerId = model.CustomerId,
                                File = new FileModel
                                {
                                    FileName = file.FileName,
                                    FileType = file.ContentType,
                                    FileData = memoryStream.ToArray(),
                                    FileDescription = model.FileDescriptions[index]
                                }
                            };

                            customerFilesToAdd.Add(customerFile);
                        }
                    }
                    index++;
                }

                if(!customerFilesToAdd.Any() && !customerFilesToUpdate.Any())
        {
                    return BadRequest("invalid-empty");
                }

                if (customerFilesToAdd.Any())
                {
                    _context.CustomerFiles.AddRange(customerFilesToAdd);
                }

                if (customerFilesToUpdate.Any())
                {
                    _context.CustomerFiles.UpdateRange(customerFilesToUpdate);
                }

                await _context.SaveChangesAsync();

                return Ok("files-saved");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }

        }


        [HttpGet("files/{customerId}")]
        public async Task<ActionResult<CustomerFileResponseDto>> GetCustomerFiles(Guid customerId)
        {
            try 
            { 
                var files = await _context.CustomerFiles
                    .Where(f => f.CustomerId == customerId)
                    .Select(f => new CustomerFileResponseDto
                    {
                        FileId = f.Id,
                        FileName = f.File.FileName,
                        Type = f.File.FileType,
                        Description = f.File.FileDescription,
                        FileData = f.File.FileData,
                        UploadedAt = f.UploadedAt.ToString("yyyy-MM-dd HH:mm:ss")
                    }).ToListAsync();

                if (!files.Any())
                {
                    return NotFound("not-found");
                }

                return Ok(files);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }

        [HttpGet("download-file/{fileId}")]
        public async Task<ActionResult> DownloadFile(Guid fileId)
        {
            try 
            { 
                var customerFile = await _context.CustomerFiles.FindAsync(fileId);
                if (customerFile == null)
                {
                    return NotFound("not-found");
                }

                return File(customerFile.File.FileData, customerFile.File.FileType, customerFile.File.FileName);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }


        //[HttpGet("view-image/{fileId}")]
        //public async Task<IActionResult> ViewImage(Guid fileId)
        //{
        //    try 
        //    { 
        //        var customerFile = await _context.CustomerFiles.FindAsync(fileId);
        //        if (customerFile == null)
        //        {
        //            return NotFound("not-found");
        //        }

        //        if (!customerFile.File.FileType.StartsWith("image/"))
        //        {
        //            return BadRequest("file-is-not-image");
        //        }

        //        return File(customerFile.File.FileData, customerFile.File.FileType);
        //    }
        //    catch (Exception)
        //    {
        //        return BadRequest("unkwnown-error");
        //    }
        //}

    }
}

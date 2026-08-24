using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiTaller.Attributes;
using MiTaller.Data;
using MiTaller.DTO.Workshop.File;
using MiTaller.Models.Domain;
using MiTaller.Models.Workshop;

namespace MiTaller.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WorkshopFileController : ControllerBase
    {
        private readonly DataContext _context;

        public WorkshopFileController(DataContext context)
        {
            _context = context;
        }

        [HttpPost("upload-files")]
        [AuthorizeEmployee("Administrador")]
        public async Task<ActionResult> UploadFiles(PostWorkshopFilesDto model)
        {
            try
            {
                var workshop = await _context.Workshops.FindAsync(model.WorkshopId);
                if (workshop == null)
                {
                    return NotFound("not-found");
                }

                if (model.Files.Count != model.FileDescriptions.Count)
                {
                    return BadRequest("The number of files and descriptions do not match.");
                }

                var workshopFilesToAdd = new List<WorkshopFile>();
                var workshopFilesToUpdate = new List<WorkshopFile>();

                int index = 0;
                foreach (var file in model.Files)
                {
                    if (file.Length > 0) // Validar que no esté vacío
                    {
                        using var memoryStream = new MemoryStream();
                        await file.CopyToAsync(memoryStream);

                        var existingFile = await _context.WorkshopFiles
                            .FirstOrDefaultAsync(wf => wf.WorkshopId == model.WorkshopId && wf.File.FileDescription == model.FileDescriptions[index]);

                        if (existingFile != null)
                        {
                            existingFile.File.FileName = file.FileName;
                            existingFile.File.FileType = file.ContentType;
                            existingFile.File.FileData = memoryStream.ToArray();
                            existingFile.UploadedAt = DateTime.Now;

                            workshopFilesToUpdate.Add(existingFile);
                        }
                        else
                        {
                            var workshopFile = new WorkshopFile
                            {
                                WorkshopId = model.WorkshopId,
                                File = new FileModel
                                {
                                    FileName = file.FileName,
                                    FileType = file.ContentType,
                                    FileData = memoryStream.ToArray(),
                                    FileDescription = model.FileDescriptions[index]
                                }
                            };

                            workshopFilesToAdd.Add(workshopFile);
                        }
                    }
                    index++;
                }

                if (!workshopFilesToAdd.Any() && !workshopFilesToUpdate.Any())
                {
                    return BadRequest("invalid-empty");
                }

                if (workshopFilesToAdd.Any())
                {
                    _context.WorkshopFiles.AddRange(workshopFilesToAdd);
                }

                if (workshopFilesToUpdate.Any())
                {
                    _context.WorkshopFiles.UpdateRange(workshopFilesToUpdate);
                }
                await _context.SaveChangesAsync();

                return Ok("files-saved");
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }

        }


        [HttpGet("files/{workshopId}")]
        public async Task<ActionResult<WorkshopFileResponseDto>> GetWorkshopFiles(Guid workshopId)
        {
            try 
            {
                var files = await _context.WorkshopFiles
                    .Where(f => f.WorkshopId == workshopId)
                    .Select(f => new WorkshopFileResponseDto
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
                var workshopFile = await _context.WorkshopFiles.FindAsync(fileId);
                if (workshopFile == null)
                {
                    return NotFound("not-found");
                }

                return File(workshopFile.File.FileData, workshopFile.File.FileType, workshopFile.File.FileName);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }


        [HttpGet("view-image/{fileId}")]
        public async Task<IActionResult> ViewImage(Guid fileId)
        {
            try 
            {
                var workhsopFile = await _context.WorkshopFiles.FindAsync(fileId);
                if (workhsopFile == null)
                {
                    return NotFound("not-found");
                }

                if (!workhsopFile.File.FileType.StartsWith("image/"))
                {
                    return BadRequest("file-not-image");
                }

                return File(workhsopFile.File.FileData, workhsopFile.File.FileType);
            }
            catch (Exception)
            {
                return BadRequest("unkwnown-error");
            }
        }
    }
}

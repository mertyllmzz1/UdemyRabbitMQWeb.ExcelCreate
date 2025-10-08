using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UdemyRabbitMQWeb.ExcelCreate.Models;

namespace UdemyRabbitMQWeb.ExcelCreate.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly AppDbContext _context;
        public FilesController(AppDbContext appDbContext)
        {
            _context = appDbContext;
        }
        public async Task<IActionResult> Upload(IFormFile file,int fileId)
        {
            if (file is not { Length: > 0 })
                return BadRequest("File is empty");

            var userFile = await _context.UserFiles.FirstAsync(p => p.Id == fileId);

            var filePath = userFile.FileName + Path.GetExtension(file.FileName);
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files", filePath);

            await using var stream = new FileStream(path, FileMode.Create);

            await file.CopyToAsync(stream);
       
            userFile.CreatedDate = DateTime.Now;
            userFile.FilePath = filePath;
            userFile.FileStatus = FileStatus.Completed;
            
            await _context.SaveChangesAsync();
            return Ok();

        }
    }
}

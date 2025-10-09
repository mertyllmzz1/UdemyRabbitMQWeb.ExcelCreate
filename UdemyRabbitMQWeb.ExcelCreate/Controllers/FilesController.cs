using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UdemyRabbitMQWeb.ExcelCreate.Hubs;
using UdemyRabbitMQWeb.ExcelCreate.Models;

namespace UdemyRabbitMQWeb.ExcelCreate.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<MyHub> _hubContext;
        public FilesController(AppDbContext appDbContext, IHubContext<MyHub> hubContext)
        {
            _context = appDbContext;
            _hubContext = hubContext;
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
            //signalr ile olustugunda kullaniciya mesaj gonderiyoruz. authnetication oldugu icin userId ile gonderebiliyoruz.
            await _hubContext.Clients.User(userFile.UserId).SendAsync("ReceiveMessage");
            return Ok();

        }
    }
}

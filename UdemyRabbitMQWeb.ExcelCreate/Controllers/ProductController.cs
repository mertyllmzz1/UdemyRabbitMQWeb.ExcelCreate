using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using UdemyRabbitMQWeb.ExcelCreate.Models;

namespace UdemyRabbitMQWeb.ExcelCreate.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        public ProductController(AppDbContext context
            , UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> CreateProductExcel()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            var fileName=$"productlist_{Guid.NewGuid().ToString().Substring(1,10)}.xlsx";
            UserFile userFile = new()
            {
                FileName = fileName,
                FileStatus = FileStatus.Creating,
                UserId = user.Id,
            };
            await _context.UserFiles.AddAsync(userFile);
            await _context.SaveChangesAsync();
            TempData["StartCreatingExcel"]=true;
            return RedirectToAction(nameof(Files));
        }
        public async Task<IActionResult> Files()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            return View(await _context.UserFiles.Where(p=>p.UserId == user.Id).ToListAsync());
        }
    }
}

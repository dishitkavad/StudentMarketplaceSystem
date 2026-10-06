
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentMarketplaceSystem.Data;

namespace StudentMarketplaceSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    [Route("Admin/Statistics")]
    public class StatisticsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StatisticsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/Statistics
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            ViewBag.TotalUsers = await _context.Users.CountAsync();

            ViewBag.TotalProducts = await _context.Products.CountAsync();

            ViewBag.SoldProducts = await _context.Products
                .CountAsync(p => p.IsSold);

            ViewBag.PendingRequests = await _context.Transactions
                .CountAsync(t => t.Status == "Pending");

            ViewBag.TotalReports = await _context.Reports.CountAsync();

            return View();
        }
    }
}

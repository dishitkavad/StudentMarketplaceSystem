using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentMarketplaceSystem.Data;
using StudentMarketplaceSystem.Models;

namespace StudentMarketplaceSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Reports
        public async Task<IActionResult> Index()
        {
            var reports = await _context.Reports
                .Include(r => r.Reporter)
                .Include(r => r.Product)
                .Include(r => r.ReportedUser)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();

            return View(reports);
        }

        // POST: Admin/Reports/Resolve
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Resolve(int id)
        {
            var report = await _context.Reports
                .FirstOrDefaultAsync(r => r.ReportId == id);

            if (report == null)
            {
                return NotFound();
            }

            report.Status = "Resolved";

            await _context.SaveChangesAsync();

            TempData["ReportMessage"] =
                "Report marked as resolved.";

            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Reports/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var report = await _context.Reports
                .FirstOrDefaultAsync(r => r.ReportId == id);

            if (report == null)
            {
                return NotFound();
            }

            _context.Reports.Remove(report);

            await _context.SaveChangesAsync();

            TempData["ReportMessage"] =
                "Report deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
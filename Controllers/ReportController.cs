using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentMarketplaceSystem.Data;
using StudentMarketplaceSystem.Models;

namespace StudentMarketplaceSystem.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReportController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Report/CreateProduct
        [HttpGet]
        public async Task<IActionResult> CreateProduct(int productId)
        {
            var product = await _context.Products
                .Include(p => p.Seller)
                .FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product == null)
            {
                return NotFound();
            }

            ViewBag.Product = product;

            return View();
        }

        // POST: Report/CreateProduct
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(
            int productId,
            string reason,
            string? description)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == productId);

            if (product == null)
            {
                return NotFound();
            }

            // Prevent users from reporting their own product
            if (product.UserId == user.Id)
            {
                TempData["ReportMessage"] =
                    "You cannot report your own product.";

                return RedirectToAction(
                    "Details",
                    "Product",
                    new { id = productId });
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["ReportMessage"] =
                    "Please select a reason for the report.";

                return RedirectToAction(
                    nameof(CreateProduct),
                    new { productId });
            }

            var report = new Report
            {
                ReporterId = user.Id,
                ProductId = productId,
                ReportedUserId = product.UserId,
                Reason = reason,
                Description = description,
                Status = "Pending",
                CreatedDate = DateTime.Now
            };

            _context.Reports.Add(report);

            await _context.SaveChangesAsync();

            TempData["ReportMessage"] =
                "Report submitted successfully.";

            return RedirectToAction(
                "Details",
                "Product",
                new { id = productId });
        }

        // GET: Report/CreateUser
        [HttpGet]
        public async Task<IActionResult> CreateUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            ViewBag.ReportedUser = user;

            return View();
        }

        // POST: Report/CreateUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(
            string userId,
            string reason,
            string? description)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var reportedUser = await _userManager.FindByIdAsync(userId);

            if (reportedUser == null)
            {
                return NotFound();
            }

            // Prevent users from reporting themselves
            if (reportedUser.Id == user.Id)
            {
                TempData["ReportMessage"] =
                    "You cannot report yourself.";

                return RedirectToAction("Index", "Home");
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["ReportMessage"] =
                    "Please select a reason for the report.";

                return RedirectToAction(
                    nameof(CreateUser),
                    new { userId });
            }

            var report = new Report
            {
                ReporterId = user.Id,
                ReportedUserId = reportedUser.Id,
                Reason = reason,
                Description = description,
                Status = "Pending",
                CreatedDate = DateTime.Now
            };

            _context.Reports.Add(report);

            await _context.SaveChangesAsync();

            TempData["ReportMessage"] =
                "User report submitted successfully.";

            return RedirectToAction("Index", "Home");
        }
    }
}
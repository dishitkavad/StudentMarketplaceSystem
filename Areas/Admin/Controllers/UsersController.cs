
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
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ==========================================
        // VIEW USERS
        // ==========================================

        [HttpGet]
        [Route("/Admin/Users")]
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                .OrderBy(u => u.Name)
                .ToListAsync();

            return View(users);
        }

        // ==========================================
        // BLOCK USER
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("/Admin/Users/Block")]
        public async Task<IActionResult> Block(string id)
        {
            var currentAdmin = await _userManager.GetUserAsync(User);

            if (currentAdmin == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Admin cannot block their own account
            if (currentAdmin.Id == id)
            {
                TempData["UserMessage"] =
                    "You cannot block your own admin account.";

                return RedirectToAction(nameof(Index));
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            user.IsBlocked = true;

            await _context.SaveChangesAsync();

            TempData["UserMessage"] =
                "User blocked successfully.";

            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // UNBLOCK USER
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("/Admin/Users/Unblock")]
        public async Task<IActionResult> Unblock(string id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            user.IsBlocked = false;

            await _context.SaveChangesAsync();

            TempData["UserMessage"] =
                "User unblocked successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}

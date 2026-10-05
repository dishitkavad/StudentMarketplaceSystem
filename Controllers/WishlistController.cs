using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentMarketplaceSystem.Data;
using StudentMarketplaceSystem.Models;

namespace StudentMarketplaceSystem.Controllers
{
    [Authorize]
    public class WishlistController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public WishlistController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ==========================================
        // MY WISHLIST
        // ==========================================

        // GET: /Wishlist
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var wishlistItems = await _context.Wishlists
                .Include(w => w.Product)
                .ThenInclude(p => p!.Seller)
                .Where(w => w.UserId == user.Id)
                .OrderByDescending(w => w.CreatedDate)
                .ToListAsync();

            return View(wishlistItems);
        }

        // ==========================================
        // ADD TO WISHLIST
        // ==========================================

        // POST: /Wishlist/Add/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Check if product exists
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            // Check if product is already in wishlist
            var alreadyExists = await _context.Wishlists
                .AnyAsync(w =>
                    w.UserId == user.Id &&
                    w.ProductId == id);

            if (!alreadyExists)
            {
                var wishlist = new Wishlist
                {
                    UserId = user.Id,
                    ProductId = id,
                    CreatedDate = DateTime.Now
                };

                _context.Wishlists.Add(wishlist);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index", "Product");
        }

        // ==========================================
        // REMOVE FROM WISHLIST
        // ==========================================

        // POST: /Wishlist/Remove/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var wishlistItem = await _context.Wishlists
                .FirstOrDefaultAsync(w =>
                    w.WishlistId == id &&
                    w.UserId == user.Id);

            if (wishlistItem == null)
            {
                return NotFound();
            }

            _context.Wishlists.Remove(wishlistItem);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
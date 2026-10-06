
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentMarketplaceSystem.Data;

namespace StudentMarketplaceSystem.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    [Route("Admin/Products")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/Products
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.Seller)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();

            return View(products);
        }

        // POST: /Admin/Products/Delete
        [HttpPost("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            // Remove related wishlist records
            var wishlists = await _context.Wishlists
                .Where(w => w.ProductId == id)
                .ToListAsync();

            _context.Wishlists.RemoveRange(wishlists);

            // Remove related report records
            var reports = await _context.Reports
                .Where(r => r.ProductId == id)
                .ToListAsync();

            _context.Reports.RemoveRange(reports);

            // Remove related transaction records
            var transactions = await _context.Transactions
                .Where(t => t.ProductId == id)
                .ToListAsync();

            _context.Transactions.RemoveRange(transactions);

            // Finally remove the product
            _context.Products.Remove(product);

            await _context.SaveChangesAsync();

            TempData["ProductMessage"] =
                "Product deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}

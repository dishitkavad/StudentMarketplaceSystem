using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentMarketplaceSystem.Data;
using StudentMarketplaceSystem.Models;
using StudentMarketplaceSystem.ViewModels;

namespace StudentMarketplaceSystem.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProductController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ==========================================
        // PRODUCT LIST + SEARCH + CATEGORY + SORTING
        // ==========================================

        // GET: /Product
        [HttpGet]
        public async Task<IActionResult> Index(
            string search,
            string category,
            string sort)
        {
            var query = _context.Products
                .Include(p => p.Seller)
                .AsQueryable();

            // SEARCH
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p =>
                    p.Title.Contains(search));
            }

            // CATEGORY FILTER
            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(p =>
                    p.Category == category);
            }

            // SORTING
            switch (sort)
            {
                case "oldest":
                    query = query
                        .OrderBy(p => p.CreatedDate);
                    break;

                case "price_low":
                    query = query
                        .OrderBy(p => p.Price);
                    break;

                case "price_high":
                    query = query
                        .OrderByDescending(p => p.Price);
                    break;

                default:
                    // Newest first
                    query = query
                        .OrderByDescending(p => p.CreatedDate);
                    break;
            }

            var products = await query.ToListAsync();

            ViewBag.Search = search;
            ViewBag.Category = category;
            ViewBag.Sort = sort;

            return View(products);
        }

        // ==========================================
        // MY PRODUCTS
        // ==========================================

        // GET: /Product/MyProducts
        [HttpGet]
        public async Task<IActionResult> MyProducts()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var products = await _context.Products
                .Include(p => p.Seller)
                .Where(p => p.UserId == user.Id)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();

            return View(products);
        }

        // ==========================================
        // PRODUCT DETAILS
        // ==========================================

        // GET: /Product/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.Seller)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // ==========================================
        // CREATE PRODUCT - GET
        // ==========================================

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // ==========================================
        // CREATE PRODUCT - POST
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var product = new Product
            {
                Title = model.Title,
                Description = model.Description,
                Price = model.Price,
                Category = model.Category,
                Condition = model.Condition,
                UserId = user.Id,
                CreatedDate = DateTime.Now,
                IsSold = false
            };

            _context.Products.Add(product);

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // ==========================================
        // EDIT PRODUCT - GET
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            // Check if current user owns this product
            if (product.UserId != user.Id)
            {
                return Forbid();
            }

            var model = new ProductViewModel
            {
                Title = product.Title,
                Description = product.Description,
                Price = product.Price,
                Category = product.Category,
                Condition = product.Condition
            };

            return View(model);
        }

        // ==========================================
        // EDIT PRODUCT - POST
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            ProductViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            // Check if current user owns this product
            if (product.UserId != user.Id)
            {
                return Forbid();
            }

            // Update product information
            product.Title = model.Title;
            product.Description = model.Description;
            product.Price = model.Price;
            product.Category = model.Category;
            product.Condition = model.Condition;

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // ==========================================
        // DELETE PRODUCT - GET
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var product = await _context.Products
                .Include(p => p.Seller)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            // Check if current user owns this product
            if (product.UserId != user.Id)
            {
                return Forbid();
            }

            return View(product);
        }

        // ==========================================
        // DELETE PRODUCT - POST
        // ==========================================

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            // Check if current user owns this product
            if (product.UserId != user.Id)
            {
                return Forbid();
            }

            // ==========================================
            // REMOVE RELATED RECORDS FIRST
            // ==========================================

            // Remove wishlist records
            var wishlists = await _context.Wishlists
                .Where(w => w.ProductId == id)
                .ToListAsync();

            _context.Wishlists.RemoveRange(wishlists);

            // Remove report records
            var reports = await _context.Reports
                .Where(r => r.ProductId == id)
                .ToListAsync();

            _context.Reports.RemoveRange(reports);

            // Remove transaction records
            var transactions = await _context.Transactions
                .Where(t => t.ProductId == id)
                .ToListAsync();

            _context.Transactions.RemoveRange(transactions);

            // ==========================================
            // NOW DELETE THE PRODUCT
            // ==========================================

            _context.Products.Remove(product);

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // ==========================================
        // MARK PRODUCT AS SOLD
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsSold(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            // Check if current user owns this product
            if (product.UserId != user.Id)
            {
                return Forbid();
            }

            // Mark product as sold
            product.IsSold = true;

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
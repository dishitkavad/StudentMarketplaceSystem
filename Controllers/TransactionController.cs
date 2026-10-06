using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentMarketplaceSystem.Data;
using StudentMarketplaceSystem.Models;

namespace StudentMarketplaceSystem.Controllers
{
    [Authorize]
    public class TransactionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TransactionController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ==========================================
        // BUY / INTERESTED REQUEST
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Buy(int id)
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

            // A student cannot buy their own product
            if (product.UserId == user.Id)
            {
                TempData["TransactionMessage"] =
                    "You cannot buy your own product.";

                return RedirectToAction(
                    "Details",
                    "Product",
                    new { id = id });
            }

            // Product cannot be purchased if already sold
            if (product.IsSold)
            {
                TempData["TransactionMessage"] =
                    "This product has already been sold.";

                return RedirectToAction(
                    "Details",
                    "Product",
                    new { id = id });
            }

            // Check for existing pending request
            var existingRequest = await _context.Transactions
                .AnyAsync(t =>
                    t.ProductId == id &&
                    t.BuyerId == user.Id &&
                    t.Status == "Pending");

            if (existingRequest)
            {
                TempData["TransactionMessage"] =
                    "You already have a pending request for this product.";

                return RedirectToAction(
                    "Details",
                    "Product",
                    new { id = id });
            }

            var transaction = new Transaction
            {
                ProductId = product.ProductId,

                BuyerId = user.Id,

                // Seller is the owner of the product
                SellerId = product.UserId,

                Status = "Pending",

                CreatedDate = DateTime.Now
            };

            _context.Transactions.Add(transaction);

            await _context.SaveChangesAsync();

            TempData["TransactionMessage"] =
                "Your buy request has been sent to the seller.";

            return RedirectToAction(
                "Details",
                "Product",
                new { id = id });
        }

        // ==========================================
        // SELLER REQUESTS
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Requests()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Find requests using the Product owner's UserId
            var requests = await _context.Transactions
                .Include(t => t.Product)
                .Include(t => t.Buyer)
                .Where(t =>
                    t.Product != null &&
                    t.Product.UserId == user.Id)
                .OrderByDescending(t => t.CreatedDate)
                .ToListAsync();

            return View(requests);
        }

        // ==========================================
        // BUYER ORDERS
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> MyOrders()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var orders = await _context.Transactions
                .Include(t => t.Product)
                .Include(t => t.Seller)
                .Where(t => t.BuyerId == user.Id)
                .OrderByDescending(t => t.CreatedDate)
                .ToListAsync();

            return View(orders);
        }

        // ==========================================
        // ACCEPT BUY REQUEST
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Accept(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Find request through the product owner
            var transaction = await _context.Transactions
                .Include(t => t.Product)
                .FirstOrDefaultAsync(t =>
                    t.TransactionId == id &&
                    t.Product != null &&
                    t.Product.UserId == user.Id);

            if (transaction == null)
            {
                return NotFound();
            }

            if (transaction.Status != "Pending")
            {
                TempData["TransactionMessage"] =
                    "This request has already been processed.";

                return RedirectToAction(nameof(Requests));
            }

            if (transaction.Product == null)
            {
                return NotFound();
            }

            if (transaction.Product.IsSold)
            {
                TempData["TransactionMessage"] =
                    "This product has already been sold.";

                return RedirectToAction(nameof(Requests));
            }

            // Accept request
            transaction.Status = "Accepted";

            // Mark product as sold
            transaction.Product.IsSold = true;

            await _context.SaveChangesAsync();

            TempData["TransactionMessage"] =
                "Buy request accepted successfully.";

            return RedirectToAction(nameof(Requests));
        }

        // ==========================================
        // REJECT BUY REQUEST
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Find request through the product owner
            var transaction = await _context.Transactions
                .Include(t => t.Product)
                .FirstOrDefaultAsync(t =>
                    t.TransactionId == id &&
                    t.Product != null &&
                    t.Product.UserId == user.Id);

            if (transaction == null)
            {
                return NotFound();
            }

            // Only Pending requests can be rejected
            if (transaction.Status != "Pending")
            {
                TempData["TransactionMessage"] =
                    "This request has already been processed.";

                return RedirectToAction(nameof(Requests));
            }

            // Change status to Rejected
            transaction.Status = "Rejected";

            await _context.SaveChangesAsync();

            TempData["TransactionMessage"] =
                "Buy request rejected.";

            return RedirectToAction(nameof(Requests));
        }

        // ==========================================
        // COMPLETE TRANSACTION
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Find the transaction through the product owner
            var transaction = await _context.Transactions
                .Include(t => t.Product)
                .FirstOrDefaultAsync(t =>
                    t.TransactionId == id &&
                    t.Product != null &&
                    t.Product.UserId == user.Id);

            if (transaction == null)
            {
                return NotFound();
            }

            // Only Accepted transactions can be completed
            if (transaction.Status != "Accepted")
            {
                TempData["TransactionMessage"] =
                    "Only accepted transactions can be completed.";

                return RedirectToAction(nameof(Requests));
            }

            // Change status to Completed
            transaction.Status = "Completed";

            await _context.SaveChangesAsync();

            TempData["TransactionMessage"] =
                "Transaction completed successfully.";

            return RedirectToAction(nameof(Requests));
        }

        // ==========================================
        // CANCEL BUY REQUEST
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Find the transaction belonging to the current buyer
            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(t =>
                    t.TransactionId == id &&
                    t.BuyerId == user.Id);

            if (transaction == null)
            {
                return NotFound();
            }

            // Only Pending requests can be cancelled
            if (transaction.Status != "Pending")
            {
                TempData["TransactionMessage"] =
                    "Only pending requests can be cancelled.";

                return RedirectToAction(nameof(MyOrders));
            }

            // Change status to Cancelled
            transaction.Status = "Cancelled";

            await _context.SaveChangesAsync();

            TempData["TransactionMessage"] =
                "Buy request cancelled successfully.";

            return RedirectToAction(nameof(MyOrders));
        }
    }
}
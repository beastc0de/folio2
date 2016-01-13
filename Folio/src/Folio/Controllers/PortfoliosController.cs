using folio.Services;
using Folio.Models;
using Folio.ViewModels;
using Newtonsoft.Json;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Mvc;
using Microsoft.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Folio.Controllers
{
    [Authorize]
    public class PortfoliosController : Controller
    {
        private ApplicationDbContext _context;
        private UserManager<ApplicationUser> _userManager; 

        public PortfoliosController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Portfolios
        public async Task<IActionResult> Index()
        {
            return View(await _context.Portfolio.ToListAsync());
        }

        // GET: Portfolios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            Portfolio portfolio = await _context.Portfolio.SingleAsync(m => m.ID == id);
            if (portfolio == null)
            {
                return HttpNotFound();
            }

            return View(portfolio);
        }

        // GET: Portfolios/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Portfolios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Portfolio portfolio)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = await _userManager.FindByIdAsync(HttpContext.User.GetUserId());
                portfolio.User = user;
                portfolio.DateCreated = DateTime.Now;
                _context.Portfolio.Add(portfolio);
                await _context.SaveChangesAsync();
                return RedirectToAction("AddStock", portfolio);
            }

            return View();
        }

        // GET: Portfolios/AddStock/5
        [HttpGet]
        public async Task<IActionResult> AddStock(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            List<Portfolio> userPortfolios = _context.Portfolio.Where(p => p.User.Id == HttpContext.User.GetUserId()).Include(s => s.PortfolioAssets).ToList();
            Portfolio workingPortfolio = userPortfolios.Find(p => p.ID == id);
            userPortfolios.Remove(userPortfolios.Find(p => p.Name == workingPortfolio.Name));

            if (HttpContext.Session.GetObjectFromJson<List<StockViewModel>>("Stocks") == null)
            {
                // Need Chris' Method to Get StockViewModel objects for all stocks.
                // Set the list of all StockViewModel objects on the session.
            }

            AddStockToPortfolioViewModel model = new AddStockToPortfolioViewModel
            {
                UserPortfolios = userPortfolios,
                WorkingPortfolio = workingPortfolio,
                AvailableAssetTickers = HttpContext.Session.GetObjectFromJson<List<StockViewModel>>("Stocks")
            };

            return View(model);
        }

        // POST: Portfolios/AddStock/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStock(int? id, string ticker, string amount)
        {
            Portfolio portfolio = await _context.Portfolio.SingleAsync(p => p.ID == id);
            portfolio.PortfolioAssets.Add(new PortfolioAsset { AssetSymbol = ticker, NumberOfAssetOwned = Int32.Parse(amount)});
            return View();
        }

        public JsonResult Autocomplete(string term)
        {
            List<string> items = HttpContext.Session.GetObjectFromJson<List<string>>("StockTickers");
            var filteredItems = items.Where(item => item.IndexOf(term, StringComparison.InvariantCultureIgnoreCase) >= 0);
            return Json(filteredItems);
        }

        private IActionResult Json(IEnumerable<string> filteredItems, object allowGet)
        {
            throw new NotImplementedException();
        }

        // GET: Portfolios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            Portfolio portfolio = await _context.Portfolio.SingleAsync(m => m.ID == id);
            if (portfolio == null)
            {
                return HttpNotFound();
            }
            return View(portfolio);
        }

        // POST: Portfolios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Portfolio portfolio)
        {
            if (ModelState.IsValid)
            {
                _context.Update(portfolio);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(portfolio);
        }

        // GET: Portfolios/Delete/5
        [ActionName("Delete")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            Portfolio portfolio = await _context.Portfolio.SingleAsync(m => m.ID == id);
            if (portfolio == null)
            {
                return HttpNotFound();
            }

            return View(portfolio);
        }

        // POST: Portfolios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Portfolio portfolio = await _context.Portfolio.SingleAsync(m => m.ID == id);
            _context.Portfolio.Remove(portfolio);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}

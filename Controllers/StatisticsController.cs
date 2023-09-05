using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using websitebanxemay.Data;
using websitebanxemay.Models;

namespace websitebanxemay.Controllers
{
    [Authorize(Roles = "Admin,NhanVien")]
    public class StatisticsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StatisticsController(ApplicationDbContext context)
        {
            _context = context;
        }      

        // GET: Statistics
        public IActionResult Index()
        {
            //var applicationDbContext = _context.Statistics.Include(s => s.BillDetail);
            //return View(await applicationDbContext.ToListAsync());
            var statisticsList = new List<Statistics>();

            var statistics = new Statistics();
            statistics.TotalOrders = _context.Bill.Count();
            statistics.TotalCustomers = _context.Bill.Select(b => b.CustomerName).Distinct().Count();
            statistics.TotalRevenue = _context.BillDetail.Sum(bd => bd.Amount);
            statistics.BillDetailId = _context.BillDetail.Count();

            statisticsList.Add(statistics);

            return View(statisticsList);
        }

        // GET: Statistics/Details/5
        //public async Task<IActionResult> Details(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var statistics = await _context.Statistics
        //        .Include(s => s.BillDetail)
        //        .FirstOrDefaultAsync(m => m.StatisticsID == id);
        //    if (statistics == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(statistics);
        //}

        //GET: Statistics/Create
        //public IActionResult Create()
        //{
        //    ViewData["BillDetailId"] = new SelectList(_context.BillDetail, "BillDetailId", "BillDetailId");
        //    return View();
        //}

        //// POST: Statistics/Create
        //// To protect from overposting attacks, enable the specific properties you want to bind to, for 
        //// more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("StatisticsID,TotalOrders,TotalCustomers,TotalRevenue,BillDetailId")] Statistics statistics)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(statistics);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    ViewData["BillDetailId"] = new SelectList(_context.BillDetail, "BillDetailId", "BillDetailId", statistics.BillDetailId);
        //    return View(statistics);
        //}

        //// GET: Statistics/Edit/5
        //public async Task<IActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var statistics = await _context.Statistics.FindAsync(id);
        //    if (statistics == null)
        //    {
        //        return NotFound();
        //    }
        //    ViewData["BillDetailId"] = new SelectList(_context.BillDetail, "BillDetailId", "BillDetailId", statistics.BillDetailId);
        //    return View(statistics);
        //}

        //// POST: Statistics/Edit/5
        //// To protect from overposting attacks, enable the specific properties you want to bind to, for 
        //// more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, [Bind("StatisticsID,TotalOrders,TotalCustomers,TotalRevenue,BillDetailId")] Statistics statistics)
        //{
        //    if (id != statistics.StatisticsID)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(statistics);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!StatisticsExists(statistics.StatisticsID))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //    ViewData["BillDetailId"] = new SelectList(_context.BillDetail, "BillDetailId", "BillDetailId", statistics.BillDetailId);
        //    return View(statistics);
        //}

        //// GET: Statistics/Delete/5
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var statistics = await _context.Statistics
        //        .Include(s => s.BillDetail)
        //        .FirstOrDefaultAsync(m => m.StatisticsID == id);
        //    if (statistics == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(statistics);
        //}

        //// POST: Statistics/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    var statistics = await _context.Statistics.FindAsync(id);
        //    _context.Statistics.Remove(statistics);
        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        //private bool StatisticsExists(int id)
        //{
        //    return _context.Statistics.Any(e => e.StatisticsID == id);
        //}
    }
}

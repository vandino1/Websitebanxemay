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
    public class ReliesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReliesController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpPost]
        public IActionResult DeleteMultiple(int[] selectedIds)
        {
            // Kiểm tra xem có bản ghi được chọn hay không
            if (selectedIds == null || selectedIds.Length == 0)
            {
                TempData["Message_error"] = "Vui lòng chọn ít nhất một bản ghi để xóa";
                return RedirectToAction("Index");
            }

            // Thực hiện xóa các bản ghi đã chọn
            var recordsToDelete = _context.Rely.Where(x => selectedIds.Contains(x.replyId)).ToList();
            _context.Rely.RemoveRange(recordsToDelete);
            _context.SaveChanges();
            TempData["Message_delete"] = "Đã xóa thành công " + recordsToDelete.Count() + " bản ghi";
            return RedirectToAction("Index");
        }

        // GET: Relies
        public async Task<IActionResult> Index(string searchString, string currentFilter, int p = 1)
        {
            int pageSize = 10;
            var relies = _context.Rely.Include(r => r.Comment)
                                      .Skip((p - 1) * pageSize)
                                      .Take(pageSize);
            ViewBag.PageNumber = p;
            ViewBag.PageRange = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((decimal)_context.Comment.Count() / pageSize);
            ViewData["CurrentFilter"] = searchString;
            if (!String.IsNullOrEmpty(searchString))
            {
                //searchString == null co the them vao
                relies = relies.Where(s => s.email.Contains(searchString) || s.mainContent.Contains(searchString));
            }
            return View(await relies.ToListAsync());
        }
        // GET: Relies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rely = await _context.Rely
                .Include(r => r.Comment)
                .FirstOrDefaultAsync(m => m.replyId == id);
            if (rely == null)
            {
                return NotFound();
            }

            return View(rely);
        }

        // GET: Relies/Create
        public IActionResult Create()
        {
            ViewData["commentId"] = new SelectList(_context.Comment, "commentId", "email");
            return View();
        }

        // POST: Relies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("replyId,email,createDate,mainContent,commentId")] Rely rely)
        {
            if (ModelState.IsValid)
            {
                _context.Add(rely);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["commentId"] = new SelectList(_context.Comment, "commentId", "email", rely.commentId);
            return View(rely);
        }

        // GET: Relies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rely = await _context.Rely.FindAsync(id);
            if (rely == null)
            {
                return NotFound();
            }
            ViewData["commentId"] = new SelectList(_context.Comment, "commentId", "email", rely.commentId);
            return View(rely);
        }

        // POST: Relies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("replyId,email,createDate,mainContent,commentId")] Rely rely)
        {
            if (id != rely.replyId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(rely);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RelyExists(rely.replyId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["commentId"] = new SelectList(_context.Comment, "commentId", "email", rely.commentId);
            return View(rely);
        }

        // GET: Relies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rely = await _context.Rely
                .Include(r => r.Comment)
                .FirstOrDefaultAsync(m => m.replyId == id);
            if (rely == null)
            {
                return NotFound();
            }

            return View(rely);
        }

        // POST: Relies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rely = await _context.Rely.FindAsync(id);
            _context.Rely.Remove(rely);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RelyExists(int id)
        {
            return _context.Rely.Any(e => e.replyId == id);
        }
    }
}

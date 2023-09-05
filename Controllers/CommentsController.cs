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
    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CommentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Comments
        public async Task<IActionResult> Index(string searchString, string currentFilter, string sortOrder, int p = 1)
        {
            int pageSize = 10;
            var comments = _context.Comment.OrderByDescending(x => x.commentId)
                                                       .Include(c => c.Product)
                                                      .Skip((p - 1) * pageSize)
                                                      .Take(pageSize);
            ViewBag.PageNumber = p;
            ViewBag.PageRange = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((decimal)_context.Comment.Count() / pageSize);
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentSort"] = sortOrder;
            ViewData["emailSortParm"] = String.IsNullOrEmpty(sortOrder) ? "email_desc" : "";
            ViewData["createDateSortParm"] = sortOrder == "createDate_asc" ? "createDate_desc" : "createDate_asc";
            ViewData["mainContentSortParm"] = sortOrder == "mainContent_asc" ? "mainContent_desc" : "mainContent_asc";

            if (!String.IsNullOrEmpty(searchString))
            {
                //searchString == null co the them vao
                comments = comments.Where(s => s.mainContent.Contains(searchString) || s.Product.Name.Contains(searchString));
            }
            switch (sortOrder)
            {
                case "email_desc":
                    comments = comments.OrderByDescending(s => s.email);
                    break;
                case "createDate_desc":
                    comments = comments.OrderByDescending(s => s.createDate);
                    break;
                case "createDate_asc":
                    comments = comments.OrderBy(s => s.createDate);
                    break;
                case "mainContent_desc":
                    comments = comments.OrderByDescending(s => s.mainContent);
                    break;
                case "mainContent_asc":
                    comments = comments.OrderBy(s => s.mainContent);
                    break;
                default:
                    comments = comments.OrderBy(s => s.email);
                    break;
            }
            return View(await comments.ToListAsync());
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
            var recordsToDelete = _context.Comment.Where(x => selectedIds.Contains(x.commentId)).ToList();
            _context.Comment.RemoveRange(recordsToDelete);
            _context.SaveChanges();
            TempData["Message_delete"] = "Đã xóa thành công " + recordsToDelete.Count() + " bản ghi";
            return RedirectToAction("Index");
        }

        // GET: Comments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comment
                .Include(c => c.Product)
                .FirstOrDefaultAsync(m => m.commentId == id);
            if (comment == null)
            {
                return NotFound();
            }

            return View(comment);
        }

        // GET: Comments/Create
        public IActionResult Create()
        {
            ViewData["ProductId"] = new SelectList(_context.Product, "ProductId", "Name");
            return View();
        }

        // POST: Comments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("commentId,email,createDate,mainContent,starRating,ProductId")] Comment comment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(comment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProductId"] = new SelectList(_context.Product, "ProductId", "Name", comment.ProductId);
            return View(comment);
        }

        // GET: Comments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comment.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }
            ViewData["ProductId"] = new SelectList(_context.Product, "ProductId", "Name", comment.ProductId);
            return View(comment);
        }

        // POST: Comments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("commentId,email,createDate,mainContent,starRating,ProductId")] Comment comment)
        {
            if (id != comment.commentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(comment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CommentExists(comment.commentId))
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
            ViewData["ProductId"] = new SelectList(_context.Product, "ProductId", "Name", comment.ProductId);
            return View(comment);
        }

        // GET: Comments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comment
                .Include(c => c.Product)
                .FirstOrDefaultAsync(m => m.commentId == id);
            if (comment == null)
            {
                return NotFound();
            }

            return View(comment);
        }

        // POST: Comments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var comment = await _context.Comment.FindAsync(id);
            _context.Comment.Remove(comment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CommentExists(int id)
        {
            return _context.Comment.Any(e => e.commentId == id);
        }
    }
}

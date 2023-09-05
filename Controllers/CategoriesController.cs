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
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Categories
        public async Task<IActionResult> Index(string searchString, string currentFilter, string sortOrder, int p = 1)
        {
            int pageSize = 10;
            var categories = _context.Category.OrderByDescending(x => x.categoryId)
                                                      .Skip((p - 1) * pageSize)
                                                      .Take(pageSize);
            ViewBag.PageNumber = p;
            ViewBag.PageRange = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((decimal)_context.Category.Count() / pageSize);
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentSort"] = sortOrder;
            ViewData["nameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";

            if (!String.IsNullOrEmpty(searchString))
            {
                //searchString == null co the them vao
                categories = categories.Where(s => s.name.Contains(searchString));
            }
            switch (sortOrder)
            {
                case "name_desc":
                    categories = categories.OrderByDescending(s => s.name);
                    break;
                default:
                    categories = categories.OrderBy(s => s.name);
                    break;
            }
            return View(await categories.ToListAsync());
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
            var recordsToDelete = _context.Category.Where(x => selectedIds.Contains(x.categoryId)).ToList();
            _context.Category.RemoveRange(recordsToDelete);
            _context.SaveChanges();
            TempData["Message_delete"] = "Đã xóa thành công " + recordsToDelete.Count() + " bản ghi";
            return RedirectToAction("Index");
        }

        // GET: Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Category
                .FirstOrDefaultAsync(m => m.categoryId == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: Categories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("categoryId,name,slug")] Category category)
        {
            if (ModelState.IsValid)
            {
                category.slug = category.name.ToLower().Replace(" ", "-");


                var slug = await _context.Category.FirstOrDefaultAsync(x => x.slug == category.slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "Tên danh mục này đã tồn tại !!!.");
                    return View(category);
                }
                // Lưu thông tin vào bảng lịch sử hoạt động
                var activityLog = new ActivityLog
                {
                    UserId = User.Identity.Name,
                    Action = "Tạo danh mục quảng cáo",
                    DateTime = DateTime.Now,
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString(),
                    Url = Request.HttpContext.Request.Path.ToString(),
                    Data = null
                };
                _context.ActivityLog.Add(activityLog);
                _context.Add(category);
                _context.SaveChanges();
                await _context.SaveChangesAsync();
                TempData["Message_create"] = "Thêm mới danh mục sản phẩm thành công.";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Category.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("categoryId,name,slug")] Category category)
        {
            if (id != category.categoryId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                category.slug = category.name.ToLower().Replace(" ", "-");

                var slug = await _context.Category.Where(x => x.categoryId != id).FirstOrDefaultAsync(x => x.slug == category.slug);

                try
                {
                    if (slug != null)
                    {
                        ModelState.AddModelError("", "Tên danh mục này đã tồn tại.");
                        return View(category);
                    }
                    var activityLog = new ActivityLog
                    {
                        UserId = User.Identity.Name,
                        Action = "Cập nhật danh mục quảng cáo",
                        DateTime = DateTime.Now,
                        IpAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString(),
                        Url = Request.HttpContext.Request.Path.ToString(),
                        Data = null
                    };
                    _context.ActivityLog.Update(activityLog);
                    _context.Update(category);
                    _context.SaveChanges();
                    await _context.SaveChangesAsync();
                    TempData["Message_edit"] = "Đã cập nhật danh mục sản phẩm .";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.categoryId))
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
            return View(category);
        }

        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Category
                .FirstOrDefaultAsync(m => m.categoryId == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Category.FindAsync(id);
            var activityLog = new ActivityLog
            {
                UserId = User.Identity.Name,
                Action = "Xóa danh mục quảng cáo",
                DateTime = DateTime.Now,
                IpAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString(),
                Url = Request.HttpContext.Request.Path.ToString(),
                Data = null
            };
            _context.ActivityLog.Add(activityLog);
            _context.Category.Remove(category);
            _context.SaveChanges();
            await _context.SaveChangesAsync();
            TempData["Message_delete"] = "Đã xóa danh mục sản phẩm.";
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Category.Any(e => e.categoryId == id);
        }
    }
}

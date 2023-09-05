using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using websitebanxemay.Data;
using websitebanxemay.Models;

namespace websitebanxemay.Controllers
{
    [Authorize(Roles = "Admin,NhanVien")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult ExportToExcel()
        {
            var ads = _context.Product.ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("ads");

                // Tạo tiêu đề động
                worksheet.Cell("A1").Value = "Danh sách sản phẩm - " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                worksheet.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Range(worksheet.Cell("A1"), worksheet.Cell("H1")).Merge().Style.Font.Bold = true;

                // Tạo tiêu đề cột
                worksheet.Cell("A2").Value = "Id";
                worksheet.Cell("B2").Value = "Tên sản phẩm";
                worksheet.Cell("C2").Value = "Thương hiệu";
                worksheet.Cell("D2").Value = "Giá";
                worksheet.Cell("E2").Value = "Số lượng";
                worksheet.Cell("F2").Value = "Mô tả";
                worksheet.Cell("G2").Value = "Màu";
                worksheet.Cell("H2").Value = "Lượt xem";
                worksheet.Cell("I2").Value = "Dung tích bình";
                worksheet.Cell("J2").Value = "Công suất cực đại";
                worksheet.Cell("K2").Value = "Moment cực đại";
                worksheet.Cell("L2").Value = "Bảo hành";
                worksheet.Cell("M2").Value = "Kiểu xe";
                worksheet.Cell("N2").Value = "Số sao";

                // Định dạng tiêu đề cột
                var titleRow = worksheet.Row(2);
                titleRow.Style.Font.Bold = true;
                titleRow.Height = 20;
                titleRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                titleRow.Style.Fill.BackgroundColor = XLColor.Gray;

                // Điều chỉnh chiều rộng cột
                worksheet.Column("A").Width = 5;
                worksheet.Column("B").Width = 20;
                worksheet.Column("C").Width = 10;
                worksheet.Column("D").Width = 10;
                worksheet.Column("E").Width = 10;
                worksheet.Column("F").Width = 40;
                worksheet.Column("G").Width = 10;
                worksheet.Column("H").Width = 10;
                worksheet.Column("I").Width = 20;
                worksheet.Column("J").Width = 20;
                worksheet.Column("K").Width = 20;
                worksheet.Column("L").Width = 10;
                worksheet.Column("M").Width = 10;
                worksheet.Column("N").Width = 10;

                // Điền dữ liệu danh sách
                var row = 3;
                foreach (var ad in ads)
                {
                    worksheet.Cell($"A{row}").Value = ad.ProductId;
                    worksheet.Cell($"B{row}").Value = ad.Name;
                    worksheet.Cell($"C{row}").Value = ad.Brand;
                    worksheet.Cell($"D{row}").Value = ad.Price;
                    worksheet.Cell($"E{row}").Value = ad.Quantity;
                    worksheet.Cell($"F{row}").Value = ad.Desciption;
                    worksheet.Cell($"G{row}").Value = ad.Color;
                    worksheet.Cell($"H{row}").Value = ad.viewCount;
                    worksheet.Cell($"I{row}").Value = ad.Capacity;
                    worksheet.Cell($"J{row}").Value = ad.MaxWattage;
                    worksheet.Cell($"K{row}").Value = ad.MaxMoment;
                    worksheet.Cell($"L{row}").Value = ad.Warranty;
                    worksheet.Cell($"M{row}").Value = ad.Type;
                    worksheet.Cell($"N{row}").Value = ad.adverageRating;
                    row++;
                }
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DS SanPham.xlsx");
                }
            }
        }
        public IActionResult Filter(decimal? minPrice, decimal? maxPrice)
        {
            var items = _context.Product.Include(b => b.Category).AsQueryable();
            if (minPrice != null)
            {
                items = items.Where(i => i.Price >= minPrice);
            }
            if (maxPrice != null)
            {
                items = items.Where(i => i.Price <= maxPrice);
            }
            return View(items.ToList());
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
            var recordsToDelete = _context.Product.Where(x => selectedIds.Contains(x.ProductId)).ToList();
            _context.Product.RemoveRange(recordsToDelete);
            _context.SaveChanges();
            TempData["Message_delete"] = "Đã xóa thành công " + recordsToDelete.Count() + " bản ghi";
            return RedirectToAction("Index");
        }
        // GET: Products
        public async Task<IActionResult> Index(string sortOrder, int p = 1)
        {
            //var applicationDbContext = _context.BanTin.Include(b => b.chuDe);
            int pageSize = 10;
            var products = from m in _context.Product.OrderByDescending(x => x.ProductId)
                                                      .Include(b => b.Category)                                                                                                          
                                                      .Skip((p - 1) * pageSize)
                                                      .Take(pageSize)
                                 select m;
            ViewBag.PageNumber = p;
            ViewBag.PageRange = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((decimal)_context.Product.Count() / pageSize);           
            ViewData["CurrentSort"] = sortOrder;
            ViewData["nameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["viewCountSortParm"] = sortOrder == "viewCount_asc" ? "viewCount_desc" : "viewCount_asc";
            ViewData["SortbyPrice"] = sortOrder == "price_asc" ? "price_desc" : "price_asc";           
            ViewData["Sortbybrand"] = sortOrder == "brand_asc" ? "brand_desc" : "brand_asc";
            ViewData["quantitySortParm"] = sortOrder == "quantity_asc" ? "quantity_desc" : "quantity_asc";
            ViewData["ratingSortParm"] = sortOrder == "rating_asc" ? "rating_desc" : "rating_asc";
          
            switch (sortOrder)
            {
                case "name_desc":
                    products = products.OrderByDescending(s => s.Name);
                    break;
                case "viewCount_desc":
                    products = products.OrderByDescending(s => s.viewCount);
                    break;
                case "viewCount_asc":
                    products = products.OrderBy(s => s.viewCount);
                    break;
                case "price_desc":
                    products = products.OrderByDescending(s => s.Price);
                    break;
                case "price_asc":
                    products = products.OrderBy(s => s.Price);
                    break;              
                case "brand_desc":
                    products = products.OrderByDescending(s => s.Brand);
                    break;
                case "brand_asc":
                    products = products.OrderBy(s => s.Brand);
                    break;
                case "quantity_desc":
                    products = products.OrderByDescending(s => s.Quantity);
                    break;
                case "quantity_asc":
                    products = products.OrderBy(s => s.Quantity);
                    break;
                case "rating_desc":
                    products = products.OrderByDescending(s => s.adverageRating);
                    break;
                case "rating_asc":
                    products = products.OrderBy(s => s.adverageRating);
                    break;
                default:
                    products = products.OrderBy(s => s.Name);
                    break;
            }
            return View(await products.AsNoTracking().ToListAsync());
        }
        [HttpGet]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> Index_Search(string searchString, string currentFilter)
        {
            var advertisements = from m in _context.Product.OrderByDescending(x => x.ProductId)
                                                      .Include(b => b.Category)                                                                                                      

                                 select m;
            ViewData["CurrentFilter"] = searchString;

            if (!String.IsNullOrEmpty(searchString))
            {
                //Điều kiện kiểm tra xem `searchString` có nằm trong thuộc tính `brand` của các quảng cáo hay không
                advertisements = advertisements.Where(s => s.Brand.Contains(searchString) || s.Name.Contains(searchString) || s.Category.name.Contains(searchString));
            }
            return View(await advertisements.AsNoTracking().ToListAsync());
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            ViewData["categoryId"] = new SelectList(_context.Category, "categoryId", "name");
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormFile file,[Bind("ProductId,Name,slug,Brand,Price,Quantity,Desciption,status,viewCount,Color,Capacity,MaxWattage,MaxMoment,Warranty,Type,categoryId,Image,adverageRating")] Product product)
        {
            if (ModelState.IsValid)
            {
                product.slug = product.Name.ToLower().Replace(" ", "-");
                var slug = await _context.Product.FirstOrDefaultAsync(x => x.slug == product.slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "Tên sản phẩm này đã tồn tại !!!.");
                    return View(product);
                }
                if (product.Price < 0)
                {
                    ModelState.AddModelError("", "Giá phải lớn hơn 0 !!!");
                    return View(product);
                }
                if (product.Quantity < 0)
                {
                    ModelState.AddModelError("", "Số lượng phải lớn hơn 0 !!!");
                    return View(product);
                }
                // Lưu thông tin vào bảng lịch sử hoạt động
                var activityLog = new ActivityLog
                {
                    UserId = User.Identity.Name,
                    Action = "Tạo quảng cáo mới",
                    DateTime = DateTime.Now,
                    IpAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString(),
                    Url = Request.HttpContext.Request.Path.ToString(),
                    Data = null
                };
                _context.ActivityLog.Add(activityLog);
                product.Image = Upload(file);
                _context.Add(product);
                _context.SaveChanges();
                await _context.SaveChangesAsync();
                TempData["Message_create"] = "Thêm mới sản phẩm thành công.";

                return RedirectToAction(nameof(Index));
            }
            else
            {

                ViewData["categoryId"] = new SelectList(_context.Category, "categoryId", "name", product.categoryId);              
                return View(product);
            }
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["categoryId"] = new SelectList(_context.Category, "categoryId", "name", product.categoryId);
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(IFormFile file, int id, [Bind("ProductId,Name,slug,Brand,Price,Quantity,Desciption,status,viewCount,Color,Capacity,MaxWattage,MaxMoment,Warranty,Type,categoryId,Image,adverageRating")] Product product)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                product.slug = product.Name.ToLower().Replace(" ", "-");


                var slug = await _context.Category.FirstOrDefaultAsync(x => x.slug == product.slug);
                try
                {
                    //Tự save ảnh
                    if (file != null)
                    {
                        product.Image = Upload(file);
                    }
                    if (slug != null)
                    {
                        ModelState.AddModelError("", "Tên sản phẩm này đã tồn tại !!!.");
                        return View(product);
                    }
                    var activityLog = new ActivityLog
                    {
                        UserId = User.Identity.Name,
                        Action = "Cập nhật tin quảng cáo",
                        DateTime = DateTime.Now,
                        IpAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString(),
                        Url = Request.HttpContext.Request.Path.ToString(),
                        Data = null
                    };
                    _context.ActivityLog.Update(activityLog);
                    _context.Update(product);
                    _context.SaveChanges();
                    await _context.SaveChangesAsync();
                    TempData["Message_edit"] = "Đã cập nhật sản phẩm.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductId))
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
            ViewData["categoryId"] = new SelectList(_context.Category, "categoryId", "name", product.categoryId);         
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Product.FindAsync(id);
            _context.Product.Remove(product);
            await _context.SaveChangesAsync();
            TempData["Message_delete"] = "Đã xóa sản phẩm.";
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Product.Any(e => e.ProductId == id);
        }
        public string Upload(IFormFile file)
        {
            string fn = null;

            if (file != null)
            {
                //Phát sinh tên
                fn = Guid.NewGuid().ToString() + "_" + file.FileName;
                //Chep file về đúng thư mục
                var path = $"wwwroot\\images\\{fn}";
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
            }
            return fn;
        }
    }
}

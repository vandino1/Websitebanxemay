using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using websitebanxemay.Data;
using websitebanxemay.Models;
using Microsoft.AspNetCore.Authorization;
using ClosedXML.Excel;
using System.IO;

namespace websitebanxemay.Controllers
{
     [Authorize(Roles = "Admin,NhanVien")]
    public class BillDetailsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BillDetailsController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult ExportToExcel()
        {
            var ads = _context.BillDetail.Include(s=>s.Bill).Include(s=>s.Product).ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("BillDetails");

                // Tạo tiêu đề động
                worksheet.Cell("A1").Value = "Danh sách chi tiết đơn hàng - " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                worksheet.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Range(worksheet.Cell("A1"), worksheet.Cell("H1")).Merge().Style.Font.Bold = true;

                // Tạo tiêu đề cột
                worksheet.Cell("A2").Value = "Id";
                worksheet.Cell("B2").Value = "Địa chỉ khách hàng";
                worksheet.Cell("C2").Value = "Tên sản phẩm";
                worksheet.Cell("D2").Value = "Giá";
                worksheet.Cell("E2").Value = "Số lượng";
                worksheet.Cell("F2").Value = "Tổng tiền";               

                // Định dạng tiêu đề cột
                var titleRow = worksheet.Row(2);
                titleRow.Style.Font.Bold = true;
                titleRow.Height = 20;
                titleRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                titleRow.Style.Fill.BackgroundColor = XLColor.Gray;

                // Điều chỉnh chiều rộng cột
                worksheet.Column("A").Width = 5;
                worksheet.Column("B").Width = 20;
                worksheet.Column("C").Width = 30;
                worksheet.Column("D").Width = 10;
                worksheet.Column("E").Width = 10;
                worksheet.Column("F").Width = 10;            

                // Điền dữ liệu danh sách
                var row = 3;
                foreach (var ad in ads)
                {
                    worksheet.Cell($"A{row}").Value = ad.BillDetailId;
                    worksheet.Cell($"B{row}").Value = ad.Bill.CustomerAddress;
                    worksheet.Cell($"C{row}").Value = ad.Product.Name;
                    worksheet.Cell($"D{row}").Value = ad.Price;
                    worksheet.Cell($"E{row}").Value = ad.Quantity;
                    worksheet.Cell($"F{row}").Value = ad.Amount;                 
                    row++;
                }

                // Lưu và trả về file Excel
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DS ChiTietDon.xlsx");
                }
            }
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
            var recordsToDelete = _context.BillDetail.Where(x => selectedIds.Contains(x.BillDetailId)).ToList();
            _context.BillDetail.RemoveRange(recordsToDelete);
            _context.SaveChanges();
            TempData["Message_delete"] = "Đã xóa thành công " + recordsToDelete.Count() + " bản ghi";
            return RedirectToAction("Index");
        }
        // GET: BillDetails        
        public async Task<IActionResult> Index(string searchString, string currentFilter, int p = 1)
        {
            int pageSize = 10;
            var billDetails = _context.BillDetail.Include(b => b.Bill).Include(b => b.Product)
                                                      .Skip((p - 1) * pageSize)
                                                      .Take(pageSize);
            ViewBag.PageNumber = p;
            ViewBag.PageRange = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((decimal)_context.BillDetail.Count() / pageSize);
            ViewData["CurrentFilter"] = searchString;

            if (!String.IsNullOrEmpty(searchString))
            {
                //searchString == null co the them vao
                billDetails = billDetails.Where(s => s.Product.Name.Contains(searchString) || s.Bill.CustomerName.Contains(searchString));
            }
            return View(await billDetails.ToListAsync());
        }

        // GET: BillDetails/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var billDetail = await _context.BillDetail
                .Include(b => b.Bill)
                .Include(b => b.Product)              
                .FirstOrDefaultAsync(m => m.BillDetailId == id);
            if (billDetail == null)
            {
                return NotFound();
            }

            return View(billDetail);
        }

        // GET: BillDetails/Create
        public IActionResult Create()
        {
            ViewData["BillId"] = new SelectList(_context.Bill, "BillId", "CustomerAddress");
            ViewData["ProductId"] = new SelectList(_context.Product, "Id", "Name");
            return View();
        }

        // POST: BillDetails/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BillDetailId,BillId,ProductId,Price,Quantity,Amount")] BillDetail billDetail)
        {
            if (ModelState.IsValid)
            {
                _context.Add(billDetail);
                await _context.SaveChangesAsync();
                TempData["AlertMessage"] = "Vừa thêm chi tiết đơn hàng thành công.";
                return RedirectToAction(nameof(Index));
            }
            ViewData["BillId"] = new SelectList(_context.Bill, "BillId", "CustomerAddress", billDetail.BillId);
            ViewData["ProductId"] = new SelectList(_context.Product, "Id", "Name", billDetail.ProductId);
            return View(billDetail);
        }

        // GET: BillDetails/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var billDetail = await _context.BillDetail.FindAsync(id);
            if (billDetail == null)
            {
                return NotFound();
            }
            ViewData["BillId"] = new SelectList(_context.Bill, "BillId", "CustomerAddress", billDetail.BillId);
            ViewData["ProductId"] = new SelectList(_context.Product, "ProductId", "Name", billDetail.ProductId);
            return View(billDetail);
        }

        // POST: BillDetails/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BillDetailId,BillId,ProductId,Price,Quantity,Amount")] BillDetail billDetail)
        {
            if (id != billDetail.BillDetailId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(billDetail);
                    await _context.SaveChangesAsync();
                    TempData["AlertMessage"] = "Vừa cập nhật chi tiết đơn hàng thành công.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BillDetailExists(billDetail.BillDetailId))
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
            ViewData["BillId"] = new SelectList(_context.Bill, "BillId", "CustomerAddress", billDetail.BillId);
            ViewData["ProductId"] = new SelectList(_context.Product, "ProductId", "Name", billDetail.ProductId);
            return View(billDetail);
        }
        // GET: BillDetails/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var billDetail = await _context.BillDetail
                .Include(b => b.Bill)
                .Include(b => b.Product)
                .FirstOrDefaultAsync(m => m.BillDetailId == id);
            if (billDetail == null)
            {
                return NotFound();
            }

            return View(billDetail);
        }

        // POST: BillDetails/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var billDetail = await _context.BillDetail.FindAsync(id);
            _context.BillDetail.Remove(billDetail);
            await _context.SaveChangesAsync();
            TempData["AlertMessage"] = "Đã xóa chi tiết đơn hàng.";
            return RedirectToAction(nameof(Index));
        }
        private bool BillDetailExists(int id)
        {
            return _context.BillDetail.Any(e => e.BillDetailId == id);
        }
    }
}

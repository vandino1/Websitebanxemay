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
using Microsoft.AspNetCore.Identity;

namespace websitebanxemay.Controllers
{   
    public class BillsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public BillsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            this._userManager = userManager;
        }
        //Xem và hủy đơn hàng
        public async Task<IActionResult> OrderList()
        {
            var userId = User.Identity.Name;
            var bills = _context.Bill.Include(b => b.BillDetails)
                                                       .Where(si => si.CustomerName == userId)
                                                       .OrderByDescending(x => x.BillId);
            return View(await bills.ToListAsync());
        }
        [HttpPost]
        public IActionResult DeleteMultiple_OrderList(int[] selectedIds)
        {
            if (selectedIds == null || selectedIds.Length == 0)
            {
                TempData["Message_error"] = "Vui lòng chọn ít nhất một đơn hàng để hủy";
                return RedirectToAction("OrderList");
            }

            var recordsToDelete = _context.Bill.Where(x => selectedIds.Contains(x.BillId) 
                                                && x.OrderStatus != "Đã giao" && x.OrderStatus != "Đang giao")
                                                .ToList();                 

            if (recordsToDelete.Count() == 0)
            {
                TempData["Message_error"] = "Đơn hàng đã vận chuyển không thể hủy";
                return RedirectToAction("OrderList");
            }
           
            foreach (var bill in recordsToDelete)
            {
                bill.OrderStatus = "Đã hủy"; // Đánh dấu trạng thái đơn hàng là "Đã hủy" thay vì xóa
            }
           
            _context.SaveChanges();
            TempData["Message_delete"] = "Đã hủy thành công " + recordsToDelete.Count() + " đơn hàng";
            return RedirectToAction("OrderList");

        }
        public IActionResult ExportToExcel()
        {
            var ads = _context.Bill.ToList();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Bills");

                // Tạo tiêu đề động
                worksheet.Cell("A1").Value = "Danh sách đơn hàng - " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                worksheet.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Range(worksheet.Cell("A1"), worksheet.Cell("H1")).Merge().Style.Font.Bold = true;

                // Tạo tiêu đề cột
                worksheet.Cell("A2").Value = "Id";
                worksheet.Cell("B2").Value = "Ngày tạo đơn";
                worksheet.Cell("C2").Value = "Tên khách hàng";
                worksheet.Cell("D2").Value = "Số điện thoại";
                worksheet.Cell("E2").Value = "Địa chỉ khách hàng";
                worksheet.Cell("F2").Value = "Tổng trị giá";
                worksheet.Cell("G2").Value = "Trạng thái";
                worksheet.Cell("H2").Value = "Trạng thái đơn";

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
                worksheet.Column("D").Width = 20;
                worksheet.Column("E").Width = 30;
                worksheet.Column("F").Width = 20;
                worksheet.Column("G").Width = 20;
                worksheet.Column("H").Width = 20;

                // Điền dữ liệu danh sách
                var row = 3;
                foreach (var ad in ads)
                {
                    worksheet.Cell($"A{row}").Value = ad.BillId;
                    worksheet.Cell($"B{row}").Value = ad.Date;
                    worksheet.Cell($"C{row}").Value = ad.CustomerName;
                    worksheet.Cell($"D{row}").Value = ad.CustomerPhone;
                    worksheet.Cell($"E{row}").Value = ad.CustomerAddress;
                    worksheet.Cell($"F{row}").Value = ad.BillTotal;
                    worksheet.Cell($"G{row}").Value = ad.BillStatus;
                    worksheet.Cell($"H{row}").Value = ad.OrderStatus;
                    row++;
                }

                // Lưu và trả về file Excel
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DS DonHang.xlsx");
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
            var recordsToDelete = _context.Bill.Where(x => selectedIds.Contains(x.BillId)).ToList();
            _context.Bill.RemoveRange(recordsToDelete);
            _context.SaveChanges();
            TempData["Message_delete"] = "Đã xóa thành công " + recordsToDelete.Count() + " bản ghi";
            return RedirectToAction("Index");
        }

        // GET: Bills
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> Index(string searchString, string currentFilter, int p = 1)
        {
            int pageSize = 10;
            var bills = _context.Bill.OrderByDescending(x => x.BillId)
                                                       .Include(c => c.BillDetails)
                                                      .Skip((p - 1) * pageSize)
                                                      .Take(pageSize);
            ViewBag.PageNumber = p;
            ViewBag.PageRange = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((decimal)_context.Bill.Count() / pageSize);
            ViewData["CurrentFilter"] = searchString;          

            if (!String.IsNullOrEmpty(searchString))
            {
                //searchString == null co the them vao
                bills = bills.Where(s => s.CustomerName.Contains(searchString) || s.CustomerAddress.Contains(searchString));
            }           
            return View(await bills.ToListAsync());
        }

        // GET: Bills/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bill = await _context.Bill
                .Include(b=>b.BillDetails).ThenInclude(b=>b.Product)
                .FirstOrDefaultAsync(m => m.BillId == id);
            if (bill == null)
            {
                return NotFound();
            }

            return View(bill);
        }
        public async Task<IActionResult> Detail_OrderList(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bill = await _context.Bill
                .Include(b => b.BillDetails).ThenInclude(b => b.Product)
                .FirstOrDefaultAsync(m => m.BillId == id);
            if (bill == null)
            {
                return NotFound();
            }

            return View(bill);
        }

        // GET: Bills/Create
        [Authorize(Roles = "Admin,NhanVien")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Bills/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]       
        public async Task<IActionResult> Create([Bind("BillId,Date,CustomerName,CustomerPhone,CustomerAddress,BillTotal,BillStatus,OrderStatus")] Bill bill)
        {
            if (ModelState.IsValid)
            {
                _context.Add(bill);
                await _context.SaveChangesAsync();
                TempData["AlertMessage"] = "Vừa thêm đơn hàng thành công.";
                return RedirectToAction(nameof(Index));
            }
            return View(bill);
        }

        // GET: Bills/Edit/5
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bill = await _context.Bill.FindAsync(id);
            if (bill == null)
            {
                return NotFound();
            }
            return View(bill);
        }

        // POST: Bills/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BillId,Date,CustomerName,CustomerPhone,CustomerAddress,BillTotal,BillStatus,OrderStatus")] Bill bill)
        {
            if (id != bill.BillId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bill);
                    await _context.SaveChangesAsync();
                    TempData["Message_edit"] = "Vừa cập nhật đơn hàng thành công.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BillExists(bill.BillId))
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
            return View(bill);
        }

        // GET: Bills/Delete/5
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bill = await _context.Bill
                .FirstOrDefaultAsync(m => m.BillId == id);
            if (bill == null)
            {
                return NotFound();
            }

            return View(bill);
        }

        // POST: Bills/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bill = await _context.Bill.FindAsync(id);
            _context.Bill.Remove(bill);
            await _context.SaveChangesAsync();
            TempData["Message_delete"] = "Đã xóa đơn hàng.";
            return RedirectToAction(nameof(Index));
        }
        private bool BillExists(int id)
        {
            return _context.Bill.Any(e => e.BillId == id);
        }
    }
}

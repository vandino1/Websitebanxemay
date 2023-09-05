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
    //nếu để admin không thì nó chặn xem lun
    [Authorize(Roles = "Admin")]
    public class ActivityLogsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ActivityLogsController(ApplicationDbContext context)
        {
            _context = context;
        }
        //public IActionResult ExportToExcel()
        //{
        //    var ads = _context.ActivityLog.ToList();

        //    using (var workbook = new XLWorkbook())
        //    {
        //        var worksheet = workbook.Worksheets.Add("Ads");
        //        worksheet.Cell("A1").Value = "Id";
        //        worksheet.Cell("B1").Value = "User name";
        //        worksheet.Cell("C1").Value = "Hành động";
        //        worksheet.Cell("D1").Value = "Ngày giờ hoạt động";
        //        worksheet.Cell("E1").Value = "Địa chỉ ID";
        //        worksheet.Cell("F1").Value = "Đường dẫn liên kết";
        //        worksheet.Cell("G1").Value = "Dữ liệu";
        //        worksheet.Column("B").Width = 15;
        //        worksheet.Column("C").Width = 50;
        //        worksheet.Column("D").Width = 25;
        //        worksheet.Column("E").Width = 35;
        //        worksheet.Column("F").Width = 25;
        //        worksheet.Column("G").Width = 35;

        //        var row = 2;
        //        foreach (var ad in ads)
        //        {
        //            worksheet.Cell($"A{row}").Value = ad.Id;
        //            worksheet.Cell($"B{row}").Value = ad.UserId;
        //            worksheet.Cell($"C{row}").Value = ad.Action;
        //            worksheet.Cell($"D{row}").Value = ad.DateTime;
        //            worksheet.Cell($"E{row}").Value = ad.IpAddress;
        //            worksheet.Cell($"F{row}").Value = ad.Url;
        //            worksheet.Cell($"G{row}").Value = ad.Data;                    
        //            row++;
        //        }
        //        using (var stream = new MemoryStream())
        //        {
        //            workbook.SaveAs(stream);
        //            var content = stream.ToArray();
        //            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DS LSHD_User.xlsx");
        //        }
        //    }
        //}
        // GET: ActivityLogs
        public async Task<IActionResult> Index()
        {
            return View(await _context.ActivityLog.OrderByDescending(s=>s.Id).ToListAsync());
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
            var recordsToDelete = _context.ActivityLog.Where(x => selectedIds.Contains(x.Id)).ToList();
            _context.ActivityLog.RemoveRange(recordsToDelete);
            _context.SaveChanges();
            TempData["Message_delete"] = "Đã xóa thành công " + recordsToDelete.Count() + " bản ghi";
            return RedirectToAction("Index");
        }      

        // GET: ActivityLogs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var activityLog = await _context.ActivityLog
                .FirstOrDefaultAsync(m => m.Id == id);
            if (activityLog == null)
            {
                return NotFound();
            }

            return View(activityLog);
        }

        // GET: ActivityLogs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ActivityLogs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,Action,DateTime,IpAddress,Url,Data")] ActivityLog activityLog)
        {
            if (ModelState.IsValid)
            {
                _context.Add(activityLog);
                await _context.SaveChangesAsync();
               
                return RedirectToAction(nameof(Index));
            }
            return View(activityLog);
        }

        // GET: ActivityLogs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var activityLog = await _context.ActivityLog.FindAsync(id);
            if (activityLog == null)
            {
                return NotFound();
            }
            return View(activityLog);
        }

        // POST: ActivityLogs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,Action,DateTime,IpAddress,Url,Data")] ActivityLog activityLog)
        {
            if (id != activityLog.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(activityLog);                  
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ActivityLogExists(activityLog.Id))
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
            return View(activityLog);
        }

        // GET: ActivityLogs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var activityLog = await _context.ActivityLog
                .FirstOrDefaultAsync(m => m.Id == id);
            if (activityLog == null)
            {
                return NotFound();
            }

            return View(activityLog);
        }

        // POST: ActivityLogs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var activityLog = await _context.ActivityLog.FindAsync(id);
            _context.ActivityLog.Remove(activityLog);
            await _context.SaveChangesAsync();
            TempData["Message_delete"] = "Đã xóa lịch sử.";
            return RedirectToAction(nameof(Index));
        }

        private bool ActivityLogExists(int id)
        {
            return _context.ActivityLog.Any(e => e.Id == id);
        }
    }
}

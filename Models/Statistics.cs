using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace websitebanxemay.Models
{
    public class Statistics
    {
        public int StatisticsID { get; set; }
        [Display(Name = "Tổng số đơn")]
        public int TotalOrders { get; set; }
        [Display(Name = "Tổng số khách đặt hàng")]
        public int TotalCustomers { get; set; }
        [Display(Name = "Tổng doanh thu")]
        public int TotalRevenue { get; set; }
        [Display(Name = "Tổng số sản phẩm đã bán")]
        public int BillDetailId { get; set; }
        [Display(Name = "Tổng số sản phẩm đã bán")]
        public BillDetail BillDetail { get; set; }
    }
}

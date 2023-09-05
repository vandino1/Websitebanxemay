using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace websitebanxemay.Models
{
    public class Bill
    {
		public int BillId { get; set; }
		[DataType(DataType.Date)]
		[Display(Name = "Ngày tạo đơn")]
		public DateTime Date { get; set; }

		[Required(ErrorMessage = "Vui lòng nhập họ tên.")]
		[Display(Name = "Tên khách hàng")]
		public string CustomerName { get; set; }

		[Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
		[Display(Name = "Số điện thoại")]
		[DataType(DataType.PhoneNumber)]
		[MaxLength(11)]
		public string CustomerPhone { get; set; }

		[Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng.")]
		[Display(Name = "Địa chỉ khách hàng")]
		public string CustomerAddress { get; set; }

		[DisplayFormat(DataFormatString = "{0:#,##0} đ")]
		[Display(Name = "Tổng trị giá")]
		public int BillTotal { get; set; }
		[Display(Name = "Trạng thái")]
		public string BillStatus { get; set; }

		[Display(Name = "Trạng thái đơn")]
		public string OrderStatus { get; set; }

		public ICollection<BillDetail> BillDetails { get; set; }	
	}
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace websitebanxemay.Models
{
    public class Product
    {
        public int ProductId { get; set; }

        [StringLength(60, ErrorMessage = "Trường Tên sản phẩm phải là một chuỗi có độ dài tối thiểu là 3 và độ dài tối đa là 60", MinimumLength = 3)]
        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm.")]
        [Display(Name = "Tên sản phẩm")]
        public string Name { get; set; }

        [Display(Name = "Chữ thường")]
        public string slug { get; set; }
        [Display(Name = "Thương hiệu")]
        public string Brand { get; set; }
        [DisplayFormat(DataFormatString = "{0:#,##0} đ")]
        [Required(ErrorMessage = "Vui lòng nhập giá.")]
        [Display(Name = "Giá")]
        public int Price { get; set; }
        [Display(Name = "Số lượng")]
        [Required(ErrorMessage = "Vui lòng nhập số lượng.")]
        public int Quantity { get; set; }
        [Display(Name = "Mô tả")]
        [Required(ErrorMessage = "Vui lòng nhập mô tả.")]

        public string Desciption { get; set; }

        [Display(Name = "Trạng thái")]
        public bool status { get; set; }

        [Display(Name = "Lượt xem")]
        public int viewCount { get; set; }
        [Display(Name = "Màu")]
        public string Color { get; set; }
        [Display(Name = "Dung tích bình")]
        public string Capacity { get; set; }
        [Display(Name = "Công suất max")]
        public string MaxWattage { get; set; }
        [Display(Name = "Moment max")]
        public string MaxMoment { get; set; }
        [Display(Name = "Bảo hành")]
        public string Warranty { get; set; }
        [Display(Name = "Kiểu")]
        public string Type { get; set; }

        [Display(Name = "Danh mục")]
        [Range(1, int.MaxValue, ErrorMessage = "Chọn một tên danh mục")]
        public int categoryId { get; set; }

        [Display(Name = "Danh mục")]
        public Category Category { get; set; }

        [Display(Name = "Hình ảnh")] 
        public string Image { get; set; }

        [Display(Name = "Rating")]
        public float adverageRating { get; set; }
        public ICollection<Comment> comments { get; set; }

        public float CalculateAverageRating(List<Comment> comments)
        {
            if (comments == null || comments.Count == 0)
            {
                return 0;
            }

            int totalStars = 0;
            foreach (var comment in comments)
            {
                totalStars += (Int32)comment.starRating;
            }

            return (float)totalStars / comments.Count;
        }

    }
}

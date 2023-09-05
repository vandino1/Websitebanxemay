﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace websitebanxemay.Models
{
    public class Comment
    {
        public int commentId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email.")]      
        [EmailAddress(ErrorMessage = "Sai định dạng Email")]      
        [Display(Name = "Email")]
        public string email { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Ngày đăng")]
        public DateTime createDate { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung.")]
        [Display(Name = "Nội dung")]
        public string mainContent { get; set; }

        [Display(Name = "Số sao")]
        public int? starRating { get; set; } // nullable int to allow for no rating

        [Display(Name = "Tên sản phẩm")]
        public int ProductId { get; set; }
        [Display(Name = "Tiêu đề quảng cáo")]
        public Product Product { get; set; }

        public ICollection<Rely> Replies { get; set; }
    }
}

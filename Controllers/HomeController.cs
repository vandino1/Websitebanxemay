using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using websitebanxemay.Data;
using websitebanxemay.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Identity;

namespace websitebanxemay.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            this._userManager = userManager;
        }     
        public IActionResult FilterByPriceRange(int priceRange)
        {
            var items = _context.Product.Where(x => x.status == true).AsQueryable();

            switch (priceRange)
            {
                case 1:
                    items = items.Where(i => i.Price < 20000000); // Giá dưới 20 triệu
                    break;
                case 2:
                    items = items.Where(i => i.Price >= 20000000 && i.Price <= 100000000); // Giá dưới 20-100 triệu
                    break;
                case 3:
                    items = items.Where(i => i.Price >= 100000000 && i.Price <= 200000000); // Giá từ 100-200 triệu
                    break;
                case 4:
                    items = items.Where(i => i.Price > 200000000); // Giá trên 200 triệu
                    break;
                default:
                    // Không lọc theo giá
                    break;
            }
            var filteredCount = items.Count(); // Đếm số lượng quảng cáo lọc được
            ViewBag.FilteredCount = filteredCount;

            return View(items.ToList());
        }
        public IActionResult FilterByPrice(decimal? minPrice, decimal? maxPrice)
        {
            var items = _context.Product.Where(x => x.status == true).AsQueryable();
            //`items` được gán giá trị của chính nó mà được lọc lần lượt theo `minPrice` và `maxPrice`.
            if (minPrice != null)
            {
                items = items.Where(i => i.Price >= minPrice);
            }
            if (maxPrice != null)
            {
                items = items.Where(i => i.Price <= maxPrice);
            }
            var filteredCount = items.Count(); // Đếm số lượng quảng cáo lọc được
            ViewBag.FilteredCount = filteredCount;

            return View(items.ToList());
        }
        public IActionResult Compare(string ids)
        {          
            var adIds = ids.Split(',').Select(int.Parse).ToList();
            var ads = _context.Product.Include(a => a.Category)//truy vấn đến các đối tượng danh mục                                        
                                            .Where(a => adIds.Contains(a.ProductId)).ToList();
            return View(ads);
        }
        public async Task<IActionResult> SortByPriceAscending()
        {
            var products = _context.Product.Where(x => x.status == true).OrderBy(x => x.Price);           
            return View(await products.AsNoTracking().ToListAsync());
        }
        public async Task<IActionResult> SortByPriceDescending()
        {
            var products = _context.Product.Where(x => x.status == true).OrderByDescending(x => x.Price);

            return View(await products.AsNoTracking().ToListAsync());
        }
        [HttpPost]
        public async Task<IActionResult> AddReply(int reply, string email, string createDate, string mainContent, int commentId)
        {
            //tạo user mới để lấy email
            var user = await _userManager.GetUserAsync(User);
            // Tạo một đối tượng Rely, sau đó thuộc tính đối tượng được gán giá trị thông qua cú pháp đối tượng
            var rely = new Rely();
            {
                //Tên đối tượng.thuộc tính và gán giá trị 
                rely.replyId = reply;
                rely.email = user.Email;
                rely.createDate = DateTime.Now;
                rely.mainContent = mainContent;
                rely.commentId = commentId;
            }
            var activityLog = new ActivityLog
            {
                UserId = User.Identity.Name,
                Action = "Phản hồi bình luận",
                DateTime = DateTime.Now,
                IpAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString(),
                Url = Request.HttpContext.Request.Path.ToString(),
                Data = "Trả lời bình luận trước đó"
            };
            // Lưu đối tượng phản hồi vào cơ sở dữ liệu
            _context.ActivityLog.Add(activityLog);
            _context.Add(rely);
            _context.SaveChanges();//lưu các thay đổi của cơ sở dữ liệu đã thực hiện.
            await _context.SaveChangesAsync();// đảm bảo mọi thay đổi được lưu trữ trước khi tiếp tục thực hiện.
            TempData["Message_create"] = "Phản hồi của bạn đã được ghi nhận";
            // Chuyển hướng trở lại trang DSSP
            return RedirectToAction("Index", new { id = rely.commentId });
        }

        // GET: Home
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Bill.Include(b => b.BillDetails);
            return View(await _context.Product.ToListAsync());
        }
        [HttpGet]
        public async Task<IActionResult> Index(string searchString, string currentFilter, int p = 1)
        {
            int pageSize = 12;

            ViewData["CurrentFilter"] = searchString;

            var products = from m in _context.Product.Skip((p - 1) * pageSize)
                                                       .Take(pageSize)
                           select m;


            if (!String.IsNullOrEmpty(searchString))
            {
                //searchString == null co the them vao
                products = products.Where(s => s.Name.Contains(searchString) || s.Brand.Contains(searchString) || s.Category.name.Contains(searchString));
            }
            int advertisementCount = _context.Product.Where(n => n.status == true).Count();
            ViewBag.AdvertisementCount = advertisementCount;
            // Lấy số lượng sản phẩm trong giỏ hàng
            int cartCount = GetCartItems().Sum(c => c.Quantity);
            // Gửi thông tin số lượng sản phẩm đến View bằng ViewBag
            ViewBag.CartCount = cartCount;

            ViewBag.PageNumber = p;
            ViewBag.PageRange = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((decimal)_context.Product.Count() / pageSize);

            return View(await products.AsNoTracking().ToListAsync());

        }
        public async Task<IActionResult> AdvertisementsByCategory(string categorySlug, int p = 1, string sort = "")
        {
            Category category = await _context.Category.Where(x => x.slug == categorySlug).FirstOrDefaultAsync();
            if (category == null) return RedirectToAction("Index");

            int pageSize = 2;
            var products = _context.Product.OrderByDescending(x => x.ProductId)
                                           .Where(x => x.categoryId == category.categoryId)
                                           .Skip((p - 1) * pageSize)
                                           .Take(pageSize);
            //Tính số lượng quảng cáo theo tên danh mục(CategoryId)
            int count = await _context.Product.Where(x => x.categoryId == category.categoryId && x.status == true).CountAsync();
            ViewBag.AdvertisementCount = count;
            // Sắp xếp theo giá tăng dần
            if (sort == "price-asc")
            {
                products = products.OrderBy(x => x.Price);
            }
            // Sắp xếp theo giá giảm dần
            else if (sort == "price-desc")
            {
                products = products.OrderByDescending(x => x.Price);
            }
            // Mặc định sắp xếp theo tin mới nhất (advertisementId)
            else
            {
                products = products.OrderByDescending(x => x.ProductId);
            }
            ViewBag.PageNumber = p;
            ViewBag.PageRange = pageSize;
            ViewBag.TotalPages = (int)Math.Ceiling((decimal)_context.Product.Where(x => x.categoryId ==
            category.categoryId).Count() / pageSize);
            ViewBag.CategoryName = category.name;
            ViewBag.CategorySlug = categorySlug;

            return View(await products.ToListAsync());
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        //Đăng bình luận
        public async Task<IActionResult> CreateComment(int commentId, string email, DateTime createDate, string mainContent, int starRating, int ProductId)
        {
            var user = await _userManager.GetUserAsync(User);
            var comment = new Comment();
            comment.commentId = commentId;
            comment.email = user.Email;
            comment.createDate = DateTime.Now;
            comment.mainContent = mainContent;
            comment.starRating = starRating;
            comment.ProductId = ProductId;
            // Lưu thông tin vào bảng lịch sử hoạt động
            var activityLog = new ActivityLog
            {
                UserId = User.Identity.Name,
                Action = "Đăng bình luận",
                DateTime = DateTime.Now,
                IpAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString(),
                Url = Request.HttpContext.Request.Path.ToString(),
                Data = "Đăng tải bình luận quảng cáo"
            };
            _context.ActivityLog.Add(activityLog);
            _context.Add(comment);
            _context.SaveChanges();
            await _context.SaveChangesAsync();
            TempData["Message_create"] = "Bình luận của bạn đã được lưu.";
            return RedirectToAction("Details", new { id = comment.ProductId });
        }

        // GET: Home/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product
               .Include(s => s.comments)
               .ThenInclude(s => s.Replies)              
               .Include(s => s.Category)
               .FirstOrDefaultAsync(m => m.ProductId == id);
            //Tính tổng số comment
            int totalComments = product.comments.Count();
            ViewBag.TotalComments = totalComments;

            var comments = _context.Comment.Where(c => c.ProductId == id).ToList();
            // Tính trung bình số sao của bình luận
            var averageRating = product.CalculateAverageRating(comments);

            // Gán giá trị trung bình số sao vào thuộc tính AverageRating của quảng cáo
            product.adverageRating = averageRating;
            if (product == null)
            {
                return NotFound();
            }
            if (product != null)
            {            
                _context.Product.Attach(product);
                product.viewCount = product.viewCount + 1;              
                _context.Entry(product).Property(x => x.viewCount).IsModified = true;
                //Lưu thay đổi vào csdl
                _context.SaveChanges();
            }
            var max = product.Price + 900000000;
            var min = product.Price - 900000000;
            // Lấy danh mục của tin quảng cáo hiện tại
            var category = product.Category;

            // Lấy ra các tin quảng cáo khác trong cùng danh mục đó
            //`Category == category`: Chỉ lấy các quảng cáo cùng trong danh mục với quảng cáo đang xét.
            //`advertisementId != id`: Loại bỏ quảng cáo đang xét khỏi danh sách kết quả (có thể bỏ).
            List<Product> dssp = _context.Product.OrderBy(x => x.Price).Where(x => x.Price >= min && x.Price <= max).Where(a => a.Category == category && a.ProductId != id && a.status == true).ToList();
            ViewBag.sanpham = dssp;
            //Đếm số lượng quảng cáo cùng danh mục
            ViewBag.NumOfAds = dssp.Count;
            return View(product);
        }
        List<CartItem> GetCartItems()
        {
            var session = HttpContext.Session;
            string jsoncart = session.GetString("shopcart");
            if (jsoncart != null)
            {
                return JsonConvert.DeserializeObject<List<CartItem>>(jsoncart);

            }          

            return new List<CartItem>();
        }
        void SaveCartSession(List<CartItem> lst)
        {
            var session = HttpContext.Session;
            string jsoncart = JsonConvert.SerializeObject(lst);
            session.SetString("shopcart", jsoncart);
        }
        void ClearCart()
        {
            var session = HttpContext.Session;
            session.Remove("shopcart");
        }
        public async Task<IActionResult> AddToCart(int id)
        {
            var product = await _context.Product
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound("Sản phẩm không tồn tại!");
            }
            //Xử lý cho hàng vào giỏ
            var cart = GetCartItems();
            var caritem = cart.Find(p => p.Product.ProductId == id);
            if (caritem != null) // mặt hàng đã có trong giỏ tăng số lượng 
            {
                caritem.Quantity++;

            }
            else
            {
                cart.Add(new CartItem() { Quantity = 1, Product = product });
            }          
            SaveCartSession(cart);//Lưu vào session           
            return RedirectToAction(nameof(Cart));//Chuyển đến trang giỏ hàng
        }
        public IActionResult Cart()
        {
            //if (!User.Identity.IsAuthenticated)
            //{
            //    // Người dùng chưa đăng nhập, chuyển hướng đến trang đăng nhập
            //    return RedirectToPage("/Account/Login", new { area = "Identity" });
            //    // Thay đổi "Login" và "Account" thành tên tương ứng với trang đăng nhập trong ứng dụng của bạn
            //}
            // Lấy số lượng sản phẩm trong giỏ hàng
            int cartCount = GetCartItems().Sum(c => c.Quantity);
            // Gửi thông tin số lượng sản phẩm đến View bằng ViewBag
            ViewBag.CartCount = cartCount;
            return View(GetCartItems());
        }

        // GET: Home/Create
        private bool ProductExists(int id)
        {
            return _context.Product.Any(e => e.ProductId == id);
        }
        public IActionResult RemoveCart(int id)
        {

            var cart = GetCartItems();
            var cartitem = cart.Find(p => p.Product.ProductId == id);

            if (cartitem != null) // mặt hàng đã có trong giỏ tăng số lượng 
            {
                cart.Remove(cartitem);

            }
            SaveCartSession(cart);//Lưu vào session           
            return RedirectToAction(nameof(Cart));//Chuyển đến trang giỏ hàng
        }
        public IActionResult UpdateCart(int id, int quantity)
        {
            var cart = GetCartItems();
            var cartitem = cart.Find(p => p.Product.ProductId == id);

            if (cartitem != null) // mặt hàng đã có trong giỏ tăng số lượng 
            {
                cartitem.Quantity = quantity;

            }
            SaveCartSession(cart);//Lưu vào session
            return RedirectToAction(nameof(Cart));//Chuyển đến trang giỏ hàng

        }
        public IActionResult DeleteAll()
        {
            ClearCart();
            return RedirectToAction(nameof(Cart));//Chuyển đến trang giỏ hàng
        }
        [Route("checkout.html")]
        public IActionResult Checkout()
        {
            return View(GetCartItems());
        }
        // Lập hóa đơn: lưu hóa đơn, lưu chi tiết hóa đơn
        [HttpPost, ActionName("CreateBill")]
        public async Task<IActionResult> CreateBill(string cusName, string cusPhone, string cusAddress, int billTotal, string billStatus, string orderStatus)
        {
            var bill = new Bill();
            bill.Date = DateTime.Now;
            bill.CustomerName = cusName;
            bill.CustomerPhone = cusPhone;
            bill.CustomerAddress = cusAddress;
            // cập nhật tổng tiền hóa đơn ?
            bill.BillTotal = billTotal;

            bill.BillStatus = billStatus;
            bill.OrderStatus = orderStatus;
            //// Lưu thông tin vào bảng lịch sử hoạt động
            //var activityLog = new ActivityLog
            //{
            //    UserId = User.Identity.Name,
            //    Action = "Tạo mới đơn đặt hàng",
            //    DateTime = DateTime.Now,
            //    IpAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString(),
            //    Url = Request.HttpContext.Request.Path.ToString(),
            //    Data = "Đơn đặt hàng mới"
            //};
            //_context.ActivityLog.Add(activityLog);
            _context.Add(bill);
            await _context.SaveChangesAsync();

            // thêm chi tiết hóa đơn
            var cart = GetCartItems();

            int amount = 0;
            int total = 0;
            //Lưu hết chi tiết hóa đơn
            foreach (var i in cart)
            {
                var b = new BillDetail();
                b.BillId = bill.BillId;
                b.ProductId = i.Product.ProductId;
                amount = i.Product.Price * i.Quantity;
                total += amount;
                b.Price = i.Product.Price;
                b.Quantity = i.Quantity;
                b.Amount = amount;               
                //view bag              
                _context.Add(b);      
            }                          
            await _context.SaveChangesAsync();
            //Xóa giỏ 
            ClearCart();
            return RedirectToAction(nameof(Thank));
        }
        public IActionResult Thank()
        {
            return View();
        }
    }
}

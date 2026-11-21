using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using _25DH110239_MusicalStore.Data;

namespace _25DH110239_MusicalStore.Controllers
{
    public class HomeController : Controller
    {
        // Kết nối Database thông qua Entity Framework
        private MusicalStore_25DH110239Entities db = new MusicalStore_25DH110239Entities();

        // ==========================================
        // 1. TRANG CHỦ & HIỂN THỊ SẢN PHẨM
        // ==========================================
        public ActionResult Index()
        {
            ViewBag.HangMoiVe = db.Products.OrderByDescending(p => p.ProductID).Take(4).ToList();
            ViewBag.SanPhamKhac = db.Products.OrderBy(p => p.ProductID).Take(4).ToList();
            return View();
        }

        // Xem danh sách nhạc cụ theo từng hãng (có lọc giá, sắp xếp, phân trang)
        public ActionResult DanhMuc(int id, decimal? minPrice, decimal? maxPrice, string sortOrder, int page = 1)
        {
            var category = db.Categories.Find(id);
            if (category == null) return HttpNotFound();

            ViewBag.Category = category;
            var query = db.Products.Where(p => p.CategoryID == id);

            if (minPrice.HasValue) query = query.Where(p => p.ProductPrice >= minPrice.Value);
            if (maxPrice.HasValue) query = query.Where(p => p.ProductPrice <= maxPrice.Value);

            switch (sortOrder)
            {
                case "price_asc": query = query.OrderBy(p => p.ProductPrice); break;
                case "price_desc": query = query.OrderByDescending(p => p.ProductPrice); break;
                default: query = query.OrderByDescending(p => p.ProductID); break;
            }

            int pageSize = 6;
            int totalItems = query.Count();
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.SortOrder = sortOrder;

            return View(query.Skip((page - 1) * pageSize).Take(pageSize).ToList());
        }

        // Tìm kiếm nhạc cụ theo từ khóa
        public ActionResult TimKiem(string keyword)
        {
            ViewBag.Keyword = keyword;
            var products = string.IsNullOrWhiteSpace(keyword)
                ? db.Products.ToList()
                : db.Products.Where(p => p.ProductName.Contains(keyword)).ToList();
            return View(products);
        }

        // Xem chi tiết 1 sản phẩm và gợi ý sản phẩm cùng hãng
        public ActionResult ChiTietSanPham(int id)
        {
            var product = db.Products.Find(id);
            if (product == null) return HttpNotFound();

            ViewBag.SanPhamCungHang = db.Products
                .Where(p => p.CategoryID == product.CategoryID && p.ProductID != id)
                .Take(4).ToList();
            return View(product);
        }

        // ==========================================
        // 2. ĐĂNG KÝ / ĐĂNG NHẬP / ĐĂNG XUẤT
        // ==========================================
        [HttpGet]
        public ActionResult DangNhap()
        {
            return View();
        }

        [HttpPost]
        public ActionResult DangNhap(string username, string password)
        {
            var user = db.Users.FirstOrDefault(u => u.Username == username && u.Password == password);
            if (user == null)
            {
                ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không chính xác!";
                return View();
            }
            if (!user.IsActive)
            {
                ViewBag.Error = "Tài khoản của bạn đã bị khóa!";
                return View();
            }

            Session["Username"] = user.Username;
            Session["UserRole"] = user.UserRole;
            Session["FullName"] = user.FullName ?? user.Username;

            if (user.UserRole == "Admin")
                return RedirectToAction("Index", "HangHoa");

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public ActionResult DangKy()
        {
            return View();
        }

        [HttpPost]
        public ActionResult DangKy(string username, string password, string confirmPassword, string fullName, string phone, string email, string address)
        {
            if (string.IsNullOrWhiteSpace(username) || username.Length < 5)
            {
                ViewBag.Error = "Tên đăng nhập phải từ 5 ký tự trở lên!";
                return View();
            }
            if (password != confirmPassword)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp!";
                return View();
            }
            if (db.Users.Any(u => u.Username == username))
            {
                ViewBag.Error = "Tên đăng nhập này đã có người sử dụng!";
                return View();
            }

            var newUser = new User
            {
                Username = username,
                Password = password,
                UserRole = "Customer",
                IsActive = true,
                CreatedAt = DateTime.Now,
                FullName = fullName,
                Email = email,
                Phone = phone
            };
            db.Users.Add(newUser);

            var newCustomer = new Customer
            {
                CustomerName = fullName,
                CustomerPhone = phone,
                CustomerEmail = email,
                CustomerAddress = address,
                Username = username
            };
            db.Customers.Add(newCustomer);
            db.SaveChanges();

            TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToAction("DangNhap");
        }

        public ActionResult DangXuat()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // ==========================================
        // 3. GIỎ HÀNG, ĐẶT HÀNG, HÓA ĐƠN & LỊCH SỬ
        // ==========================================
        public ActionResult GioHang()
        {
            return View();
        }

        [HttpGet]
        public ActionResult DatHang()
        {
            if (Session["Username"] == null)
                return RedirectToAction("DangNhap");

            string username = Session["Username"].ToString();
            var customer = db.Customers.FirstOrDefault(c => c.Username == username);
            return View(customer);
        }

        [HttpPost]
        public JsonResult KiemTraVoucher(string code, decimal subTotal)
        {
            var voucher = db.Vouchers.FirstOrDefault(v => v.Code == code && v.IsActive && v.ExpiryDate >= DateTime.Now);
            if (voucher == null)
                return Json(new { success = false, message = "Mã giảm giá không hợp lệ hoặc đã hết hạn!" });

            if (subTotal < voucher.MinOrder)
                return Json(new { success = false, message = "Đơn hàng chưa đạt giá trị tối thiểu để áp dụng mã!" });

            decimal discountAmount = subTotal * voucher.DiscountPercent / 100;
            return Json(new { success = true, discountAmount = discountAmount, discountPercent = voucher.DiscountPercent });
        }

        [HttpPost]
        public JsonResult TaoDonHang(string receiverName, string receiverPhone, string receiverEmail, string addressDelivery, string paymentMethod, string voucherCode, string cartJson)
        {
            if (Session["Username"] == null)
                return Json(new { success = false, message = "Vui lòng đăng nhập để đặt hàng!" });
            var items = Newtonsoft.Json.JsonConvert.DeserializeObject<List<CartItemModel>>(cartJson);
            if (items == null || items.Count == 0)
                return Json(new { success = false, message = "Giỏ hàng trống!" });

            string username = Session["Username"].ToString();
            var customer = db.Customers.FirstOrDefault(c => c.Username == username);

            decimal subTotal = 0;
            foreach (var item in items)
            {
                var p = db.Products.Find(item.id);
                if (p == null || p.ProductStock < item.quantity)
                    return Json(new { success = false, message = "Sản phẩm không đủ số lượng tồn kho!" });

                decimal actualPrice = p.ProductPrice * (100 - p.DiscountPercent) / 100;
                subTotal += actualPrice * item.quantity;
            }

            decimal discountAmount = 0;
            if (!string.IsNullOrEmpty(voucherCode))
            {
                var voucher = db.Vouchers.FirstOrDefault(v => v.Code == voucherCode && v.IsActive && v.ExpiryDate >= DateTime.Now && subTotal >= v.MinOrder);
                if (voucher != null)
                    discountAmount = subTotal * voucher.DiscountPercent / 100;
            }

            decimal afterDiscount = subTotal - discountAmount;
            decimal shippingFee = afterDiscount >= 1000000 ? 0 : 30000;
            decimal totalAmount = afterDiscount + shippingFee;

            var order = new Order
            {
                CustomerID = customer != null ? (int?)customer.CustomerID : null,
                OrderDate = DateTime.Now,
                TotalAmount = totalAmount,
                PaymentStatus = paymentMethod == "Chuyển khoản" ? "Đã thanh toán" : "Chưa thanh toán",
                AddressDelivery = addressDelivery,
                ReceiverName = receiverName,
                ReceiverPhone = receiverPhone,
                ReceiverEmail = receiverEmail,
                PaymentMethod = paymentMethod,
                ShippingFee = shippingFee,
                OrderStatus = "Chờ xác nhận",
                VoucherCode = voucherCode,
                DiscountAmount = discountAmount
            };
            db.Orders.Add(order);
            db.SaveChanges();

            foreach (var item in items)
            {
                var p = db.Products.Find(item.id);
                decimal actualPrice = p.ProductPrice * (100 - p.DiscountPercent) / 100;

                db.OrderDetails.Add(new OrderDetail
                {
                    OrderID = order.OrderID,
                    ProductID = p.ProductID,
                    ProductName = p.ProductName,
                    Quantity = item.quantity,
                    UnitPrice = actualPrice
                });

                p.ProductStock -= item.quantity;
            }
            db.SaveChanges();

            return Json(new { success = true, orderId = order.OrderID });
        }

        public ActionResult Bill(int id)
        {
            var order = db.Orders.Find(id);
            if (order == null) return HttpNotFound();
            return View(order);
        }

        public ActionResult LichSuMuaHang()
        {
            if (Session["Username"] == null) return RedirectToAction("DangNhap");
            string username = Session["Username"].ToString();
            var customer = db.Customers.FirstOrDefault(c => c.Username == username);

            var orders = customer != null
                ? db.Orders.Where(o => o.CustomerID == customer.CustomerID).OrderByDescending(o => o.OrderDate).ToList()
                : new List<Order>();

            return View(orders);
        }

        [HttpPost]
        public ActionResult HuyDonHang(int id)
        {
            var order = db.Orders.Find(id);
            if (order != null && order.OrderStatus == "Chờ xác nhận")
            {
                order.OrderStatus = "Đã hủy";
                if (order.PaymentStatus == "Đã thanh toán")
                    order.PaymentStatus = "Chờ hoàn tiền";

                foreach (var detail in order.OrderDetails)
                {
                    var p = db.Products.Find(detail.ProductID);
                    if (p != null) p.ProductStock += detail.Quantity;
                }
                db.SaveChanges();
            }
            return RedirectToAction("LichSuMuaHang");
        }
    }

    public class CartItemModel
    {
        public int id { get; set; }
        public string name { get; set; }
        public decimal price { get; set; }
        public int quantity { get; set; }
        public string image { get; set; }
    }
}
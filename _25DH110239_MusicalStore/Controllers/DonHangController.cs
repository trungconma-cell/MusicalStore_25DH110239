using System.Linq;
using System.Web.Mvc;
using _25DH110239_MusicalStore.Data;

namespace _25DH110239_MusicalStore.Controllers
{
    public class DonHangController : Controller
    {
        private MusicalStore_25DH110239Entities db = new MusicalStore_25DH110239Entities();

        // Kiểm tra quyền Admin trước khi cho phép truy cập
        private bool IsAdmin()
        {
            return Session["UserRole"] != null && Session["UserRole"].ToString() == "Admin";
        }

        // Danh sách toàn bộ đơn hàng (có lọc theo trạng thái)
        public ActionResult Index(string trangThai)
        {
            if (!IsAdmin()) return RedirectToAction("DangNhap", "Home");

            var query = db.Orders.AsQueryable();
            if (!string.IsNullOrEmpty(trangThai))
                query = query.Where(o => o.OrderStatus == trangThai);

            ViewBag.TrangThai = trangThai;
            return View(query.OrderByDescending(o => o.OrderDate).ToList());
        }

        // Chuyển trạng thái đơn hàng: Chờ xác nhận -> Đã xác nhận -> Đang giao -> Hoàn thành (hoặc Hủy)
        [HttpPost]
        public ActionResult DoiTrangThai(int id, string trangThaiMoi)
        {
            if (!IsAdmin()) return RedirectToAction("DangNhap", "Home");

            var order = db.Orders.Find(id);
            if (order != null)
            {
                // Nếu Admin hủy đơn thì hoàn lại tồn kho sản phẩm
                if (trangThaiMoi == "Đã hủy" && order.OrderStatus != "Đã hủy")
                {
                    foreach (var d in order.OrderDetails)
                    {
                        var p = db.Products.Find(d.ProductID);
                        if (p != null) p.ProductStock += d.Quantity;
                    }
                    if (order.PaymentStatus == "Đã thanh toán")
                        order.PaymentStatus = "Đã hoàn tiền";
                }

                // Khi đơn hoàn thành tự động cập nhật đã thanh toán
                if (trangThaiMoi == "Hoàn thành")
                    order.PaymentStatus = "Đã thanh toán";

                order.OrderStatus = trangThaiMoi;
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}
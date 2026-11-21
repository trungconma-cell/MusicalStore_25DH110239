using System.Linq;
using System.Web.Mvc;
using _25DH110239_MusicalStore.Data;

namespace _25DH110239_MusicalStore.Controllers
{
    public class ThongKeController : Controller
    {
        private MusicalStore_25DH110239Entities db = new MusicalStore_25DH110239Entities();

        private bool IsAdmin()
        {
            return Session["UserRole"] != null && Session["UserRole"].ToString() == "Admin";
        }

        // Thống kê doanh thu, số đơn hàng, sản phẩm bán chạy
        public ActionResult Index()
        {
            if (!IsAdmin()) return RedirectToAction("DangNhap", "Home");

            // Tính tổng doanh thu các đơn không bị hủy
            var validOrders = db.Orders.Where(o => o.OrderStatus != "Đã hủy").ToList();
            ViewBag.TongDoanhThu = validOrders.Sum(o => (decimal?)o.TotalAmount) ?? 0;
            ViewBag.TongDonHang = validOrders.Count;
            ViewBag.DonHoanThanh = validOrders.Count(o => o.OrderStatus == "Hoàn thành");
            ViewBag.DonDaHuy = db.Orders.Count(o => o.OrderStatus == "Đã hủy");

            // Top sản phẩm bán chạy (gom nhóm theo ProductName trong OrderDetail của đơn hợp lệ)
            var topProducts = db.OrderDetails
                .Where(od => od.Order.OrderStatus != "Đã hủy")
                .GroupBy(od => od.ProductName)
                .Select(g => new TopProductStat
                {
                    ProductName = g.Key,
                    TotalSold = g.Sum(x => x.Quantity),
                    Revenue = g.Sum(x => x.Quantity * x.UnitPrice)
                })
                .OrderByDescending(x => x.TotalSold)
                .ToList();

            return View(topProducts);
        }
    }

    public class TopProductStat
    {
        public string ProductName { get; set; }
        public int TotalSold { get; set; }
        public decimal Revenue { get; set; }
    }
}
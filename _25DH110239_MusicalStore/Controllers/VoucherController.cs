using System;
using System.Linq;
using System.Web.Mvc;
using _25DH110239_MusicalStore.Data;

namespace _25DH110239_MusicalStore.Controllers
{
    public class VoucherController : Controller
    {
        private MusicalStore_25DH110239Entities db = new MusicalStore_25DH110239Entities();

        private bool IsAdmin()
        {
            return Session["UserRole"] != null && Session["UserRole"].ToString() == "Admin";
        }

        // Danh sách mã giảm giá
        public ActionResult Index()
        {
            if (!IsAdmin()) return RedirectToAction("DangNhap", "Home");
            return View(db.Vouchers.OrderByDescending(v => v.VoucherID).ToList());
        }

        // Thêm voucher mới
        [HttpPost]
        public ActionResult Them(string code, int discountPercent, decimal minOrder, DateTime expiryDate, string description)
        {
            if (!IsAdmin()) return RedirectToAction("DangNhap", "Home");
            code = code.Trim().ToUpper();
            if (!db.Vouchers.Any(v => v.Code == code))
            {
                db.Vouchers.Add(new Voucher
                {
                    Code = code,
                    DiscountPercent = discountPercent,
                    MinOrder = minOrder,
                    ExpiryDate = expiryDate,
                    Description = description,
                    IsActive = true
                });
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // Bật hoặc tắt trạng thái hoạt động của voucher
        [HttpPost]
        public ActionResult BatTat(int id)
        {
            if (!IsAdmin()) return RedirectToAction("DangNhap", "Home");
            var v = db.Vouchers.Find(id);
            if (v != null)
            {
                v.IsActive = !v.IsActive;
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}
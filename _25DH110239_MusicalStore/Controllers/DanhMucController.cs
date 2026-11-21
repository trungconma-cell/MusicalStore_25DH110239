using System.Linq;
using System.Web.Mvc;
using _25DH110239_MusicalStore.Data;

namespace _25DH110239_MusicalStore.Controllers
{
    public class DanhMucController : Controller
    {
        private MusicalStore_25DH110239Entities db = new MusicalStore_25DH110239Entities();

        private bool IsAdmin()
        {
            return Session["UserRole"] != null && Session["UserRole"].ToString() == "Admin";
        }

        // Danh sách thương hiệu nhạc cụ
        public ActionResult Index()
        {
            if (!IsAdmin()) return RedirectToAction("DangNhap", "Home");
            return View(db.Categories.ToList());
        }

        // Thêm mới thương hiệu
        [HttpPost]
        public ActionResult Them(string categoryName, string description)
        {
            if (!IsAdmin()) return RedirectToAction("DangNhap", "Home");
            if (!string.IsNullOrWhiteSpace(categoryName))
            {
                if (db.Categories.Any(c => c.CategoryName == categoryName))
                {
                    TempData["Error"] = "Tên thương hiệu đã tồn tại!";
                    return RedirectToAction("Index");
                }
                db.Categories.Add(new Category { CategoryName = categoryName, Description = description });
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // Xóa thương hiệu (chặn xóa nếu đang có sản phẩm)
        [HttpPost]
        public ActionResult Xoa(int id)
        {
            if (!IsAdmin()) return RedirectToAction("DangNhap", "Home");
            var cat = db.Categories.Find(id);
            if (cat != null)
            {
                if (cat.Products.Any())
                {
                    TempData["Error"] = "Không thể xóa thương hiệu đang có sản phẩm!";
                    return RedirectToAction("Index");
                }
                db.Categories.Remove(cat);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}
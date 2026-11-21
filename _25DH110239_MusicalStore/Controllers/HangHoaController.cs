using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using _25DH110239_MusicalStore.Data;

namespace _25DH110239_MusicalStore.Controllers
{
    public class HangHoaController : Controller
    {
        private MusicalStore_25DH110239Entities db = new MusicalStore_25DH110239Entities();

        private bool IsAdmin()
        {
            return Session["UserRole"] != null && Session["UserRole"].ToString() == "Admin";
        }

        // 1. Danh sách hàng hóa (có tìm kiếm và lọc theo hãng)
        public ActionResult Index(string q, int? categoryId)
        {
            if (!IsAdmin()) return RedirectToAction("DangNhap", "Home");

            ViewBag.Categories = db.Categories.ToList();
            ViewBag.Keyword = q;
            ViewBag.CategoryId = categoryId;

            var query = db.Products.AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(p => p.ProductName.Contains(q));
            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryID == categoryId.Value);

            return View(query.OrderByDescending(p => p.ProductID).ToList());
        }

        // 2. Thêm nhạc cụ mới [GET]
        [HttpGet]
        public ActionResult Them()
        {
            if (!IsAdmin()) return RedirectToAction("DangNhap", "Home");
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName");
            return View();
        }

        // 3. Thêm nhạc cụ mới [POST] - Hỗ trợ cả dán link ảnh hoặc tải file ảnh từ máy
        [HttpPost]
        public ActionResult Them(Product model, HttpPostedFileBase imageFile)
        {
            if (!IsAdmin()) return RedirectToAction("DangNhap", "Home");

            if (imageFile != null && imageFile.ContentLength > 0)
            {
                string folder = Server.MapPath("~/Content/image");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                string fileName = Guid.NewGuid().ToString().Substring(0, 8) + "_" + Path.GetFileName(imageFile.FileName);
                imageFile.SaveAs(Path.Combine(folder, fileName));
                model.ProductImage = fileName;
            }

            db.Products.Add(model);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // 4. Sửa nhạc cụ [GET]
        [HttpGet]
        public ActionResult Sua(int id)
        {
            if (!IsAdmin()) return RedirectToAction("DangNhap", "Home");
            var product = db.Products.Find(id);
            if (product == null) return HttpNotFound();

            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        // 5. Sửa nhạc cụ [POST]
        [HttpPost]
        public ActionResult Sua(Product model, HttpPostedFileBase imageFile)
        {
            if (!IsAdmin()) return RedirectToAction("DangNhap", "Home");

            var existing = db.Products.Find(model.ProductID);
            if (existing != null)
            {
                existing.CategoryID = model.CategoryID;
                existing.ProductName = model.ProductName;
                existing.ProductDescription = model.ProductDescription;
                existing.ProductPrice = model.ProductPrice;
                existing.ProductStock = model.ProductStock;
                existing.DiscountPercent = model.DiscountPercent;

                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    string folder = Server.MapPath("~/Content/image");
                    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                    string fileName = Guid.NewGuid().ToString().Substring(0, 8) + "_" + Path.GetFileName(imageFile.FileName);
                    imageFile.SaveAs(Path.Combine(folder, fileName));
                    existing.ProductImage = fileName;
                }
                else if (!string.IsNullOrWhiteSpace(model.ProductImage))
                {
                    existing.ProductImage = model.ProductImage;
                }

                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        // 6. Xóa nhạc cụ (chặn xóa nếu sản phẩm đã nằm trong đơn hàng)
        [HttpPost]
        public ActionResult Xoa(int id)
        {
            if (!IsAdmin()) return RedirectToAction("DangNhap", "Home");

            var product = db.Products.Find(id);
            if (product != null)
            {
                if (db.OrderDetails.Any(od => od.ProductID == id))
                {
                    TempData["Error"] = "Không thể xóa nhạc cụ này vì đã có khách đặt mua!";
                    return RedirectToAction("Index");
                }
                db.Products.Remove(product);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}
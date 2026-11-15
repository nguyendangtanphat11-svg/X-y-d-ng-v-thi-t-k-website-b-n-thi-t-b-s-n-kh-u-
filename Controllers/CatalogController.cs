
using System;
using System.Linq;
using System.Web.Mvc;
using Wedside_TVS_SFX.Models;

namespace STAGEFX_PRO.Controllers
{
    public class CatalogController : Controller
    {
        private readonly StageFxEntities db = new StageFxEntities();

        // GET: /Catalog
        public ActionResult Index()
        {
            // Lấy danh mục đang hoạt động
            var categories = db.DanhMuc
                .Where(c => c.TrangThai)
                .OrderBy(c => c.TenDanhMuc)
                .Select(c => new
                {
                    id = c.MaDanhMuc,
                    name = c.TenDanhMuc
                })
                .ToList();

            // Lấy sản phẩm đang hoạt động và ảnh chính
            var rawProducts = db.SanPham
                .Where(p => p.TrangThai)
                .OrderByDescending(p => p.NgayTao)
                .Select(p => new
                {
                    id = p.MaSanPham,
                    sku = p.SKU,
                    name = p.TenSanPham,
                    description = p.MoTaNgan,
                    detail = p.MoTaChiTiet,
                    specifications = p.ThongSoKyThuat,
                    b2bPrice = p.GiaBan,
                    rentalPrice = p.GiaThueNgay,
                    stock = p.SoLuongTon,
                    categoryId = p.MaDanhMuc,

                    categoryLabel = p.DanhMuc != null
                        ? p.DanhMuc.TenDanhMuc
                        : "Chưa phân loại",

                    image = p.HinhAnhSanPham
                        .Where(h => h.LaAnhChinh)
                        .OrderBy(h => h.ThuTuHienThi)
                        .Select(h => h.DuongDanAnh)
                        .FirstOrDefault()
                })
                .ToList();

            // Chuẩn hóa đường dẫn ảnh sau khi lấy dữ liệu từ SQL Server
            var products = rawProducts
                .Select(p => new
                {
                    p.id,
                    p.sku,
                    p.name,
                    p.description,
                    p.detail,
                    p.specifications,
                    p.b2bPrice,
                    p.rentalPrice,
                    p.stock,
                    p.categoryId,
                    p.categoryLabel,
                    image = NormalizeImagePath(p.image)
                })
                .ToList();

            // Chuyển dữ liệu sang JSON cho JavaScript
            ViewBag.CategoriesJson =
                System.Web.Helpers.Json.Encode(categories);

            ViewBag.ProductsJson =
                System.Web.Helpers.Json.Encode(products);

            return View();
        }

        /// <summary>
        /// Chuẩn hóa đường dẫn ảnh từ SQL Server thành URL hợp lệ.
        /// </summary>
        private string NormalizeImagePath(string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
                return "";

            imagePath = imagePath.Trim().Replace("\\", "/");

            // Giữ nguyên URL bên ngoài
            if (imagePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                imagePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return imagePath;
            }

            // Xử lý các dạng đường dẫn lưu trong database
            imagePath = imagePath.TrimStart('/');

            // Loại bỏ ký hiệu ~/ nếu có
            if (imagePath.StartsWith("~/"))
            {
                imagePath = imagePath.Substring(2);
            }

            // Nếu chỉ có tên file
            if (!imagePath.Contains("/"))
            {
                imagePath = "Content/uploads/products/" + imagePath;
            }
            // Nếu có dạng uploads/products/...
            else if (imagePath.StartsWith("uploads/", StringComparison.OrdinalIgnoreCase))
            {
                imagePath = "Content/" + imagePath;
            }

            // Nếu đã có Content/ thì không thêm lần nữa
            return Url.Content("~/" + imagePath);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}

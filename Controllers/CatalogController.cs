using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Helpers;
using Wedside_TVS_SFX.Models;

namespace STAGEFX_PRO.Controllers
{
    public class CatalogController : Controller
    {
        private readonly StageFxEntities db = new StageFxEntities();

        // GET: /Catalog
        public ActionResult Index()
        {
            // Danh mục đang hoạt động
            var categories = db.DanhMuc
                .Where(c => c.TrangThai)
                .OrderBy(c => c.TenDanhMuc)
                .Select(c => new
                {
                    id = c.MaDanhMuc,
                    name = c.TenDanhMuc
                })
                .ToList();

            // Sản phẩm đang hoạt động + danh mục + ảnh chính
            var products = db.SanPham
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

            ViewBag.CategoriesJson = System.Web.Helpers.Json.Encode(categories);
            ViewBag.ProductsJson = System.Web.Helpers.Json.Encode(products);

            return View();
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

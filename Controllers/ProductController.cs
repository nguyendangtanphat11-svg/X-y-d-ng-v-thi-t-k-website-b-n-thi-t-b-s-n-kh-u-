
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Wedside_TVS_SFX.Models;
using Wedside_TVS_SFX.ViewModels;

namespace STAGEFX_PRO.Controllers
{
    public class ProductController : Controller
    {
        private readonly StageFxEntities db = new StageFxEntities();

        // GET: /Product/Details/1
        public ActionResult Details(string id)
        {
            int productId;

            // Kiểm tra mã sản phẩm
            if (!int.TryParse(id, out productId))
            {
                return HttpNotFound();
            }

            // Lấy sản phẩm từ database
            var product = db.SanPham
                .Include(p => p.DanhMuc)
                .Include(p => p.HinhAnhSanPham)
                .FirstOrDefault(p =>
                    p.MaSanPham == productId &&
                    p.TrangThai == true
                );

            if (product == null)
            {
                return HttpNotFound();
            }

            // Chuyển dữ liệu sang ViewModel
            var model = new ProductDetailViewModel
            {
                MaSanPham = product.MaSanPham,
                SKU = product.SKU,
                TenSanPham = product.TenSanPham,
                MoTaNgan = product.MoTaNgan,
                MoTaChiTiet = product.MoTaChiTiet,
                ThongSoKyThuat = product.ThongSoKyThuat,

                GiaBan = product.GiaBan,
                GiaThueNgay = product.GiaThueNgay,
                SoLuongTon = product.SoLuongTon,

                TenDanhMuc = product.DanhMuc != null
                    ? product.DanhMuc.TenDanhMuc
                    : "Chưa phân loại",

                DanhSachAnh = product.HinhAnhSanPham
                    .OrderByDescending(h => h.LaAnhChinh)
                    .ThenBy(h => h.ThuTuHienThi)
                    .Select(h => h.DuongDanAnh)
                    .ToList()
            };

            return View("Details", model);
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

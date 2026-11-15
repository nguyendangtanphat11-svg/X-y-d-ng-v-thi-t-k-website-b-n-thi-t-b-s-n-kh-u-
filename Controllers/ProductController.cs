
using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
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

            if (!int.TryParse(id, out productId))
                return HttpNotFound();

            var product = db.SanPham
                .Include(p => p.DanhMuc)
                .Include(p => p.HinhAnhSanPham)
                .FirstOrDefault(p =>
                    p.MaSanPham == productId &&
                    p.TrangThai == true
                );

            if (product == null)
                return HttpNotFound();

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
                    .Select(h => NormalizeImagePath(h.DuongDanAnh))
                    .Where(path => !string.IsNullOrEmpty(path))
                    .ToList()
            };

            LoadProductReviews(model, productId);
            LoadEligibleReviewOrders(model, productId);

            return View("Details", model);
        }

        // Đọc đánh giá đã duyệt và thống kê
        private void LoadProductReviews(
            ProductDetailViewModel model,
            int productId)
        {
            var reviews = db.DanhGiaSanPham
                .AsNoTracking()
                .Where(r =>
                    r.MaSanPham == productId &&
                    r.DaDuyet
                );

            model.TongSoDanhGia = reviews.Count();

            if (model.TongSoDanhGia == 0)
            {
                model.DiemDanhGiaTrungBinh = 0;
                return;
            }

            model.DiemDanhGiaTrungBinh = Math.Round(
                reviews.Average(r => (double)r.SoSao),
                1
            ) is double average ? (decimal)average : 0;

            model.SoDanhGia5Sao = reviews.Count(r => r.SoSao == 5);
            model.SoDanhGia4Sao = reviews.Count(r => r.SoSao == 4);
            model.SoDanhGia3Sao = reviews.Count(r => r.SoSao == 3);
            model.SoDanhGia2Sao = reviews.Count(r => r.SoSao == 2);
            model.SoDanhGia1Sao = reviews.Count(r => r.SoSao == 1);

            model.DanhSachDanhGia = reviews
                .OrderByDescending(r => r.NgayTao)
                .Select(r => new ProductReviewViewModel
                {
                    MaDanhGia = r.MaDanhGia,
                    HoTenNguoiDung = r.NguoiDung.HoTen,
                    SoSao = r.SoSao,
                    NhanXet = r.NhanXet,
                    NgayTao = r.NgayTao
                })
                .ToList();
        }

        // Lấy các chi tiết đơn hàng đủ điều kiện đánh giá
        private void LoadEligibleReviewOrders(
            ProductDetailViewModel model,
            int productId)
        {
            if (Session["MaNguoiDung"] == null)
                return;

            int userId;

            if (!int.TryParse(
                Session["MaNguoiDung"].ToString(),
                out userId))
                return;

            // Chỉ khách hàng mới được đánh giá
            int roleId;
            if (Session["MaVaiTro"] == null ||
                !int.TryParse(Session["MaVaiTro"].ToString(), out roleId) ||
                roleId != 3)
                return;

            model.DanhSachDonHangCoTheDanhGia =
                db.ChiTietDonHang
                .AsNoTracking()
                .Where(ct =>
                    ct.MaSanPham == productId &&
                    ct.DonHang.MaNguoiDung == userId &&
                    ct.DonHang.TrangThai == "DaXacNhan" &&
                    !ct.DanhGiaSanPham.Any(r =>
                        r.MaNguoiDung == userId
                    )
                )
                .OrderByDescending(ct => ct.DonHang.NgayDat)
                .Select(ct => new ProductReviewOrderViewModel
                {
                    MaChiTietDonHang = ct.MaChiTietDonHang,
                    MaSoDonHang = ct.DonHang.MaSoDonHang,
                    TenSanPhamLucDat = ct.TenSanPhamLucDat,
                    NgayDat = ct.DonHang.NgayDat
                })
                .ToList();
        }

        // POST: /Product/SubmitReview
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitReview(
            int MaSanPham,
            int MaChiTietDonHang,
            byte SoSao,
            string NhanXet)
        {
            int userId;
            int roleId;

            if (Session["MaNguoiDung"] == null ||
                !int.TryParse(Session["MaNguoiDung"].ToString(), out userId))
            {
                TempData["ReviewError"] = "Vui lòng đăng nhập để đánh giá sản phẩm.";
                return RedirectToAction("Details", new { id = MaSanPham });
            }

            if (Session["MaVaiTro"] == null ||
                !int.TryParse(Session["MaVaiTro"].ToString(), out roleId) ||
                roleId != 3)
            {
                TempData["ReviewError"] = "Chỉ khách hàng mới có quyền đánh giá sản phẩm.";
                return RedirectToAction("Details", new { id = MaSanPham });
            }

            if (SoSao < 1 || SoSao > 5)
            {
                TempData["ReviewError"] = "Số sao đánh giá phải từ 1 đến 5.";
                return RedirectToAction("Details", new { id = MaSanPham });
            }

            if (NhanXet != null && NhanXet.Length > 2000)
            {
                TempData["ReviewError"] = "Nội dung đánh giá không được vượt quá 2000 ký tự.";
                return RedirectToAction("Details", new { id = MaSanPham });
            }

            // Kiểm tra tài khoản còn hoạt động
            var user = db.NguoiDung.FirstOrDefault(u =>
                u.MaNguoiDung == userId &&
                u.TrangThai
            );

            if (user == null)
            {
                TempData["ReviewError"] = "Tài khoản không hợp lệ hoặc đã bị khóa.";
                return RedirectToAction("Details", new { id = MaSanPham });
            }

            // Kiểm tra chi tiết đơn hàng thuộc đúng khách hàng,
            // đúng sản phẩm và đơn hàng đã xác nhận.
            var orderDetail = db.ChiTietDonHang.FirstOrDefault(ct =>
                ct.MaChiTietDonHang == MaChiTietDonHang &&
                ct.MaSanPham == MaSanPham &&
                ct.DonHang.MaNguoiDung == userId &&
                ct.DonHang.TrangThai == "DaXacNhan"
            );

            if (orderDetail == null)
            {
                TempData["ReviewError"] =
                    "Bạn không có đơn hàng hợp lệ để đánh giá sản phẩm này.";

                return RedirectToAction("Details", new { id = MaSanPham });
            }

            // Kiểm tra đã đánh giá chi tiết đơn hàng này chưa
            bool alreadyReviewed = db.DanhGiaSanPham.Any(r =>
                r.MaNguoiDung == userId &&
                r.MaChiTietDonHang == MaChiTietDonHang
            );

            if (alreadyReviewed)
            {
                TempData["ReviewError"] =
                    "Bạn đã đánh giá sản phẩm trong đơn hàng này rồi.";

                return RedirectToAction("Details", new { id = MaSanPham });
            }

            var review = new DanhGiaSanPham
            {
                MaSanPham = MaSanPham,
                MaNguoiDung = userId,
                MaChiTietDonHang = MaChiTietDonHang,
                SoSao = SoSao,
                NhanXet = string.IsNullOrWhiteSpace(NhanXet)
                    ? null
                    : NhanXet.Trim(),
                DaDuyet = false,
                NgayTao = DateTime.Now,
                NgayCapNhat = null
            };

            db.DanhGiaSanPham.Add(review);

            try
            {
                db.SaveChanges();

                TempData["ReviewSuccess"] =
                    "Đánh giá của bạn đã được gửi và đang chờ quản trị viên duyệt.";
            }
            catch (DbUpdateException)
            {
                TempData["ReviewError"] =
                    "Đánh giá chưa được lưu. Có thể bạn đã gửi đánh giá cho đơn hàng này.";
            }

            return RedirectToAction("Details", new { id = MaSanPham });
        }

        /// <summary>
        /// Chuẩn hóa đường dẫn ảnh sản phẩm.
        /// </summary>
        private string NormalizeImagePath(string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
                return "";

            imagePath = imagePath.Trim().Replace("\\", "/");

            if (imagePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                imagePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return imagePath;

            imagePath = imagePath.TrimStart('/');

            if (imagePath.StartsWith("~/"))
                imagePath = imagePath.Substring(2);

            if (!imagePath.Contains("/"))
                imagePath = "Content/uploads/products/" + imagePath;
            else if (imagePath.StartsWith("uploads/", StringComparison.OrdinalIgnoreCase))
                imagePath = "Content/" + imagePath;

            return Url.Content("~/" + imagePath);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }
    }
}

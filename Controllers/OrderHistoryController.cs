using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Wedside_TVS_SFX.Models;
using Wedside_TVS_SFX.ViewModels;

namespace STAGEFX_PRO.Controllers
{
    public class OrderHistoryController : Controller
    {
        private readonly StageFxEntities db = new StageFxEntities();

        // Kiểm tra tài khoản đăng nhập
        private int? CurrentUserId
        {
            get
            {
                if (Session["MaNguoiDung"] == null)
                    return null;

                return Convert.ToInt32(Session["MaNguoiDung"]);
            }
        }

        private ActionResult RequireLogin()
        {
            TempData["OrderHistoryMessage"] =
                "Vui lòng đăng nhập để xem lịch sử đơn hàng.";

            return RedirectToAction("Login", "Account");
        }

        // ==========================================
        // 1. DANH SÁCH ĐƠN HÀNG
        // ==========================================
        public ActionResult Index(string trangThai = "")
        {
            if (!CurrentUserId.HasValue)
                return RequireLogin();

            int userId = CurrentUserId.Value;

            var query = db.DonHang
                .AsNoTracking()
                .Where(x => x.MaNguoiDung == userId);

            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                query = query.Where(x => x.TrangThai == trangThai);
            }

            var orders = query
                .Include(x => x.ChiTietDonHang)
                .Include(x => x.ThanhToan)
                .Include(x => x.YeuCauHuyDon)
                .OrderByDescending(x => x.NgayDat)
                .ToList();

            var model = new OrderHistoryViewModel
            {
                TrangThaiLoc = trangThai,
                TongSoDon = orders.Count,

                DonHangs = orders.Select(x =>
                {
                    var payment = x.ThanhToan
                        .OrderByDescending(t => t.NgayTao)
                        .FirstOrDefault();

                    var cancellation = x.YeuCauHuyDon
                        .OrderByDescending(y => y.NgayYeuCau)
                        .FirstOrDefault();

                    return new OrderHistoryItemViewModel
                    {
                        MaDonHang = x.MaDonHang,
                        MaSoDonHang = x.MaSoDonHang,
                        NgayDat = x.NgayDat,
                        TongTien = x.TongTien,
                        TienCoc = x.TienCoc,
                        TrangThai = x.TrangThai,
                        TrangThaiThanhToan = x.TrangThaiThanhToan,

                        SoLuongSanPham = x.ChiTietDonHang
                            .Sum(c => (int?)c.SoLuong) ?? 0,

                        PhuongThucThanhToan = payment != null
                            ? payment.PhuongThuc
                            : null,

                        TrangThaiYeuCauHuy = cancellation != null
                            ? cancellation.TrangThaiYeuCau
                            : null
                    };
                }).ToList()
            };

            return View(model);
        }

        // ==========================================
        // 2. CHI TIẾT ĐƠN HÀNG
        // ==========================================
        public ActionResult Details(int id)
        {
            if (!CurrentUserId.HasValue)
                return RequireLogin();

            int userId = CurrentUserId.Value;

            var order = db.DonHang
                .AsNoTracking()
                .Include(x => x.ChiTietDonHang)
                .Include(x => x.ThanhToan)
                .Include(x => x.YeuCauHuyDon)
                .FirstOrDefault(x =>
                    x.MaDonHang == id &&
                    x.MaNguoiDung == userId);

            if (order == null)
                return HttpNotFound("Không tìm thấy đơn hàng.");

            var model = new OrderDetailViewModel
            {
                MaDonHang = order.MaDonHang,
                MaSoDonHang = order.MaSoDonHang,
                NgayDat = order.NgayDat,

                TenNguoiNhan = order.TenNguoiNhan,
                SoDienThoaiNhan = order.SoDienThoaiNhan,
                DiaChiNhanHang = order.DiaChiNhanHang,
                GhiChu = order.GhiChu,

                TienHang = order.TienHang,
                TienGiamGia = order.TienGiamGia,
                PhiVanChuyen = order.PhiVanChuyen,
                TongTien = order.TongTien,
                TienCoc = order.TienCoc,
                TyLeDatCoc = order.TyLeDatCoc,

                TrangThai = order.TrangThai,
                TrangThaiThanhToan = order.TrangThaiThanhToan,

                ChiTiet = order.ChiTietDonHang.Select(x =>
                    new OrderDetailItemViewModel
                    {
                        TenSanPham = x.TenSanPhamLucDat,
                        SKU = x.SKULucDat,
                        DonGia = x.DonGia,
                        SoLuong = x.SoLuong,
                        ThanhTien = x.ThanhTien ?? 0,
                        HinhThucDat = x.HinhThucDat,
                        SoNgayThue = x.SoNgayThue,
                        CoFlightcase = x.CoFlightcase,
                        PhiFlightcase = x.PhiFlightcase
                    }).ToList(),

                ThanhToans = order.ThanhToan
                    .OrderByDescending(x => x.NgayTao)
                    .Select(x => new OrderPaymentViewModel
                    {
                        PhuongThuc = x.PhuongThuc,
                        SoTien = x.SoTien,
                        TrangThaiThanhToan = x.TrangThaiThanhToan,
                        MaGiaoDich = x.MaGiaoDich,
                        DonViThanhToan = x.DonViThanhToan,
                        NgayTao = x.NgayTao,
                        NgayThanhToan = x.NgayThanhToan
                    }).ToList(),

                YeuCauHuy = order.YeuCauHuyDon
                    .OrderByDescending(x => x.NgayYeuCau)
                    .Select(x => new OrderCancellationViewModel
                    {
                        LyDoHuy = x.LyDoHuy,
                        TrangThaiYeuCau = x.TrangThaiYeuCau,
                        GhiChuQuanTri = x.GhiChuQuanTri,
                        NgayYeuCau = x.NgayYeuCau,
                        NgayXuLy = x.NgayXuLy
                    }).ToList()
            };

            return View(model);
        }

        // ==========================================
        // 3. KIỂM TRA ĐIỀU KIỆN HỦY ĐƠN
        // ==========================================
        private bool CanRequestCancel(string status)
        {
            return status == "ChoXacNhan"
                || status == "DaXacNhan"
                || status == "DangChuanBi";
        }

        // ==========================================
        // 4. HIỂN THỊ FORM YÊU CẦU HỦY
        // ==========================================
        [HttpGet]
        public ActionResult RequestCancel(int id)
        {
            if (!CurrentUserId.HasValue)
                return RequireLogin();

            int userId = CurrentUserId.Value;

            var order = db.DonHang.FirstOrDefault(x =>
                x.MaDonHang == id &&
                x.MaNguoiDung == userId);

            if (order == null)
                return HttpNotFound("Không tìm thấy đơn hàng.");

            if (!CanRequestCancel(order.TrangThai))
            {
                TempData["OrderHistoryError"] =
                    "Đơn hàng hiện tại không thể gửi yêu cầu hủy.";

                return RedirectToAction("Details", new { id = id });
            }

            bool hasPending = db.YeuCauHuyDon.Any(x =>
                x.MaDonHang == id &&
                x.MaNguoiDung == userId &&
                x.TrangThaiYeuCau == "ChoXuLy");

            if (hasPending)
            {
                TempData["OrderHistoryError"] =
                    "Đơn hàng đã có yêu cầu hủy đang chờ xử lý.";

                return RedirectToAction("Details", new { id = id });
            }

            var model = new OrderCancelRequestViewModel
            {
                MaDonHang = order.MaDonHang,
                MaSoDonHang = order.MaSoDonHang
            };

            return View(model);
        }

        // ==========================================
        // 5. GỬI YÊU CẦU HỦY ĐƠN
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RequestCancel(OrderCancelRequestViewModel model)
        {
            if (!CurrentUserId.HasValue)
                return RequireLogin();

            int userId = CurrentUserId.Value;

            var order = db.DonHang
                .Include(x => x.YeuCauHuyDon)
                .FirstOrDefault(x =>
                    x.MaDonHang == model.MaDonHang &&
                    x.MaNguoiDung == userId);

            if (order == null)
                return HttpNotFound("Không tìm thấy đơn hàng.");

            if (!CanRequestCancel(order.TrangThai))
            {
                TempData["OrderHistoryError"] =
                    "Đơn hàng hiện tại không đủ điều kiện hủy.";

                return RedirectToAction("Details",
                    new { id = model.MaDonHang });
            }

            bool hasPending = order.YeuCauHuyDon.Any(x =>
                x.TrangThaiYeuCau == "ChoXuLy");

            if (hasPending)
            {
                TempData["OrderHistoryError"] =
                    "Đơn hàng đã có yêu cầu hủy đang chờ xử lý.";

                return RedirectToAction("Details",
                    new { id = model.MaDonHang });
            }

            if (!ModelState.IsValid)
            {
                model.MaSoDonHang = order.MaSoDonHang;
                return View(model);
            }

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var request = new YeuCauHuyDon
                    {
                        MaDonHang = order.MaDonHang,
                        MaNguoiDung = userId,
                        LyDoHuy = model.LyDoHuy.Trim(),
                        TrangThaiYeuCau = "ChoXuLy",
                        NgayYeuCau = DateTime.Now
                    };

                    db.YeuCauHuyDon.Add(request);

                    order.TrangThai = "ChoHuy";
                    order.NgayCapNhat = DateTime.Now;

                    db.SaveChanges();

                    transaction.Commit();

                    TempData["OrderHistorySuccess"] =
                        "Yêu cầu hủy đơn đã được gửi thành công. Vui lòng chờ quản trị viên xử lý.";

                    return RedirectToAction("Details",
                        new { id = order.MaDonHang });
                }
                catch
                {
                    transaction.Rollback();

                    ModelState.AddModelError("",
                        "Không thể gửi yêu cầu hủy. Vui lòng thử lại.");

                    model.MaSoDonHang = order.MaSoDonHang;

                    return View(model);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }
    }
}
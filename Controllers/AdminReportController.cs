
using STAGEFX_PRO.Controllers.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Wedside_TVS_SFX.Models;
using Wedside_TVS_SFX.ViewModels.Admin;
using System.Data.Entity;

namespace STAGEFX_PRO.Controllers
{
    [AdminAuthorize]
    public class AdminReportController : Controller
    {
        private readonly StageFxEntities db = new StageFxEntities();

        // ==========================================
        // TRANG BÁO CÁO
        // URL: /AdminReport/Reports
        // ==========================================
        public ActionResult Reports(DateTime? fromDate, DateTime? toDate)
        {
            DateTime today = DateTime.Today;

            DateTime startDate = fromDate ?? new DateTime(
                today.Year,
                today.Month,
                1
            );

            DateTime endDate = toDate ?? today;

            if (startDate > endDate)
            {
                ModelState.AddModelError(
                    "",
                    "Ngày bắt đầu không được lớn hơn ngày kết thúc."
                );

                startDate = new DateTime(today.Year, today.Month, 1);
                endDate = today;
            }

            // Ngày kết thúc được tính trọn ngày.
            DateTime endExclusive = endDate.Date.AddDays(1);

            var model = new AdminReportViewModel
            {
                FromDate = startDate.Date,
                ToDate = endDate.Date
            };

            // ==========================================
            // 1. LẤY ĐƠN HÀNG THEO KHOẢNG THỜI GIAN
            // ==========================================
            var orders = db.DonHang
                .AsNoTracking()
                .Where(x =>
                    x.NgayDat >= startDate.Date &&
                    x.NgayDat < endExclusive
                )
                .Select(x => new
                {
                    x.MaDonHang,
                    x.MaSoDonHang,
                    x.NgayDat,
                    x.TongTien,
                    x.TienCoc,
                    x.TrangThai,
                    x.TrangThaiThanhToan
                })
                .ToList();

            model.TotalOrders = orders.Count;

            model.ConfirmedOrders = orders.Count(x =>
                x.TrangThai == "DaXacNhan");

            model.CancelledOrders = orders.Count(x =>
                x.TrangThai == "DaHuy");

            model.RejectedOrders = orders.Count(x =>
                x.TrangThai == "TuChoi");

            var confirmedOrders = orders
                .Where(x => x.TrangThai == "DaXacNhan")
                .ToList();

            var confirmedOrderIds = confirmedOrders
                .Select(x => x.MaDonHang)
                .ToList();

            // ==========================================
            // 2. DOANH THU ĐƠN HÀNG HỢP LỆ
            // ==========================================
            model.TotalOrderRevenue = confirmedOrders.Sum(x => x.TongTien);

            model.AverageOrderValue =
                model.ConfirmedOrders > 0
                    ? model.TotalOrderRevenue / model.ConfirmedOrders
                    : 0;

            model.TotalDeposit = confirmedOrders.Sum(x => x.TienCoc);

            // ==========================================
            // 3. THỐNG KÊ THANH TOÁN
            // ==========================================
            var payments = db.ThanhToan
                .AsNoTracking()
                .Where(x =>
                    x.NgayTao >= startDate.Date &&
                    x.NgayTao < endExclusive
                )
                .Select(x => new
                {
                    x.MaDonHang,
                    x.SoTien,
                    x.TrangThaiThanhToan
                })
                .ToList();

            // Chỉ thống kê khoản đang chờ xử lý.
            model.PendingPayment = payments
                .Where(x => x.TrangThaiThanhToan == "ChoXuLy")
                .Sum(x => x.SoTien);

            // Chưa có mã trạng thái thanh toán thành công
            // được xác nhận trong dữ liệu hiện tại.
            model.TotalPaid = 0;

            model.PaymentStatuses = payments
                .GroupBy(x => x.TrangThaiThanhToan)
                .Select(g => new AdminReportStatusItemViewModel
                {
                    Status = g.Key,
                    DisplayName = GetPaymentStatusName(g.Key),
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            // ==========================================
            // 4. THỐNG KÊ TRẠNG THÁI ĐƠN HÀNG
            // ==========================================
            model.OrderStatuses = orders
                .GroupBy(x => x.TrangThai)
                .Select(g => new AdminReportStatusItemViewModel
                {
                    Status = g.Key,
                    DisplayName = GetOrderStatusName(g.Key),
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            // ==========================================
            // 5. BIỂU ĐỒ DOANH THU THEO NGÀY
            // Chỉ tính đơn đã xác nhận.
            // ==========================================
            model.RevenueChart = confirmedOrders
                .GroupBy(x => x.NgayDat.Date)
                .OrderBy(g => g.Key)
                .Select(g => new AdminReportChartItemViewModel
                {
                    Label = g.Key.ToString("dd/MM"),
                    Revenue = g.Sum(x => x.TongTien),
                    OrderCount = g.Count()
                })
                .ToList();

            // ==========================================
            // 6. SẢN PHẨM BÁN CHẠY
            // ==========================================
            if (confirmedOrderIds.Any())
            {
                var details = db.ChiTietDonHang
                    .AsNoTracking()
                    .Where(x => confirmedOrderIds.Contains(x.MaDonHang))
                    .Select(x => new
                    {
                        x.MaSanPham,
                        x.TenSanPhamLucDat,
                        x.SoLuong,
                        x.DonGia,
                        x.ThanhTien
                    })
                    .ToList();

                model.TotalProductsSold = details.Sum(x => x.SoLuong);

                model.TopProducts = details
                    .GroupBy(x => new
                    {
                        x.MaSanPham,
                        x.TenSanPhamLucDat
                    })
                    .Select(g => new AdminReportProductItemViewModel
                    {
                        MaSanPham = g.Key.MaSanPham,
                        TenSanPham = g.Key.TenSanPhamLucDat,
                        SoLuongBan = g.Sum(x => x.SoLuong),
                        TongDoanhThu = g.Sum(x =>
                            x.ThanhTien ?? (x.DonGia * x.SoLuong))
                    })
                    .OrderByDescending(x => x.SoLuongBan)
                    .Take(5)
                    .ToList();
            }

            return View("~/Views/Admin/Reports.cshtml", model);
        }

        // ==========================================
        // TÊN HIỂN THỊ TRẠNG THÁI ĐƠN HÀNG
        // ==========================================
        private string GetOrderStatusName(string status)
        {
            switch (status)
            {
                case "DaXacNhan":
                    return "Đã xác nhận";

                case "DaHuy":
                    return "Đã hủy";

                case "TuChoi":
                    return "Từ chối";

                default:
                    return string.IsNullOrWhiteSpace(status)
                        ? "Chưa xác định"
                        : status;
            }
        }

        // ==========================================
        // TÊN HIỂN THỊ TRẠNG THÁI THANH TOÁN
        // ==========================================
        private string GetPaymentStatusName(string status)
        {
            switch (status)
            {
                case "ChoXuLy":
                    return "Chờ xử lý";

                case "ChuaThanhToan":
                    return "Chưa thanh toán";

                default:
                    return string.IsNullOrWhiteSpace(status)
                        ? "Chưa xác định"
                        : status;
            }
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

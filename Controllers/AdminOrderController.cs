
using System;
using System.Linq;
using System.Data;
using System.Data.Entity;
using System.Web.Mvc;

using Wedside_TVS_SFX.Models;
using Wedside_TVS_SFX.ViewModels.Admin;
using STAGEFX_PRO.Controllers.Base;

namespace STAGEFX_PRO.Controllers
{
    [AdminAuthorize]
    public class AdminOrderController : Controller
    {
        private const int PAGE_SIZE = 10;

        // =====================================================
        // 1. DANH SÁCH ĐƠN HÀNG
        // =====================================================

        public ActionResult Orders(
            string search,
            string orderStatus,
            string paymentStatus,
            DateTime? dateFrom,
            DateTime? dateTo,
            int page = 1)
        {
            using (var db = new StageFxEntities())
            {
                IQueryable<DonHang> query =
                    db.DonHang.AsNoTracking();

                search = string.IsNullOrWhiteSpace(search)
                    ? null
                    : search.Trim();

                orderStatus = string.IsNullOrWhiteSpace(orderStatus)
                    ? null
                    : orderStatus.Trim();

                paymentStatus = string.IsNullOrWhiteSpace(paymentStatus)
                    ? null
                    : paymentStatus.Trim();

                var model = new AdminOrderListViewModel();

                // THỐNG KÊ
                model.TotalOrders = query.Count();

                model.PendingOrders = query.Count(
                    x => x.TrangThai == "ChoXacNhan");

                model.ProcessingOrders = query.Count(
                    x => x.TrangThai == "DaXacNhan" ||
                         x.TrangThai == "DangChuanBi" ||
                         x.TrangThai == "DangGiao");

                model.CompletedOrders = query.Count(
                    x => x.TrangThai == "HoanThanh");

                // TÌM KIẾM
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(x =>
                        (x.MaSoDonHang != null &&
                         x.MaSoDonHang.Contains(search)) ||

                        (x.TenNguoiNhan != null &&
                         x.TenNguoiNhan.Contains(search)) ||

                        (x.NguoiDung != null &&
                         x.NguoiDung.HoTen != null &&
                         x.NguoiDung.HoTen.Contains(search))
                    );
                }

                // LỌC TRẠNG THÁI ĐƠN HÀNG
                if (!string.IsNullOrWhiteSpace(orderStatus))
                {
                    query = query.Where(
                        x => x.TrangThai == orderStatus);
                }

                // LỌC TRẠNG THÁI THANH TOÁN
                if (!string.IsNullOrWhiteSpace(paymentStatus))
                {
                    query = query.Where(
                        x => x.TrangThaiThanhToan == paymentStatus);
                }

                // LỌC NGÀY ĐẶT
                if (dateFrom.HasValue)
                {
                    DateTime startDate = dateFrom.Value.Date;

                    query = query.Where(
                        x => x.NgayDat >= startDate);
                }

                if (dateTo.HasValue)
                {
                    DateTime endDate =
                        dateTo.Value.Date.AddDays(1);

                    query = query.Where(
                        x => x.NgayDat < endDate);
                }

                // PHÂN TRANG
                model.TotalItems = query.Count();
                model.PageSize = PAGE_SIZE;

                model.TotalPages = (int)Math.Ceiling(
                    (double)model.TotalItems / PAGE_SIZE);

                if (model.TotalPages < 1)
                    model.TotalPages = 1;

                if (page < 1)
                    page = 1;

                if (page > model.TotalPages)
                    page = model.TotalPages;

                model.CurrentPage = page;

                // LẤY DỮ LIỆU ĐƠN HÀNG
                model.Orders = query
                    .OrderByDescending(x => x.NgayDat)
                    .ThenByDescending(x => x.MaDonHang)
                    .Skip((page - 1) * PAGE_SIZE)
                    .Take(PAGE_SIZE)
                    .Select(x => new AdminOrderItemViewModel
                    {
                        MaDonHang = x.MaDonHang,
                        MaSoDonHang = x.MaSoDonHang,

                        CustomerName = x.NguoiDung != null
                            ? x.NguoiDung.HoTen
                            : null,

                        RecipientName = x.TenNguoiNhan,
                        NgayDat = x.NgayDat,

                        TongTien = x.TongTien,
                        TienCoc = x.TienCoc,

                        TrangThai = x.TrangThai,
                        TrangThaiThanhToan =
                            x.TrangThaiThanhToan
                    })
                    .ToList();

                foreach (var item in model.Orders)
                {
                    SetOrderStatus(item);
                    SetPaymentStatus(item);
                }

                // GIỮ ĐIỀU KIỆN LỌC
                model.SearchTerm = search;
                model.OrderStatus = orderStatus;
                model.PaymentStatus = paymentStatus;
                model.DateFrom = dateFrom;
                model.DateTo = dateTo;

                ViewBag.Title = "Quản lý đơn hàng";

                return View(
                    "~/Views/Admin/Orders.cshtml",
                    model
                );
            }
        }

        // =====================================================
        // 2. CHI TIẾT ĐƠN HÀNG
        // =====================================================

        public ActionResult OrderDetail(int id)
        {
            if (id <= 0)
                return HttpNotFound();

            using (var db = new StageFxEntities())
            {
                var order = db.DonHang
                    .AsNoTracking()
                    .Include("NguoiDung")
                    .Include("ChiTietDonHang")
                    .Include("ThanhToan")
                    .FirstOrDefault(x => x.MaDonHang == id);

                if (order == null)
                    return HttpNotFound();

                var model = new AdminOrderDetailViewModel
                {
                    MaDonHang = order.MaDonHang,
                    MaSoDonHang = order.MaSoDonHang,

                    CustomerName = order.NguoiDung != null
                        ? order.NguoiDung.HoTen
                        : "Không xác định",

                    CustomerEmail = order.NguoiDung != null
                        ? order.NguoiDung.Email
                        : "",

                    CustomerPhone = order.NguoiDung != null
                        ? order.NguoiDung.SoDienThoai
                        : "",

                    RecipientName = order.TenNguoiNhan,
                    RecipientPhone = order.SoDienThoaiNhan,
                    ShippingAddress = order.DiaChiNhanHang,

                    NgayDat = order.NgayDat,

                    TienHang = order.TienHang,
                    TienGiamGia = order.TienGiamGia,
                    PhiVanChuyen = order.PhiVanChuyen,
                    TongTien = order.TongTien,
                    TienCoc = order.TienCoc,

                    TrangThai = order.TrangThai,
                    TrangThaiThanhToan =
                        order.TrangThaiThanhToan,

                    GhiChu = order.GhiChu
                };

                // TRẠNG THÁI
                var statusItem = new AdminOrderItemViewModel
                {
                    TrangThai = order.TrangThai,
                    TrangThaiThanhToan =
                        order.TrangThaiThanhToan
                };

                SetOrderStatus(statusItem);
                SetPaymentStatus(statusItem);

                model.OrderStatusText =
                    statusItem.OrderStatusText;

                model.OrderStatusClass =
                    statusItem.OrderStatusClass;

                model.PaymentStatusText =
                    statusItem.PaymentStatusText;

                model.PaymentStatusClass =
                    statusItem.PaymentStatusClass;

                // SẢN PHẨM
                model.Products = order.ChiTietDonHang
                    .OrderBy(x => x.MaChiTietDonHang)
                    .Select(x => new AdminOrderDetailProductViewModel
                    {
                        MaChiTietDonHang = x.MaChiTietDonHang,

                        TenSanPham = x.TenSanPhamLucDat,
                        SKU = x.SKULucDat,

                        DonGia = x.DonGia,
                        SoLuong = x.SoLuong,
                        ThanhTien = x.ThanhTien,

                        HinhThucDat = x.HinhThucDat,
                        SoNgayThue = x.SoNgayThue,

                        CoFlightcase = x.CoFlightcase,
                        PhiFlightcase = x.PhiFlightcase
                    })
                    .ToList();

                // LỊCH SỬ THANH TOÁN
                model.Payments = order.ThanhToan
                    .OrderByDescending(x => x.NgayTao)
                    .Select(x => new AdminOrderPaymentViewModel
                    {
                        MaThanhToan = x.MaThanhToan,

                        PhuongThuc = x.PhuongThuc,
                        SoTien = x.SoTien,

                        TrangThaiThanhToan =
                            x.TrangThaiThanhToan,

                        MaGiaoDich = x.MaGiaoDich,
                        DonViThanhToan = x.DonViThanhToan,

                        NgayThanhToan = x.NgayThanhToan,
                        NgayTao = x.NgayTao
                    })
                    .ToList();

                foreach (var payment in model.Payments)
                {
                    SetPaymentRecordStatus(payment);
                }

                ViewBag.Title = "Chi tiết đơn hàng";

                return View(
                    "~/Views/Admin/OrderDetail.cshtml",
                    model
                );
            }
        }

        // =====================================================
        // 3. XÁC NHẬN ĐƠN HÀNG
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmOrder(int orderId)
        {
            return UpdateOrderStatus(
                orderId,
                "DaXacNhan",
                "Admin xác nhận đơn hàng."
            );
        }

        // =====================================================
        // 4. TỪ CHỐI ĐƠN HÀNG
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RejectOrder(int orderId, string reason)
        {
            reason = string.IsNullOrWhiteSpace(reason)
                ? null
                : reason.Trim();

            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["OrderError"] =
                    "Vui lòng nhập lý do từ chối đơn hàng.";

                return RedirectToAction("Orders");
            }

            return UpdateOrderStatus(
                orderId,
                "TuChoi",
                reason
            );
        }

        // =====================================================
        // 5. XỬ LÝ CẬP NHẬT TRẠNG THÁI VÀ GHI LỊCH SỬ
        // =====================================================

        private ActionResult UpdateOrderStatus(
            int orderId,
            string newStatus,
            string reason)
        {
            if (orderId <= 0)
                return HttpNotFound();

            // Kiểm tra Session Admin
            if (Session["MaNguoiDung"] == null)
                return new HttpStatusCodeResult(403);

            int adminId;

            if (!int.TryParse(
                Convert.ToString(Session["MaNguoiDung"]),
                out adminId) || adminId <= 0)
            {
                return new HttpStatusCodeResult(403);
            }

            try
            {
                using (var db = new StageFxEntities())
                using (var transaction =
                    db.Database.BeginTransaction(
                        IsolationLevel.Serializable))
                {
                    // LẤY ĐƠN HÀNG
                    var order = db.DonHang.FirstOrDefault(
                        x => x.MaDonHang == orderId);

                    if (order == null)
                        return HttpNotFound();

                    // CHỈ CHO PHÉP XỬ LÝ ĐƠN CHỜ XÁC NHẬN
                    if (order.TrangThai != "ChoXacNhan")
                    {
                        TempData["OrderError"] =
                            "Đơn hàng không còn ở trạng thái chờ xác nhận. " +
                            "Vui lòng tải lại danh sách.";

                        return RedirectToAction("Orders");
                    }

                    DateTime now = DateTime.Now;
                    string oldStatus = order.TrangThai;

                    // CẬP NHẬT ĐƠN HÀNG
                    order.TrangThai = newStatus;
                    order.NgayCapNhat = now;

                    // GHI LỊCH SỬ
                    var history = new LichSuTrangThaiDonHang
                    {
                        MaDonHang = order.MaDonHang,
                        MaNguoiDungThayDoi = adminId,

                        TrangThaiCu = oldStatus,
                        TrangThaiMoi = newStatus,

                        GhiChu = reason,
                        NgayThayDoi = now
                    };

                    db.Set<LichSuTrangThaiDonHang>().Add(history);

                    // LƯU CẢ HAI THAY ĐỔI
                    db.SaveChanges();

                    // HOÀN TẤT TRANSACTION
                    transaction.Commit();

                    TempData["OrderSuccess"] =
                        newStatus == "DaXacNhan"
                            ? "Đã xác nhận đơn hàng thành công."
                            : "Đã từ chối đơn hàng thành công.";
                }
            }
            catch (Exception)
            {
                TempData["OrderError"] =
                    "Không thể cập nhật đơn hàng. " +
                    "Vui lòng kiểm tra dữ liệu và thử lại.";
            }

            return RedirectToAction("Orders");
        }

        // =====================================================
        // 6. CHUYỂN MÃ TRẠNG THÁI ĐƠN HÀNG
        // =====================================================

        private void SetOrderStatus(
            AdminOrderItemViewModel item)
        {
            switch (item.TrangThai)
            {
                case "ChoXacNhan":
                    item.OrderStatusText = "Chờ xác nhận";
                    item.OrderStatusClass = "pending";
                    break;

                case "DaXacNhan":
                    item.OrderStatusText = "Đã xác nhận";
                    item.OrderStatusClass = "confirmed";
                    break;

                case "DangChuanBi":
                    item.OrderStatusText = "Đang chuẩn bị";
                    item.OrderStatusClass = "processing";
                    break;

                case "DangGiao":
                    item.OrderStatusText = "Đang giao";
                    item.OrderStatusClass = "shipping";
                    break;

                case "HoanThanh":
                    item.OrderStatusText = "Hoàn thành";
                    item.OrderStatusClass = "completed";
                    break;

                case "DaHuy":
                    item.OrderStatusText = "Đã hủy";
                    item.OrderStatusClass = "cancelled";
                    break;

                case "ChoHuy":
                    item.OrderStatusText = "Chờ hủy";
                    item.OrderStatusClass = "pending";
                    break;

                case "TuChoi":
                    item.OrderStatusText = "Từ chối";
                    item.OrderStatusClass = "cancelled";
                    break;

                default:
                    item.OrderStatusText =
                        item.TrangThai ?? "Không xác định";

                    item.OrderStatusClass = "unknown";
                    break;
            }
        }

        // =====================================================
        // 7. CHUYỂN MÃ TRẠNG THÁI THANH TOÁN ĐƠN HÀNG
        // =====================================================

        private void SetPaymentStatus(
            AdminOrderItemViewModel item)
        {
            switch (item.TrangThaiThanhToan)
            {
                case "DaThanhToan":
                    item.PaymentStatusText = "Đã thanh toán";
                    item.PaymentStatusClass = "completed";
                    break;

                case "ChoThanhToan":
                    item.PaymentStatusText = "Chờ thanh toán";
                    item.PaymentStatusClass = "pending";
                    break;

                case "ChuaThanhToan":
                    item.PaymentStatusText = "Chưa thanh toán";
                    item.PaymentStatusClass = "unpaid";
                    break;

                case "ChoXuLy":
                    item.PaymentStatusText = "Chờ xử lý";
                    item.PaymentStatusClass = "processing";
                    break;

                case "ThatBai":
                    item.PaymentStatusText = "Thất bại";
                    item.PaymentStatusClass = "cancelled";
                    break;

                case "DaHoanTien":
                    item.PaymentStatusText = "Đã hoàn tiền";
                    item.PaymentStatusClass = "refunded";
                    break;

                default:
                    item.PaymentStatusText =
                        item.TrangThaiThanhToan ?? "Không xác định";

                    item.PaymentStatusClass = "unknown";
                    break;
            }
        }

        // =====================================================
        // 8. CHUYỂN MÃ TRẠNG THÁI TỪNG GIAO DỊCH
        // =====================================================

        private void SetPaymentRecordStatus(
            AdminOrderPaymentViewModel item)
        {
            switch (item.TrangThaiThanhToan)
            {
                case "DaThanhToan":
                    item.PaymentStatusText = "Đã thanh toán";
                    item.PaymentStatusClass = "completed";
                    break;

                case "ChoThanhToan":
                    item.PaymentStatusText = "Chờ thanh toán";
                    item.PaymentStatusClass = "pending";
                    break;

                case "ChuaThanhToan":
                    item.PaymentStatusText = "Chưa thanh toán";
                    item.PaymentStatusClass = "unpaid";
                    break;

                case "ChoXuLy":
                    item.PaymentStatusText = "Chờ xử lý";
                    item.PaymentStatusClass = "processing";
                    break;

                case "ThatBai":
                    item.PaymentStatusText = "Thất bại";
                    item.PaymentStatusClass = "cancelled";
                    break;

                case "DaHoanTien":
                    item.PaymentStatusText = "Đã hoàn tiền";
                    item.PaymentStatusClass = "refunded";
                    break;

                default:
                    item.PaymentStatusText =
                        item.TrangThaiThanhToan ?? "Không xác định";

                    item.PaymentStatusClass = "unknown";
                    break;
            }
        }
    }
}

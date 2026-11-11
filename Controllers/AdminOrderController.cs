using System;
using System.Collections.Generic;
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

        private static readonly IDictionary<string, string[]> AllowedTransitions =
            new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                { "ChoXacNhan", new[] { "DaXacNhan", "TuChoi" } },
                { "DaXacNhan", new[] { "DangChuanBi", "ChoHuy" } },
                { "DangChuanBi", new[] { "DangGiao", "ChoHuy" } },
                { "DangGiao", new[] { "HoanThanh", "ChoHuy" } },
                { "ChoHuy", new string[0] },
                { "HoanThanh", new string[0] },
                { "DaHuy", new string[0] },
                { "TuChoi", new string[0] }
            };

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

                model.AllowedStatusTransitions = GetAllowedStatusItems(order.TrangThai);
                model.StatusHistory = db.LichSuTrangThaiDonHang
                    .AsNoTracking()
                    .Where(x => x.MaDonHang == id)
                    .OrderByDescending(x => x.NgayThayDoi)
                    .Select(x => new AdminOrderStatusHistoryViewModel
                    {
                        TrangThaiCu = x.TrangThaiCu,
                        TrangThaiMoi = x.TrangThaiMoi,
                        NguoiThucHien = x.NguoiDung == null ? "Không xác định" : x.NguoiDung.HoTen,
                        NgayThayDoi = x.NgayThayDoi,
                        GhiChu = x.GhiChu
                    })
                    .ToList();

                foreach (var history in model.StatusHistory)
                {
                    history.TrangThaiCuText = GetOrderStatusText(history.TrangThaiCu);
                    history.TrangThaiMoiText = GetOrderStatusText(history.TrangThaiMoi);
                }

                var cancellation = db.YeuCauHuyDon
                    .AsNoTracking()
                    .Where(x => x.MaDonHang == id)
                    .OrderByDescending(x => x.NgayYeuCau)
                    .Select(x => new AdminOrderCancellationViewModel
                    {
                        MaYeuCauHuy = x.MaYeuCauHuy,
                        LyDoHuy = x.LyDoHuy,
                        TrangThaiYeuCau = x.TrangThaiYeuCau,
                        GhiChuQuanTri = x.GhiChuQuanTri,
                        NgayYeuCau = x.NgayYeuCau,
                        NgayXuLy = x.NgayXuLy
                    })
                    .FirstOrDefault();
                model.CancellationRequest = cancellation;

                ViewBag.Title = "Chi tiết đơn hàng";

                return View(
                    "~/Views/Admin/OrderDetail.cshtml",
                    model
                );
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(int orderId, string newStatus, string note)
        {
            if (!IsKnownStatus(newStatus))
            {
                TempData["OrderError"] = "Trạng thái cập nhật không hợp lệ.";
                return RedirectToAction("OrderDetail", new { id = orderId });
            }

            return UpdateOrderStatus(orderId, newStatus, note);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ApproveCancellation(int orderId, int requestId, string note)
        {
            return ProcessCancellation(orderId, requestId, true, note);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RejectCancellation(int orderId, int requestId, string note)
        {
            return ProcessCancellation(orderId, requestId, false, note);
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
                    var order = db.DonHang.FirstOrDefault(
                        x => x.MaDonHang == orderId);

                    if (order == null)
                        return HttpNotFound();

                    string[] transitions;
                    if (!AllowedTransitions.TryGetValue(order.TrangThai ?? "", out transitions) ||
                        !transitions.Contains(newStatus, StringComparer.OrdinalIgnoreCase))
                    {
                        TempData["OrderError"] =
                            "Không thể chuyển trạng thái theo quy trình hiện tại.";

                        return RedirectToAction("OrderDetail", new { id = orderId });
                    }

                    DateTime now = DateTime.Now;
                    string oldStatus = order.TrangThai;

                    order.TrangThai = newStatus;
                    order.NgayCapNhat = now;

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

                    db.SaveChanges();

                    transaction.Commit();

                    TempData["OrderSuccess"] = "Đã cập nhật trạng thái đơn hàng thành công.";
                }
            }
            catch (Exception)
            {
                TempData["OrderError"] =
                    "Không thể cập nhật đơn hàng. " +
                    "Vui lòng kiểm tra dữ liệu và thử lại.";
            }

            return RedirectToAction("OrderDetail", new { id = orderId });
        }

        private ActionResult ProcessCancellation(int orderId, int requestId, bool approve, string note)
        {
            int adminId;
            if (!TryGetAdminId(out adminId))
                return new HttpStatusCodeResult(403);

            using (var db = new StageFxEntities())
            using (var transaction = db.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                var request = db.YeuCauHuyDon.FirstOrDefault(x =>
                    x.MaYeuCauHuy == requestId && x.MaDonHang == orderId);
                var order = db.DonHang.FirstOrDefault(x => x.MaDonHang == orderId);

                if (request == null || order == null)
                    return HttpNotFound();

                if (!IsPendingCancellation(request.TrangThaiYeuCau))
                {
                    TempData["OrderError"] = "Yêu cầu hủy đơn này đã được xử lý trước đó.";
                    return RedirectToAction("OrderDetail", new { id = orderId });
                }

                DateTime now = DateTime.Now;
                string oldStatus = order.TrangThai;
                string adminNote = string.IsNullOrWhiteSpace(note) ? null : note.Trim();

                if (approve)
                {
                    if (order.TrangThai != "ChoHuy")
                    {
                        TempData["OrderError"] = "Chỉ yêu cầu hủy đang chờ xử lý mới được chấp nhận.";
                        return RedirectToAction("OrderDetail", new { id = orderId });
                    }
                    order.TrangThai = "DaHuy";
                    request.TrangThaiYeuCau = "DaChapNhan";
                }
                else
                {
                    request.TrangThaiYeuCau = "TuChoi";
                }

                request.GhiChuQuanTri = adminNote;
                request.NgayXuLy = now;
                request.MaNguoiXuLy = adminId;

                if (approve)
                {
                    order.NgayCapNhat = now;
                    db.LichSuTrangThaiDonHang.Add(new LichSuTrangThaiDonHang
                    {
                        MaDonHang = orderId,
                        MaNguoiDungThayDoi = adminId,
                        TrangThaiCu = oldStatus,
                        TrangThaiMoi = "DaHuy",
                        GhiChu = adminNote ?? "Admin chấp nhận yêu cầu hủy đơn.",
                        NgayThayDoi = now
                    });
                }

                db.SaveChanges();
                transaction.Commit();
                TempData["OrderSuccess"] = approve
                    ? "Đã chấp nhận yêu cầu hủy và chuyển đơn sang đã hủy."
                    : "Đã từ chối yêu cầu hủy đơn.";
            }

            return RedirectToAction("OrderDetail", new { id = orderId });
        }

        private bool TryGetAdminId(out int adminId)
        {
            return int.TryParse(Convert.ToString(Session["MaNguoiDung"]), out adminId) && adminId > 0;
        }

        private static bool IsPendingCancellation(string status)
        {
            return string.Equals(status, "ChoXuLy", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(status, "ChoDuyet", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsKnownStatus(string status)
        {
            return !string.IsNullOrWhiteSpace(status) &&
                   AllowedTransitions.ContainsKey(status);
        }

        private List<SelectListItem> GetAllowedStatusItems(string currentStatus)
        {
            string[] transitions;
            if (!AllowedTransitions.TryGetValue(currentStatus ?? "", out transitions))
                return new List<SelectListItem>();

            return transitions.Select(x => new SelectListItem
            {
                Value = x,
                Text = GetOrderStatusText(x)
            }).ToList();
        }

        private string GetOrderStatusText(string status)
        {
            var item = new AdminOrderItemViewModel { TrangThai = status };
            SetOrderStatus(item);
            return item.OrderStatusText;
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
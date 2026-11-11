using System;
using System.Collections.Generic;

namespace Wedside_TVS_SFX.ViewModels.Admin
{
    public class AdminOrderDetailViewModel
    {
        public int MaDonHang { get; set; }
        public string MaSoDonHang { get; set; }

        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }

        public string RecipientName { get; set; }
        public string RecipientPhone { get; set; }
        public string ShippingAddress { get; set; }

        public DateTime NgayDat { get; set; }

        public decimal TienHang { get; set; }
        public decimal TienGiamGia { get; set; }
        public decimal PhiVanChuyen { get; set; }
        public decimal TongTien { get; set; }
        public decimal TienCoc { get; set; }

        public string TrangThai { get; set; }
        public string TrangThaiThanhToan { get; set; }

        public string OrderStatusText { get; set; }
        public string OrderStatusClass { get; set; }

        public string PaymentStatusText { get; set; }
        public string PaymentStatusClass { get; set; }

        public string GhiChu { get; set; }

<<<<<<< HEAD
        public List<System.Web.Mvc.SelectListItem> AllowedStatusTransitions { get; set; }
            = new List<System.Web.Mvc.SelectListItem>();

        public List<AdminOrderStatusHistoryViewModel> StatusHistory { get; set; }
            = new List<AdminOrderStatusHistoryViewModel>();

        public AdminOrderCancellationViewModel CancellationRequest { get; set; }

=======
>>>>>>> 8dd10d954939e9e3bea89f79058d14c043feda13
        public List<AdminOrderDetailProductViewModel> Products { get; set; }
            = new List<AdminOrderDetailProductViewModel>();

        public List<AdminOrderPaymentViewModel> Payments { get; set; }
            = new List<AdminOrderPaymentViewModel>();
    }

<<<<<<< HEAD
    public class AdminOrderStatusHistoryViewModel
    {
        public string TrangThaiCu { get; set; }
        public string TrangThaiMoi { get; set; }
        public string NguoiThucHien { get; set; }
        public DateTime NgayThayDoi { get; set; }
        public string GhiChu { get; set; }
        public string TrangThaiCuText { get; set; }
        public string TrangThaiMoiText { get; set; }
    }

    public class AdminOrderCancellationViewModel
    {
        public int MaYeuCauHuy { get; set; }
        public string LyDoHuy { get; set; }
        public string TrangThaiYeuCau { get; set; }
        public string GhiChuQuanTri { get; set; }
        public DateTime NgayYeuCau { get; set; }
        public DateTime? NgayXuLy { get; set; }
    }

=======
>>>>>>> 8dd10d954939e9e3bea89f79058d14c043feda13
    public class AdminOrderDetailProductViewModel
    {
        public int MaChiTietDonHang { get; set; }

        public string TenSanPham { get; set; }
        public string SKU { get; set; }

        public decimal DonGia { get; set; }
        public int SoLuong { get; set; }
        public decimal? ThanhTien { get; set; }

        public string HinhThucDat { get; set; }
        public int? SoNgayThue { get; set; }

        public bool CoFlightcase { get; set; }
        public decimal PhiFlightcase { get; set; }
    }

    public class AdminOrderPaymentViewModel
    {
        public int MaThanhToan { get; set; }

        public string PhuongThuc { get; set; }
        public decimal SoTien { get; set; }

        public string TrangThaiThanhToan { get; set; }
        public string PaymentStatusText { get; set; }
        public string PaymentStatusClass { get; set; }

        public string MaGiaoDich { get; set; }
        public string DonViThanhToan { get; set; }

        public DateTime? NgayThanhToan { get; set; }
        public DateTime NgayTao { get; set; }
    }
}
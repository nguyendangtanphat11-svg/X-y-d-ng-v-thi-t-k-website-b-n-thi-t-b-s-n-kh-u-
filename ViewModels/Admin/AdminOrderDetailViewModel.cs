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

        public List<AdminOrderDetailProductViewModel> Products { get; set; }
            = new List<AdminOrderDetailProductViewModel>();

        public List<AdminOrderPaymentViewModel> Payments { get; set; }
            = new List<AdminOrderPaymentViewModel>();
    }

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
using System;
using System.Collections.Generic;

namespace Wedside_TVS_SFX.ViewModels
{
    public class OrderDetailViewModel
    {
        public int MaDonHang { get; set; }

        public string MaSoDonHang { get; set; }

        public DateTime NgayDat { get; set; }

        public string TenNguoiNhan { get; set; }

        public string SoDienThoaiNhan { get; set; }

        public string DiaChiNhanHang { get; set; }

        public string GhiChu { get; set; }

        public decimal TienHang { get; set; }

        public decimal TienGiamGia { get; set; }

        public decimal PhiVanChuyen { get; set; }

        public decimal TongTien { get; set; }

        public decimal TienCoc { get; set; }

        public decimal TyLeDatCoc { get; set; }

        public string TrangThai { get; set; }

        public string TrangThaiThanhToan { get; set; }

        public List<OrderDetailItemViewModel> ChiTiet { get; set; }
            = new List<OrderDetailItemViewModel>();

        public List<OrderPaymentViewModel> ThanhToans { get; set; }
            = new List<OrderPaymentViewModel>();

        public List<OrderCancellationViewModel> YeuCauHuy { get; set; }
            = new List<OrderCancellationViewModel>();
    }

    public class OrderDetailItemViewModel
    {
        public string TenSanPham { get; set; }

        public string SKU { get; set; }

        public decimal DonGia { get; set; }

        public int SoLuong { get; set; }

        public decimal ThanhTien { get; set; }

        public string HinhThucDat { get; set; }

        public int? SoNgayThue { get; set; }

        public bool CoFlightcase { get; set; }

        public decimal PhiFlightcase { get; set; }
    }

    public class OrderPaymentViewModel
    {
        public string PhuongThuc { get; set; }

        public decimal SoTien { get; set; }

        public string TrangThaiThanhToan { get; set; }

        public string MaGiaoDich { get; set; }

        public string DonViThanhToan { get; set; }

        public DateTime NgayTao { get; set; }

        public DateTime? NgayThanhToan { get; set; }
    }

    public class OrderCancellationViewModel
    {
        public string LyDoHuy { get; set; }

        public string TrangThaiYeuCau { get; set; }

        public string GhiChuQuanTri { get; set; }

        public DateTime NgayYeuCau { get; set; }

        public DateTime? NgayXuLy { get; set; }
    }
}
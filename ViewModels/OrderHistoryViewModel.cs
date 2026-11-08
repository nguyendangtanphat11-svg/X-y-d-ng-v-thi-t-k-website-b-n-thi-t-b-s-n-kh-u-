using System;
using System.Collections.Generic;

namespace Wedside_TVS_SFX.ViewModels
{
    public class OrderHistoryViewModel
    {
        public string TrangThaiLoc { get; set; }

        public int TongSoDon { get; set; }

        public List<OrderHistoryItemViewModel> DonHangs { get; set; }
            = new List<OrderHistoryItemViewModel>();
    }

    public class OrderHistoryItemViewModel
    {
        public int MaDonHang { get; set; }

        public string MaSoDonHang { get; set; }

        public DateTime NgayDat { get; set; }

        public decimal TongTien { get; set; }

        public decimal TienCoc { get; set; }

        public string TrangThai { get; set; }

        public string TrangThaiThanhToan { get; set; }

        public int SoLuongSanPham { get; set; }

        public string PhuongThucThanhToan { get; set; }

        public string TrangThaiYeuCauHuy { get; set; }
    }
}
using System;
using System;
using System.Collections.Generic;

namespace Wedside_TVS_SFX.ViewModels.Admin
{
    public class AdminDashboardViewModel
    {
        public AdminDashboardViewModel()
        {
            OrderStatusStatistics = new AdminOrderStatusStatistics();
            RecentOrders = new List<AdminRecentOrderViewModel>();
            TopProducts = new List<AdminTopProductViewModel>();
        }

        public int TotalOrders { get; set; }

        public int PendingOrders { get; set; }

        // Ch? t�nh t�i kho?n kh�ch h�ng ?ang ho?t ??ng (MaVaiTro = 3, TrangThai = true).
        public int TotalCustomers { get; set; }

        public int TotalProducts { get; set; }

        public AdminOrderStatusStatistics OrderStatusStatistics { get; set; }

        public IList<AdminRecentOrderViewModel> RecentOrders { get; set; }

        public IList<AdminTopProductViewModel> TopProducts { get; set; }
    }

    public class AdminOrderStatusStatistics
    {
        public int ChoXacNhan { get; set; }
        public int DaXacNhan { get; set; }
        public int DangChuanBi { get; set; }
        public int DangGiao { get; set; }
        public int HoanThanh { get; set; }
        public int DaHuy { get; set; }
        public int ChoHuy { get; set; }
        public int TuChoi { get; set; }
    }

    public class AdminRecentOrderViewModel
    {
        public int MaDonHang { get; set; }
        public string MaSoDonHang { get; set; }
        public string TenNguoiNhan { get; set; }
        public DateTime NgayDat { get; set; }
        public decimal TongTien { get; set; }
        public string TrangThai { get; set; }
        public string TrangThaiThanhToan { get; set; }
    }

    public class AdminTopProductViewModel
    {
        public int MaSanPham { get; set; }
        public string TenSanPham { get; set; }
        public string SKU { get; set; }
        public int SoLuong { get; set; }

        // ?�y l� t?ng gi� tr? d�ng h�ng, kh�ng ph?i doanh thu ?� thu ti?n.
        public decimal GiaTriDonHang { get; set; }
    }
}

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

<<<<<<< HEAD
        // Ch? tính tài kho?n khách hàng ?ang ho?t ??ng (MaVaiTro = 3, TrangThai = true).
=======
        // Ch? tï¿½nh tï¿½i kho?n khï¿½ch hï¿½ng ?ang ho?t ??ng (MaVaiTro = 3, TrangThai = true).
>>>>>>> 8dd10d954939e9e3bea89f79058d14c043feda13
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

<<<<<<< HEAD
        // ?ây là t?ng giá tr? dòng hàng, không ph?i doanh thu ?ã thu ti?n.
=======
        // ?ï¿½y lï¿½ t?ng giï¿½ tr? dï¿½ng hï¿½ng, khï¿½ng ph?i doanh thu ?ï¿½ thu ti?n.
>>>>>>> 8dd10d954939e9e3bea89f79058d14c043feda13
        public decimal GiaTriDonHang { get; set; }
    }
}

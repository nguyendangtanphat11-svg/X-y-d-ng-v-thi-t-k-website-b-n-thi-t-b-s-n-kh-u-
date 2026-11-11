using System;
using System.Collections.Generic;

namespace Wedside_TVS_SFX.ViewModels.Admin
{
    public class AdminProductDetailViewModel
    {
        public int MaSanPham { get; set; }

        public string SKU { get; set; }

        public string TenSanPham { get; set; }

        public string TenDanhMuc { get; set; }

        public string MoTaNgan { get; set; }

        public string MoTaChiTiet { get; set; }

        public string ThongSoKyThuat { get; set; }

        public decimal GiaBan { get; set; }

        public decimal? GiaThueNgay { get; set; }

        public int SoLuongTon { get; set; }

        public bool TrangThai { get; set; }

        public DateTime NgayTao { get; set; }

        public DateTime? NgayCapNhat { get; set; }

        public IList<AdminProductImageViewModel> Images { get; set; }
            = new List<AdminProductImageViewModel>();

        // Sử dụng chung lớp AdminInventoryHistoryViewModel từ AdminInventoryViewModel.cs
        public IList<AdminInventoryHistoryViewModel> InventoryHistory { get; set; }
            = new List<AdminInventoryHistoryViewModel>();
    }
}
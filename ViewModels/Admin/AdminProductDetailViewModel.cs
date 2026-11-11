<<<<<<< HEAD

=======
>>>>>>> 8dd10d954939e9e3bea89f79058d14c043feda13
using System;
using System.Collections.Generic;

namespace Wedside_TVS_SFX.ViewModels.Admin
{
    public class AdminProductDetailViewModel
    {
        public int MaSanPham { get; set; }
<<<<<<< HEAD

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

        public IList<AdminProductInventoryHistoryViewModel> InventoryHistory { get; set; }
    }

    // Lich su ton kho trong chi tiet san pham
    public class AdminProductInventoryHistoryViewModel
    {
        public DateTime NgayTao { get; set; }

        public string LoaiGiaoDich { get; set; }

        public int SoLuong { get; set; }

        public int TonTruoc { get; set; }

        public int TonSau { get; set; }

=======
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
        public IList<AdminInventoryHistoryViewModel> InventoryHistory { get; set; }
    }

    public class AdminInventoryHistoryViewModel
    {
        public DateTime NgayTao { get; set; }
        public string LoaiGiaoDich { get; set; }
        public int SoLuong { get; set; }
        public int TonTruoc { get; set; }
        public int TonSau { get; set; }
>>>>>>> 8dd10d954939e9e3bea89f79058d14c043feda13
        public string GhiChu { get; set; }
    }
}

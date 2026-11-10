using System.Collections.Generic;
using System.Linq;

namespace Wedside_TVS_SFX.Models
{
    public class CartItemViewModel
    {
        public int MaChiTietGioHang { get; set; }
        public int MaSanPham { get; set; }

        public string SKU { get; set; }
        public string TenSanPham { get; set; }

        public int SoLuong { get; set; }

        public string HinhThucDat { get; set; }
        public int? SoNgayThue { get; set; }

        public bool CoFlightcase { get; set; }

        public decimal GiaCoBan { get; set; }
        public decimal PhiFlightcaseDonVi { get; set; }
        public decimal ThanhTien { get; set; }
    }

    public class CartIndexViewModel
    {
        public List<CartItemViewModel> Items { get; set; }

        public decimal TienHang { get; set; }
        public int TongSoLuong { get; set; }

        public CartIndexViewModel()
        {
            Items = new List<CartItemViewModel>();
        }
    }
}
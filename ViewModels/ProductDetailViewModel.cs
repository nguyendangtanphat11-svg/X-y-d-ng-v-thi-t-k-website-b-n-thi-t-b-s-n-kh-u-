using System.Collections.Generic;

namespace Wedside_TVS_SFX.ViewModels
{
    public class ProductDetailViewModel
    {
        public int MaSanPham { get; set; }
        public string SKU { get; set; }
        public string TenSanPham { get; set; }
        public string MoTaNgan { get; set; }
        public string MoTaChiTiet { get; set; }
        public string ThongSoKyThuat { get; set; }

        public decimal? GiaBan { get; set; }
        public int? SoLuongTon { get; set; }

        public string TenDanhMuc { get; set; }

        public List<string> DanhSachAnh { get; set; }
        public decimal? GiaThueNgay { get; set; }
    }
}
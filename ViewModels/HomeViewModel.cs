
using System.Collections.Generic;

namespace STAGEFX_PRO.ViewModels
{
    public class HomeViewModel
    {
        public List<HomeCategoryViewModel> Categories { get; set; }
        public List<HomeProductViewModel> Products { get; set; }

        public HomeViewModel()
        {
            Categories = new List<HomeCategoryViewModel>();
            Products = new List<HomeProductViewModel>();
        }
    }

    public class HomeCategoryViewModel
    {
        public int MaDanhMuc { get; set; }
        public string TenDanhMuc { get; set; }
        public string MoTa { get; set; }
        public int SoLuongSanPham { get; set; }
    }

    public class HomeProductViewModel
    {
        public int MaSanPham { get; set; }
        public int MaDanhMuc { get; set; }
        public string SKU { get; set; }
        public string TenSanPham { get; set; }
        public string MoTaNgan { get; set; }
        public string ThongSoKyThuat { get; set; }
        public decimal GiaBan { get; set; }
        public int SoLuongTon { get; set; }
        public string TenDanhMuc { get; set; }
        public string DuongDanAnh { get; set; }
    }
}


using System;
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
        public decimal? GiaThueNgay { get; set; }

        public string TenDanhMuc { get; set; }
        public List<string> DanhSachAnh { get; set; }

        // Thống kê đánh giá
        public decimal DiemDanhGiaTrungBinh { get; set; }
        public int TongSoDanhGia { get; set; }

        // Phân bố số sao
        public int SoDanhGia5Sao { get; set; }
        public int SoDanhGia4Sao { get; set; }
        public int SoDanhGia3Sao { get; set; }
        public int SoDanhGia2Sao { get; set; }
        public int SoDanhGia1Sao { get; set; }

        // Đánh giá đã được duyệt
        public List<ProductReviewViewModel> DanhSachDanhGia { get; set; }

        // Chi tiết đơn hàng đủ điều kiện đánh giá
        public List<ProductReviewOrderViewModel> DanhSachDonHangCoTheDanhGia { get; set; }

        public ProductDetailViewModel()
        {
            DanhSachAnh = new List<string>();
            DanhSachDanhGia = new List<ProductReviewViewModel>();
            DanhSachDonHangCoTheDanhGia = new List<ProductReviewOrderViewModel>();
        }
    }

    public class ProductReviewViewModel
    {
        public int MaDanhGia { get; set; }
        public string HoTenNguoiDung { get; set; }
        public byte SoSao { get; set; }
        public string NhanXet { get; set; }
        public DateTime NgayTao { get; set; }
    }

    public class ProductReviewOrderViewModel
    {
        public int MaChiTietDonHang { get; set; }
        public string MaSoDonHang { get; set; }
        public string TenSanPhamLucDat { get; set; }
        public DateTime NgayDat { get; set; }
    }
}

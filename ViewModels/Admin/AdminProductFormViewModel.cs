
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace Wedside_TVS_SFX.ViewModels.Admin
{
    public class AdminProductFormViewModel
    {
        // ==========================================
        // THÔNG TIN CƠ BẢN
        // ==========================================

        public int MaSanPham { get; set; }

        [Required(ErrorMessage = "SKU là bắt buộc.")]
        [StringLength(100, ErrorMessage = "SKU không được vượt quá 100 ký tự.")]
        public string SKU { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc.")]
        [StringLength(250, ErrorMessage = "Tên sản phẩm không được vượt quá 250 ký tự.")]
        public string TenSanPham { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục.")]
        public int? MaDanhMuc { get; set; }

        // ==========================================
        // MÔ TẢ SẢN PHẨM
        // ==========================================

        public string MoTaNgan { get; set; }

        public string MoTaChiTiet { get; set; }

        public string ThongSoKyThuat { get; set; }

        // ==========================================
        // GIÁ VÀ KHO HÀNG
        // ==========================================

        [Range(
            typeof(decimal),
            "0",
            "79228162514264337593543950335",
            ErrorMessage = "Giá bán không được âm."
        )]
        public decimal GiaBan { get; set; }

        [Range(
            typeof(decimal),
            "0",
            "79228162514264337593543950335",
            ErrorMessage = "Giá thuê không được âm."
        )]
        public decimal? GiaThueNgay { get; set; }

        [Range(
            0,
            int.MaxValue,
            ErrorMessage = "Tồn kho không được âm."
        )]
        public int SoLuongTon { get; set; }

        public bool TrangThai { get; set; } = true;

        // ==========================================
        // DANH MỤC
        // ==========================================

        public IEnumerable<SelectListItem> Categories { get; set; }
            = new List<SelectListItem>();

        // ==========================================
        // HÌNH ẢNH HIỆN CÓ
        // ==========================================

        public IList<AdminProductImageViewModel> ExistingImages { get; set; }
            = new List<AdminProductImageViewModel>();

        // ==========================================
        // HÌNH ẢNH TẢI LÊN
        // ==========================================

        public IEnumerable<HttpPostedFileBase> NewImages { get; set; }

        // ==========================================
        // QUẢN LÝ ẢNH CHÍNH
        // ==========================================

        public int? MainImageId { get; set; }

        public int? MainImageNewIndex { get; set; }

        // ==========================================
        // DANH SÁCH ẢNH CẦN XÓA
        // ==========================================

        public int[] RemoveImageIds { get; set; }
            = new int[0];
    }

    public class AdminProductImageViewModel
    {
        public int MaHinhAnh { get; set; }

        public string DuongDanAnh { get; set; }

        public string MoTaAnh { get; set; }

        public bool LaAnhChinh { get; set; }

        public int ThuTuHienThi { get; set; }
    }
}

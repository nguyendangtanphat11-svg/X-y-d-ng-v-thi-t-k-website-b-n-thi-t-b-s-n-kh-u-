
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Wedside_TVS_SFX.ViewModels.Admin
{
    // ==========================================
    // DANH SÁCH KHUYẾN MÃI
    // ==========================================
    public class AdminPromotionViewModel
    {
        public List<AdminPromotionItemViewModel> Promotions { get; set; }
            = new List<AdminPromotionItemViewModel>();

        public string SearchTerm { get; set; }

        public string StatusFilter { get; set; }

        public string DiscountTypeFilter { get; set; }

        public int CurrentPage { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public int TotalItems { get; set; }

        public int TotalPages { get; set; }

        public int TotalPromotions { get; set; }

        public int ActivePromotions { get; set; }

        public int InactivePromotions { get; set; }

        public int ExpiredPromotions { get; set; }
    }

    // ==========================================
    // THÔNG TIN TỪNG KHUYẾN MÃI
    // ==========================================
    public class AdminPromotionItemViewModel
    {
        public int MaKhuyenMai { get; set; }

        public string MaCode { get; set; }

        public string TenKhuyenMai { get; set; }

        public string MoTa { get; set; }

        public string LoaiGiamGia { get; set; }

        public decimal GiaTriGiam { get; set; }

        public decimal GiaTriDonHangToiThieu { get; set; }

        public decimal? GiamToiDa { get; set; }

        public DateTime NgayBatDau { get; set; }

        public DateTime NgayKetThuc { get; set; }

        public int? GioiHanSuDung { get; set; }

        public int SoLanDaSuDung { get; set; }

        public bool TrangThai { get; set; }
    }

    // ==========================================
    // FORM THÊM / CHỈNH SỬA KHUYẾN MÃI
    // ==========================================
    public class AdminPromotionFormViewModel
    {
        public int MaKhuyenMai { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã khuyến mãi.")]
        [StringLength(50, ErrorMessage = "Mã khuyến mãi tối đa 50 ký tự.")]
        public string MaCode { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên khuyến mãi.")]
        [StringLength(200, ErrorMessage = "Tên khuyến mãi tối đa 200 ký tự.")]
        public string TenKhuyenMai { get; set; }

        [StringLength(1000, ErrorMessage = "Mô tả tối đa 1000 ký tự.")]
        public string MoTa { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn loại giảm giá.")]
        [StringLength(20)]
        public string LoaiGiamGia { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá trị giảm.")]
        [Range(
            typeof(decimal),
            "0.01",
            "999999999999",
            ErrorMessage = "Giá trị giảm phải lớn hơn 0."
        )]
        public decimal? GiaTriGiam { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá trị đơn hàng tối thiểu.")]
        [Range(
            typeof(decimal),
            "0",
            "999999999999",
            ErrorMessage = "Giá trị đơn hàng tối thiểu không hợp lệ."
        )]
        public decimal? GiaTriDonHangToiThieu { get; set; }

        [Range(
            typeof(decimal),
            "0",
            "999999999999",
            ErrorMessage = "Mức giảm tối đa không hợp lệ."
        )]
        public decimal? GiamToiDa { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày bắt đầu.")]
        public DateTime? NgayBatDau { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày kết thúc.")]
        public DateTime? NgayKetThuc { get; set; }

        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Giới hạn sử dụng phải lớn hơn 0."
        )]
        public int? GioiHanSuDung { get; set; }

        public bool TrangThai { get; set; } = true;
    }
}


using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Wedside_TVS_SFX.ViewModels
{
    public class CheckoutViewModel
    {
        // Thông tin doanh nghiệp
        [Required(ErrorMessage = "Vui lòng nhập tên công ty.")]
        [StringLength(400)]
        public string Company { get; set; }

        [StringLength(40)]
        public string TaxId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập người liên hệ.")]
        [StringLength(300)]
        public string ContactName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [StringLength(60)]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        [StringLength(512)]
        public string Email { get; set; }

        // Thông tin sự kiện
        [Required(ErrorMessage = "Vui lòng nhập tên sự kiện.")]
        [StringLength(400)]
        public string EventName { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày tổ chức.")]
        [DataType(DataType.Date)]
        public DateTime? EventDate { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn loại địa điểm.")]
        public string VenueType { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa điểm tổ chức.")]
        [StringLength(1000)]
        public string EventLocation { get; set; }

        // Thông tin an toàn
        public string StructureInfo { get; set; }

        public bool SiteTechnician { get; set; }

        public string SafetyNotes { get; set; }

        public bool SiteReview { get; set; }

        public bool EmergencyPlan { get; set; }

        public bool QualifiedOperator { get; set; }

        // Phương thức thanh toán
        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán.")]
        public string PaymentMethod { get; set; }

        // Dữ liệu giỏ hàng do máy chủ lấy từ database
        public List<CheckoutCartItemViewModel> Items { get; set; }
            = new List<CheckoutCartItemViewModel>();

        public int TongSoLuong { get; set; }

        public decimal TienHang { get; set; }

        public decimal PhiVanChuyen { get; set; }

        public decimal VAT { get; set; }

        public decimal TienCoc { get; set; }

        public decimal TongTien { get; set; }
    }

    public class CheckoutCartItemViewModel
    {
        public int MaSanPham { get; set; }

        public string SKU { get; set; }

        public string TenSanPham { get; set; }

        public string HinhThucDat { get; set; }

        public int SoLuong { get; set; }

        public int? SoNgayThue { get; set; }

        public bool CoFlightcase { get; set; }

        public decimal DonGia { get; set; }

        public decimal PhiFlightcase { get; set; }

        public decimal ThanhTien { get; set; }
    }
}

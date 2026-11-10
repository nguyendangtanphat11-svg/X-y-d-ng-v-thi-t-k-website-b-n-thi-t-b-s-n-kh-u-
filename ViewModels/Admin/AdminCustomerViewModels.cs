using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Wedside_TVS_SFX.ViewModels.Admin
{
    public class B2BCustomerListItemViewModel
    {
        public int MaNguoiDung { get; set; }
        public string HoTen { get; set; }
        public string Email { get; set; }
        public string SoDienThoai { get; set; }
        public string NhomB2B { get; set; }
        public decimal PhanTramChietKhau { get; set; }
        public bool TrangThai { get; set; }
        public int TongDonHang { get; set; }
        public decimal TongChiTieu { get; set; }
        public DateTime NgayTao { get; set; }
    }

    public class CustomerFormViewModel
    {
        public int MaNguoiDung { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên khách hàng")]
        public string HoTen { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        public string SoDienThoai { get; set; }

        public bool TrangThai { get; set; } = true;
    }

    public class CustomerOrderHistoryViewModel
    {
        public int MaNguoiDung { get; set; }
        public string HoTen { get; set; }
        public decimal TongChiTieu { get; set; }
        public int TongDonHang { get; set; }
        public List<CustomerOrderItemViewModel> DanhSachDonHang { get; set; } = new List<CustomerOrderItemViewModel>();
    }

    public class CustomerOrderItemViewModel
    {
        public string MaSoDonHang { get; set; }
        public string TenSanPham { get; set; }
        public int SoLuong { get; set; }
        public decimal ThanhTien { get; set; }
        public DateTime NgayDat { get; set; }
        public string TrangThai { get; set; }
    }
}
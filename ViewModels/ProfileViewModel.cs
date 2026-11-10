using System;
using System.ComponentModel.DataAnnotations;

namespace Wedside_TVS_SFX.ViewModels
{
    public class ProfileViewModel
    {
        public int MaNguoiDung { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
        [StringLength(300, ErrorMessage = "Họ và tên không được vượt quá 300 ký tự.")]
        public string HoTen { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        [StringLength(300, ErrorMessage = "Email không được vượt quá 300 ký tự.")]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [StringLength(60, ErrorMessage = "Số điện thoại không được vượt quá 60 ký tự.")]
        public string SoDienThoai { get; set; }

        public int MaVaiTro { get; set; }

        public string TenVaiTro { get; set; }

        public bool TrangThai { get; set; }

        public DateTime? NgayTao { get; set; }

        public DateTime? LanDangNhapCuoi { get; set; }
    }
}
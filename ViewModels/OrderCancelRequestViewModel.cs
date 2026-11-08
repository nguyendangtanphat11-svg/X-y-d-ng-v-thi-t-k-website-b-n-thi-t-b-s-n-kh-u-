using System.ComponentModel.DataAnnotations;

namespace Wedside_TVS_SFX.ViewModels
{
    public class OrderCancelRequestViewModel
    {
        public int MaDonHang { get; set; }

        public string MaSoDonHang { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lý do hủy đơn.")]
        [StringLength(2000, MinimumLength = 5,
            ErrorMessage = "Lý do hủy phải từ 5 đến 2000 ký tự.")]
        [Display(Name = "Lý do hủy đơn")]
        public string LyDoHuy { get; set; }
    }
}
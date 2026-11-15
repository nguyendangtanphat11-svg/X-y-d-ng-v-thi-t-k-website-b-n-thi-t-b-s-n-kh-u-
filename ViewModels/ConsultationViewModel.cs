
using System.ComponentModel.DataAnnotations;

namespace STAGEFX_PRO.ViewModels
{
    public class ConsultationViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên công ty hoặc người liên hệ.")]
        [StringLength(200, ErrorMessage = "Tên công ty hoặc người liên hệ không được vượt quá 200 ký tự.")]
        public string CompanyName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        [StringLength(200, ErrorMessage = "Email không được vượt quá 200 ký tự.")]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự.")]
        public string Phone { get; set; }

        [StringLength(100, ErrorMessage = "Quy mô sự kiện không hợp lệ.")]
        public string Scale { get; set; }

        [StringLength(250, ErrorMessage = "Tên sản phẩm không hợp lệ.")]
        public string RequestedProduct { get; set; }

        [StringLength(1000, ErrorMessage = "Nội dung yêu cầu không được vượt quá 1000 ký tự.")]
        public string Content { get; set; }
    }
}

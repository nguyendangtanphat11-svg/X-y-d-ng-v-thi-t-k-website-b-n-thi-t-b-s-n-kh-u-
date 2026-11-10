
using System.ComponentModel.DataAnnotations;

namespace STAGEFX_PRO.ViewModels
{
    public class ConsultationViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên công ty hoặc người liên hệ.")]
        public string CompanyName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        public string Phone { get; set; }

        public string Scale { get; set; }

        public string RequestedProduct { get; set; }
    }
}

using System.Web.Mvc;

namespace STAGEFX_PRO.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(
                    "",
                    "Vui lòng nhập email và mật khẩu."
                );

                return View();
            }

            // TODO: Tìm tài khoản trong database,
            // xác minh mật khẩu đã băm và tạo phiên đăng nhập.
            ModelState.AddModelError(
                "",
                "Chức năng xác thực chưa được kết nối."
            );

            return View();
        }

        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(string name, string company,
            string email, string password, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(name) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(
                    "",
                    "Vui lòng nhập đầy đủ thông tin bắt buộc."
                );

                return View();
            }

            if (password != confirmPassword)
            {
                ModelState.AddModelError(
                    "",
                    "Mật khẩu xác nhận không khớp."
                );

                return View();
            }

            // TODO: Kiểm tra email trùng, băm mật khẩu,
            // lưu tài khoản vào database.
            ModelState.AddModelError(
                "",
                "Chức năng đăng ký chưa được kết nối."
            );

            return View();
        }

        public ActionResult Logout()
        {
            // TODO: Đăng xuất bằng cơ chế xác thực đã cấu hình.
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Profile()
        {
            // TODO: Yêu cầu đăng nhập và tải hồ sơ tài khoản.
            return View();
        }
    }
}
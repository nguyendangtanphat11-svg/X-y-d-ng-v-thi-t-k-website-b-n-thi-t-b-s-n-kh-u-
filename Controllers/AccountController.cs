
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Web.Mvc;
using Wedside_TVS_SFX.Models;
using Wedside_TVS_SFX.ViewModels;
namespace STAGEFX_PRO.Controllers
{
    public class AccountController : Controller
    {
        private const int CUSTOMER_ROLE_ID = 3;

        // =========================
        // ĐĂNG KÝ
        // =========================

        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(
              string name,
                string company,
                string email,
                string password,
                string confirmPassword,
    bool? terms)
        {
            name = (name ?? "").Trim();
            email = (email ?? "").Trim().ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(name) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ thông tin.");
                return View();
            }

            if (password != confirmPassword)
            {
                ModelState.AddModelError("", "Mật khẩu xác nhận không khớp.");
                return View();
            }

            if (password.Length < 8)
            {
                ModelState.AddModelError("", "Mật khẩu phải có ít nhất 8 ký tự.");
                return View();
            }

            if (!email.Contains("@"))
            {
                ModelState.AddModelError("", "Email không hợp lệ.");
                return View();
            }

            using (var db = new StageFxEntities())
            {
                // Kiểm tra email đã tồn tại
                bool emailExists = db.NguoiDung.Any(
                    u => u.Email == email
                );

                if (emailExists)
                {
                    ModelState.AddModelError("", "Email này đã được đăng ký.");
                    return View();
                }

                // Lấy vai trò KhachHang
                var customerRole = db.VaiTro.Find(CUSTOMER_ROLE_ID);

                if (customerRole == null)
                {
                    ModelState.AddModelError(
                        "",
                        "Không tìm thấy vai trò Khách hàng trong hệ thống."
                    );

                    return View();
                }

                // Tạo tài khoản
                var user = new NguoiDung
                {
                    HoTen = name,
                    Email = email,
                    MatKhauHash = HashPassword(password),
                    MaVaiTro = CUSTOMER_ROLE_ID,
                    TrangThai = true,
                    NgayTao = DateTime.Now
                };

                db.NguoiDung.Add(user);
                db.SaveChanges();
            }

            TempData["AuthSuccess"] = "Đăng ký tài khoản thành công. Hãy đăng nhập!";
            return RedirectToAction("Login");
        }

        // =========================
        // ĐĂNG NHẬP
        // =========================

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string email, string password)
        {
            email = (email ?? "").Trim().ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Vui lòng nhập email và mật khẩu.");
                return View();
            }

            using (var db = new StageFxEntities())
            {
                var user = db.NguoiDung.FirstOrDefault(
                    u => u.Email == email
                );

                if (user == null || !VerifyPassword(password, user.MatKhauHash))
                {
                    ModelState.AddModelError("", "Email hoặc mật khẩu không chính xác.");
                    return View();
                }

                if (!user.TrangThai)
                {
                    ModelState.AddModelError("", "Tài khoản của bạn hiện đang bị khóa.");
                    return View();
                }

                user.LanDangNhapCuoi = DateTime.Now;
                db.SaveChanges();

                // Lưu thông tin đăng nhập vào Session
                Session["MaNguoiDung"] = user.MaNguoiDung;
                Session["HoTen"] = user.HoTen;
                Session["Email"] = user.Email;
                Session["MaVaiTro"] = user.MaVaiTro;
                Session["TenVaiTro"] = user.VaiTro != null
                    ? user.VaiTro.TenVaiTro
                    : "";

                if (user.MaVaiTro == 1)
                {
                    return RedirectToAction("Index", "Admin");
                }
            }

            return RedirectToAction("Index", "Home");
        }

        // =========================
        // ĐĂNG XUẤT
        // =========================

        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();

            return RedirectToAction("Login");
        }

        // =========================
        // TRANG CÁ NHÂN
        // =========================

        // =========================
        // TRANG CÁ NHÂN - GET
        // =========================

        [HttpGet]
        public ActionResult Profile()
        {
            if (Session["MaNguoiDung"] == null)
            {
                TempData["AuthError"] = "Vui lòng đăng nhập để xem thông tin tài khoản.";
                return RedirectToAction("Login");
            }

            int userId = Convert.ToInt32(Session["MaNguoiDung"]);

            using (var db = new StageFxEntities())
            {
                var user = db.NguoiDung.Find(userId);

                if (user == null)
                {
                    Session.Clear();
                    return RedirectToAction("Login");
                }

                var model = new ProfileViewModel
                {
                    MaNguoiDung = user.MaNguoiDung,
                    HoTen = user.HoTen,
                    Email = user.Email,
                    SoDienThoai = user.SoDienThoai,
                    MaVaiTro = user.MaVaiTro,
                    TenVaiTro = user.VaiTro != null
                        ? user.VaiTro.TenVaiTro
                        : "Khách hàng",
                    TrangThai = user.TrangThai,
                    NgayTao = user.NgayTao,
                    LanDangNhapCuoi = user.LanDangNhapCuoi
                };

                return View(model);
            }
        }

        // =========================
        // CẬP NHẬT THÔNG TIN - POST
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Profile(ProfileViewModel model)
        {
            if (Session["MaNguoiDung"] == null)
            {
                return RedirectToAction("Login");
            }

            int userId = Convert.ToInt32(Session["MaNguoiDung"]);

            using (var db = new StageFxEntities())
            {
                var user = db.NguoiDung.Find(userId);

                if (user == null)
                {
                    Session.Clear();
                    return RedirectToAction("Login");
                }

                // Chuẩn hóa dữ liệu
                model.HoTen = (model.HoTen ?? "").Trim();
                model.Email = (model.Email ?? "").Trim().ToLowerInvariant();
                model.SoDienThoai = (model.SoDienThoai ?? "").Trim();

                // Không cho người dùng tự thay đổi thông tin hệ thống
                model.MaNguoiDung = user.MaNguoiDung;
                model.MaVaiTro = user.MaVaiTro;
                model.TenVaiTro = user.VaiTro != null
                    ? user.VaiTro.TenVaiTro
                    : "Khách hàng";
                model.TrangThai = user.TrangThai;
                model.NgayTao = user.NgayTao;
                model.LanDangNhapCuoi = user.LanDangNhapCuoi;

                // Kiểm tra dữ liệu
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Kiểm tra email đã được tài khoản khác sử dụng chưa
                bool emailExists = db.NguoiDung.Any(x =>
                    x.Email == model.Email &&
                    x.MaNguoiDung != userId
                );

                if (emailExists)
                {
                    ModelState.AddModelError(
                        "Email",
                        "Email này đã được sử dụng bởi tài khoản khác."
                    );

                    return View(model);
                }

                try
                {
                    // Chỉ cập nhật thông tin được phép chỉnh sửa
                    user.HoTen = model.HoTen;
                    user.Email = model.Email;
                    user.SoDienThoai = string.IsNullOrWhiteSpace(model.SoDienThoai)
                        ? null
                        : model.SoDienThoai;

                    user.NgayCapNhat = DateTime.Now;

                    db.SaveChanges();

                    // Đồng bộ thông tin mới vào Session
                    Session["HoTen"] = user.HoTen;
                    Session["Email"] = user.Email;

                    TempData["ProfileSuccess"] = "Cập nhật thông tin tài khoản thành công.";

                    return RedirectToAction("Profile");
                }
                catch (Exception)
                {
                    ModelState.AddModelError(
                        "",
                        "Không thể cập nhật thông tin. Vui lòng thử lại sau."
                    );

                    return View(model);
                }
            }
        }
        // =========================
        // MÃ HÓA MẬT KHẨU
        // =========================

        private string HashPassword(string password)
        {
            const int iterations = 100000;

            byte[] salt = new byte[16];

            using (var random = RandomNumberGenerator.Create())
            {
                random.GetBytes(salt);
            }

            using (var pbkdf2 = new Rfc2898DeriveBytes(
                password, salt, iterations))
            {
                byte[] hash = pbkdf2.GetBytes(32);

                return "PBKDF2$" + iterations +
                       "$" + Convert.ToBase64String(salt) +
                       "$" + Convert.ToBase64String(hash);
            }
        }

        // =========================
        // KIỂM TRA MẬT KHẨU
        // =========================

        private bool VerifyPassword(string password, string storedHash)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(storedHash))
                    return false;

                string[] parts = storedHash.Split('$');

                if (parts.Length != 4 || parts[0] != "PBKDF2")
                    return false;

                int iterations = int.Parse(parts[1]);

                if (iterations < 10000 || iterations > 1000000)
                    return false;

                byte[] salt = Convert.FromBase64String(parts[2]);
                byte[] originalHash = Convert.FromBase64String(parts[3]);

                using (var pbkdf2 = new Rfc2898DeriveBytes(
                    password, salt, iterations))
                {
                    byte[] newHash = pbkdf2.GetBytes(originalHash.Length);

                    if (newHash.Length != originalHash.Length)
                        return false;

                    int difference = 0;

                    for (int i = 0; i < newHash.Length; i++)
                    {
                        difference |= newHash[i] ^ originalHash[i];
                    }

                    return difference == 0;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}

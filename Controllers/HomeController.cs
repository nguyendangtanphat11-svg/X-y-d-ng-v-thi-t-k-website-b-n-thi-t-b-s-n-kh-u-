
using System;
using System.Linq;
using System.Web.Mvc;
using STAGEFX_PRO.ViewModels;
using Wedside_TVS_SFX.Models;

namespace STAGEFX_PRO.Controllers
{
    public class HomeController : Controller
    {
        // ==========================================
        // TRANG CHỦ
        // Lấy danh mục và sản phẩm từ database
        // ==========================================
        public ActionResult Index()
        {
            using (var db = new StageFxEntities())
            {
                // Lấy danh sách danh mục đang hoạt động
                var categories = db.DanhMuc
                    .Where(d => d.TrangThai)
                    .OrderBy(d => d.TenDanhMuc)
                    .Select(d => new HomeCategoryViewModel
                    {
                        MaDanhMuc = d.MaDanhMuc,
                        TenDanhMuc = d.TenDanhMuc,
                        MoTa = d.MoTa,

                        SoLuongSanPham = db.SanPham.Count(p =>
                            p.MaDanhMuc == d.MaDanhMuc &&
                            p.TrangThai)
                    })
                    .ToList();

                // Lấy 8 sản phẩm mới nhất đang hoạt động
                var products = db.SanPham
                    .Where(p => p.TrangThai && p.DanhMuc.TrangThai)
                    .OrderByDescending(p => p.NgayTao)
                    .Take(8)
                    .Select(p => new HomeProductViewModel
                    {
                        MaSanPham = p.MaSanPham,
                        MaDanhMuc = p.MaDanhMuc,
                        SKU = p.SKU,
                        TenSanPham = p.TenSanPham,
                        MoTaNgan = p.MoTaNgan,
                        ThongSoKyThuat = p.ThongSoKyThuat,
                        GiaBan = p.GiaBan,
                        SoLuongTon = p.SoLuongTon,

                        TenDanhMuc = p.DanhMuc.TenDanhMuc,

                        // Lấy ảnh chính của sản phẩm
                        DuongDanAnh = p.HinhAnhSanPham
                            .OrderByDescending(a => a.LaAnhChinh)
                            .ThenBy(a => a.ThuTuHienThi)
                            .Select(a => a.DuongDanAnh)
                            .FirstOrDefault()
                    })
                    .ToList();

                // Đưa dữ liệu vào ViewModel
                var model = new HomeViewModel
                {
                    Categories = categories,
                    Products = products
                };

                return View(model);
            }
        }


        // ==========================================
        // GỬI YÊU CẦU TƯ VẤN
        // Lưu thông tin khách hàng vào database
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GuiYeuCauTuVan(ConsultationViewModel model)
        {
            // Kiểm tra dữ liệu đầu vào
            if (model == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Dữ liệu gửi lên không hợp lệ."
                });
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .Where(e => !string.IsNullOrWhiteSpace(e))
                    .ToList();

                return Json(new
                {
                    success = false,
                    message = errors.FirstOrDefault()
                        ?? "Vui lòng kiểm tra lại thông tin."
                });
            }

            try
            {
                // Lấy mã người dùng nếu khách hàng đã đăng nhập
                int? maNguoiDung = null;
                int parsedId;

                if (Session["MaNguoiDung"] != null &&
                    int.TryParse(
                        Session["MaNguoiDung"].ToString(),
                        out parsedId))
                {
                    maNguoiDung = parsedId;
                }

                // Xử lý sản phẩm khách hàng quan tâm
                string sanPham = string.IsNullOrWhiteSpace(
                    model.RequestedProduct)
                    ? "Chưa chọn sản phẩm"
                    : model.RequestedProduct.Trim();

                // Tạo chủ đề tư vấn
                string chuDe = "Tư vấn: " + sanPham;

                // Tạo nội dung yêu cầu
                string noiDung =
                    "Sản phẩm quan tâm: " + sanPham +
                    "\nQuy mô sự kiện: " +
                    (string.IsNullOrWhiteSpace(model.Scale)
                        ? "Chưa cung cấp"
                        : model.Scale.Trim());

                using (var db = new StageFxEntities())
                {
                    // Tạo yêu cầu tư vấn mới
                    var yeuCau = new YeuCauTuVan
                    {
                        MaNguoiDung = maNguoiDung,

                        HoTen = model.CompanyName.Trim(),

                        Email = model.Email.Trim(),

                        SoDienThoai = string.IsNullOrWhiteSpace(
                            model.Phone)
                            ? null
                            : model.Phone.Trim(),

                        ChuDe = chuDe,

                        NoiDung = noiDung,

                        // Phải đúng CHECK CONSTRAINT trong SQL Server
                        TrangThai = "Moi",

                        NgayTao = DateTime.Now
                    };

                    // Thêm vào database
                    db.YeuCauTuVan.Add(yeuCau);

                    // Lưu dữ liệu
                    db.SaveChanges();
                }

                // Trả kết quả thành công
                return Json(new
                {
                    success = true,
                    message = "Gửi yêu cầu tư vấn thành công! STAGEFX PRO sẽ liên hệ với bạn."
                });
            }
            catch (Exception ex)
            {
                // Ghi lỗi chi tiết vào Output của Visual Studio
                System.Diagnostics.Debug.WriteLine(
                    "LỖI GỬI YÊU CẦU TƯ VẤN:");

                System.Diagnostics.Debug.WriteLine(ex.ToString());

                // Thông báo lỗi khi đang kiểm tra trên máy local
                return Json(new
                {
                    success = false,
                    message = "Lỗi lưu yêu cầu: "
                        + ex.GetBaseException().Message
                });
            }
        }
    }
}

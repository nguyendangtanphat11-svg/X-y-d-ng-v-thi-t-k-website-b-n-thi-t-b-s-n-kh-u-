using STAGEFX_PRO.Controllers;
using STAGEFX_PRO.Controllers.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Wedside_TVS_SFX.Models;
using Wedside_TVS_SFX.ViewModels.Admin;

namespace Wedside_TVS_SFX.Controllers
{
    [AdminAuthorize]
    public class AdminCustomerController : Controller
    {
        private StageFxEntities db = new StageFxEntities();

        // 1. DANH SÁCH KHÁCH HÀNG
        public ActionResult Index(string search, string tier)
        {
            var users = db.NguoiDung.Where(u => u.MaVaiTro == 3).ToList();
            var customerList = new List<B2BCustomerListItemViewModel>();

            foreach (var u in users)
            {
                var validOrders = u.DonHang.Where(d => d.TrangThai != "TuChoi").ToList();
                decimal totalSpent = validOrders.Sum(d => d.TongTien); // Sửa CS0019: Bỏ ?? 0
                int totalOrders = validOrders.Count;

                string nhomB2B = "Silver";
                decimal chietKhau = 5;

                if (totalSpent >= 50000000)
                {
                    nhomB2B = "VIP";
                    chietKhau = 15;
                }
                else if (totalSpent >= 20000000)
                {
                    nhomB2B = "Gold";
                    chietKhau = 10;
                }

                customerList.Add(new B2BCustomerListItemViewModel
                {
                    MaNguoiDung = u.MaNguoiDung,
                    HoTen = u.HoTen,
                    Email = u.Email,
                    SoDienThoai = string.IsNullOrEmpty(u.SoDienThoai) ? "Chưa cập nhật" : u.SoDienThoai,
                    NhomB2B = nhomB2B,
                    PhanTramChietKhau = chietKhau,
                    TrangThai = u.TrangThai,
                    TongDonHang = totalOrders,
                    TongChiTieu = totalSpent,
                    NgayTao = u.NgayTao
                });
            }

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower().Trim();
                customerList = customerList.Where(x => x.HoTen.ToLower().Contains(search) ||
                                                       x.Email.ToLower().Contains(search) ||
                                                       x.SoDienThoai.Contains(search)).ToList();
            }

            if (!string.IsNullOrEmpty(tier))
            {
                customerList = customerList.Where(x => x.NhomB2B.Equals(tier, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            ViewBag.TotalCustomers = customerList.Count;
            ViewBag.TotalVip = customerList.Count(x => x.NhomB2B == "VIP");
            ViewBag.TotalSales = customerList.Sum(x => x.TongChiTieu);

            return View("~/Views/Admin/Customers.cshtml", customerList);
        }

        // 2. THÊM / SỬA KHÁCH HÀNG (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult SaveCustomer(CustomerFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Vui lòng điền đầy đủ thông tin bắt buộc!" });
            }

            if (model.MaNguoiDung == 0) // THÊM MỚI
            {
                if (db.NguoiDung.Any(u => u.Email == model.Email))
                {
                    return Json(new { success = false, message = "Email này đã tồn tại trong hệ thống!" });
                }

                var user = new NguoiDung
                {
                    HoTen = model.HoTen,
                    Email = model.Email,
                    SoDienThoai = model.SoDienThoai,
                    TrangThai = model.TrangThai,
                    MaVaiTro = 3, // Khách hàng
                    NgayTao = DateTime.Now
                }; // Sửa CS0117: Không gán MatKhau

                db.NguoiDung.Add(user);
                db.SaveChanges();
                return Json(new { success = true, message = "Thêm khách hàng thành công!" });
            }
            else // CẬP NHẬT
            {
                var user = db.NguoiDung.Find(model.MaNguoiDung);
                if (user == null) return Json(new { success = false, message = "Không tìm thấy khách hàng!" });

                if (db.NguoiDung.Any(u => u.Email == model.Email && u.MaNguoiDung != model.MaNguoiDung))
                {
                    return Json(new { success = false, message = "Email này đã được dùng bởi tài khoản khác!" });
                }

                user.HoTen = model.HoTen;
                user.Email = model.Email;
                user.SoDienThoai = model.SoDienThoai;
                user.TrangThai = model.TrangThai; // Sửa CS1061: Không gán MatKhau

                db.SaveChanges();
                return Json(new { success = true, message = "Cập nhật thông tin thành công!" });
            }
        }

        // 3. LẤY THÔNG TIN CHI TIẾT ĐỂ SỬA
        [HttpGet]
        public JsonResult GetCustomerDetail(int id)
        {
            var user = db.NguoiDung.Find(id);
            if (user == null) return Json(null, JsonRequestBehavior.AllowGet);

            return Json(new
            {
                maNguoiDung = user.MaNguoiDung,
                hoTen = user.HoTen,
                email = user.Email,
                soDienThoai = user.SoDienThoai,
                trangThai = user.TrangThai
            }, JsonRequestBehavior.AllowGet);
        }

        // 4. KHÓA / MỞ KHÓA TÀI KHOẢN
        [HttpPost]
        public JsonResult ToggleStatus(int id)
        {
            var user = db.NguoiDung.Find(id);
            if (user == null) return Json(new { success = false, message = "Không tìm thấy khách hàng!" });

            user.TrangThai = !user.TrangThai;
            db.SaveChanges();

            string msg = user.TrangThai ? "Đã mở khóa tài khoản thành công!" : "Đã khóa tài khoản khách hàng!";
            return Json(new { success = true, message = msg });
        }

        // 5. LỊCH SỬ MUA HÀNG
        [HttpGet]
        public JsonResult GetOrderHistory(int maNguoiDung)
        {
            var user = db.NguoiDung.Find(maNguoiDung);
            if (user == null) return Json(null, JsonRequestBehavior.AllowGet);

            var orders = (from d in db.DonHang
                          join ct in db.ChiTietDonHang on d.MaDonHang equals ct.MaDonHang
                          where d.MaNguoiDung == maNguoiDung && d.TrangThai != "TuChoi"
                          orderby d.NgayDat descending
                          select new CustomerOrderItemViewModel
                          {
                              MaSoDonHang = d.MaSoDonHang,
                              TenSanPham = ct.TenSanPhamLucDat,
                              SoLuong = ct.SoLuong,
                              ThanhTien = ct.ThanhTien ?? 0,
                              NgayDat = d.NgayDat,
                              TrangThai = d.TrangThai
                          }).Take(15).ToList();

            var result = new CustomerOrderHistoryViewModel
            {
                MaNguoiDung = user.MaNguoiDung,
                HoTen = user.HoTen,
                TongDonHang = user.DonHang.Count(d => d.TrangThai != "TuChoi"),
                TongChiTieu = user.DonHang.Where(d => d.TrangThai != "TuChoi").Sum(d => d.TongTien),
                DanhSachDonHang = orders
            };

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
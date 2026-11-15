
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using STAGEFX_PRO.Controllers.Base;
using Wedside_TVS_SFX.Models;
using Wedside_TVS_SFX.ViewModels.Admin;

namespace STAGEFX_PRO.Controllers
{
    [AdminAuthorize]
    public class AdminPromotionController : Controller
    {
        private readonly StageFxEntities db = new StageFxEntities();

        // ==========================================
        // 1. DANH SÁCH KHUYẾN MÃI
        // ==========================================
        public ActionResult Promotions(
            string search,
            string status,
            string discountType,
            int page = 1)
        {
            const int pageSize = 10;

            DateTime now = DateTime.Now;

            var query = db.KhuyenMai.AsQueryable();

            // Tìm kiếm
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(x =>
                    x.MaCode.Contains(search) ||
                    x.TenKhuyenMai.Contains(search) ||
                    x.MoTa.Contains(search));
            }

            // Lọc trạng thái
            if (status == "active")
            {
                query = query.Where(x =>
                    x.TrangThai &&
                    x.NgayBatDau <= now &&
                    x.NgayKetThuc >= now);
            }
            else if (status == "inactive")
            {
                query = query.Where(x => !x.TrangThai);
            }
            else if (status == "expired")
            {
                query = query.Where(x => x.NgayKetThuc < now);
            }
            else if (status == "upcoming")
            {
                query = query.Where(x =>
                    x.TrangThai &&
                    x.NgayBatDau > now);
            }

            // Lọc loại giảm giá
            if (!string.IsNullOrWhiteSpace(discountType))
            {
                query = query.Where(x =>
                    x.LoaiGiamGia == discountType);
            }

            int totalItems = query.Count();

            int totalPages = (int)Math.Ceiling(
                totalItems / (double)pageSize);

            if (totalPages < 1)
                totalPages = 1;

            page = Math.Max(1, Math.Min(page, totalPages));

            var promotions = query
                .OrderByDescending(x => x.MaKhuyenMai)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new AdminPromotionItemViewModel
                {
                    MaKhuyenMai = x.MaKhuyenMai,
                    MaCode = x.MaCode,
                    TenKhuyenMai = x.TenKhuyenMai,
                    MoTa = x.MoTa,
                    LoaiGiamGia = x.LoaiGiamGia,
                    GiaTriGiam = x.GiaTriGiam,
                    GiaTriDonHangToiThieu = x.GiaTriDonHangToiThieu,
                    GiamToiDa = x.GiamToiDa,
                    NgayBatDau = x.NgayBatDau,
                    NgayKetThuc = x.NgayKetThuc,
                    GioiHanSuDung = x.GioiHanSuDung,
                    SoLanDaSuDung = x.SoLanDaSuDung,
                    TrangThai = x.TrangThai
                })
                .ToList();

            var model = new AdminPromotionViewModel
            {
                Promotions = promotions,

                SearchTerm = search,
                StatusFilter = status,
                DiscountTypeFilter = discountType,

                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages,

                TotalPromotions = db.KhuyenMai.Count(),

                ActivePromotions = db.KhuyenMai.Count(x =>
                    x.TrangThai &&
                    x.NgayBatDau <= now &&
                    x.NgayKetThuc >= now),

                InactivePromotions = db.KhuyenMai.Count(x =>
                    !x.TrangThai),

                ExpiredPromotions = db.KhuyenMai.Count(x =>
                    x.NgayKetThuc < now)
            };

            return View("~/Views/Admin/Promotions.cshtml", model);
        }


        // ==========================================
        // 2. THÊM KHUYẾN MÃI - GET
        // ==========================================
        public ActionResult Create()
        {
            var model = new AdminPromotionFormViewModel
            {
                NgayBatDau = DateTime.Today,
                NgayKetThuc = DateTime.Today.AddDays(30),
                GiaTriDonHangToiThieu = 0,
                TrangThai = true
            };

            return View("~/Views/Admin/PromotionCreate.cshtml", model);
        }


        // ==========================================
        // 3. THÊM KHUYẾN MÃI - POST
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AdminPromotionFormViewModel model)
        {
            ValidatePromotion(model, null);

            if (!ModelState.IsValid)
            {
                return View("~/Views/Admin/PromotionCreate.cshtml", model);
            }

            string code = model.MaCode.Trim().ToUpperInvariant();

            bool codeExists = db.KhuyenMai.Any(x =>
                x.MaCode == code);

            if (codeExists)
            {
                ModelState.AddModelError(
                    "MaCode",
                    "Mã khuyến mãi đã tồn tại.");

                return View("~/Views/Admin/PromotionCreate.cshtml", model);
            }

            var promotion = new KhuyenMai
            {
                MaCode = code,
                TenKhuyenMai = model.TenKhuyenMai.Trim(),
                MoTa = string.IsNullOrWhiteSpace(model.MoTa)
                    ? null
                    : model.MoTa.Trim(),

                LoaiGiamGia = model.LoaiGiamGia,

                GiaTriGiam = model.GiaTriGiam.Value,

                GiaTriDonHangToiThieu =
                    model.GiaTriDonHangToiThieu.Value,

                GiamToiDa = model.LoaiGiamGia == "PhanTram"
                    ? model.GiamToiDa
                    : null,

                NgayBatDau = model.NgayBatDau.Value,
                NgayKetThuc = model.NgayKetThuc.Value,

                GioiHanSuDung = model.GioiHanSuDung,

                SoLanDaSuDung = 0,
                TrangThai = model.TrangThai
            };

            db.KhuyenMai.Add(promotion);
            db.SaveChanges();

            TempData["Success"] = "Thêm khuyến mãi thành công.";

            return RedirectToAction("Promotions");
        }


        // ==========================================
        // 4. CHỈNH SỬA KHUYẾN MÃI - GET
        // ==========================================
        public ActionResult Edit(int id)
        {
            var promotion = db.KhuyenMai.Find(id);

            if (promotion == null)
                return HttpNotFound();

            var model = new AdminPromotionFormViewModel
            {
                MaKhuyenMai = promotion.MaKhuyenMai,
                MaCode = promotion.MaCode,
                TenKhuyenMai = promotion.TenKhuyenMai,
                MoTa = promotion.MoTa,

                LoaiGiamGia = promotion.LoaiGiamGia,
                GiaTriGiam = promotion.GiaTriGiam,
                GiaTriDonHangToiThieu =
                    promotion.GiaTriDonHangToiThieu,

                GiamToiDa = promotion.GiamToiDa,

                NgayBatDau = promotion.NgayBatDau,
                NgayKetThuc = promotion.NgayKetThuc,

                GioiHanSuDung = promotion.GioiHanSuDung,
                TrangThai = promotion.TrangThai
            };

            return View("~/Views/Admin/PromotionEdit.cshtml", model);
        }


        // ==========================================
        // 5. CHỈNH SỬA KHUYẾN MÃI - POST
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            int id,
            AdminPromotionFormViewModel model)
        {
            var promotion = db.KhuyenMai.Find(id);

            if (promotion == null)
                return HttpNotFound();

            ValidatePromotion(model, id);

            if (model.GioiHanSuDung.HasValue &&
                model.GioiHanSuDung.Value < promotion.SoLanDaSuDung)
            {
                ModelState.AddModelError(
                    "GioiHanSuDung",
                    "Giới hạn sử dụng không được nhỏ hơn số lần đã sử dụng.");
            }

            if (!ModelState.IsValid)
            {
                model.MaKhuyenMai = id;

                return View("~/Views/Admin/PromotionEdit.cshtml", model);
            }

            string code = model.MaCode.Trim().ToUpperInvariant();

            bool codeExists = db.KhuyenMai.Any(x =>
                x.MaCode == code &&
                x.MaKhuyenMai != id);

            if (codeExists)
            {
                ModelState.AddModelError(
                    "MaCode",
                    "Mã khuyến mãi đã được sử dụng.");

                model.MaKhuyenMai = id;

                return View("~/Views/Admin/PromotionEdit.cshtml", model);
            }

            promotion.MaCode = code;
            promotion.TenKhuyenMai = model.TenKhuyenMai.Trim();

            promotion.MoTa = string.IsNullOrWhiteSpace(model.MoTa)
                ? null
                : model.MoTa.Trim();

            promotion.LoaiGiamGia = model.LoaiGiamGia;
            promotion.GiaTriGiam = model.GiaTriGiam.Value;

            promotion.GiaTriDonHangToiThieu =
                model.GiaTriDonHangToiThieu.Value;

            promotion.GiamToiDa = model.LoaiGiamGia == "PhanTram"
                ? model.GiamToiDa
                : null;

            promotion.NgayBatDau = model.NgayBatDau.Value;
            promotion.NgayKetThuc = model.NgayKetThuc.Value;

            promotion.GioiHanSuDung = model.GioiHanSuDung;
            promotion.TrangThai = model.TrangThai;

            // Giữ nguyên SoLanDaSuDung
            db.SaveChanges();

            TempData["Success"] = "Cập nhật khuyến mãi thành công.";

            return RedirectToAction("Promotions");
        }


        // ==========================================
        // 6. XÓA KHUYẾN MÃI
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var promotion = db.KhuyenMai
                .Include(x => x.SuDungKhuyenMai)
                .Include(x => x.DonHang)
                .FirstOrDefault(x => x.MaKhuyenMai == id);

            if (promotion == null)
                return HttpNotFound();

            // Không xóa khuyến mãi đã có lịch sử sử dụng
            if (promotion.SuDungKhuyenMai.Any() ||
                promotion.DonHang.Any())
            {
                TempData["Error"] =
                    "Không thể xóa khuyến mãi đã được sử dụng hoặc liên kết với đơn hàng.";

                return RedirectToAction("Promotions");
            }

            db.KhuyenMai.Remove(promotion);
            db.SaveChanges();

            TempData["Success"] = "Xóa khuyến mãi thành công.";

            return RedirectToAction("Promotions");
        }


        // ==========================================
        // 7. BẬT / TẮT TRẠNG THÁI
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleStatus(int id)
        {
            var promotion = db.KhuyenMai.Find(id);

            if (promotion == null)
                return HttpNotFound();

            if (!promotion.TrangThai)
            {
                if (promotion.NgayKetThuc < DateTime.Now)
                {
                    TempData["Error"] =
                        "Không thể kích hoạt chương trình đã hết hạn.";

                    return RedirectToAction("Promotions");
                }

                if (promotion.GioiHanSuDung.HasValue &&
                    promotion.SoLanDaSuDung >= promotion.GioiHanSuDung.Value)
                {
                    TempData["Error"] =
                        "Khuyến mãi đã đạt giới hạn sử dụng.";

                    return RedirectToAction("Promotions");
                }
            }

            promotion.TrangThai = !promotion.TrangThai;

            db.SaveChanges();

            TempData["Success"] = promotion.TrangThai
                ? "Đã kích hoạt khuyến mãi."
                : "Đã ngừng kích hoạt khuyến mãi.";

            return RedirectToAction("Promotions");
        }


        // ==========================================
        // 8. KIỂM TRA DỮ LIỆU KHUYẾN MÃI
        // ==========================================
        private void ValidatePromotion(
            AdminPromotionFormViewModel model,
            int? excludeId)
        {
            if (model == null)
            {
                ModelState.AddModelError(
                    "",
                    "Dữ liệu khuyến mãi không hợp lệ.");

                return;
            }

            // Kiểm tra loại giảm giá
            if (model.LoaiGiamGia != "PhanTram" &&
                model.LoaiGiamGia != "SoTien")
            {
                ModelState.AddModelError(
                    "LoaiGiamGia",
                    "Loại giảm giá không hợp lệ.");
            }

            // Kiểm tra phần trăm
            if (model.LoaiGiamGia == "PhanTram" &&
                model.GiaTriGiam.HasValue &&
                model.GiaTriGiam.Value > 100)
            {
                ModelState.AddModelError(
                    "GiaTriGiam",
                    "Mức giảm phần trăm không được vượt quá 100%.");
            }

            // Kiểm tra giá trị giảm
            if (model.GiaTriGiam.HasValue &&
                model.GiaTriGiam.Value <= 0)
            {
                ModelState.AddModelError(
                    "GiaTriGiam",
                    "Giá trị giảm phải lớn hơn 0.");
            }

            // Kiểm tra đơn hàng tối thiểu
            if (model.GiaTriDonHangToiThieu.HasValue &&
                model.GiaTriDonHangToiThieu.Value < 0)
            {
                ModelState.AddModelError(
                    "GiaTriDonHangToiThieu",
                    "Giá trị đơn hàng tối thiểu không được âm.");
            }

            // Kiểm tra mức giảm tối đa
            if (model.GiamToiDa.HasValue &&
                model.GiamToiDa.Value <= 0)
            {
                ModelState.AddModelError(
                    "GiamToiDa",
                    "Mức giảm tối đa phải lớn hơn 0.");
            }

            // Kiểm tra ngày
            if (model.NgayBatDau.HasValue &&
                model.NgayKetThuc.HasValue &&
                model.NgayKetThuc.Value <= model.NgayBatDau.Value)
            {
                ModelState.AddModelError(
                    "NgayKetThuc",
                    "Ngày kết thúc phải sau ngày bắt đầu.");
            }

            // Kiểm tra giới hạn sử dụng
            if (model.GioiHanSuDung.HasValue &&
                model.GioiHanSuDung.Value <= 0)
            {
                ModelState.AddModelError(
                    "GioiHanSuDung",
                    "Giới hạn sử dụng phải lớn hơn 0.");
            }
        }


        // ==========================================
        // 9. GIẢI PHÓNG DATABASE
        // ==========================================
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}

using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using STAGEFX_PRO.Controllers.Base;
using Wedside_TVS_SFX.Models;

namespace STAGEFX_PRO.Controllers
{
    [AdminAuthorize]
    public class AdminReviewController : Controller
    {
        // =========================================
        // 1. DANH SÁCH ĐÁNH GIÁ & THỐNG KÊ & LỌC
        // =========================================
        public ActionResult Reviews(string search, string status, int? rating)
        {
            using (var db = new StageFxEntities())
            {
                var query = db.DanhGiaSanPham
                    .Include(r => r.SanPham)
                    .Include(r => r.NguoiDung)
                    .AsNoTracking()
                    .AsQueryable();

                // THỐNG KÊ
                ViewBag.TotalReviews = query.Count();
                ViewBag.PendingReviews = query.Count(r => !r.DaDuyet);
                ViewBag.ApprovedReviews = query.Count(r => r.DaDuyet);
                ViewBag.AvgRating = query.Any() ? Math.Round(query.Average(r => (double)r.SoSao), 1) : 0;

                // TÌM KIẾM (Tên sản phẩm, Tên người đánh giá, Nội dung nhận xét)
                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.Trim().ToLower();
                    query = query.Where(r =>
                        (r.SanPham != null && r.SanPham.TenSanPham.ToLower().Contains(search)) ||
                        (r.NguoiDung != null && r.NguoiDung.HoTen.ToLower().Contains(search)) ||
                        (r.NhanXet != null && r.NhanXet.ToLower().Contains(search))
                    );
                }

                // LỌC TRẠNG THÁI DUYỆT
                if (!string.IsNullOrEmpty(status))
                {
                    if (status == "Pending")
                        query = query.Where(r => !r.DaDuyet);
                    else if (status == "Approved")
                        query = query.Where(r => r.DaDuyet);
                }

                // LỌC THEO SỐ SAO
                if (rating.HasValue && rating.Value >= 1 && rating.Value <= 5)
                {
                    query = query.Where(r => r.SoSao == rating.Value);
                }

                ViewBag.SearchTerm = search;
                ViewBag.StatusFilter = status;
                ViewBag.RatingFilter = rating;

                var list = query
                    .OrderByDescending(r => r.NgayTao)
                    .ThenByDescending(r => r.MaDanhGia)
                    .ToList();

                ViewBag.TotalItems = list.Count;
                ViewBag.Title = "Quản lý đánh giá sản phẩm";

                return View("~/Views/Admin/Reviews.cshtml", list);
            }
        }

        // =========================================
        // 2. TOGGLE DUYỆT / ẨN ĐÁNH GIÁ (AJAX)
        // =========================================
        [HttpPost]
        public JsonResult ToggleApprove(int id)
        {
            try
            {
                using (var db = new StageFxEntities())
                {
                    var review = db.DanhGiaSanPham.FirstOrDefault(r => r.MaDanhGia == id);
                    if (review == null)
                    {
                        return Json(new { success = false, message = "Không tìm thấy đánh giá!" });
                    }

                    review.DaDuyet = !review.DaDuyet;
                    review.NgayCapNhat = DateTime.Now;

                    db.SaveChanges();

                    string statusText = review.DaDuyet ? "Đã duyệt" : "Đã ẩn";
                    return Json(new { success = true, isApproved = review.DaDuyet, message = $"Chuyển trạng thái sang '{statusText}' thành công!" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi cập nhật: " + ex.Message });
            }
        }

        // =========================================
        // 3. XÓA ĐÁNH GIÁ (AJAX)
        // =========================================
        [HttpPost]
        public JsonResult DeleteReview(int id)
        {
            try
            {
                using (var db = new StageFxEntities())
                {
                    var review = db.DanhGiaSanPham.FirstOrDefault(r => r.MaDanhGia == id);
                    if (review == null)
                    {
                        return Json(new { success = false, message = "Không tìm thấy đánh giá!" });
                    }

                    db.DanhGiaSanPham.Remove(review);
                    db.SaveChanges();

                    return Json(new { success = true, message = "Xóa đánh giá thành công!" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi xóa đánh giá: " + ex.Message });
            }
        }
    }
}
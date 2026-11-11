using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using STAGEFX_PRO.Controllers.Base;
using Wedside_TVS_SFX.Models;

namespace STAGEFX_PRO.Controllers
{
    [AdminAuthorize]
    public class AdminConsultationController : Controller
    {
        // =========================================
        // 1. DANH SÁCH YÊU CẦU TƯ VẤN & LỌC
        // =========================================
        public ActionResult Consultations(string search, string status)
        {
            using (var db = new StageFxEntities())
            {
                var query = db.YeuCauTuVan
                    .AsNoTracking()
                    .AsQueryable();

                // THỐNG KÊ (Đếm chuẩn cả HoanTat và DaHoanTat)
                ViewBag.TotalRequests = query.Count();
                ViewBag.NewRequests = query.Count(x => x.TrangThai == "Moi");
                ViewBag.ProcessingRequests = query.Count(x => x.TrangThai == "DangXuLy");
                ViewBag.CompletedRequests = query.Count(x => x.TrangThai == "HoanTat" || x.TrangThai == "DaHoanTat");

                // CHUẨN HÓA TỪ KHÓA
                search = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
                status = string.IsNullOrWhiteSpace(status) ? null : status.Trim();

                // TÌM KIẾM
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(x =>
                        (x.HoTen != null && x.HoTen.Contains(search)) ||
                        (x.Email != null && x.Email.Contains(search)) ||
                        (x.SoDienThoai != null && x.SoDienThoai.Contains(search))
                    );
                }

                // LỌC TRẠNG THÁI
                if (!string.IsNullOrEmpty(status))
                {
                    if (status == "HoanTat" || status == "DaHoanTat")
                    {
                        query = query.Where(x => x.TrangThai == "HoanTat" || x.TrangThai == "DaHoanTat");
                    }
                    else
                    {
                        query = query.Where(x => x.TrangThai == status);
                    }
                }

                ViewBag.SearchTerm = search;
                ViewBag.StatusFilter = status;

                var model = query
                    .OrderByDescending(x => x.NgayTao)
                    .ThenByDescending(x => x.MaYeuCauTuVan)
                    .ToList();

                ViewBag.TotalItems = model.Count;
                ViewBag.Title = "Quản lý yêu cầu tư vấn";

                return View("~/Views/Admin/Consultations.cshtml", model);
            }
        }

        // =========================================
        // 2. LẤY CHI TIẾT YÊU CẦU (AJAX GET)
        // =========================================
        [HttpGet]
        public JsonResult GetDetail(int id)
        {
            try
            {
                using (var db = new StageFxEntities())
                {
                    var item = db.YeuCauTuVan
                        .AsNoTracking()
                        .FirstOrDefault(x => x.MaYeuCauTuVan == id);

                    if (item == null)
                    {
                        return Json(new { success = false, message = "Không tìm thấy yêu cầu tư vấn!" }, JsonRequestBehavior.AllowGet);
                    }

                    var data = new
                    {
                        MaYeuCauTuVan = item.MaYeuCauTuVan,
                        HoTen = item.HoTen ?? "",
                        Email = item.Email ?? "",
                        SoDienThoai = item.SoDienThoai ?? "---",
                        ChuDe = item.ChuDe ?? "",
                        NoiDung = item.NoiDung ?? "",
                        TrangThai = item.TrangThai ?? "Moi",
                        PhanHoi = item.PhanHoi ?? "",
                        NgayTao = item.NgayTao.ToString("dd/MM/yyyy HH:mm")
                    };

                    return Json(new { success = true, data = data }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // =========================================
        // 3. CẬP NHẬT TRẠNG THÁI VÀ PHẢN HỒI (AJAX POST)
        // =========================================
        [HttpPost]
        public JsonResult UpdateStatus(int id, string status, string response)
        {
            try
            {
                using (var db = new StageFxEntities())
                {
                    var item = db.YeuCauTuVan.FirstOrDefault(x => x.MaYeuCauTuVan == id);
                    if (item == null)
                    {
                        return Json(new { success = false, message = "Không tìm thấy yêu cầu tư vấn!" });
                    }

                    // Chuyển DaHoanTat thành HoanTat để không bị lỗi CHECK constraint SQL
                    if (status == "DaHoanTat")
                    {
                        status = "HoanTat";
                    }

                    item.TrangThai = status;
                    item.PhanHoi = response;

                    if (Session["MaNguoiDung"] != null)
                    {
                        int userId = Convert.ToInt32(Session["MaNguoiDung"]);
                        if (db.NguoiDung.Any(u => u.MaNguoiDung == userId))
                        {
                            item.MaNhanVienPhuTrach = userId;
                        }
                    }

                    if (status == "HoanTat")
                    {
                        item.NgayHoanTat = DateTime.Now;
                    }
                    else
                    {
                        item.NgayHoanTat = null;
                    }

                    db.SaveChanges();
                    return Json(new { success = true, message = "Cập nhật thành công!" });
                }
            }
            catch (Exception ex)
            {
                string innerErr = ex.InnerException != null
                    ? (ex.InnerException.InnerException != null ? ex.InnerException.InnerException.Message : ex.InnerException.Message)
                    : ex.Message;
                return Json(new { success = false, message = "Lỗi Database: " + innerErr });
            }
        }
    }
}
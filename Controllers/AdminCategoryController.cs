
using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using STAGEFX_PRO.Controllers.Base;
using Wedside_TVS_SFX.Models;
using Wedside_TVS_SFX.ViewModels.Admin;

namespace STAGEFX_PRO.Controllers
{
    [AdminAuthorize]
    public class AdminCategoryController : Controller
    {
        private readonly StageFxEntities db = new StageFxEntities();

        private const int PageSize = 10;
        private const int MaxImageSize = 5 * 1024 * 1024;

        private const string CategoryImageFolder =
            "~/Content/uploads/categories/";

        // =====================================================
        // DANH SACH DANH MUC
        // URL: /AdminCategory/Categories
        // =====================================================

        public ActionResult Categories(
            string search,
            string status,
            int page = 1)
        {
            page = Math.Max(1, page);

            var query = db.DanhMuc
                .AsNoTracking()
                .AsQueryable();

            // Tim kiem theo ten danh muc
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(x =>
                    x.TenDanhMuc.Contains(search));
            }

            // Loc trang thai
            if (status == "active")
            {
                query = query.Where(x => x.TrangThai);
            }
            else if (status == "inactive")
            {
                query = query.Where(x => !x.TrangThai);
            }

            // Thong ke
            var totalCategories = db.DanhMuc.Count();

            var activeCategories = db.DanhMuc
                .Count(x => x.TrangThai);

            var inactiveCategories =
                totalCategories - activeCategories;

            var totalProducts = db.DanhMuc
                .SelectMany(x => x.SanPham)
                .Count();

            // Phan trang
            var totalItems = query.Count();

            var totalPages = (int)Math.Ceiling(
                totalItems / (double)PageSize);

            if (totalPages > 0 && page > totalPages)
            {
                page = totalPages;
            }

            // Lay danh sach danh muc
            var categories = query
                .OrderByDescending(x => x.NgayTao)
                .ThenByDescending(x => x.MaDanhMuc)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .Select(x => new AdminCategoryItemViewModel
                {
                    MaDanhMuc = x.MaDanhMuc,
                    TenDanhMuc = x.TenDanhMuc,
                    MoTa = x.MoTa,
                    TrangThai = x.TrangThai,
                    NgayTao = x.NgayTao,
                    SoLuongSanPham = x.SanPham.Count(),
                    AnhDanhMuc = x.AnhDanhMuc
                })
                .ToList();

            var model = new AdminCategoryViewModel
            {
                Categories = categories,
                SearchTerm = search,
                StatusFilter = status,
                CurrentPage = page,
                PageSize = PageSize,
                TotalItems = totalItems,
                TotalPages = totalPages,
                TotalCategories = totalCategories,
                ActiveCategories = activeCategories,
                InactiveCategories = inactiveCategories,
                TotalProducts = totalProducts
            };

            return View(
                "~/Views/Admin/Categories.cshtml",
                model);
        }

        // =====================================================
        // THEM DANH MUC - GET
        // =====================================================

        [HttpGet]
        public ActionResult Create()
        {
            var model = new AdminCategoryFormViewModel
            {
                TrangThai = true,
                NgayTao = DateTime.Now
            };

            return View(
                "~/Views/Admin/CategoryCreate.cshtml",
                model);
        }

        // =====================================================
        // THEM DANH MUC - POST
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            AdminCategoryFormViewModel model)
        {
            if (model == null)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest);
            }

            ValidateCategoryName(
                model.TenDanhMuc,
                null);

            ValidateCategoryDescription(model.MoTa);
            ValidateCategoryImage(model.AnhUpload);

            if (!ModelState.IsValid)
            {
                return View(
                    "~/Views/Admin/CategoryCreate.cshtml",
                    model);
            }

            string savedImagePath = null;

            try
            {
                // Luu anh neu nguoi dung co chon
                if (model.AnhUpload != null &&
                    model.AnhUpload.ContentLength > 0)
                {
                    savedImagePath =
                        SaveCategoryImage(model.AnhUpload);
                }

                var category = new DanhMuc
                {
                    TenDanhMuc = model.TenDanhMuc.Trim(),
                    MoTa = model.MoTa,
                    TrangThai = model.TrangThai,
                    NgayTao = DateTime.Now,
                    AnhDanhMuc = savedImagePath
                };

                db.DanhMuc.Add(category);
                db.SaveChanges();

                TempData["SuccessMessage"] =
                    "Thêm danh mục thành công.";

                return RedirectToAction("Categories");
            }
            catch (Exception)
            {
                // Don anh neu luu database that bai
                if (!string.IsNullOrWhiteSpace(savedImagePath))
                {
                    DeleteCategoryImage(savedImagePath);
                }

                ModelState.AddModelError(
                    "",
                    "Không thể thêm danh mục. Vui lòng kiểm tra dữ liệu.");

                return View(
                    "~/Views/Admin/CategoryCreate.cshtml",
                    model);
            }
        }

        // =====================================================
        // CHINH SUA DANH MUC - GET
        // =====================================================

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var category = db.DanhMuc.Find(id);

            if (category == null)
            {
                return HttpNotFound(
                    "Không tìm thấy danh mục.");
            }

            var model = new AdminCategoryFormViewModel
            {
                MaDanhMuc = category.MaDanhMuc,
                TenDanhMuc = category.TenDanhMuc,
                MoTa = category.MoTa,
                TrangThai = category.TrangThai,
                NgayTao = category.NgayTao,
                AnhDanhMuc = category.AnhDanhMuc
            };

            return View(
                "~/Views/Admin/CategoryEdit.cshtml",
                model);
        }

        // =====================================================
        // CHINH SUA DANH MUC - POST
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            AdminCategoryFormViewModel model)
        {
            if (model == null)
            {
                return new HttpStatusCodeResult(
                    HttpStatusCode.BadRequest);
            }

            var category = db.DanhMuc.Find(
                model.MaDanhMuc);

            if (category == null)
            {
                return HttpNotFound(
                    "Không tìm thấy danh mục.");
            }

            ValidateCategoryName(
                model.TenDanhMuc,
                model.MaDanhMuc);

            ValidateCategoryDescription(model.MoTa);
            ValidateCategoryImage(model.AnhUpload);

            if (!ModelState.IsValid)
            {
                model.AnhDanhMuc = category.AnhDanhMuc;

                return View(
                    "~/Views/Admin/CategoryEdit.cshtml",
                    model);
            }

            string oldImagePath = category.AnhDanhMuc;
            string newImagePath = null;

            try
            {
                // Chi luu anh moi khi nguoi dung chon anh
                if (model.AnhUpload != null &&
                    model.AnhUpload.ContentLength > 0)
                {
                    newImagePath =
                        SaveCategoryImage(model.AnhUpload);
                }

                category.TenDanhMuc =
                    model.TenDanhMuc.Trim();

                category.MoTa = model.MoTa;
                category.TrangThai = model.TrangThai;

                if (!string.IsNullOrWhiteSpace(newImagePath))
                {
                    category.AnhDanhMuc = newImagePath;
                }

                // Khong thay doi NgayTao khi chinh sua

                db.SaveChanges();

                // Chi xoa anh cu sau khi database da cap nhat
                if (!string.IsNullOrWhiteSpace(newImagePath) &&
                    !string.IsNullOrWhiteSpace(oldImagePath))
                {
                    DeleteCategoryImage(oldImagePath);
                }

                TempData["SuccessMessage"] =
                    "Cập nhật danh mục thành công.";

                return RedirectToAction("Categories");
            }
            catch (Exception)
            {
                // Don anh moi neu cap nhat that bai
                if (!string.IsNullOrWhiteSpace(newImagePath))
                {
                    DeleteCategoryImage(newImagePath);
                }

                ModelState.AddModelError(
                    "",
                    "Không thể cập nhật danh mục. Vui lòng thử lại.");

                model.AnhDanhMuc = oldImagePath;

                return View(
                    "~/Views/Admin/CategoryEdit.cshtml",
                    model);
            }
        }

        // =====================================================
        // XOA DANH MUC
        // Chi xoa khi khong co san pham lien quan
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var category = db.DanhMuc.Find(id);

            if (category == null)
            {
                TempData["ErrorMessage"] =
                    "Danh mục không tồn tại.";

                return RedirectToAction("Categories");
            }

            try
            {
                // Tai san pham lien quan
                db.Entry(category)
                    .Collection(x => x.SanPham)
                    .Load();

                if (category.SanPham.Any())
                {
                    TempData["ErrorMessage"] =
                        "Không thể xóa danh mục đang có sản phẩm. " +
                        "Vui lòng chuyển sản phẩm sang danh mục khác trước.";

                    return RedirectToAction("Categories");
                }

                string imagePath = category.AnhDanhMuc;

                db.DanhMuc.Remove(category);
                db.SaveChanges();

                // Xoa anh sau khi xoa danh muc thanh cong
                if (!string.IsNullOrWhiteSpace(imagePath))
                {
                    DeleteCategoryImage(imagePath);
                }

                TempData["SuccessMessage"] =
                    "Xóa danh mục thành công.";
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] =
                    "Không thể xóa danh mục do dữ liệu đang được sử dụng.";
            }

            return RedirectToAction("Categories");
        }

        // =====================================================
        // BAT / TAT TRANG THAI
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleStatus(int id)
        {
            var category = db.DanhMuc.Find(id);

            if (category == null)
            {
                TempData["ErrorMessage"] =
                    "Không tìm thấy danh mục.";

                return RedirectToAction("Categories");
            }

            try
            {
                category.TrangThai = !category.TrangThai;

                db.SaveChanges();

                TempData["SuccessMessage"] =
                    category.TrangThai
                        ? "Đã kích hoạt danh mục."
                        : "Đã ngừng hoạt động danh mục.";
            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] =
                    "Không thể thay đổi trạng thái danh mục.";
            }

            return RedirectToAction("Categories");
        }

        // =====================================================
        // KIEM TRA TEN DANH MUC
        // Ten toi da 150 ky tu
        // =====================================================

        private void ValidateCategoryName(
            string categoryName,
            int? excludeId)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                ModelState.AddModelError(
                    "TenDanhMuc",
                    "Vui lòng nhập tên danh mục.");

                return;
            }

            categoryName = categoryName.Trim();

            if (categoryName.Length > 150)
            {
                ModelState.AddModelError(
                    "TenDanhMuc",
                    "Tên danh mục không được vượt quá 150 ký tự.");

                return;
            }

            var exists = db.DanhMuc.Any(x =>
                x.TenDanhMuc == categoryName &&
                (!excludeId.HasValue ||
                 x.MaDanhMuc != excludeId.Value));

            if (exists)
            {
                ModelState.AddModelError(
                    "TenDanhMuc",
                    "Tên danh mục này đã tồn tại.");
            }
        }

        // =====================================================
        // KIEM TRA MO TA
        // Mo ta toi da 1000 ky tu
        // =====================================================

        private void ValidateCategoryDescription(string description)
        {
            if (!string.IsNullOrEmpty(description) &&
                description.Length > 1000)
            {
                ModelState.AddModelError(
                    "MoTa",
                    "Mô tả không được vượt quá 1000 ký tự.");
            }
        }

        // =====================================================
        // KIEM TRA ANH DANH MUC
        // JPG / JPEG / PNG / WEBP
        // Toi da 5 MB
        // =====================================================

        private void ValidateCategoryImage(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0)
            {
                return;
            }

            if (file.ContentLength > MaxImageSize)
            {
                ModelState.AddModelError(
                    "AnhUpload",
                    "Dung lượng ảnh không được vượt quá 5 MB.");

                return;
            }

            string extension = Path.GetExtension(file.FileName);

            if (string.IsNullOrWhiteSpace(extension))
            {
                ModelState.AddModelError(
                    "AnhUpload",
                    "File ảnh không hợp lệ.");

                return;
            }

            extension = extension.ToLowerInvariant();

            if (extension != ".jpg" &&
                extension != ".jpeg" &&
                extension != ".png" &&
                extension != ".webp")
            {
                ModelState.AddModelError(
                    "AnhUpload",
                    "Chỉ chấp nhận ảnh JPG, JPEG, PNG hoặc WEBP.");

                return;
            }

            // Kiem tra chu ky file, khong chi dua vao duoi file
            try
            {
                byte[] header = new byte[12];
                int totalRead = 0;

                file.InputStream.Position = 0;

                while (totalRead < header.Length)
                {
                    int read = file.InputStream.Read(
                        header,
                        totalRead,
                        header.Length - totalRead);

                    if (read <= 0)
                        break;

                    totalRead += read;
                }

                file.InputStream.Position = 0;

                bool isJpeg =
                    totalRead >= 3 &&
                    header[0] == 0xFF &&
                    header[1] == 0xD8 &&
                    header[2] == 0xFF;

                bool isPng =
                    totalRead >= 8 &&
                    header[0] == 0x89 &&
                    header[1] == 0x50 &&
                    header[2] == 0x4E &&
                    header[3] == 0x47 &&
                    header[4] == 0x0D &&
                    header[5] == 0x0A &&
                    header[6] == 0x1A &&
                    header[7] == 0x0A;

                bool isWebp =
                    totalRead >= 12 &&
                    header[0] == 'R' &&
                    header[1] == 'I' &&
                    header[2] == 'F' &&
                    header[3] == 'F' &&
                    header[8] == 'W' &&
                    header[9] == 'E' &&
                    header[10] == 'B' &&
                    header[11] == 'P';

                bool validImage =
                    (extension == ".jpg" || extension == ".jpeg")
                        ? isJpeg
                        : extension == ".png"
                            ? isPng
                            : isWebp;

                if (!validImage)
                {
                    ModelState.AddModelError(
                        "AnhUpload",
                        "Nội dung file không đúng định dạng ảnh đã chọn.");
                }
            }
            catch (Exception)
            {
                ModelState.AddModelError(
                    "AnhUpload",
                    "Không thể kiểm tra file ảnh. Vui lòng chọn ảnh khác.");
            }
            finally
            {
                if (file.InputStream.CanSeek)
                {
                    file.InputStream.Position = 0;
                }
            }
        }

        // =====================================================
        // LUU ANH DANH MUC
        // =====================================================

        private string SaveCategoryImage(HttpPostedFileBase file)
        {
            string folder = Server.MapPath(CategoryImageFolder);

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            string extension = Path.GetExtension(file.FileName)
                .ToLowerInvariant();

            string fileName =
                Guid.NewGuid().ToString("N") + extension;

            string fullPath = Path.Combine(folder, fileName);

            file.SaveAs(fullPath);

            return CategoryImageFolder + fileName;
        }

        // =====================================================
        // XOA FILE ANH DANH MUC
        // Chi xoa file nam trong thu muc categories
        // =====================================================

        private void DeleteCategoryImage(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return;
            }

            if (!relativePath.StartsWith(
                CategoryImageFolder,
                StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            string fileName =
                relativePath.Substring(CategoryImageFolder.Length);

            // Khong cho phep duong dan con
            if (string.IsNullOrWhiteSpace(fileName) ||
                fileName != Path.GetFileName(fileName))
            {
                return;
            }

            try
            {
                string fullPath = Server.MapPath(relativePath);

                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
            }
            catch (IOException)
            {
                // Khong lam gian doan thao tac danh muc
            }
            catch (UnauthorizedAccessException)
            {
                // Khong co quyen xoa file
            }
        }

        // =====================================================
        // DISPOSE
        // =====================================================

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

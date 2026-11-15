using STAGEFX_PRO.Controllers.Base;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Wedside_TVS_SFX.Models;
using Wedside_TVS_SFX.ViewModels.Admin;

namespace STAGEFX_PRO.Controllers
{
    [AdminAuthorize]
    public class AdminProductController : Controller
    {
        private readonly StageFxEntities db = new StageFxEntities();

        private const int PAGE_SIZE = 10;
        private const int MAX_IMAGE_BYTES = 5 * 1024 * 1024;
        private const int MAX_IMAGE_COUNT = 10;

        private const string PRODUCT_IMAGE_FOLDER =
            "~/Content/uploads/products/";

        private static readonly string[] IMAGE_EXTENSIONS =
        {
            ".jpg", ".jpeg", ".png", ".webp"
        };

        private static readonly string[] IMAGE_MIME_TYPES =
        {
            "image/jpeg", "image/png", "image/webp"
        };

        // =========================================================
        // 1. DANH SÁCH SẢN PHẨM
        // =========================================================

        [HttpGet]
        public ActionResult Products(
            string search,
            int? categoryId,
            string status,
            int page = 1)
        {
            try
            {
                int totalProducts = db.SanPham.Count();
                int activeProducts = db.SanPham.Count(x => x.TrangThai);
                int inactiveProducts = db.SanPham.Count(x => !x.TrangThai);
                int outOfStockProducts = db.SanPham.Count(x => x.SoLuongTon <= 0);

                IQueryable<SanPham> query =
                    db.SanPham.AsNoTracking();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    string keyword = search.Trim();

                    query = query.Where(x =>
                        (x.TenSanPham != null &&
                         x.TenSanPham.Contains(keyword)) ||
                        (x.SKU != null &&
                         x.SKU.Contains(keyword)));
                }

                if (categoryId.HasValue)
                {
                    query = query.Where(
                        x => x.MaDanhMuc == categoryId.Value);
                }

                if (status == "active")
                {
                    query = query.Where(x => x.TrangThai);
                }
                else if (status == "inactive")
                {
                    query = query.Where(x => !x.TrangThai);
                }

                int totalItems = query.Count();

                int totalPages = Math.Max(
                    1,
                    (int)Math.Ceiling(totalItems / (double)PAGE_SIZE));

                page = Math.Max(1, Math.Min(page, totalPages));

                var products = query
                    .OrderByDescending(x => x.NgayTao)
                    .ThenByDescending(x => x.MaSanPham)
                    .Skip((page - 1) * PAGE_SIZE)
                    .Take(PAGE_SIZE)
                    .Select(x => new AdminProductItemViewModel
                    {
                        MaSanPham = x.MaSanPham,
                        SKU = x.SKU,
                        TenSanPham = x.TenSanPham,
                        TenDanhMuc = x.DanhMuc != null
                            ? x.DanhMuc.TenDanhMuc
                            : null,
                        GiaBan = x.GiaBan,
                        GiaThueNgay = x.GiaThueNgay,
                        SoLuongTon = x.SoLuongTon,
                        TrangThai = x.TrangThai,
                        NgayTao = x.NgayTao
                    })
                    .ToList();

                var categories = db.DanhMuc
                    .AsNoTracking()
                    .Where(x => x.TrangThai)
                    .OrderBy(x => x.TenDanhMuc)
                    .Select(x => new SelectListItem
                    {
                        Value = x.MaDanhMuc.ToString(),
                        Text = x.TenDanhMuc
                    })
                    .ToList();

                var model = new AdminProductListViewModel
                {
                    Products = products,
                    Categories = categories,
                    SearchTerm = search,
                    CategoryId = categoryId,
                    Status = status,
                    TotalProducts = totalProducts,
                    ActiveProducts = activeProducts,
                    InactiveProducts = inactiveProducts,
                    OutOfStockProducts = outOfStockProducts,
                    TotalItems = totalItems,
                    CurrentPage = page,
                    PageSize = PAGE_SIZE,
                    TotalPages = totalPages
                };

                return View("~/Views/Admin/Products.cshtml", model);
            }
            catch (Exception ex)
            {
                LogError("Products", ex);

                TempData["ProductError"] =
                    "Không thể tải danh sách sản phẩm. Vui lòng thử lại.";

                return View(
                    "~/Views/Admin/Products.cshtml",
                    new AdminProductListViewModel
                    {
                        Products = new List<AdminProductItemViewModel>(),
                        Categories = new List<SelectListItem>(),
                        PageSize = PAGE_SIZE,
                        CurrentPage = 1,
                        TotalPages = 1
                    });
            }
        }

        // =========================================================
        // 2. TẠO SẢN PHẨM - GET
        // =========================================================

        [HttpGet]
        public ActionResult CreateProduct()
        {
            try
            {
                var model = PrepareForm(
                    new AdminProductFormViewModel());

                return View("~/Views/Admin/ProductCreate.cshtml", model);
            }
            catch (Exception ex)
            {
                LogError("CreateProduct", ex);

                TempData["ProductError"] =
                    "Không thể mở biểu mẫu thêm sản phẩm.";

                return RedirectToAction("Products");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateProduct(AdminProductFormViewModel model)
        {
            NormalizePriceValues(model);
            ValidateProduct(model, null);

            if (!ModelState.IsValid)
            {
                return View(
                    "~/Views/Admin/ProductCreate.cshtml",
                    PrepareForm(model));
            }

            var savedFiles = new List<string>();

            try
            {
                var product = new SanPham
                {
                    SKU = model.SKU.Trim(),
                    TenSanPham = model.TenSanPham.Trim(),
                    MaDanhMuc = model.MaDanhMuc.Value,
                    MoTaNgan = model.MoTaNgan,
                    MoTaChiTiet = model.MoTaChiTiet,
                    ThongSoKyThuat = model.ThongSoKyThuat,
                    GiaBan = model.GiaBan,
                    GiaThueNgay = model.GiaThueNgay,
                    SoLuongTon = model.SoLuongTon,
                    TrangThai = model.TrangThai,
                    NgayTao = DateTime.Now
                };

                db.SanPham.Add(product);
                db.SaveChanges();

                var newImages = SaveImages(
                    model.NewImages,
                    product.MaSanPham,
                    savedFiles);

                SetMainImageAfterCreate(
                    newImages,
                    model.MainImageNewIndex);

                db.SaveChanges();

                TempData["ProductSuccess"] =
                    "Đã thêm sản phẩm thành công.";

                return RedirectToAction("Products");
            }
            catch (Exception ex)
            {
                DeleteFiles(savedFiles);
                LogError("CreateProduct", ex);

                ModelState.AddModelError(
                    "",
                    "Không thể thêm sản phẩm. Vui lòng thử lại.");

                return View(
                    "~/Views/Admin/ProductCreate.cshtml",
                    PrepareForm(model));
            }
        }

        // =========================================================
        // 3. CHỈNH SỬA SẢN PHẨM - GET
        // =========================================================

        [HttpGet]
        public ActionResult EditProduct(int id)
        {
            try
            {
                var product = db.SanPham
                    .AsNoTracking()
                    .FirstOrDefault(x => x.MaSanPham == id);

                if (product == null)
                    return HttpNotFound();

                var model = ToForm(product);

                return View("~/Views/Admin/ProductEdit.cshtml", model);
            }
            catch (Exception ex)
            {
                LogError("EditProduct", ex);

                TempData["ProductError"] =
                    "Không thể tải thông tin sản phẩm.";

                return RedirectToAction("Products");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProduct(AdminProductFormViewModel model)
        {
            NormalizePriceValues(model);
            ValidateProduct(model, model == null ? (int?)null : model.MaSanPham);

            if (!ModelState.IsValid)
            {
                return View(
                    "~/Views/Admin/ProductEdit.cshtml",
                    PrepareForm(model));
            }

            var savedFiles = new List<string>();
            var removedFiles = new List<string>();

            try
            {
                var product = db.SanPham
                    .FirstOrDefault(x => x.MaSanPham == model.MaSanPham);

                if (product == null)
                    return HttpNotFound();

                product.SKU = model.SKU.Trim();
                product.TenSanPham = model.TenSanPham.Trim();
                product.MaDanhMuc = model.MaDanhMuc.Value;
                product.MoTaNgan = model.MoTaNgan;
                product.MoTaChiTiet = model.MoTaChiTiet;
                product.ThongSoKyThuat = model.ThongSoKyThuat;
                product.GiaBan = model.GiaBan;
                product.GiaThueNgay = model.GiaThueNgay;
                product.SoLuongTon = model.SoLuongTon;
                product.TrangThai = model.TrangThai;
                product.NgayCapNhat = DateTime.Now;

                removedFiles = RemoveImages(
                    product.MaSanPham,
                    model.RemoveImageIds);

                var newImages = SaveImages(
                    model.NewImages,
                    product.MaSanPham,
                    savedFiles);

                db.SaveChanges();

                SetMainImageAfterEdit(
                    product.MaSanPham,
                    model.MainImageId,
                    model.MainImageNewIndex,
                    newImages);

                db.SaveChanges();
                DeleteFiles(removedFiles);

                TempData["ProductSuccess"] =
                    "Đã cập nhật sản phẩm thành công.";

                return RedirectToAction(
                    "ProductDetail",
                    new { id = product.MaSanPham });
            }
            catch (Exception ex)
            {
                DeleteFiles(savedFiles);
                LogError("EditProduct", ex);

                ModelState.AddModelError(
                    "",
                    "Không thể cập nhật sản phẩm. Vui lòng thử lại.");

                return View(
                    "~/Views/Admin/ProductEdit.cshtml",
                    PrepareForm(model));
            }
        }

        // =========================================================
        // 4. CHI TIẾT SẢN PHẨM - GET
        // =========================================================

        [HttpGet]
        public ActionResult ProductDetail(int id)
        {
            try
            {
                var product = db.SanPham
                    .AsNoTracking()
                    .Include(x => x.DanhMuc)
                    .Include(x => x.HinhAnhSanPham)
                    .FirstOrDefault(x => x.MaSanPham == id);

                if (product == null)
                    return HttpNotFound();

                var model = new AdminProductDetailViewModel
                {
                    MaSanPham = product.MaSanPham,
                    SKU = product.SKU,
                    TenSanPham = product.TenSanPham,

                    TenDanhMuc = product.DanhMuc != null
                        ? product.DanhMuc.TenDanhMuc
                        : "Chưa phân loại",

                    MoTaNgan = product.MoTaNgan,
                    MoTaChiTiet = product.MoTaChiTiet,
                    ThongSoKyThuat = product.ThongSoKyThuat,

                    GiaBan = product.GiaBan,
                    GiaThueNgay = product.GiaThueNgay,
                    SoLuongTon = product.SoLuongTon,
                    TrangThai = product.TrangThai,

                    NgayTao = product.NgayTao,
                    NgayCapNhat = product.NgayCapNhat,

                    Images = product.HinhAnhSanPham
                        .OrderBy(x => x.ThuTuHienThi)
                        .Select(ToImageViewModel)
                        .ToList(),

                    InventoryHistory = db.LichSuKho
    .AsNoTracking()
    .Where(x => x.MaSanPham == id)
    .OrderByDescending(x => x.NgayTao)
    .ToList()
    .Select(x => new AdminInventoryHistoryItemViewModel
    {
        MaGiaoDichKho = x.MaGiaoDichKho,
        SKU = x.SanPham != null ? x.SanPham.SKU : "",
        TenSanPham = x.SanPham != null
            ? x.SanPham.TenSanPham
            : "",
        LoaiGiaoDich = x.LoaiGiaoDich,
        SoLuong = x.SoLuong,
        TonTruoc = x.TonTruoc,
        TonSau = x.TonSau,
        NguoiThucHien = x.NguoiDung != null
            ? x.NguoiDung.HoTen
            : "Hệ thống",
        GhiChu = x.GhiChu,
        NgayTao = x.NgayTao
    })
    .ToList()
                };

                return View(
                    "~/Views/Admin/ProductDetail.cshtml",
                    model);
            }
            catch (Exception ex)
            {
                LogError("ProductDetail", ex);

                TempData["ProductError"] =
                    "Không thể tải chi tiết sản phẩm.";

                return RedirectToAction("Products");
            }
        }


        // =========================================================
        // 5. CHỌN ẢNH CHÍNH
        // =========================================================

        private void SetMainImageAfterCreate(
            IList<HinhAnhSanPham> newImages,
            int? selectedNewIndex)
        {
            if (newImages == null || newImages.Count == 0)
                return;

            int selectedIndex =
                selectedNewIndex.HasValue &&
                selectedNewIndex.Value >= 0 &&
                selectedNewIndex.Value < newImages.Count
                    ? selectedNewIndex.Value
                    : 0;

            for (int i = 0; i < newImages.Count; i++)
            {
                newImages[i].LaAnhChinh = (i == selectedIndex);
            }
        }

        private void SetMainImageAfterEdit(
            int productId,
            int? existingImageId,
            int? selectedNewIndex,
            IList<HinhAnhSanPham> newImages)
        {
            var images = db.HinhAnhSanPham
                .Where(x => x.MaSanPham == productId)
                .ToList();

            if (images.Count == 0)
                return;

            HinhAnhSanPham selectedImage = null;

            // Ưu tiên ảnh mới nếu người dùng chọn ảnh mới làm ảnh chính.
            if (selectedNewIndex.HasValue &&
                newImages != null &&
                selectedNewIndex.Value >= 0 &&
                selectedNewIndex.Value < newImages.Count)
            {
                int newImageId =
                    newImages[selectedNewIndex.Value].MaHinhAnh;

                selectedImage = images.FirstOrDefault(
                    x => x.MaHinhAnh == newImageId);
            }

            // Nếu không chọn ảnh mới, thử dùng ảnh cũ.
            if (selectedImage == null && existingImageId.HasValue)
            {
                selectedImage = images.FirstOrDefault(
                    x => x.MaHinhAnh == existingImageId.Value);
            }

            // Nếu ảnh được chọn không còn tồn tại, chọn ảnh đầu tiên.
            if (selectedImage == null)
            {
                selectedImage = images
                    .OrderBy(x => x.ThuTuHienThi)
                    .First();
            }

            foreach (var image in images)
            {
                image.LaAnhChinh =
                    image.MaHinhAnh == selectedImage.MaHinhAnh;
            }
        }

        // =========================================================
        // 6. BẬT / TẮT KINH DOANH
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ToggleProductStatus(int id)
        {
            try
            {
                var product = db.SanPham
                    .FirstOrDefault(x => x.MaSanPham == id);

                if (product == null)
                    return HttpNotFound();

                product.TrangThai = !product.TrangThai;
                product.NgayCapNhat = DateTime.Now;

                db.SaveChanges();

                TempData["ProductSuccess"] = product.TrangThai
                    ? "Đã kích hoạt sản phẩm."
                    : "Đã ngừng kinh doanh sản phẩm.";

                return RedirectToAction("Products");
            }
            catch (Exception ex)
            {
                LogError("ToggleProductStatus", ex);

                TempData["ProductError"] =
                    "Không thể thay đổi trạng thái sản phẩm.";

                return RedirectToAction("Products");
            }
        }

        // =========================================================
        // 7. XÓA SẢN PHẨM AN TOÀN
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteProduct(int id)
        {
            try
            {
                var product = db.SanPham
                    .FirstOrDefault(x => x.MaSanPham == id);

                if (product == null)
                    return HttpNotFound();

                bool referenced =
                    db.ChiTietDonHang.Any(x => x.MaSanPham == id) ||
                    db.ChiTietGioHang.Any(x => x.MaSanPham == id) ||
                    db.DanhGiaSanPham.Any(x => x.MaSanPham == id) ||
                    db.HinhAnhSanPham.Any(x => x.MaSanPham == id) ||
                    db.LichSuKho.Any(x => x.MaSanPham == id);

                if (referenced)
                {
                    product.TrangThai = false;
                    product.NgayCapNhat = DateTime.Now;

                    db.SaveChanges();

                    TempData["ProductSuccess"] =
                        "Sản phẩm có dữ liệu liên quan nên đã được ngừng kinh doanh thay vì xóa.";

                    return RedirectToAction("Products");
                }

                db.SanPham.Remove(product);
                db.SaveChanges();

                TempData["ProductSuccess"] =
                    "Đã xóa sản phẩm thành công.";

                return RedirectToAction("Products");
            }
            catch (Exception ex)
            {
                LogError("DeleteProduct", ex);

                TempData["ProductError"] =
                    "Không thể xóa sản phẩm. Vui lòng thử lại.";

                return RedirectToAction("Products");
            }
        }

        // =========================================================
        // 8. CHUẨN BỊ VIEWMODEL
        // =========================================================

        private AdminProductFormViewModel PrepareForm(
            AdminProductFormViewModel model)
        {
            if (model == null)
                model = new AdminProductFormViewModel();

            model.Categories = db.DanhMuc
                .AsNoTracking()
                .Where(x => x.TrangThai)
                .OrderBy(x => x.TenDanhMuc)
                .Select(x => new SelectListItem
                {
                    Value = x.MaDanhMuc.ToString(),
                    Text = x.TenDanhMuc,
                    Selected = model.MaDanhMuc == x.MaDanhMuc
                })
                .ToList();

            if (model.MaSanPham == 0)
            {
                model.ExistingImages =
                    new List<AdminProductImageViewModel>();
            }
            else
            {
                var imageEntities = db.HinhAnhSanPham
                    .AsNoTracking()
                    .Where(x => x.MaSanPham == model.MaSanPham)
                    .OrderBy(x => x.ThuTuHienThi)
                    .ToList();

                model.ExistingImages = imageEntities
                    .Select(ToImageViewModel)
                    .ToList();
            }

            return model;
        }

        private AdminProductFormViewModel ToForm(SanPham product)
        {
            if (product == null)
                return null;

            var model = new AdminProductFormViewModel
            {
                MaSanPham = product.MaSanPham,
                SKU = product.SKU,
                TenSanPham = product.TenSanPham,
                MaDanhMuc = product.MaDanhMuc,
                MoTaNgan = product.MoTaNgan,
                MoTaChiTiet = product.MoTaChiTiet,
                ThongSoKyThuat = product.ThongSoKyThuat,
                GiaBan = product.GiaBan,
                GiaThueNgay = product.GiaThueNgay,
                SoLuongTon = product.SoLuongTon,
                TrangThai = product.TrangThai
            };

            return PrepareForm(model);
        }

        // =========================================================
        // 9. VALIDATE DỮ LIỆU SẢN PHẨM
        // =========================================================

        private void ValidateProduct(
            AdminProductFormViewModel model,
            int? productId)
        {
            if (model == null)
            {
                ModelState.AddModelError(
                    "",
                    "Dữ liệu sản phẩm không hợp lệ.");
                return;
            }

            if (!string.IsNullOrWhiteSpace(model.SKU))
            {
                string sku = model.SKU.Trim();

                bool skuExists = db.SanPham.Any(x =>
                    x.SKU == sku &&
                    (!productId.HasValue ||
                     x.MaSanPham != productId.Value));

                if (skuExists)
                {
                    ModelState.AddModelError(
                        "SKU",
                        "SKU đã tồn tại. Vui lòng nhập mã khác.");
                }
            }

            if (!model.MaDanhMuc.HasValue ||
                !db.DanhMuc.Any(x =>
                    x.MaDanhMuc == model.MaDanhMuc.Value &&
                    x.TrangThai))
            {
                ModelState.AddModelError(
                    "MaDanhMuc",
                    "Danh mục không tồn tại hoặc đang ngừng hoạt động.");
            }

            ValidateImages(model.NewImages, model.MainImageNewIndex);
        }

        private void NormalizePriceValues(AdminProductFormViewModel model)
        {
            if (model == null)
                return;

            NormalizePriceValue("GiaBan", false, value => model.GiaBan = value ?? 0m);
            NormalizePriceValue("GiaThueNgay", true, value => model.GiaThueNgay = value);
        }

        private void NormalizePriceValue(
            string key,
            bool nullable,
            Action<decimal?> assign)
        {
            var valueProviderResult = ValueProvider.GetValue(key);
            string rawValue = valueProviderResult == null
                ? null
                : valueProviderResult.AttemptedValue;

            if (string.IsNullOrWhiteSpace(rawValue))
            {
                if (nullable)
                    assign(null);

                ModelState.Remove(key);
                return;
            }

            string normalized = rawValue.Trim().Replace(" ", "");
            int commaIndex = normalized.LastIndexOf(',');
            int dotIndex = normalized.LastIndexOf('.');

            if (commaIndex >= 0 && dotIndex >= 0)
            {
                char decimalSeparator = commaIndex > dotIndex ? ',' : '.';
                char thousandsSeparator = decimalSeparator == ',' ? '.' : ',';
                normalized = normalized.Replace(thousandsSeparator.ToString(), "");

                if (decimalSeparator == ',')
                    normalized = normalized.Replace(',', '.');
            }
            else if (commaIndex >= 0)
            {
                normalized = normalized.Replace(',', '.');
            }

            decimal parsedValue;
            if (decimal.TryParse(
                normalized,
                System.Globalization.NumberStyles.AllowDecimalPoint |
                System.Globalization.NumberStyles.AllowLeadingSign,
                System.Globalization.CultureInfo.InvariantCulture,
                out parsedValue))
            {
                assign(parsedValue);
                ModelState.Remove(key);

                if (parsedValue < 0)
                    ModelState.AddModelError(key, "Giá trị không được âm.");
            }
            else
            {
                ModelState.AddModelError(key, "Giá trị không hợp lệ.");
            }
        }

        // =========================================================
        // 10. VALIDATE ẢNH UPLOAD
        // =========================================================

        private void ValidateImages(
            IEnumerable<HttpPostedFileBase> files,
            int? mainImageNewIndex)
        {
            var imageFiles = (files ??
                Enumerable.Empty<HttpPostedFileBase>())
                .Where(x => x != null && x.ContentLength > 0)
                .ToList();

            if (imageFiles.Count > MAX_IMAGE_COUNT)
            {
                ModelState.AddModelError(
                    "NewImages",
                    "Chỉ được tải lên tối đa 10 ảnh trong một lần.");
            }

            for (int i = 0; i < imageFiles.Count; i++)
            {
                var file = imageFiles[i];

                string extension = Path
                    .GetExtension(file.FileName ?? "")
                    .ToLowerInvariant();

                string contentType = file.ContentType ?? "";

                if (!IMAGE_EXTENSIONS.Contains(extension) ||
                    !IMAGE_MIME_TYPES.Contains(
                        contentType,
                        StringComparer.OrdinalIgnoreCase) ||
                    file.ContentLength > MAX_IMAGE_BYTES)
                {
                    ModelState.AddModelError(
                        "NewImages",
                        "Ảnh phải có định dạng JPG, JPEG, PNG hoặc WEBP và không vượt quá 5 MB mỗi ảnh.");
                }
            }

            if (mainImageNewIndex.HasValue &&
                (mainImageNewIndex.Value < 0 ||
                 mainImageNewIndex.Value >= imageFiles.Count))
            {
                ModelState.AddModelError(
                    "MainImageNewIndex",
                    "Ảnh chính được chọn không hợp lệ.");
            }
        }

        // =========================================================
        // 11. LƯU ẢNH
        // =========================================================

        private IList<HinhAnhSanPham> SaveImages(
            IEnumerable<HttpPostedFileBase> files,
            int productId,
            IList<string> savedFiles)
        {
            var result = new List<HinhAnhSanPham>();

            var imageFiles = (files ??
                Enumerable.Empty<HttpPostedFileBase>())
                .Where(x => x != null && x.ContentLength > 0)
                .ToList();

            if (imageFiles.Count == 0)
                return result;

            string directory = Server.MapPath(PRODUCT_IMAGE_FOLDER);

            if (string.IsNullOrWhiteSpace(directory))
            {
                throw new InvalidOperationException(
                    "Không xác định được thư mục lưu ảnh sản phẩm.");
            }

            Directory.CreateDirectory(directory);

            int order = db.HinhAnhSanPham
                .Where(x => x.MaSanPham == productId)
                .Select(x => (int?)x.ThuTuHienThi)
                .Max() ?? -1;

            bool hasMain = db.HinhAnhSanPham.Any(
                x => x.MaSanPham == productId && x.LaAnhChinh);

            foreach (var file in imageFiles)
            {
                string extension = Path
                    .GetExtension(file.FileName ?? "")
                    .ToLowerInvariant();

                // Tên file do hệ thống sinh, không lấy tên gốc làm tên lưu.
                string fileName =
                    Guid.NewGuid().ToString("N") + extension;

                string physicalPath =
                    Path.Combine(directory, fileName);

                file.SaveAs(physicalPath);
                savedFiles.Add(physicalPath);

                var image = new HinhAnhSanPham
                {
                    MaSanPham = productId,
                    DuongDanAnh =
                        PRODUCT_IMAGE_FOLDER + fileName,
                    LaAnhChinh = !hasMain,
                    ThuTuHienThi = ++order
                };

                db.HinhAnhSanPham.Add(image);
                result.Add(image);

                hasMain = true;
            }

            return result;
        }

        // =========================================================
        // 12. XÓA ẢNH CŨ TRONG DATABASE
        // =========================================================

        private List<string> RemoveImages(
            int productId,
            int[] imageIds)
        {
            var removedFiles = new List<string>();

            if (imageIds == null || imageIds.Length == 0)
                return removedFiles;

            var images = db.HinhAnhSanPham
                .Where(x =>
                    x.MaSanPham == productId &&
                    imageIds.Contains(x.MaHinhAnh))
                .ToList();

            foreach (var image in images)
            {
                string physicalPath =
                    GetSafeProductImagePhysicalPath(
                        image.DuongDanAnh);

                if (!string.IsNullOrWhiteSpace(physicalPath) &&
                    System.IO.File.Exists(physicalPath))
                {
                    removedFiles.Add(physicalPath);
                }

                db.HinhAnhSanPham.Remove(image);
            }

            return removedFiles;
        }

        // =========================================================
        // 13. KIỂM TRA ĐƯỜNG DẪN FILE AN TOÀN
        // =========================================================

        private string GetSafeProductImagePhysicalPath(
            string virtualPath)
        {
            if (string.IsNullOrWhiteSpace(virtualPath))
                return null;

            try
            {
                string root = Server.MapPath(PRODUCT_IMAGE_FOLDER);
                string fullRoot = Path.GetFullPath(root);

                string rootWithSeparator =
                    fullRoot.TrimEnd(
                        Path.DirectorySeparatorChar,
                        Path.AltDirectorySeparatorChar)
                    + Path.DirectorySeparatorChar;

                string fullPath = Path.GetFullPath(
                    Server.MapPath(virtualPath));

                if (!fullPath.StartsWith(
                    rootWithSeparator,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                return fullPath;
            }
            catch
            {
                return null;
            }
        }

        // =========================================================
        // 14. CHUYỂN ENTITY ẢNH SANG VIEWMODEL
        // =========================================================

        private static AdminProductImageViewModel ToImageViewModel(
            HinhAnhSanPham image)
        {
            return new AdminProductImageViewModel
            {
                MaHinhAnh = image.MaHinhAnh,
                DuongDanAnh = NormalizeProductImagePath(
                    image.DuongDanAnh),
                MoTaAnh = image.MoTaAnh,
                LaAnhChinh = image.LaAnhChinh,
                ThuTuHienThi = image.ThuTuHienThi
            };
        }

        private static string NormalizeProductImagePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return path;

            if (path.StartsWith("~/", StringComparison.Ordinal))
                return path;

            return "~/" + path.TrimStart('/', '\\');
        }

        // =========================================================
        // 15. XÓA FILE VẬT LÝ
        // =========================================================

        private static void DeleteFiles(IEnumerable<string> files)
        {
            foreach (string file in
                files ?? Enumerable.Empty<string>())
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(file) &&
                        System.IO.File.Exists(file))
                    {
                        System.IO.File.Delete(file);
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceError(
                        "[AdminProductController] Delete file error: {0}",
                        ex);
                }
            }
        }

        // =========================================================
        // 16. GHI LOG LỖI
        // =========================================================

        private static void LogError(
            string action,
            Exception exception)
        {
            Trace.TraceError(
                "[AdminProductController - {0}] {1}",
                action,
                exception);
        }

        // =========================================================
        // 17. DISPOSE
        // =========================================================

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

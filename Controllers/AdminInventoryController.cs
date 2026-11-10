
using STAGEFX_PRO.Controllers.Base;
using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Wedside_TVS_SFX.Models;
using Wedside_TVS_SFX.ViewModels.Admin;

namespace STAGEFX_PRO.Controllers
{
    [AdminAuthorize]
    public class AdminInventoryController : Controller
    {
        private readonly StageFxEntities db = new StageFxEntities();

        private const int PAGE_SIZE = 10;

        // =====================================================
        // 1. DANH SACH TON KHO
        // URL: /AdminInventory/Inventory
        // =====================================================

        [HttpGet]
        public ActionResult Inventory(
            string search,
            int? categoryId,
            string stockStatus,
            string sortBy,
            int page = 1)
        {
            try
            {
                page = Math.Max(1, page);

                IQueryable<SanPham> query =
                    db.SanPham.AsNoTracking();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.Trim();

                    query = query.Where(x =>
                        (x.SKU != null && x.SKU.Contains(search)) ||
                        (x.TenSanPham != null &&
                         x.TenSanPham.Contains(search)));
                }

                if (categoryId.HasValue && categoryId.Value > 0)
                {
                    query = query.Where(x =>
                        x.MaDanhMuc == categoryId.Value);
                }

                if (stockStatus == "available")
                {
                    query = query.Where(x => x.SoLuongTon > 0);
                }
                else if (stockStatus == "out")
                {
                    query = query.Where(x => x.SoLuongTon <= 0);
                }

                int totalProducts = db.SanPham.Count();

                int availableProducts =
                    db.SanPham.Count(x => x.SoLuongTon > 0);

                int outOfStockProducts =
                    db.SanPham.Count(x => x.SoLuongTon <= 0);

                int totalStockQuantity =
                    db.SanPham
                        .Select(x => (int?)x.SoLuongTon)
                        .Sum() ?? 0;

                int totalItems = query.Count();

                if (sortBy == "name")
                {
                    query = query
                        .OrderBy(x => x.TenSanPham)
                        .ThenBy(x => x.MaSanPham);
                }
                else if (sortBy == "quantity")
                {
                    query = query
                        .OrderBy(x => x.SoLuongTon)
                        .ThenBy(x => x.TenSanPham);
                }
                else
                {
                    query = query
                        .OrderByDescending(x => x.NgayTao)
                        .ThenByDescending(x => x.MaSanPham);
                }

                int totalPages = Math.Max(
                    1,
                    (int)Math.Ceiling(
                        totalItems / (double)PAGE_SIZE));

                page = Math.Min(page, totalPages);

                var products = query
                    .Skip((page - 1) * PAGE_SIZE)
                    .Take(PAGE_SIZE)
                    .Select(x => new AdminInventoryItemViewModel
                    {
                        MaSanPham = x.MaSanPham,
                        SKU = x.SKU,
                        TenSanPham = x.TenSanPham,

                        TenDanhMuc = x.DanhMuc != null
                            ? x.DanhMuc.TenDanhMuc
                            : "Chưa phân loại",

                        GiaBan = x.GiaBan,
                        GiaThueNgay = x.GiaThueNgay,
                        SoLuongTon = x.SoLuongTon,
                        TrangThai = x.TrangThai
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

                var model = new AdminInventoryViewModel
                {
                    Products = products,
                    Categories = categories,

                    SearchTerm = search,
                    CategoryId = categoryId,
                    StockStatus = stockStatus,
                    SortBy = sortBy,

                    CurrentPage = page,
                    PageSize = PAGE_SIZE,
                    TotalItems = totalItems,
                    TotalPages = totalPages,

                    TotalProducts = totalProducts,
                    AvailableProducts = availableProducts,
                    OutOfStockProducts = outOfStockProducts,
                    TotalStockQuantity = totalStockQuantity
                };

                return View(
                    "~/Views/Admin/Inventory.cshtml",
                    model);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(
                    "[AdminInventoryController.Inventory] {0}",
                    ex);

                return new HttpStatusCodeResult(
                    500,
                    "Không thể tải danh sách kho hàng.");
            }
        }

        // =====================================================
        // 2. NHAP KHO
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ImportStock(
            int productId,
            int quantity,
            string note)
        {
            return UpdateStock(
                productId,
                quantity,
                note,
                "NHAP");
        }

        // =====================================================
        // 3. XUAT KHO
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ExportStock(
            int productId,
            int quantity,
            string note)
        {
            return UpdateStock(
                productId,
                quantity,
                note,
                "XUAT");
        }

        // =====================================================
        // 4. DIEU CHINH TON KHO
        // =====================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AdjustStock(
            int productId,
            int actualQuantity,
            string note)
        {
            if (productId <= 0 || actualQuantity < 0)
            {
                TempData["InventoryError"] =
                    "Thông tin điều chỉnh tồn kho không hợp lệ.";

                return RedirectToAction("Inventory");
            }

            using (var transaction =
                db.Database.BeginTransaction(
                    IsolationLevel.Serializable))
            {
                try
                {
                    var product = db.SanPham
                        .FirstOrDefault(
                            x => x.MaSanPham == productId);

                    if (product == null)
                    {
                        transaction.Rollback();

                        TempData["InventoryError"] =
                            "Không tìm thấy sản phẩm.";

                        return RedirectToAction("Inventory");
                    }

                    int before = product.SoLuongTon;

                    int difference = Math.Abs(
                        actualQuantity - before);

                    if (difference == 0)
                    {
                        transaction.Rollback();

                        TempData["InventoryNotice"] =
                            "Số lượng tồn không thay đổi.";

                        return RedirectToAction("Inventory");
                    }

                    int userId = GetCurrentUserId();
                    DateTime now = DateTime.Now;

                    var history = new LichSuKho
                    {
                        MaSanPham = product.MaSanPham,
                        MaNguoiDungThayDoi = userId,
                        MaDonHang = null,
                        LoaiGiaoDich = "DIEUCHINH",
                        SoLuong = difference,
                        TonTruoc = before,
                        TonSau = actualQuantity,
                        GhiChu = NormalizeNote(note),
                        NgayTao = now
                    };

                    product.SoLuongTon = actualQuantity;
                    product.NgayCapNhat = now;

                    db.LichSuKho.Add(history);
                    db.SaveChanges();

                    transaction.Commit();

                    TempData["InventorySuccess"] =
                        "Điều chỉnh tồn kho thành công.";
                }
                catch (Exception ex)
                {
                    transaction.Rollback();

                    System.Diagnostics.Trace.TraceError(
                        "[AdminInventoryController.AdjustStock] {0}",
                        ex);

                    TempData["InventoryError"] =
                        "Không thể điều chỉnh tồn kho. Vui lòng thử lại.";
                }
            }

            return RedirectToAction("Inventory");
        }

        // =====================================================
        // 5. XU LY NHAP / XUAT KHO
        // =====================================================

        private ActionResult UpdateStock(
            int productId,
            int quantity,
            string note,
            string transactionType)
        {
            if (productId <= 0 || quantity <= 0)
            {
                TempData["InventoryError"] =
                    "Sản phẩm hoặc số lượng giao dịch không hợp lệ.";

                return RedirectToAction("Inventory");
            }

            if (transactionType != "NHAP" &&
                transactionType != "XUAT")
            {
                TempData["InventoryError"] =
                    "Loại giao dịch kho không hợp lệ.";

                return RedirectToAction("Inventory");
            }

            using (var transaction =
                db.Database.BeginTransaction(
                    IsolationLevel.Serializable))
            {
                try
                {
                    var product = db.SanPham
                        .FirstOrDefault(
                            x => x.MaSanPham == productId);

                    if (product == null)
                    {
                        transaction.Rollback();

                        TempData["InventoryError"] =
                            "Không tìm thấy sản phẩm.";

                        return RedirectToAction("Inventory");
                    }

                    int before = product.SoLuongTon;
                    int after;

                    if (before < 0)
                    {
                        transaction.Rollback();

                        TempData["InventoryError"] =
                            "Tồn kho hiện tại không hợp lệ.";

                        return RedirectToAction("Inventory");
                    }

                    if (transactionType == "NHAP")
                    {
                        after = checked(before + quantity);
                    }
                    else
                    {
                        if (quantity > before)
                        {
                            transaction.Rollback();

                            TempData["InventoryError"] =
                                "Số lượng xuất vượt quá tồn kho hiện tại.";

                            return RedirectToAction("Inventory");
                        }

                        after = before - quantity;
                    }

                    int userId = GetCurrentUserId();
                    DateTime now = DateTime.Now;

                    var history = new LichSuKho
                    {
                        MaSanPham = product.MaSanPham,
                        MaNguoiDungThayDoi = userId,
                        MaDonHang = null,
                        LoaiGiaoDich = transactionType,
                        SoLuong = quantity,
                        TonTruoc = before,
                        TonSau = after,
                        GhiChu = NormalizeNote(note),
                        NgayTao = now
                    };

                    product.SoLuongTon = after;
                    product.NgayCapNhat = now;

                    db.LichSuKho.Add(history);
                    db.SaveChanges();

                    transaction.Commit();

                    TempData["InventorySuccess"] =
                        transactionType == "NHAP"
                            ? "Nhập kho thành công."
                            : "Xuất kho thành công.";
                }
                catch (Exception ex)
                {
                    transaction.Rollback();

                    System.Diagnostics.Trace.TraceError(
                        "[AdminInventoryController.UpdateStock] {0}",
                        ex);

                    TempData["InventoryError"] =
                        "Giao dịch kho thất bại. Dữ liệu chưa được cập nhật.";
                }
            }

            return RedirectToAction("Inventory");
        }

        // =====================================================
        // 6. LICH SU KHO
        // URL: /AdminInventory/History
        // =====================================================

        [HttpGet]
        public ActionResult History(
            string search,
            string transactionType,
            DateTime? dateFrom,
            DateTime? dateTo,
            int page = 1)
        {
            try
            {
                page = Math.Max(1, page);

                IQueryable<LichSuKho> query =
                    db.LichSuKho.AsNoTracking();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.Trim();

                    query = query.Where(x =>
                        (x.SanPham.SKU != null &&
                         x.SanPham.SKU.Contains(search)) ||

                        (x.SanPham.TenSanPham != null &&
                         x.SanPham.TenSanPham.Contains(search)));
                }

                if (!string.IsNullOrWhiteSpace(transactionType))
                {
                    query = query.Where(x =>
                        x.LoaiGiaoDich == transactionType);
                }

                if (dateFrom.HasValue)
                {
                    DateTime startDate = dateFrom.Value.Date;

                    query = query.Where(x =>
                        x.NgayTao >= startDate);
                }

                if (dateTo.HasValue)
                {
                    DateTime endDate =
                        dateTo.Value.Date.AddDays(1);

                    query = query.Where(x =>
                        x.NgayTao < endDate);
                }

                int totalItems = query.Count();

                int totalPages = Math.Max(
                    1,
                    (int)Math.Ceiling(
                        totalItems / (double)PAGE_SIZE));

                page = Math.Min(page, totalPages);

                var transactions = query
                    .OrderByDescending(x => x.NgayTao)
                    .ThenByDescending(x => x.MaGiaoDichKho)
                    .Skip((page - 1) * PAGE_SIZE)
                    .Take(PAGE_SIZE)
                    .Select(x => new AdminInventoryHistoryItemViewModel
                    {
                        MaGiaoDichKho = x.MaGiaoDichKho,
                        SKU = x.SanPham.SKU,
                        TenSanPham = x.SanPham.TenSanPham,
                        LoaiGiaoDich = x.LoaiGiaoDich,
                        SoLuong = x.SoLuong,
                        TonTruoc = x.TonTruoc,
                        TonSau = x.TonSau,

                        NguoiThucHien = x.NguoiDung != null
                            ? x.NguoiDung.HoTen
                            : "Tài khoản không tồn tại",

                        GhiChu = x.GhiChu,
                        NgayTao = x.NgayTao
                    })
                    .ToList();

                var model = new AdminInventoryHistoryViewModel
                {
                    Transactions = transactions,
                    SearchTerm = search,
                    TransactionType = transactionType,
                    DateFrom = dateFrom,
                    DateTo = dateTo,
                    CurrentPage = page,
                    TotalPages = totalPages,
                    TotalItems = totalItems
                };

                return View(
                    "~/Views/Admin/InventoryHistory.cshtml",
                    model);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(
                    "[AdminInventoryController.History] {0}",
                    ex);

                return new HttpStatusCodeResult(
                    500,
                    "Không thể tải lịch sử kho hàng.");
            }
        }

        // =====================================================
        // 7. LAY ID NGUOI DUNG DANG DANG NHAP
        // =====================================================

        private int GetCurrentUserId()
        {
            object value = Session["MaNguoiDung"];

            int userId;

            if (value == null ||
                !int.TryParse(value.ToString(), out userId) ||
                userId <= 0)
            {
                throw new UnauthorizedAccessException(
                    "Không xác định được tài khoản đang đăng nhập.");
            }

            return userId;
        }

        // =====================================================
        // 8. CHUAN HOA GHI CHU
        // =====================================================

        private string NormalizeNote(string note)
        {
            if (string.IsNullOrWhiteSpace(note))
                return null;

            note = note.Trim();

            return note.Length > 500
                ? note.Substring(0, 500)
                : note;
        }

        // =====================================================
        // 9. DISPOSE
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

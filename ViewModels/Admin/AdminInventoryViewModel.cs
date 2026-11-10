
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Wedside_TVS_SFX.ViewModels.Admin
{
    // ==========================================
    // DANH SACH TON KHO
    // ==========================================
    public class AdminInventoryViewModel
    {
        public AdminInventoryViewModel()
        {
            Products = new List<AdminInventoryItemViewModel>();
            Categories = new List<SelectListItem>();
        }

        public List<AdminInventoryItemViewModel> Products { get; set; }

        public List<SelectListItem> Categories { get; set; }

        public string SearchTerm { get; set; }

        public int? CategoryId { get; set; }

        public string StockStatus { get; set; }

        public string SortBy { get; set; }

        public int CurrentPage { get; set; }

        public int PageSize { get; set; }

        public int TotalItems { get; set; }

        public int TotalPages { get; set; }

        public int TotalProducts { get; set; }

        public int AvailableProducts { get; set; }

        public int OutOfStockProducts { get; set; }

        public int TotalStockQuantity { get; set; }
    }

    // ==========================================
    // THONG TIN SAN PHAM TRONG KHO
    // ==========================================
    public class AdminInventoryItemViewModel
    {
        public int MaSanPham { get; set; }

        public string SKU { get; set; }

        public string TenSanPham { get; set; }

        public string TenDanhMuc { get; set; }

        public decimal GiaBan { get; set; }

        public decimal? GiaThueNgay { get; set; }

        public int SoLuongTon { get; set; }

        public bool TrangThai { get; set; }
    }

    // ==========================================
    // THONG TIN GIAO DICH KHO
    // ==========================================
    public class AdminInventoryTransactionViewModel
    {
        public int MaSanPham { get; set; }

        public string TenSanPham { get; set; }

        public string SKU { get; set; }

        public int SoLuongTonHienTai { get; set; }

        public int SoLuong { get; set; }

        public string GhiChu { get; set; }

        public string LoaiGiaoDich { get; set; }
    }

    // ==========================================
    // DANH SACH LICH SU KHO
    // ==========================================
    public class AdminInventoryHistoryViewModel
    {
        public AdminInventoryHistoryViewModel()
        {
            Transactions = new List<AdminInventoryHistoryItemViewModel>();
        }

        public List<AdminInventoryHistoryItemViewModel> Transactions { get; set; }

        public string SearchTerm { get; set; }

        public string TransactionType { get; set; }

        public DateTime? DateFrom { get; set; }

        public DateTime? DateTo { get; set; }

        public int CurrentPage { get; set; }

        public int TotalPages { get; set; }

        public int TotalItems { get; set; }
    }

    // ==========================================
    // CHI TIET MOT GIAO DICH KHO
    // ==========================================
    public class AdminInventoryHistoryItemViewModel
    {
        public int MaGiaoDichKho { get; set; }

        public string SKU { get; set; }

        public string TenSanPham { get; set; }

        public string LoaiGiaoDich { get; set; }

        public int SoLuong { get; set; }

        public int TonTruoc { get; set; }

        public int TonSau { get; set; }

        public string NguoiThucHien { get; set; }

        public string GhiChu { get; set; }

        public DateTime NgayTao { get; set; }
    }
}

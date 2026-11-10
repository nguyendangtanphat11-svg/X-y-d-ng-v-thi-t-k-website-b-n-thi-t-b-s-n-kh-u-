
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Wedside_TVS_SFX.ViewModels.Admin
{
    public class AdminProductListViewModel
    {
        public AdminProductListViewModel()
        {
            Products = new List<AdminProductItemViewModel>();
            Categories = new List<SelectListItem>();
        }

        public List<AdminProductItemViewModel> Products { get; set; }

        public List<SelectListItem> Categories { get; set; }

        public string SearchTerm { get; set; }

        public int? CategoryId { get; set; }

        // "", "active", "inactive"
        public string Status { get; set; }

        public int TotalProducts { get; set; }

        public int ActiveProducts { get; set; }

        public int InactiveProducts { get; set; }

        public int OutOfStockProducts { get; set; }

        public int TotalItems { get; set; }

        public int CurrentPage { get; set; }

        public int PageSize { get; set; }

        public int TotalPages { get; set; }
    }

    public class AdminProductItemViewModel
    {
        public int MaSanPham { get; set; }

        public string SKU { get; set; }

        public string TenSanPham { get; set; }

        public string TenDanhMuc { get; set; }

        public decimal GiaBan { get; set; }

        public decimal? GiaThueNgay { get; set; }

        public int SoLuongTon { get; set; }

        public bool TrangThai { get; set; }

        public DateTime NgayTao { get; set; }
    }
}

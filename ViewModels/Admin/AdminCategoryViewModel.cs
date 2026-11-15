
using System;
using System.Collections.Generic;
using System.Web;

namespace Wedside_TVS_SFX.ViewModels.Admin
{
    public class AdminCategoryViewModel
    {
        public List<AdminCategoryItemViewModel> Categories { get; set; }
            = new List<AdminCategoryItemViewModel>();

        public string SearchTerm { get; set; }

        public string StatusFilter { get; set; }

        public int CurrentPage { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public int TotalItems { get; set; }

        public int TotalPages { get; set; }

        public int TotalCategories { get; set; }

        public int ActiveCategories { get; set; }

        public int InactiveCategories { get; set; }

        public int TotalProducts { get; set; }
    }

    public class AdminCategoryItemViewModel
    {
        public int MaDanhMuc { get; set; }

        public string TenDanhMuc { get; set; }

        public string MoTa { get; set; }

        public bool TrangThai { get; set; }

        public DateTime NgayTao { get; set; }

        public int SoLuongSanPham { get; set; }

        public string AnhDanhMuc { get; set; }
    }

    public class AdminCategoryFormViewModel
    {
        public int MaDanhMuc { get; set; }

        public string TenDanhMuc { get; set; }

        public string MoTa { get; set; }

        public bool TrangThai { get; set; } = true;

        public DateTime NgayTao { get; set; }
        public string AnhDanhMuc { get; set; }
        public HttpPostedFileBase AnhUpload { get; set; }
    }
}

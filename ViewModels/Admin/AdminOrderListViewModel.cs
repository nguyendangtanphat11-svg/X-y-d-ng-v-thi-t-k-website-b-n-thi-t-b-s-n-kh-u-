using System;
using System.Collections.Generic;

namespace Wedside_TVS_SFX.ViewModels.Admin
{
    public class AdminOrderListViewModel
    {
        public AdminOrderListViewModel()
        {
            Orders = new List<AdminOrderItemViewModel>();
        }

        public List<AdminOrderItemViewModel> Orders { get; set; }

        public string SearchTerm { get; set; }
        public string OrderStatus { get; set; }
        public string PaymentStatus { get; set; }

        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }

        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int ProcessingOrders { get; set; }
        public int CompletedOrders { get; set; }
    }

    public class AdminOrderItemViewModel
    {
        public int MaDonHang { get; set; }
        public string MaSoDonHang { get; set; }

        public string CustomerName { get; set; }
        public string RecipientName { get; set; }

        public DateTime NgayDat { get; set; }

        public decimal TongTien { get; set; }
        public decimal TienCoc { get; set; }

        public string TrangThai { get; set; }
        public string TrangThaiThanhToan { get; set; }

        public string OrderStatusText { get; set; }
        public string OrderStatusClass { get; set; }

        public string PaymentStatusText { get; set; }
        public string PaymentStatusClass { get; set; }
    }
}
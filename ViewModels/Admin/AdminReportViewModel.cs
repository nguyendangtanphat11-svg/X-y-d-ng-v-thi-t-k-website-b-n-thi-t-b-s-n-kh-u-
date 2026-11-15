
using System;
using System.Collections.Generic;

namespace Wedside_TVS_SFX.ViewModels.Admin
{
    public class AdminReportViewModel
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        // Tổng quan
        public int TotalOrders { get; set; }
        public int ConfirmedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public int RejectedOrders { get; set; }

        // Doanh thu đơn hàng hợp lệ
        public decimal TotalOrderRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }

        // Thanh toán
        public decimal TotalPaid { get; set; }
        public decimal TotalDeposit { get; set; }
        public decimal PendingPayment { get; set; }

        // Sản phẩm
        public int TotalProductsSold { get; set; }
        public List<AdminReportProductItemViewModel> TopProducts
        { get; set; } = new List<AdminReportProductItemViewModel>();

        // Biểu đồ
        public List<AdminReportChartItemViewModel> RevenueChart
        { get; set; } = new List<AdminReportChartItemViewModel>();

        // Trạng thái đơn hàng
        public List<AdminReportStatusItemViewModel> OrderStatuses
        { get; set; } = new List<AdminReportStatusItemViewModel>();

        public List<AdminReportStatusItemViewModel> PaymentStatuses
        { get; set; } = new List<AdminReportStatusItemViewModel>();
    }

    public class AdminReportProductItemViewModel
    {
        public int MaSanPham { get; set; }
        public string TenSanPham { get; set; }
        public int SoLuongBan { get; set; }
        public decimal TongDoanhThu { get; set; }
    }

    public class AdminReportChartItemViewModel
    {
        public string Label { get; set; }
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }

    public class AdminReportStatusItemViewModel
    {
        public string Status { get; set; }
        public string DisplayName { get; set; }
        public int Count { get; set; }
    }
}

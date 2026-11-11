using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using STAGEFX_PRO.Controllers.Base;
using Wedside_TVS_SFX.Models;
using Wedside_TVS_SFX.ViewModels.Admin;

namespace STAGEFX_PRO.Controllers
{
    [AdminAuthorize]
    public class AdminController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            var model = new AdminDashboardViewModel();

            try
            {
                using (var db = new StageFxEntities())
                {
                    model.TotalOrders = db.DonHang
                        .AsNoTracking()
                        .Count();

                    model.PendingOrders = db.DonHang
                        .AsNoTracking()
                        .Count(x => x.TrangThai == "ChoXacNhan");

                    model.TotalCustomers = db.NguoiDung
                        .AsNoTracking()
                        .Count(x => x.MaVaiTro == 3 && x.TrangThai);

                    model.TotalProducts = db.SanPham
                        .AsNoTracking()
                        .Count();

                    model.OrderStatusStatistics = GetOrderStatusStatistics(db);
                    model.RecentOrders = GetRecentOrders(db);
                    model.TopProducts = GetTopProducts(db);
                }
            }
            catch (Exception)
            {
<<<<<<< HEAD
                // Gi? dashboard ? tr?ng thái r?ng khi database không kh? d?ng.
                // Không ??a chi ti?t exception ra giao di?n qu?n tr?.
=======
                // Gi? dashboard ? tr?ng thï¿½i r?ng khi database khï¿½ng kh? d?ng.
                // Khï¿½ng ??a chi ti?t exception ra giao di?n qu?n tr?.
>>>>>>> 8dd10d954939e9e3bea89f79058d14c043feda13
                model = new AdminDashboardViewModel();
            }

            return View(model);
        }

        private static AdminOrderStatusStatistics GetOrderStatusStatistics(
            StageFxEntities db)
        {
            var counts = db.DonHang
                .AsNoTracking()
                .GroupBy(x => x.TrangThai)
                .Select(x => new
                {
                    Status = x.Key,
                    Count = x.Count()
                })
                .ToList()
                .ToDictionary(x => x.Status, x => x.Count);

            return new AdminOrderStatusStatistics
            {
                ChoXacNhan = GetStatusCount(counts, "ChoXacNhan"),
                DaXacNhan = GetStatusCount(counts, "DaXacNhan"),
                DangChuanBi = GetStatusCount(counts, "DangChuanBi"),
                DangGiao = GetStatusCount(counts, "DangGiao"),
                HoanThanh = GetStatusCount(counts, "HoanThanh"),
                DaHuy = GetStatusCount(counts, "DaHuy"),
                ChoHuy = GetStatusCount(counts, "ChoHuy"),
                TuChoi = GetStatusCount(counts, "TuChoi")
            };
        }

        private static IList<AdminRecentOrderViewModel> GetRecentOrders(
            StageFxEntities db)
        {
            return db.DonHang
                .AsNoTracking()
                .OrderByDescending(x => x.NgayDat)
                .ThenByDescending(x => x.MaDonHang)
                .Take(10)
                .Select(x => new AdminRecentOrderViewModel
                {
                    MaDonHang = x.MaDonHang,
                    MaSoDonHang = x.MaSoDonHang,
                    TenNguoiNhan = x.TenNguoiNhan,
                    NgayDat = x.NgayDat,
                    TongTien = x.TongTien,
                    TrangThai = x.TrangThai,
                    TrangThaiThanhToan = x.TrangThaiThanhToan
                })
                .ToList();
        }

        private static IList<AdminTopProductViewModel> GetTopProducts(
            StageFxEntities db)
        {
            var excludedStatuses = new[] { "DaHuy", "TuChoi" };

            var aggregates = db.ChiTietDonHang
                .AsNoTracking()
                .Where(x => !excludedStatuses.Contains(x.DonHang.TrangThai))
                .GroupBy(x => x.MaSanPham)
                .Select(x => new
                {
                    MaSanPham = x.Key,
                    SoLuong = x.Sum(y => y.SoLuong),
                    GiaTriDonHang = x.Sum(y => y.ThanhTien ?? 0m)
                })
                .OrderByDescending(x => x.SoLuong)
                .ThenByDescending(x => x.GiaTriDonHang)
                .Take(5)
                .ToList();

            if (!aggregates.Any())
                return new List<AdminTopProductViewModel>();

            var productIds = aggregates
                .Select(x => x.MaSanPham)
                .ToList();

            var snapshots = db.ChiTietDonHang
                .AsNoTracking()
                .Where(x => productIds.Contains(x.MaSanPham))
                .Select(x => new
                {
                    x.MaSanPham,
                    x.TenSanPhamLucDat,
                    x.SKULucDat,
                    NgayDat = x.DonHang.NgayDat
                })
                .ToList()
                .GroupBy(x => x.MaSanPham)
                .ToDictionary(
                    x => x.Key,
                    x => x.OrderByDescending(y => y.NgayDat).First());

            return aggregates
                .Select(x =>
                {
                    snapshots.TryGetValue(x.MaSanPham, out var snapshot);

                    return new AdminTopProductViewModel
                    {
                        MaSanPham = x.MaSanPham,
                        TenSanPham = snapshot != null
                            ? snapshot.TenSanPhamLucDat
<<<<<<< HEAD
                            : "Không xác ??nh",
=======
                            : "Khï¿½ng xï¿½c ??nh",
>>>>>>> 8dd10d954939e9e3bea89f79058d14c043feda13
                        SKU = snapshot != null
                            ? snapshot.SKULucDat
                            : null,
                        SoLuong = x.SoLuong,
                        GiaTriDonHang = x.GiaTriDonHang
                    };
                })
                .ToList();
        }

        private static int GetStatusCount(
            IDictionary<string, int> counts,
            string status)
        {
            int count;
            return counts.TryGetValue(status, out count) ? count : 0;
        }
    }
}

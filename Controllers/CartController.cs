using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Wedside_TVS_SFX.Models;

namespace STAGEFX_PRO.Controllers
{
    public class CartController : Controller
    {
        private const decimal FLIGHTCASE_FEE = 350000m;

        private StageFxEntities db = new StageFxEntities();

        // ==========================================
        // LẤY MÃ NGƯỜI DÙNG ĐANG ĐĂNG NHẬP
        // ==========================================

        private int? CurrentUserId
        {
            get
            {
                int userId;

                if (Session["MaNguoiDung"] == null)
                    return null;

                if (!int.TryParse(
                    Convert.ToString(Session["MaNguoiDung"]),
                    out userId))
                {
                    return null;
                }

                return userId > 0 ? (int?)userId : null;
            }
        }

        // ==========================================
        // CHUẨN HÓA HÌNH THỨC ĐẶT
        // ==========================================

        private string NormalizeOrderType(string value)
        {
            if (string.Equals(value, "Thue",
                StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "ThueThietBi",
                StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "Rental",
                StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "Rent",
                StringComparison.OrdinalIgnoreCase))
            {
                return "Thue";
            }

            if (string.Equals(value, "Mua",
                StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "MuaHang",
                StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "Purchase",
                StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "Buy",
                StringComparison.OrdinalIgnoreCase))
            {
                return "Mua";
            }

            return null;
        }

        // ==========================================
        // LẤY GIỎ HÀNG CỦA NGƯỜI DÙNG
        // ==========================================

        private GioHang GetUserCart(StageFxEntities db, int userId)
        {
            return db.GioHang.FirstOrDefault(
                x => x.MaNguoiDung == userId
            );
        }

        // ==========================================
        // TRANG GIỎ HÀNG
        // ==========================================

        [HttpGet]
        public ActionResult Index()
        {
            if (!CurrentUserId.HasValue)
                return RedirectToAction("Login", "Account");

            int userId = CurrentUserId.Value;

            using (var context = new StageFxEntities())
            {
                var cart = context.GioHang
                    .Include("ChiTietGioHang.SanPham")
                    .FirstOrDefault(x => x.MaNguoiDung == userId);

                var model = new CartIndexViewModel();

                if (cart == null)
                    return View(model);

                foreach (var item in cart.ChiTietGioHang)
                {
                    var product = item.SanPham;

                    if (product == null)
                        continue;

                    decimal giaCoBan;

                    if (item.HinhThucDat == "Thue")
                    {
                        if (!product.GiaThueNgay.HasValue ||
                            !item.SoNgayThue.HasValue)
                        {
                            continue;
                        }

                        giaCoBan =
                            product.GiaThueNgay.Value *
                            item.SoNgayThue.Value;
                    }
                    else
                    {
                        giaCoBan = product.GiaBan;
                    }

                    decimal phiFlightcase =
                        item.CoFlightcase
                            ? FLIGHTCASE_FEE
                            : 0m;

                    decimal thanhTien =
                        (giaCoBan + phiFlightcase) *
                        item.SoLuong;

                    model.Items.Add(new CartItemViewModel
                    {
                        MaChiTietGioHang = item.MaChiTietGioHang,
                        MaSanPham = product.MaSanPham,
                        SKU = product.SKU,
                        TenSanPham = product.TenSanPham,

                        SoLuong = item.SoLuong,

                        HinhThucDat = item.HinhThucDat,
                        SoNgayThue = item.SoNgayThue,

                        CoFlightcase = item.CoFlightcase,

                        GiaCoBan = giaCoBan,
                        PhiFlightcaseDonVi = phiFlightcase,
                        ThanhTien = thanhTien
                    });
                }

                model.TienHang = model.Items.Sum(x => x.ThanhTien);
                model.TongSoLuong = model.Items.Sum(x => x.SoLuong);

                return View(model);
            }
        }

        // ==========================================
        // THÊM SẢN PHẨM VÀO GIỎ HÀNG
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(
            int id,
            int quantity = 1,
            string hinhThucDat = "Mua",
            int? soNgayThue = null,
            bool coFlightcase = false)
        {
            if (!CurrentUserId.HasValue)
                return RedirectToAction("Login", "Account");

            if (id <= 0 || quantity <= 0)
            {
                TempData["CartError"] = "Thông tin sản phẩm không hợp lệ.";
                return RedirectToAction("Index");
            }

            string orderType = NormalizeOrderType(hinhThucDat);

            if (orderType == null)
            {
                TempData["CartError"] = "Hình thức đặt hàng không hợp lệ.";
                return RedirectToAction("Index");
            }

            if (orderType == "Thue" &&
                (!soNgayThue.HasValue || soNgayThue.Value <= 0))
            {
                TempData["CartError"] = "Vui lòng chọn số ngày thuê hợp lệ.";
                return RedirectToAction("Index");
            }

            int userId = CurrentUserId.Value;

            using (var context = new StageFxEntities())
            {
                var product = context.SanPham.Find(id);

                if (product == null || !product.TrangThai)
                {
                    TempData["CartError"] = "Sản phẩm không tồn tại hoặc đã ngừng kinh doanh.";
                    return RedirectToAction("Index");
                }

                if (orderType == "Thue" &&
                    (!product.GiaThueNgay.HasValue ||
                     product.GiaThueNgay.Value <= 0))
                {
                    TempData["CartError"] = "Sản phẩm này chưa được cấu hình giá thuê.";
                    return RedirectToAction("Index");
                }

                if (quantity > product.SoLuongTon)
                {
                    TempData["CartError"] = "Số lượng sản phẩm vượt quá tồn kho.";
                    return RedirectToAction("Index");
                }

                var cart = GetUserCart(context, userId);

                if (cart == null)
                {
                    cart = new GioHang
                    {
                        MaNguoiDung = userId,
                        NgayTao = DateTime.Now,
                        NgayCapNhat = DateTime.Now
                    };

                    context.GioHang.Add(cart);
                    context.SaveChanges();
                }

                int? rentalDays =
                    orderType == "Thue" ? soNgayThue : null;

                var existingItem = context.ChiTietGioHang.FirstOrDefault(x =>
                    x.MaGioHang == cart.MaGioHang &&
                    x.MaSanPham == id &&
                    x.HinhThucDat == orderType &&
                    x.SoNgayThue == rentalDays &&
                    x.CoFlightcase == coFlightcase
                );

                int newQuantity = quantity;

                if (existingItem != null)
                    newQuantity += existingItem.SoLuong;

                if (newQuantity > product.SoLuongTon)
                {
                    TempData["CartError"] =
                        "Tổng số lượng trong giỏ vượt quá tồn kho hiện tại.";

                    return RedirectToAction("Index");
                }

                if (existingItem != null)
                {
                    existingItem.SoLuong = newQuantity;
                    existingItem.PhiFlightcase =
                        coFlightcase ? FLIGHTCASE_FEE : 0m;
                }
                else
                {
                    var cartItem = new ChiTietGioHang
                    {
                        MaGioHang = cart.MaGioHang,
                        MaSanPham = id,
                        SoLuong = quantity,
                        NgayThem = DateTime.Now,

                        HinhThucDat = orderType,
                        SoNgayThue = rentalDays,

                        CoFlightcase = coFlightcase,
                        PhiFlightcase =
                            coFlightcase ? FLIGHTCASE_FEE : 0m
                    };

                    context.ChiTietGioHang.Add(cartItem);
                }

                cart.NgayCapNhat = DateTime.Now;

                context.SaveChanges();
            }

            TempData["CartSuccess"] = "Đã thêm sản phẩm vào giỏ hàng.";

            return RedirectToAction("Index");
        }

        // ==========================================
        // CẬP NHẬT SỐ LƯỢNG / NGÀY THUÊ / FLIGHTCASE
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(
            int id,
            int quantity,
            int? soNgayThue = null,
            bool coFlightcase = false)
        {
            if (!CurrentUserId.HasValue)
                return RedirectToAction("Login", "Account");

            if (id <= 0 || quantity <= 0)
            {
                TempData["CartError"] = "Số lượng không hợp lệ.";
                return RedirectToAction("Index");
            }

            int userId = CurrentUserId.Value;

            using (var context = new StageFxEntities())
            {
                var cart = GetUserCart(context, userId);

                if (cart == null)
                    return RedirectToAction("Index");

                var item = context.ChiTietGioHang.FirstOrDefault(x =>
                    x.MaChiTietGioHang == id &&
                    x.MaGioHang == cart.MaGioHang
                );

                if (item == null)
                    return HttpNotFound();

                var product = context.SanPham.Find(item.MaSanPham);

                if (product == null || !product.TrangThai)
                {
                    TempData["CartError"] = "Sản phẩm không còn khả dụng.";
                    return RedirectToAction("Index");
                }

                if (quantity > product.SoLuongTon)
                {
                    TempData["CartError"] = "Số lượng vượt quá tồn kho.";
                    return RedirectToAction("Index");
                }

                if (item.HinhThucDat == "Thue")
                {
                    if (!soNgayThue.HasValue || soNgayThue.Value <= 0)
                    {
                        TempData["CartError"] = "Số ngày thuê không hợp lệ.";
                        return RedirectToAction("Index");
                    }

                    if (!product.GiaThueNgay.HasValue ||
                        product.GiaThueNgay.Value <= 0)
                    {
                        TempData["CartError"] = "Sản phẩm chưa có giá thuê.";
                        return RedirectToAction("Index");
                    }

                    item.SoNgayThue = soNgayThue;
                }
                else
                {
                    item.SoNgayThue = null;
                }

                item.SoLuong = quantity;
                item.CoFlightcase = coFlightcase;
                item.PhiFlightcase =
                    coFlightcase ? FLIGHTCASE_FEE : 0m;

                cart.NgayCapNhat = DateTime.Now;

                context.SaveChanges();
            }

            TempData["CartSuccess"] = "Đã cập nhật giỏ hàng.";

            return RedirectToAction("Index");
        }

        // ==========================================
        // XÓA SẢN PHẨM KHỎI GIỎ
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Remove(int id)
        {
            if (!CurrentUserId.HasValue)
                return RedirectToAction("Login", "Account");

            int userId = CurrentUserId.Value;

            using (var context = new StageFxEntities())
            {
                var cart = GetUserCart(context, userId);

                if (cart == null)
                    return RedirectToAction("Index");

                var item = context.ChiTietGioHang.FirstOrDefault(x =>
                    x.MaChiTietGioHang == id &&
                    x.MaGioHang == cart.MaGioHang
                );

                if (item == null)
                    return HttpNotFound();

                context.ChiTietGioHang.Remove(item);

                cart.NgayCapNhat = DateTime.Now;

                context.SaveChanges();
            }

            TempData["CartSuccess"] = "Đã xóa sản phẩm khỏi giỏ hàng.";

            return RedirectToAction("Index");
        }
        // ==========================================
        // LẤY SỐ LƯỢNG SẢN PHẨM TRÊN HEADER
        // ==========================================


        [ChildActionOnly]
        public ActionResult CartCount()
        {
            int count = 0;

            try
            {
                if (CurrentUserId.HasValue)
                {
                    int userId = CurrentUserId.Value;

                    using (var context = new StageFxEntities())
                    {
                        var cart = context.GioHang
                            .FirstOrDefault(x => x.MaNguoiDung == userId);

                        if (cart != null)
                        {
                            count = context.ChiTietGioHang
                                .Where(x => x.MaGioHang == cart.MaGioHang)
                                .Select(x => (int?)x.SoLuong)
                                .Sum() ?? 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Lỗi CartCount: " + ex.Message
                );
            }

            return PartialView("~/Views/Cart/_CartCount.cshtml", count);
        }



        // ==========================================
        // GIẢI PHÓNG TÀI NGUYÊN
        // ==========================================

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using Wedside_TVS_SFX.Models;
using Wedside_TVS_SFX.ViewModels;

namespace STAGEFX_PRO.Controllers
{
    public class CheckoutController : Controller
    {
        private const decimal FLIGHTCASE_FEE = 350000m;
        private const decimal SHIPPING_THRESHOLD = 20000000m;
        private const decimal SHIPPING_FEE = 350000m;
        private const decimal RENTAL_DEPOSIT_RATE = 0.30m;

        // ==========================================
        // 1. LẤY NGƯỜI DÙNG ĐANG ĐĂNG NHẬP
        // ==========================================

        private int? CurrentUserId
        {
            get
            {
                object value = Session["MaNguoiDung"];
                int userId;

                if (value != null &&
                    int.TryParse(value.ToString(), out userId))
                {
                    return userId;
                }

                return null;
            }
        }

        // ==========================================
        // 2. CHUẨN HÓA HÌNH THỨC ĐẶT HÀNG
        // ==========================================

        private string NormalizeOrderType(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            string normalized = value.Trim().ToLowerInvariant();

            if (normalized == "mua" || normalized == "buy")
                return "Mua";

            if (normalized == "thue" ||
                normalized == "thuê" ||
                normalized == "rent")
            {
                return "Thue";
            }

            return null;
        }

        // ==========================================
        // 3. CHUẨN HÓA PHƯƠNG THỨC THANH TOÁN
        // ==========================================

        private string NormalizePaymentMethod(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            string normalized = value.Trim().ToLowerInvariant();

            // Chuyển khoản / thanh toán trực tuyến.
            if (normalized == "bank_transfer" ||
                normalized == "tructuyen")
            {
                return "TRUCTUYEN";
            }

            // Thanh toán khi nhận hàng.
            if (normalized == "cod")
            {
                return "COD";
            }

            // Không tự động chuyển card/credit
            // khi database chưa hỗ trợ các phương thức này.
            return null;
        }

        // ==========================================
        // 4. LẤY GIỎ HÀNG NGƯỜI DÙNG
        // ==========================================

        private GioHang GetUserCart(StageFxEntities db, int userId)
        {
            return db.GioHang
                .Include("ChiTietGioHang.SanPham")
                .FirstOrDefault(x => x.MaNguoiDung == userId);
        }

        // ==========================================
        // 5. TÍNH TỔNG TIỀN GIỎ HÀNG
        // ==========================================

        private bool PopulateCartSummary(
            CheckoutViewModel model,
            int userId)
        {
            using (var db = new StageFxEntities())
            {
                var cart = GetUserCart(db, userId);

                model.Items = new List<CheckoutCartItemViewModel>();

                if (cart == null ||
                    cart.ChiTietGioHang == null ||
                    !cart.ChiTietGioHang.Any())
                {
                    return false;
                }

                decimal goodsSubtotal = 0m;
                decimal rentalSubtotal = 0m;
                int totalQuantity = 0;

                foreach (var cartItem in cart.ChiTietGioHang)
                {
                    var product = cartItem.SanPham;

                    if (product == null)
                        return false;

                    string orderType =
                        NormalizeOrderType(cartItem.HinhThucDat);

                    if (orderType == null || cartItem.SoLuong <= 0)
                        return false;

                    decimal unitPrice;
                    decimal baseAmount;

                    if (orderType == "Mua")
                    {
                        unitPrice = product.GiaBan;

                        baseAmount =
                            unitPrice * cartItem.SoLuong;
                    }
                    else
                    {
                        if (!product.GiaThueNgay.HasValue ||
                            product.GiaThueNgay.Value <= 0 ||
                            !cartItem.SoNgayThue.HasValue ||
                            cartItem.SoNgayThue.Value <= 0)
                        {
                            return false;
                        }

                        unitPrice = product.GiaThueNgay.Value;

                        baseAmount =
                            unitPrice *
                            cartItem.SoNgayThue.Value *
                            cartItem.SoLuong;

                        rentalSubtotal += baseAmount;
                    }

                    decimal flightcaseFee =
                        cartItem.CoFlightcase
                        ? FLIGHTCASE_FEE
                        : 0m;

                    decimal lineTotal =
                        baseAmount +
                        flightcaseFee * cartItem.SoLuong;

                    goodsSubtotal += lineTotal;
                    totalQuantity += cartItem.SoLuong;

                    model.Items.Add(new CheckoutCartItemViewModel
                    {
                        MaSanPham = product.MaSanPham,
                        SKU = product.SKU,
                        TenSanPham = product.TenSanPham,
                        HinhThucDat = orderType,
                        SoLuong = cartItem.SoLuong,
                        SoNgayThue = cartItem.SoNgayThue,
                        CoFlightcase = cartItem.CoFlightcase,
                        DonGia = unitPrice,
                        PhiFlightcase = flightcaseFee,
                        ThanhTien = lineTotal
                    });
                }

                model.TongSoLuong = totalQuantity;
                model.TienHang = goodsSubtotal;

                model.PhiVanChuyen =
                    goodsSubtotal >= SHIPPING_THRESHOLD
                    ? 0m
                    : SHIPPING_FEE;

                // VAT hiện chưa được tính tại Checkout.
                model.VAT = 0m;

                // Tiền cọc chỉ tính trên tiền thuê thiết bị.
                // Không bao gồm phí Flightcase.
                model.TienCoc =
                    rentalSubtotal * RENTAL_DEPOSIT_RATE;

                model.TongTien =
                    model.TienHang + model.PhiVanChuyen;

                return model.Items.Any();
            }
        }

        // ==========================================
        // 6. HIỂN THỊ TRANG CHECKOUT
        // ==========================================

        [HttpGet]
        public ActionResult Index()
        {
            if (!CurrentUserId.HasValue)
                return RedirectToAction("Login", "Account");

            var model = new CheckoutViewModel();

            bool hasItems = PopulateCartSummary(
                model,
                CurrentUserId.Value
            );

            if (!hasItems)
                return RedirectToAction("Index", "Cart");

            model.ContactName = Session["HoTen"] != null
                ? Session["HoTen"].ToString()
                : "";

            model.Email = Session["Email"] != null
                ? Session["Email"].ToString()
                : "";

            return View(model);
        }

        // ==========================================
        // 7. KIỂM TRA DỮ LIỆU CHECKOUT
        // ==========================================

        private void ValidateCheckout(CheckoutViewModel model)
        {
            if (!model.SiteReview)
            {
                ModelState.AddModelError(
                    "SiteReview",
                    "Vui lòng xác nhận yêu cầu khảo sát địa điểm."
                );
            }

            if (!model.EmergencyPlan)
            {
                ModelState.AddModelError(
                    "EmergencyPlan",
                    "Vui lòng xác nhận kế hoạch ứng phó."
                );
            }

            if (!model.QualifiedOperator)
            {
                ModelState.AddModelError(
                    "QualifiedOperator",
                    "Vui lòng xác nhận yêu cầu người vận hành đủ chuyên môn."
                );
            }

            if (!model.EventDate.HasValue)
            {
                ModelState.AddModelError(
                    "EventDate",
                    "Vui lòng chọn ngày tổ chức."
                );
            }
            else if (model.EventDate.Value.Date < DateTime.Today)
            {
                ModelState.AddModelError(
                    "EventDate",
                    "Ngày tổ chức không được nằm trong quá khứ."
                );
            }

            if (model.VenueType != "indoor" &&
                model.VenueType != "outdoor")
            {
                ModelState.AddModelError(
                    "VenueType",
                    "Loại địa điểm không hợp lệ."
                );
            }

            if (NormalizePaymentMethod(model.PaymentMethod) == null)
            {
                ModelState.AddModelError(
                    "PaymentMethod",
                    "Phương thức thanh toán không hợp lệ. Vui lòng chọn chuyển khoản hoặc COD."
                );
            }

            if (string.IsNullOrWhiteSpace(model.Company))
            {
                ModelState.AddModelError(
                    "Company",
                    "Vui lòng nhập tên công ty."
                );
            }

            if (string.IsNullOrWhiteSpace(model.ContactName))
            {
                ModelState.AddModelError(
                    "ContactName",
                    "Vui lòng nhập người liên hệ."
                );
            }

            if (string.IsNullOrWhiteSpace(model.Phone))
            {
                ModelState.AddModelError(
                    "Phone",
                    "Vui lòng nhập số điện thoại."
                );
            }

            if (string.IsNullOrWhiteSpace(model.Email))
            {
                ModelState.AddModelError(
                    "Email",
                    "Vui lòng nhập email."
                );
            }

            if (string.IsNullOrWhiteSpace(model.EventName))
            {
                ModelState.AddModelError(
                    "EventName",
                    "Vui lòng nhập tên sự kiện."
                );
            }

            if (string.IsNullOrWhiteSpace(model.EventLocation))
            {
                ModelState.AddModelError(
                    "EventLocation",
                    "Vui lòng nhập địa điểm tổ chức."
                );
            }
        }

        // ==========================================
        // 8. TẠO ĐƠN HÀNG
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PlaceOrder(CheckoutViewModel model)
        {
            if (!CurrentUserId.HasValue)
                return RedirectToAction("Login", "Account");

            if (model == null)
                return RedirectToAction("Index", "Cart");

            int userId = CurrentUserId.Value;

            // Kiểm tra dữ liệu đầu vào.
            ValidateCheckout(model);

            if (!ModelState.IsValid)
            {
                PopulateCartSummary(model, userId);
                return View("Index", model);
            }

            // Giá trị chuẩn dùng riêng cho bảng ThanhToan.
            string paymentMethod =
                NormalizePaymentMethod(model.PaymentMethod);

            using (var db = new StageFxEntities())
            using (var transaction = db.Database.BeginTransaction())
            {
                string currentStage = "Khởi tạo transaction";

                try
                {
                    Debug.WriteLine("");
                    Debug.WriteLine(
                        "========== STAGEFX CHECKOUT START =========="
                    );

                    // --------------------------------------
                    // BƯỚC 1: ĐỌC GIỎ HÀNG
                    // --------------------------------------

                    currentStage = "Đọc giỏ hàng";

                    var cart = GetUserCart(db, userId);

                    if (cart == null ||
                        cart.ChiTietGioHang == null ||
                        !cart.ChiTietGioHang.Any())
                    {
                        throw new Exception(
                            "Giỏ hàng không có sản phẩm."
                        );
                    }

                    decimal goodsSubtotal = 0m;
                    decimal rentalSubtotal = 0m;

                    var orderLines = new List<OrderLineData>();

                    // --------------------------------------
                    // BƯỚC 2: KIỂM TRA SẢN PHẨM VÀ TÍNH GIÁ
                    // --------------------------------------

                    currentStage = "Kiểm tra sản phẩm và tính giá";

                    foreach (var cartItem in cart.ChiTietGioHang)
                    {
                        var product = cartItem.SanPham;

                        if (product == null)
                        {
                            throw new Exception(
                                "Không tìm thấy sản phẩm trong giỏ hàng."
                            );
                        }

                        if (!product.TrangThai)
                        {
                            throw new Exception(
                                "Sản phẩm " + product.TenSanPham +
                                " hiện không còn kinh doanh."
                            );
                        }

                        if (cartItem.SoLuong <= 0)
                        {
                            throw new Exception(
                                "Số lượng sản phẩm không hợp lệ."
                            );
                        }

                        string orderType =
                            NormalizeOrderType(cartItem.HinhThucDat);

                        if (orderType == null)
                        {
                            throw new Exception(
                                "Hình thức đặt của sản phẩm " +
                                product.TenSanPham +
                                " không hợp lệ."
                            );
                        }

                        if (product.GiaBan < 0 ||
                            (product.GiaThueNgay.HasValue &&
                             product.GiaThueNgay.Value < 0))
                        {
                            throw new Exception(
                                "Giá sản phẩm không hợp lệ: " +
                                product.TenSanPham
                            );
                        }

                        if (product.SoLuongTon < cartItem.SoLuong)
                        {
                            throw new Exception(
                                "Sản phẩm " + product.TenSanPham +
                                " không đủ số lượng tồn kho."
                            );
                        }

                        decimal unitPrice;
                        int? rentalDays = null;
                        decimal baseAmount;

                        if (orderType == "Mua")
                        {
                            unitPrice = product.GiaBan;

                            baseAmount =
                                unitPrice * cartItem.SoLuong;
                        }
                        else
                        {
                            if (!product.GiaThueNgay.HasValue ||
                                product.GiaThueNgay.Value <= 0)
                            {
                                throw new Exception(
                                    "Sản phẩm " + product.TenSanPham +
                                    " chưa có giá thuê hợp lệ."
                                );
                            }

                            if (!cartItem.SoNgayThue.HasValue ||
                                cartItem.SoNgayThue.Value <= 0)
                            {
                                throw new Exception(
                                    "Số ngày thuê không hợp lệ: " +
                                    product.TenSanPham
                                );
                            }

                            unitPrice = product.GiaThueNgay.Value;
                            rentalDays = cartItem.SoNgayThue.Value;

                            baseAmount =
                                unitPrice *
                                rentalDays.Value *
                                cartItem.SoLuong;

                            rentalSubtotal += baseAmount;
                        }

                        decimal flightcaseFee =
                            cartItem.CoFlightcase
                            ? FLIGHTCASE_FEE
                            : 0m;

                        decimal lineTotal =
                            baseAmount +
                            flightcaseFee * cartItem.SoLuong;

                        goodsSubtotal += lineTotal;

                        orderLines.Add(new OrderLineData
                        {
                            Product = product,
                            UnitPrice = unitPrice,
                            Quantity = cartItem.SoLuong,
                            OrderType = orderType,
                            RentalDays = rentalDays,
                            HasFlightcase = cartItem.CoFlightcase,
                            FlightcaseFee = flightcaseFee
                        });
                    }

                    if (!orderLines.Any())
                    {
                        throw new Exception(
                            "Không có sản phẩm hợp lệ để tạo đơn hàng."
                        );
                    }

                    // --------------------------------------
                    // BƯỚC 3: TÍNH TỔNG TIỀN
                    // --------------------------------------

                    currentStage = "Tính tổng tiền đơn hàng";

                    decimal shippingFee =
                        goodsSubtotal >= SHIPPING_THRESHOLD
                        ? 0m
                        : SHIPPING_FEE;

                    decimal deposit =
                        rentalSubtotal * RENTAL_DEPOSIT_RATE;

                    decimal totalAmount =
                        goodsSubtotal + shippingFee;

                    bool hasRental = rentalSubtotal > 0m;

                    // --------------------------------------
                    // BƯỚC 4: LƯU ĐƠN HÀNG
                    // --------------------------------------

                    currentStage = "Lưu bảng DonHang";

                    string orderCode =
                        "SFX-" +
                        DateTime.Now.ToString("yyyyMMddHHmmss") +
                        "-" +
                        Guid.NewGuid().ToString("N")
                            .Substring(0, 6)
                            .ToUpperInvariant();

                    var order = new DonHang
                    {
                        MaSoDonHang = orderCode,
                        MaNguoiDung = userId,

                        TenNguoiNhan = model.ContactName.Trim(),
                        SoDienThoaiNhan = model.Phone.Trim(),
                        DiaChiNhanHang = model.EventLocation.Trim(),

                        NgayDat = DateTime.Now,

                        TienHang = goodsSubtotal,
                        TienGiamGia = 0m,
                        PhiVanChuyen = shippingFee,
                        TongTien = totalAmount,

                        TrangThai = "ChoXacNhan",
                        TrangThaiThanhToan = "ChuaThanhToan",

                        GhiChu = model.SafetyNotes,

                        TienCoc = deposit,
                        TyLeDatCoc = hasRental
                            ? RENTAL_DEPOSIT_RATE
                            : 0m,

                        NgayTao = DateTime.Now
                    };

                    db.DonHang.Add(order);
                    db.SaveChanges();

                    Debug.WriteLine(
                        "[STAGEFX] DonHang saved. ID = " +
                        order.MaDonHang
                    );

                    // --------------------------------------
                    // BƯỚC 5: LƯU CHI TIẾT ĐƠN HÀNG
                    // --------------------------------------

                    currentStage =
                        "Lưu bảng ChiTietDonHang và cập nhật tồn kho";

                    foreach (var line in orderLines)
                    {
                        var detail = new ChiTietDonHang
                        {
                            MaDonHang = order.MaDonHang,
                            MaSanPham = line.Product.MaSanPham,

                            TenSanPhamLucDat =
                                line.Product.TenSanPham,

                            SKULucDat =
                                line.Product.SKU,

                            DonGia = line.UnitPrice,
                            SoLuong = line.Quantity,

                            HinhThucDat = line.OrderType,
                            SoNgayThue = line.RentalDays,

                            CoFlightcase = line.HasFlightcase,
                            PhiFlightcase = line.FlightcaseFee
                        };

                        // ThanhTien là cột computed trong SQL.
                        db.ChiTietDonHang.Add(detail);

                        // Chỉ trừ tồn kho khi mua sản phẩm.
                        if (line.OrderType == "Mua")
                        {
                            line.Product.SoLuongTon -= line.Quantity;
                            line.Product.NgayCapNhat = DateTime.Now;
                        }
                    }

                    db.SaveChanges();

                    Debug.WriteLine(
                        "[STAGEFX] ChiTietDonHang saved."
                    );

                    // --------------------------------------
                    // BƯỚC 6: LƯU THÔNG TIN DỰ ÁN
                    // --------------------------------------

                    currentStage = "Lưu bảng ThongTinDuAn";

                    string venueType =
                        model.VenueType == "outdoor"
                        ? "NgoaiTroi"
                        : "TrongNha";

                    var projectInfo = new ThongTinDuAn
                    {
                        MaDonHang = order.MaDonHang,

                        TenCongTy = model.Company.Trim(),

                        MaSoThue = string.IsNullOrWhiteSpace(model.TaxId)
                            ? null
                            : model.TaxId.Trim(),

                        TenNguoiLienHe = model.ContactName.Trim(),
                        EmailLienHe = model.Email.Trim(),

                        TenSuKien = model.EventName.Trim(),
                        NgayToChuc = model.EventDate,

                        LoaiDiaDiem = venueType,
                        DiaDiemToChuc = model.EventLocation.Trim(),

                        ThongTinKetCau = model.StructureInfo,
                        CoKyThuatVien = model.SiteTechnician,
                        GhiChuAnToan = model.SafetyNotes,

                        // Giữ nguyên giá trị giao diện ở bảng dự án.
                        // Bảng ThanhToan dùng mã chuẩn TRUCTUYEN/COD.
                        PhuongThucThanhToan = model.PaymentMethod,

                        NgayTao = DateTime.Now
                    };

                    db.ThongTinDuAn.Add(projectInfo);
                    db.SaveChanges();

                    Debug.WriteLine(
                        "[STAGEFX] ThongTinDuAn saved."
                    );

                    // --------------------------------------
                    // BƯỚC 7: LƯU THÔNG TIN THANH TOÁN
                    // --------------------------------------

                    // --------------------------------------
                    // BƯỚC 7: LƯU THÔNG TIN THANH TOÁN
                    // --------------------------------------

                    currentStage = "Lưu bảng ThanhToan";

                    var payment = new ThanhToan
                    {
                        MaDonHang = order.MaDonHang,

                        // Database chỉ chấp nhận TRUCTUYEN hoặc COD.
                        PhuongThuc = paymentMethod,

                        // Tiền cọc dự kiến, chưa phát sinh giao dịch.
                        SoTien = deposit,

                        // Database chỉ chấp nhận:
                        // ChoXuLy, DaThanhToan, ThatBai, DaHoanTien.
                        TrangThaiThanhToan = "ChoXuLy",

                        MaGiaoDich = null,
                        DonViThanhToan = null,
                        NgayThanhToan = null,

                        NgayTao = DateTime.Now
                    };

                    db.ThanhToan.Add(payment);
                    db.SaveChanges();

                    Debug.WriteLine(
                        "[STAGEFX] ThanhToan saved. Method = " +
                        paymentMethod +
                        " | Status = " +
                        payment.TrangThaiThanhToan
                    );
                    // --------------------------------------
                    // BƯỚC 8: XÓA GIỎ HÀNG
                    // --------------------------------------

                    currentStage = "Xóa chi tiết giỏ hàng";

                    foreach (var cartItem in cart.ChiTietGioHang.ToList())
                    {
                        db.ChiTietGioHang.Remove(cartItem);
                    }

                    db.SaveChanges();

                    Debug.WriteLine(
                        "[STAGEFX] Cart cleared."
                    );

                    // --------------------------------------
                    // BƯỚC 9: COMMIT TRANSACTION
                    // --------------------------------------

                    currentStage = "Commit transaction";

                    transaction.Commit();

                    Debug.WriteLine(
                        "[STAGEFX] ORDER CREATED SUCCESSFULLY: " +
                        orderCode
                    );

                    Debug.WriteLine(
                        "========== STAGEFX CHECKOUT SUCCESS =========="
                    );

                    TempData["MaSoDonHang"] = orderCode;
                    TempData["TongTien"] = totalAmount;
                    TempData["TienCoc"] = deposit;

                    return RedirectToAction("Confirmation");
                }
                catch (Exception ex)
                {
                    Exception baseException = ex.GetBaseException();

                    Debug.WriteLine("");
                    Debug.WriteLine(
                        "========== STAGEFX CHECKOUT ERROR =========="
                    );

                    Debug.WriteLine(
                        "FAILED STAGE: " + currentStage
                    );

                    Debug.WriteLine(ex.ToString());

                    // Rollback toàn bộ thao tác trong transaction.
                    try
                    {
                        transaction.Rollback();

                        Debug.WriteLine(
                            "[STAGEFX] Transaction rolled back."
                        );
                    }
                    catch (Exception rollbackEx)
                    {
                        Debug.WriteLine(
                            "[STAGEFX] ROLLBACK ERROR: " +
                            rollbackEx.ToString()
                        );
                    }

                    // Hiển thị lỗi để kiểm tra trong giai đoạn phát triển.
                    ModelState.AddModelError(
                        "",
                        "Lỗi tại: " + currentStage +
                        " | Chi tiết: " + baseException.Message
                    );

                    PopulateCartSummary(model, userId);

                    return View("Index", model);
                }
            }
        }

        // ==========================================
        // 9. TRANG XÁC NHẬN ĐƠN HÀNG
        // ==========================================

        [HttpGet]
        public ActionResult Confirmation()
        {
            if (TempData["MaSoDonHang"] == null)
                return RedirectToAction("Index", "Cart");

            ViewBag.MaSoDonHang = TempData["MaSoDonHang"];
            ViewBag.TongTien = TempData["TongTien"];
            ViewBag.TienCoc = TempData["TienCoc"];

            return View();
        }

        // ==========================================
        // 10. CLASS LƯU DỮ LIỆU TẠM SẢN PHẨM
        // ==========================================

        private class OrderLineData
        {
            public SanPham Product { get; set; }

            public decimal UnitPrice { get; set; }

            public int Quantity { get; set; }

            public string OrderType { get; set; }

            public int? RentalDays { get; set; }

            public bool HasFlightcase { get; set; }

            public decimal FlightcaseFee { get; set; }
        }
    }
}

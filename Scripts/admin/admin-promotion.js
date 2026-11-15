
document.addEventListener("DOMContentLoaded", function () {

    // ==========================================
    // 1. XÁC NHẬN TRƯỚC KHI XÓA KHUYẾN MÃI
    // ==========================================

    document.querySelectorAll(".promotion-delete-form").forEach(function (form) {

        form.addEventListener("submit", function (event) {

            if (form.dataset.submitting === "true") {
                return;
            }

            const confirmed = window.confirm(
                "Bạn có chắc chắn muốn xóa chương trình khuyến mãi này không?\n\n" +
                "Lưu ý: Không thể xóa khuyến mãi đã được sử dụng " +
                "hoặc liên kết với đơn hàng."
            );

            if (!confirmed) {
                event.preventDefault();
                return;
            }

            form.dataset.submitting = "true";
            disableSubmitButton(form);

        });

    });


    // ==========================================
    // 2. XÁC NHẬN BẬT / TẮT KHUYẾN MÃI
    // ==========================================

    document.querySelectorAll(".promotion-toggle-form").forEach(function (form) {

        form.addEventListener("submit", function (event) {

            if (form.dataset.submitting === "true") {
                return;
            }

            const button = form.querySelector("button");

            const icon = button
                ? button.querySelector(".material-symbols-outlined")
                : null;

            const isActive = icon &&
                icon.textContent.trim() === "toggle_on";

            const message = isActive
                ? "Bạn có chắc chắn muốn ngừng kích hoạt khuyến mãi này không?"
                : "Bạn có chắc chắn muốn kích hoạt khuyến mãi này không?";

            const confirmed = window.confirm(message);

            if (!confirmed) {
                event.preventDefault();
                return;
            }

            form.dataset.submitting = "true";
            disableSubmitButton(form);

        });

    });


    // ==========================================
    // 3. HÀM KHÓA NÚT SAU KHI XÁC NHẬN
    // ==========================================

    function disableSubmitButton(form) {

        const button = form.querySelector("button[type='submit']");

        if (!button) {
            return;
        }

        button.disabled = true;
        button.style.opacity = "0.5";
        button.style.cursor = "not-allowed";

    }


    // ==========================================
    // 4. TỰ ĐỘNG ẨN THÔNG BÁO
    // ==========================================

    document.querySelectorAll(".promotion-alert").forEach(function (alert) {

        window.setTimeout(function () {

            alert.style.transition =
                "opacity .3s ease, transform .3s ease";

            alert.style.opacity = "0";
            alert.style.transform = "translateY(-5px)";

            window.setTimeout(function () {
                alert.remove();
            }, 350);

        }, 5000);

    });


    // ==========================================
    // 5. HIỂN THỊ TRƯỜNG GIẢM TỐI ĐA
    // ==========================================

    const discountType = document.getElementById("LoaiGiamGia");

    const maxDiscountGroup = document.getElementById(
        "promotion-max-discount-group"
    );

    const discountHint = document.getElementById(
        "promotion-discount-hint"
    );

    function updateDiscountFields() {

        if (!discountType) {
            return;
        }

        const isPercent = discountType.value === "PhanTram";

        if (maxDiscountGroup) {
            maxDiscountGroup.style.display = isPercent ? "" : "none";
        }

        if (discountHint) {
            discountHint.textContent = isPercent
                ? "Nhập tỷ lệ giảm từ 0.01 đến 100 (%)."
                : "Nhập số tiền giảm bằng VNĐ.";
        }

    }

    if (discountType) {

        discountType.addEventListener(
            "change",
            updateDiscountFields
        );

        updateDiscountFields();

    }


    // ==========================================
    // 6. KIỂM TRA NGÀY BẮT ĐẦU / KẾT THÚC
    // ==========================================

    const startDate = document.querySelector(
        "input[name='NgayBatDau']"
    );

    const endDate = document.querySelector(
        "input[name='NgayKetThuc']"
    );

    function updateEndDateMinimum() {

        if (!startDate || !endDate) {
            return;
        }

        if (startDate.value) {
            endDate.min = startDate.value;
        } else {
            endDate.removeAttribute("min");
        }

    }

    if (startDate && endDate) {

        startDate.addEventListener(
            "change",
            updateEndDateMinimum
        );

        updateEndDateMinimum();

    }


    // ==========================================
    // 7. CHỐNG GỬI FORM THÊM / SỬA NHIỀU LẦN
    // ==========================================

    const promotionForm = document.getElementById("promotion-form");

    if (promotionForm) {

        promotionForm.addEventListener("submit", function (event) {

            if (promotionForm.dataset.submitting === "true") {
                event.preventDefault();
                return;
            }

            if (!promotionForm.checkValidity()) {
                return;
            }

            promotionForm.dataset.submitting = "true";

            const submitButton = promotionForm.querySelector(
                "button[type='submit']"
            );

            if (submitButton) {

                submitButton.disabled = true;
                submitButton.style.opacity = "0.6";
                submitButton.style.cursor = "not-allowed";

            }

        });

    }

});

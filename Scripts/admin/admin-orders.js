document.addEventListener("DOMContentLoaded", function () {
    const searchInput = document.getElementById("orderSearch");
    const clearButton = document.getElementById("clearOrderSearch");
    const filterForm = document.getElementById("orderFilterForm");

    if (!searchInput || !clearButton || !filterForm) {
        return;
    }

    function updateClearButton() {
        if (searchInput.value.trim().length > 0) {
            clearButton.classList.add("show");
        } else {
            clearButton.classList.remove("show");
        }
    }

    searchInput.addEventListener("input", updateClearButton);

    clearButton.addEventListener("click", function () {
        searchInput.value = "";
        updateClearButton();
        searchInput.focus();
    });

    searchInput.addEventListener("keydown", function (event) {
        if (event.key === "Enter") {
            event.preventDefault();
            filterForm.submit();
        }
    });

    // Kiểm tra ngày lọc
    filterForm.addEventListener("submit", function (event) {
        const dateFrom = filterForm.querySelector(
            'input[name="dateFrom"]'
        );

        const dateTo = filterForm.querySelector(
            'input[name="dateTo"]'
        );

        if (
            dateFrom &&
            dateTo &&
            dateFrom.value &&
            dateTo.value &&
            dateFrom.value > dateTo.value
        ) {
            event.preventDefault();
            alert("Ngày bắt đầu không được lớn hơn ngày kết thúc.");
        }
    });

    updateClearButton();
});

document.addEventListener("DOMContentLoaded", function () {

    // XÁC NHẬN ĐƠN HÀNG
    document.querySelectorAll("[data-confirm-order-form]")
        .forEach(form => {
            form.addEventListener("submit", function (event) {
                const confirmed = window.confirm(
                    "Bạn có chắc chắn muốn xác nhận đơn hàng này?"
                );

                if (!confirmed) {
                    event.preventDefault();
                }
            });
        });

    // TỪ CHỐI ĐƠN HÀNG
    document.querySelectorAll("[data-reject-order-form]")
        .forEach(form => {
            form.addEventListener("submit", function (event) {
                const reason = window.prompt(
                    "Nhập lý do từ chối đơn hàng:"
                );

                if (reason === null) {
                    event.preventDefault();
                    return;
                }

                if (!reason.trim()) {
                    event.preventDefault();
                    window.alert(
                        "Vui lòng nhập lý do từ chối đơn hàng."
                    );
                    return;
                }

                const reasonInput =
                    form.querySelector('input[name="reason"]');

                reasonInput.value = reason.trim();
            });
        });

});

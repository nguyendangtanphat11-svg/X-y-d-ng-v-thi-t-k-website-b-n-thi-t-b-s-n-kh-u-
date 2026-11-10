
document.addEventListener('DOMContentLoaded', function () {
    'use strict';

    // ==========================================
    // 1. ELEMENTS
    // ==========================================

    const searchInput = document.getElementById('productSearch');
    const clearButton = document.getElementById('clearProductSearch');
    const filterForm = document.getElementById('productFilterForm');

    const filterControls = filterForm
        ? filterForm.querySelectorAll('select')
        : [];

    const submitButton = filterForm
        ? filterForm.querySelector('[type="submit"]')
        : null;

    // ==========================================
    // 2. CLEAR SEARCH
    // ==========================================

    if (searchInput && clearButton && filterForm) {

        function updateClearButton() {
            clearButton.hidden = searchInput.value.trim().length === 0;
        }

        updateClearButton();

        searchInput.addEventListener('input', updateClearButton);

        clearButton.addEventListener('click', function () {

            searchInput.value = '';

            updateClearButton();

            filterForm.submit();

        });

        // ==========================================
        // 3. SEARCH WITH ENTER
        // ==========================================

        searchInput.addEventListener('keydown', function (event) {

            if (event.key === 'Enter') {

                event.preventDefault();

                filterForm.requestSubmit();

            }

        });

    }

    // ==========================================
    // 4. FILTER CHANGE
    // ==========================================

    filterControls.forEach(function (control) {

        control.addEventListener('change', function () {

            if (filterForm) {
                filterForm.requestSubmit();
            }

        });

    });

    // ==========================================
    // 5. PREVENT MULTIPLE SUBMISSIONS
    // ==========================================

    if (filterForm) {

        filterForm.addEventListener('submit', function () {

            if (submitButton) {

                submitButton.disabled = true;

                submitButton.classList.add('is-loading');

            }

        });

    }

    // ==========================================
    // 6. DELETE PRODUCT CONFIRMATION
    // ==========================================

    document.querySelectorAll('.product-delete-form')
        .forEach(function (form) {

            form.addEventListener('submit', function (event) {

                const confirmed = window.confirm(
                    'Bạn có chắc chắn muốn xử lý xóa sản phẩm này?\n\n' +
                    'Nếu sản phẩm đã phát sinh dữ liệu liên quan, hệ thống ' +
                    'sẽ chuyển sản phẩm sang trạng thái ngừng kinh doanh.'
                );

                if (!confirmed) {
                    event.preventDefault();
                }

            });

        });

    // ==========================================
    // 7. TOGGLE PRODUCT STATUS
    // ==========================================

    document.querySelectorAll('.product-toggle-form')
        .forEach(function (form) {

            form.addEventListener('submit', function (event) {

                const confirmed = window.confirm(
                    'Bạn có chắc chắn muốn thay đổi trạng thái kinh doanh của sản phẩm?'
                );

                if (!confirmed) {
                    event.preventDefault();
                }

            });

        });

});

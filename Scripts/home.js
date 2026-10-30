
'use strict';

/* =========================================================
   STAGEFX PRO - HOME JAVASCRIPT
   ASP.NET MVC 5 + Entity Framework + SQL Server
========================================================= */


/* =========================================================
   1. MENU MOBILE
========================================================= */

const menuToggle = document.querySelector('.menu-toggle');
const mainNav = document.querySelector('#mainNav');

if (menuToggle && mainNav) {

    menuToggle.addEventListener('click', () => {

        const isOpen = mainNav.classList.toggle('open');

        menuToggle.setAttribute('aria-expanded', String(isOpen));
        menuToggle.textContent = isOpen ? '✕' : '☰';

    });

    mainNav.querySelectorAll('a, button').forEach((item) => {

        item.addEventListener('click', () => {

            mainNav.classList.remove('open');

            menuToggle.setAttribute('aria-expanded', 'false');
            menuToggle.textContent = '☰';

        });

    });

}


/* =========================================================
   2. CUỘN ĐẾN SECTION
========================================================= */

document.querySelectorAll('[data-scroll]').forEach((button) => {

    button.addEventListener('click', () => {

        const selector = button.dataset.scroll;

        if (!selector) return;

        const target = document.querySelector(selector);

        if (target) {

            target.scrollIntoView({
                behavior: 'smooth',
                block: 'start'
            });

        }

    });

});


/* =========================================================
   3. LỌC SẢN PHẨM THEO MÃ DANH MỤC DATABASE
========================================================= */

const filterButtons = document.querySelectorAll('[data-filter]');
const productCards = document.querySelectorAll('[data-product-category]');

if (filterButtons.length > 0 && productCards.length > 0) {

    filterButtons.forEach((button) => {

        button.addEventListener('click', () => {

            const selectedCategory = button.dataset.filter;

            // Cập nhật nút đang được chọn
            filterButtons.forEach((item) => {

                item.classList.toggle('active', item === button);

            });

            // Lọc sản phẩm
            productCards.forEach((card) => {

                const productCategory = card.dataset.productCategory;

                const shouldShow =
                    selectedCategory === 'all' ||
                    productCategory === selectedCategory;

                card.hidden = !shouldShow;

            });

        });

    });

}


/* =========================================================
   4. CHỌN SẢN PHẨM ĐỂ YÊU CẦU BÁO GIÁ
========================================================= */

const requestedProduct = document.querySelector('#requestedProduct');
const consultForm = document.querySelector('#consultForm');

document.querySelectorAll('[data-product]').forEach((button) => {

    button.addEventListener('click', () => {

        const productName = button.dataset.product || '';

        // Điền sản phẩm vào form
        if (requestedProduct) {

            requestedProduct.value = productName;

        }

        // Cuộn đến form tư vấn
        if (consultForm) {

            consultForm.scrollIntoView({
                behavior: 'smooth',
                block: 'center'
            });

            // Focus ô tên công ty
            const companyInput = document.querySelector('#companyName');

            if (companyInput) {

                companyInput.focus({
                    preventScroll: true
                });

            }

        }

    });

});



/* =========================================================
   5. GỬI FORM TƯ VẤN LÊN ASP.NET MVC - DEBUG
========================================================= */

const formMessage = document.querySelector('#formMessage');

if (consultForm && formMessage) {

    consultForm.addEventListener('submit', async (event) => {

        event.preventDefault();

        if (!consultForm.reportValidity()) return;

        const submitButton = consultForm.querySelector(
            'button[type="submit"]'
        );

        const originalText = submitButton
            ? submitButton.textContent
            : '';

        try {

            if (submitButton) {
                submitButton.disabled = true;
                submitButton.textContent = 'ĐANG GỬI...';
            }

            formMessage.textContent = 'Đang gửi yêu cầu...';
            formMessage.className = 'form-message';

            const formData = new FormData(consultForm);

            console.log('URL gửi:', consultForm.action);
            console.log('Dữ liệu gửi:', [...formData.entries()]);

            const response = await fetch(consultForm.action, {
                method: 'POST',
                body: formData,
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });

            // Đọc dạng text trước để không mất lỗi HTML hoặc JSON
            const responseText = await response.text();

            console.log('HTTP Status:', response.status);
            console.log('Server Response:', responseText);

            if (!response.ok) {
                throw new Error(
                    `HTTP ${response.status}: ${responseText.substring(0, 500)}`
                );
            }

            let result;

            try {
                result = JSON.parse(responseText);
            } catch (parseError) {
                throw new Error(
                    'Máy chủ không trả về JSON hợp lệ: ' +
                    responseText.substring(0, 500)
                );
            }

            if (typeof result.success !== 'boolean') {
                throw new Error(
                    'Phản hồi máy chủ thiếu thuộc tính success.'
                );
            }

            formMessage.textContent =
                result.message || 'Không có thông báo từ máy chủ.';

            if (result.success) {

                formMessage.className = 'form-message success';

                consultForm.reset();

                if (requestedProduct) {
                    requestedProduct.value = '';
                }

            } else {

                formMessage.className = 'form-message error';

            }

        } catch (error) {

            console.error(
                'STAGEFX PRO - Lỗi gửi tư vấn:',
                error
            );

            formMessage.textContent =
                error.message || 'Không thể gửi yêu cầu tư vấn.';

            formMessage.className = 'form-message error';

        } finally {

            if (submitButton) {
                submitButton.disabled = false;
                submitButton.textContent = originalText;
            }

        }

    });

}



/* =========================================================
   6. CẬP NHẬT NĂM HIỆN TẠI Ở FOOTER
========================================================= */

const year = document.querySelector('#year');

if (year) {

    year.textContent = new Date().getFullYear();

}


/* =========================================================
   7. KHỞI TẠO HOÀN TẤT
========================================================= */

console.log('STAGEFX PRO - Home JavaScript initialized.');

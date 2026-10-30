
'use strict';

document.addEventListener('DOMContentLoaded', function () {

    // DROPDOWN TÀI KHOẢN
    const userDropdown = document.getElementById('siteUserDropdown');
    const userToggle = document.getElementById('userDropdownToggle');

    if (userDropdown && userToggle) {
        function closeUserDropdown() {
            userDropdown.classList.remove('active');
            userToggle.setAttribute('aria-expanded', 'false');
        }

        userToggle.addEventListener('click', function (event) {
            event.stopPropagation();

            const isOpen = userDropdown.classList.toggle('active');
            userToggle.setAttribute('aria-expanded', String(isOpen));
        });

        document.addEventListener('click', function (event) {
            if (!userDropdown.contains(event.target)) {
                closeUserDropdown();
            }
        });

        document.addEventListener('keydown', function (event) {
            if (event.key === 'Escape') {
                closeUserDropdown();
            }
        });
    }

    // MENU MOBILE
    const menuToggle = document.querySelector('.site-shell-menu-toggle');
    const nav = document.getElementById('site-shell-nav');

    if (menuToggle && nav) {
        function closeMobileMenu() {
            nav.classList.remove('open');
            menuToggle.setAttribute('aria-expanded', 'false');
            menuToggle.setAttribute('aria-label', 'Mở menu');

            const icon = menuToggle.querySelector('.material-symbols-outlined');
            if (icon) icon.textContent = 'menu';
        }

        menuToggle.addEventListener('click', function () {
            const isOpen = nav.classList.toggle('open');

            menuToggle.setAttribute('aria-expanded', String(isOpen));
            menuToggle.setAttribute('aria-label', isOpen ? 'Đóng menu' : 'Mở menu');

            const icon = menuToggle.querySelector('.material-symbols-outlined');
            if (icon) icon.textContent = isOpen ? 'close' : 'menu';
        });

        nav.querySelectorAll('a').forEach(function (link) {
            link.addEventListener('click', closeMobileMenu);
        });

        document.addEventListener('keydown', function (event) {
            if (event.key === 'Escape') closeMobileMenu();
        });
    }

    // GIỎ HÀNG
    function updateCartCount() {
        const countElement = document.getElementById('site-cart-count');

        if (!countElement) return;

        try {
            const cart = JSON.parse(
                localStorage.getItem('stagefx-demo-cart') || '[]'
            );

            if (!Array.isArray(cart)) {
                countElement.textContent = '0';
                return;
            }

            const total = cart.reduce(function (sum, item) {
                const quantity = Number(item?.quantity);

                return sum + (
                    Number.isFinite(quantity) && quantity > 0
                        ? quantity
                        : 0
                );
            }, 0);

            countElement.textContent = String(total);

        } catch (error) {
            console.error('Lỗi đọc giỏ hàng:', error);
            countElement.textContent = '0';
        }
    }

    updateCartCount();

    window.addEventListener('stagefx:cart-change', updateCartCount);

    window.addEventListener('storage', function (event) {
        if (event.key === 'stagefx-demo-cart') {
            updateCartCount();
        }
    });

});

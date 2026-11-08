
'use strict';

document.addEventListener('DOMContentLoaded', function () {

    // ==========================================
    // DROPDOWN TÀI KHOẢN
    // ==========================================
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

    // ==========================================
    // MENU MOBILE
    // ==========================================
    const menuToggle = document.querySelector('.site-shell-menu-toggle');
    const nav = document.getElementById('site-shell-nav');

    if (menuToggle && nav) {
        function closeMobileMenu() {
            nav.classList.remove('open');
            menuToggle.setAttribute('aria-expanded', 'false');
            menuToggle.setAttribute('aria-label', 'Mở menu');

            const icon = menuToggle.querySelector('.material-symbols-outlined');

            if (icon) {
                icon.textContent = 'menu';
            }
        }

        menuToggle.addEventListener('click', function () {
            const isOpen = nav.classList.toggle('open');

            menuToggle.setAttribute('aria-expanded', String(isOpen));
            menuToggle.setAttribute(
                'aria-label',
                isOpen ? 'Đóng menu' : 'Mở menu'
            );

            const icon = menuToggle.querySelector('.material-symbols-outlined');

            if (icon) {
                icon.textContent = isOpen ? 'close' : 'menu';
            }
        });

        nav.querySelectorAll('a').forEach(function (link) {
            link.addEventListener('click', closeMobileMenu);
        });

        document.addEventListener('keydown', function (event) {
            if (event.key === 'Escape') {
                closeMobileMenu();
            }
        });
    }

    // ==========================================
    // GIỎ HÀNG
    // SỐ LƯỢNG ĐƯỢC LẤY TỪ SQL SERVER
    // THÔNG QUA CARTCONTROLLER
    // ==========================================

    // Không đọc localStorage nữa.
    // Mỗi lần tải trang, Razor gọi CartCount()
    // để lấy số lượng mới nhất từ database.

});


(function () {
    'use strict';

    function initializeAdminLayout() {
        var shell = document.querySelector('.admin-shell');

        if (!shell) {
            return;
        }

        var sidebarToggle = document.querySelector('[data-admin-sidebar-toggle]');
        var sidebarCloseButtons = document.querySelectorAll('[data-admin-sidebar-close]');
        var account = document.querySelector('[data-admin-account]');
        var accountToggle = document.querySelector('[data-admin-account-toggle]');
        var accountMenu = document.getElementById('admin-account-menu');
        var dashboardLink = document.querySelector('.admin-nav-item[href]');

        function isMobile() {
            return window.matchMedia('(max-width: 720px)').matches;
        }

        function setSidebarState(collapsed) {
            shell.classList.toggle('is-sidebar-collapsed', collapsed);

            if (sidebarToggle) {
                sidebarToggle.setAttribute(
                    'aria-expanded',
                    collapsed ? 'false' : 'true'
                );

                sidebarToggle.setAttribute(
                    'aria-label',
                    collapsed ? 'Mở rộng menu' : 'Thu gọn menu'
                );
            }
        }

        function closeMobileSidebar() {
            shell.classList.remove('is-mobile-nav-open');

            if (sidebarToggle) {
                sidebarToggle.setAttribute('aria-expanded', 'false');
            }
        }

        function closeAccountMenu() {
            if (!account || !accountMenu || !accountToggle) {
                return;
            }

            account.classList.remove('is-open');
            accountMenu.hidden = true;
            accountToggle.setAttribute('aria-expanded', 'false');
        }

        // Sidebar toggle
        if (sidebarToggle) {
            sidebarToggle.addEventListener('click', function () {
                if (isMobile()) {
                    var isOpen = shell.classList.toggle('is-mobile-nav-open');

                    sidebarToggle.setAttribute(
                        'aria-expanded',
                        isOpen ? 'true' : 'false'
                    );

                    return;
                }

                setSidebarState(
                    !shell.classList.contains('is-sidebar-collapsed')
                );
            });
        }

        // Close mobile sidebar
        Array.prototype.forEach.call(sidebarCloseButtons, function (button) {
            button.addEventListener('click', closeMobileSidebar);
        });

        // Account dropdown
        if (accountToggle && accountMenu && account) {
            accountToggle.addEventListener('click', function () {
                var isOpen = !account.classList.contains('is-open');

                account.classList.toggle('is-open', isOpen);
                accountMenu.hidden = !isOpen;

                accountToggle.setAttribute(
                    'aria-expanded',
                    isOpen ? 'true' : 'false'
                );
            });
        }

        // Close dropdown when clicking outside
        document.addEventListener('click', function (event) {
            if (account && !account.contains(event.target)) {
                closeAccountMenu();
            }
        });

        // Close menus with Escape
        document.addEventListener('keydown', function (event) {
            if (event.key === 'Escape' || event.key === 'Esc') {
                closeAccountMenu();
                closeMobileSidebar();
            }
        });

        // Reset mobile sidebar when resizing to desktop
        window.addEventListener('resize', function () {
            if (!isMobile()) {
                shell.classList.remove('is-mobile-nav-open');
            }
        });

        // Close mobile sidebar after dashboard navigation
        if (dashboardLink && isMobile()) {
            dashboardLink.addEventListener('click', closeMobileSidebar);
        }
    }

    if (document.readyState === 'loading') {
        document.addEventListener(
            'DOMContentLoaded',
            initializeAdminLayout
        );
    } else {
        initializeAdminLayout();
    }
})();

'use strict';

(() => {
    const scriptUrl = document.currentScript.src;
    const siteRoot = new URL('../../', scriptUrl);
    const headerUrl = new URL('./header.html', scriptUrl);
    const placeholder = document.querySelector('#site-header');
    const pageLinks = {
        home: 'pages/home.html',
        catalog: 'pages/catalog.html',
        product: 'pages/product-detail.html',
        cart: 'pages/cart.html',
        checkout: 'pages/checkout.html',
        login: 'pages/login.html',
        register: 'pages/register.html'
    };

    function showHeaderError(error) {
        console.error('Không thể tải header dùng chung:', error);
        if (placeholder) {
            placeholder.textContent = 'Không thể tải thanh điều hướng. Vui lòng tải lại trang.';
            placeholder.setAttribute('role', 'alert');
        }
    }

    function storedCartQuantity() {
        try {
            const saved = JSON.parse(localStorage.getItem('stagefx-demo-cart') || '[]');
            if (!Array.isArray(saved)) throw new TypeError('Dữ liệu giỏ hàng không đúng định dạng.');
            return saved.reduce((total, item) => {
                const quantity = Number(item?.quantity);
                return total + (Number.isFinite(quantity) && quantity > 0 ? quantity : 0);
            }, 0);
        } catch (error) {
            console.error('Không thể đọc số lượng giỏ hàng:', error);
            return 0;
        }
    }

    function setSiteLink(link, page, anchor) {
        const target = new URL(pageLinks[page], siteRoot);
        if (anchor) target.hash = anchor;
        link.href = target.href;
    }

    function configureHeader() {
        const path = window.location.pathname.replace(/\\/g, '/');
        const pageName = path.split('/').pop();

        placeholder.querySelectorAll('[data-site-link]').forEach((link) => {
            const page = link.dataset.siteLink;
            const anchor = link.dataset.homeAnchor;
            setSiteLink(link, page, anchor);
            if (page === 'catalog' && pageName === 'catalog.html') {
                link.setAttribute('aria-current', 'page');
            }
            if (page === 'home' && pageName === 'home.html' && !anchor) {
                link.setAttribute('aria-current', 'page');
            }
            if (page === 'login' && ['login.html', 'register.html'].includes(pageName)) {
                link.setAttribute('aria-current', 'page');
            }
        });

        const searchForm = placeholder.querySelector('.site-shell-search');
        const searchInput = placeholder.querySelector('#site-shell-search-input');
        searchForm.addEventListener('submit', (event) => {
            event.preventDefault();
            const target = new URL(pageLinks.catalog, siteRoot);
            const query = searchInput.value.trim();
            if (query) target.searchParams.set('q', query);
            window.location.assign(target.href);
        });

        const menuToggle = placeholder.querySelector('.site-shell-menu-toggle');
        const nav = placeholder.querySelector('#site-shell-nav');
        menuToggle.addEventListener('click', () => {
            const isOpen = nav.classList.toggle('open');
            menuToggle.setAttribute('aria-expanded', String(isOpen));
            menuToggle.setAttribute('aria-label', isOpen ? 'Đóng menu' : 'Mở menu');
            const icon = menuToggle.querySelector('.material-symbols-outlined');
            if (icon) icon.textContent = isOpen ? 'close' : 'menu';
        });
        nav.querySelectorAll('a').forEach((link) => {
            link.addEventListener('click', () => {
                nav.classList.remove('open');
                menuToggle.setAttribute('aria-expanded', 'false');
                menuToggle.setAttribute('aria-label', 'Mở menu');
            });
        });

        updateCartCount();
        window.addEventListener('stagefx:cart-change', updateCartCount);
        window.addEventListener('storage', (event) => {
            if (event.key === 'stagefx-demo-cart') updateCartCount();
        });
    }

    function updateCartCount() {
        const countElement = placeholder.querySelector('#site-cart-count');
        if (countElement) countElement.textContent = String(storedCartQuantity());
    }

    fetch(headerUrl)
        .then((response) => {
            if (!response.ok) throw new Error(`Lỗi HTTP ${response.status} khi tải ${headerUrl.pathname}`);
            return response.text();
        })
        .then((html) => {
            if (!placeholder) throw new Error('Không tìm thấy vị trí #site-header trong trang.');
            placeholder.innerHTML = html;
            configureHeader();
        })
        .catch(showHeaderError);
})();

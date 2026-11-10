'use strict';

(() => {
    const scriptUrl = document.currentScript.src;
    const siteRoot = new URL('../../', scriptUrl);
    const footerUrl = new URL('./footer.html', scriptUrl);
    const placeholder = document.querySelector('#site-footer');
    const pageLinks = {
        home: 'pages/home.html',
        catalog: 'pages/catalog.html',
        product: 'pages/product-detail.html',
        cart: 'pages/cart.html',
        checkout: 'pages/checkout.html',
        login: 'pages/login.html',
        register: 'pages/register.html'
    };

    function showFooterError(error) {
        console.error('Không thể tải footer dùng chung:', error);
        if (placeholder) {
            placeholder.textContent = 'Không thể tải thông tin chân trang. Vui lòng tải lại trang.';
            placeholder.setAttribute('role', 'alert');
        }
    }

    function setSiteLink(link, page, anchor) {
        const target = new URL(pageLinks[page], siteRoot);
        if (anchor) target.hash = anchor;
        link.href = target.href;
    }

    function configureFooter() {
        placeholder.querySelectorAll('[data-site-link]').forEach((link) => {
            setSiteLink(link, link.dataset.siteLink, link.dataset.homeAnchor);
        });

        placeholder.querySelectorAll('[data-catalog-query]').forEach((link) => {
            const target = new URL(pageLinks.catalog, siteRoot);
            target.searchParams.set('q', link.dataset.catalogQuery);
            link.href = target.href;
        });

        const year = placeholder.querySelector('#site-shell-year');
        if (year) year.textContent = String(new Date().getFullYear());
    }

    fetch(footerUrl)
        .then((response) => {
            if (!response.ok) throw new Error(`Lỗi HTTP ${response.status} khi tải ${footerUrl.pathname}`);
            return response.text();
        })
        .then((html) => {
            if (!placeholder) throw new Error('Không tìm thấy vị trí #site-footer trong trang.');
            placeholder.innerHTML = html;
            configureFooter();
        })
        .catch(showFooterError);
})();

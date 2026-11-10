
'use strict';

/*
 * STAGEFX PRO - PRODUCT DETAIL
 * Kết nối:
 * ProductController -> Details.cshtml -> product-detail.js
 * product-detail.js -> CartController.Add -> SQL Server
 *
 * Hình thức đặt:
 * - Mua
 * - Thue
 *
 * Flightcase:
 * - 350.000 VND / sản phẩm / số lượng
 *
 * Lưu ý:
 * Giá cuối cùng luôn được kiểm tra lại tại Controller.
 */

(() => {

    // ==========================================
    // 1. CẤU HÌNH
    // ==========================================

    const FLIGHTCASE_FEE = 350000;

    const money = new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND',
        maximumFractionDigits: 0
    });

    const CART_STORAGE_KEY = 'stagefx-demo-cart';

    function escapeHtml(value) {
        return String(value ?? '').replace(/[&<>"']/g, character => ({
            '&': '&amp;',
            '<': '&lt;',
            '>': '&gt;',
            '"': '&quot;',
            "'": '&#39;'
        })[character]);
    }

    function showToast(message) {
        if (!elements.toast) {
            console.info(message);
            return;
        }

        elements.toast.textContent = message;
        elements.toast.classList.add('visible');

        clearTimeout(showToast.timeoutId);

        showToast.timeoutId = setTimeout(() => {
            elements.toast.classList.remove('visible');
        }, 3200);
    }

    function parseNumber(value, fallback = 0) {
        const number = Number(value);

        return Number.isFinite(number) ? number : fallback;
    }

    // ==========================================
    // 2. LẤY DỮ LIỆU TỪ MVC
    // ==========================================

    function getProductFromMvc() {

        const data = window.STAGEFX_PRODUCT;

        if (!data || data.id === undefined || data.id === null) {
            return null;
        }

        const images = Array.isArray(data.images)
            ? [...new Set(
                data.images.filter(image =>
                    typeof image === 'string' &&
                    image.trim() !== ''
                )
            )]
            : [];

        return {
            id: data.id,
            sku: data.sku || '',
            name: data.name || 'Sản phẩm chưa có tên',
            category: data.category || 'Chưa phân loại',
            description: data.description || '',
            detail: data.detail || '',
            specifications: data.specifications || '',

            price: Math.max(0, parseNumber(data.price)),
            rentalPrice: Math.max(0, parseNumber(data.rentalPrice)),
            stock: Math.max(
                0,
                Number.parseInt(data.stock, 10) || 0
            ),

            images
        };
    }

    const initialProduct = getProductFromMvc();

    if (!initialProduct) {

        console.error(
            'Không nhận được window.STAGEFX_PRODUCT. ' +
            'Hãy kiểm tra ProductDetail.cshtml.'
        );

        document.addEventListener('DOMContentLoaded', () => {

            const message = document.createElement('p');

            message.textContent =
                'Không tìm thấy thông tin sản phẩm. Vui lòng quay lại danh mục.';

            message.style.cssText =
                'color:#fff;text-align:center;padding:40px 20px;';

            document.body.prepend(message);

        });

        return;
    }

    // ==========================================
    // 3. TRẠNG THÁI
    // ==========================================

    const state = {

        product: initialProduct,

        quantity: initialProduct.stock > 0 ? 1 : 0,

        selectedImage: initialProduct.images[0] || '',

        activeTab: 'specs',

        orderType: 'Mua',

        rentalDays: 1,

        coFlightcase: false,

        submitting: false

    };

    // ==========================================
    // 4. LẤY PHẦN TỬ HTML
    // ==========================================

    const elements = {

        image: document.querySelector('#product-image'),
        category: document.querySelector('#product-category'),
        stock: document.querySelector('#product-stock'),
        gallery: document.querySelector('#product-gallery'),

        sku: document.querySelector('#product-sku'),
        breadcrumbSku: document.querySelector('#breadcrumb-sku'),
        name: document.querySelector('#product-name'),
        description: document.querySelector('#product-description'),

        priceLabel: document.querySelector('#price-label'),
        price: document.querySelector('#product-price'),

        rentalDaysWrap: document.querySelector('#rental-days-wrap'),
        rentalDays: document.querySelector('#rental-days'),

        volumePricing: document.querySelector('#volume-pricing'),
        flightcaseSection: document.querySelector('#flightcase-section'),
        flightcaseOptions: document.querySelector('#flightcase-options'),

        quantity: document.querySelector('#quantity'),
        subtotal: document.querySelector('#subtotal'),

        tabPanel: document.querySelector('#tab-panel'),
        related: document.querySelector('#related-products'),

        toast: document.querySelector('#toast'),

        buyMode: document.querySelector('#buy-mode'),
        rentMode: document.querySelector('#rent-mode'),

        decreaseQuantity: document.querySelector('#decrease-quantity'),
        increaseQuantity: document.querySelector('#increase-quantity'),

        addToCart: document.querySelector('#add-to-cart'),
        buyNow: document.querySelector('#buy-now'),

        cartForm: document.querySelector('#add-to-cart-form'),

        cartProductId: document.querySelector('#cart-product-id'),
        cartQuantity: document.querySelector('#cart-quantity'),
        cartOrderType: document.querySelector('#cart-order-type'),
        cartRentalDays: document.querySelector('#cart-rental-days'),
        cartFlightcase: document.querySelector('#cart-flightcase')

    };

    // ==========================================
    // 5. GIÁ VÀ TÍNH TẠM TÍNH
    // ==========================================

    function getUnitPrice() {

        if (state.orderType === 'Thue') {
            return state.product.rentalPrice;
        }

        return state.product.price;
    }

    function getFlightcaseTotal() {

        if (!state.coFlightcase) {
            return 0;
        }

        return FLIGHTCASE_FEE * state.quantity;
    }

    function getSubtotal() {

        const unitPrice = getUnitPrice();

        let total = unitPrice * state.quantity;

        if (state.orderType === 'Thue') {
            total *= state.rentalDays;
        }

        total += getFlightcaseTotal();

        return total;
    }

    // ==========================================
    // 6. THƯ VIỆN HÌNH ẢNH
    // ==========================================

    function renderGallery() {

        if (!elements.image || !elements.gallery) {
            return;
        }

        const images = state.product.images;

        if (images.length === 0) {

            elements.image.removeAttribute('src');
            elements.image.alt = 'Sản phẩm chưa có hình ảnh';

            elements.gallery.replaceChildren();

            return;
        }

        if (!images.includes(state.selectedImage)) {
            state.selectedImage = images[0];
        }

        elements.image.src = state.selectedImage;
        elements.image.alt = state.product.name;

        elements.gallery.replaceChildren(

            ...images.map((image, index) => {

                const button = document.createElement('button');

                button.type = 'button';

                button.className =
                    `thumbnail${image === state.selectedImage ? ' active' : ''}`;

                button.setAttribute(
                    'aria-label',
                    `Xem ảnh ${index + 1} của ${state.product.name}`
                );

                button.setAttribute(
                    'aria-pressed',
                    String(image === state.selectedImage)
                );

                const thumbnail = document.createElement('img');

                thumbnail.src = image;
                thumbnail.alt = '';
                thumbnail.loading = 'lazy';

                button.append(thumbnail);

                button.addEventListener('click', () => {

                    state.selectedImage = image;

                    renderGallery();

                });

                return button;

            })

        );
    }

    // ==========================================
    // 7. FLIGHTCASE
    // ==========================================

    function renderFlightcase() {

        if (!elements.flightcaseOptions) {
            return;
        }

        elements.flightcaseOptions.innerHTML = `
            <label class="flightcase-option">

                <input
                    type="checkbox"
                    id="flightcase-checkbox"
                    ${state.coFlightcase ? 'checked' : ''}
                />

                <span class="flightcase-content">

                    <strong>
                        Thêm Flightcase bảo vệ thiết bị
                    </strong>

                    <small>
                        Phụ phí ${money.format(FLIGHTCASE_FEE)}
                        / sản phẩm
                    </small>

                </span>

            </label>
        `;

        const checkbox = document.querySelector('#flightcase-checkbox');

        if (checkbox) {

            checkbox.addEventListener('change', event => {

                state.coFlightcase = event.currentTarget.checked;

                render();

            });

        }
    }

    // ==========================================
    // 8. CÁC TAB THÔNG TIN
    // ==========================================

    function renderTabs() {

        if (!elements.tabPanel) {
            return;
        }

        document.querySelectorAll('.tab-button').forEach(button => {

            const active =
                button.dataset.tab === state.activeTab;

            button.classList.toggle('active', active);

            button.setAttribute(
                'aria-selected',
                String(active)
            );

            if (active) {

                elements.tabPanel.setAttribute(
                    'aria-labelledby',
                    button.id
                );

            }

        });

        const product = state.product;

        const specifications = product.specifications
            ? escapeHtml(product.specifications)
                .replace(/\r\n/g, '\n')
                .replace(/\n/g, '<br>')
            : 'Chưa cập nhật thông số kỹ thuật.';

        const detail = product.detail
            ? escapeHtml(product.detail)
                .replace(/\r\n/g, '\n')
                .replace(/\n/g, '<br>')
            : 'Chưa có mô tả chi tiết.';

        const contents = {

            specs: `
                <section class="info-card">

                    <h3>Thông số kỹ thuật</h3>

                    <div class="product-specifications">
                        ${specifications}
                    </div>

                    <h3>Mô tả chi tiết</h3>

                    <div class="product-detail-description">
                        ${detail}
                    </div>

                </section>
            `,

            dmx: `
                <section class="info-card">

                    <h3>Thông tin điều khiển</h3>

                    <p>
                        Thông tin DMX và cấu hình điều khiển
                        được cập nhật theo tài liệu chính thức
                        của từng sản phẩm.
                    </p>

                    <div class="info-row">

                        <span>Mã sản phẩm</span>

                        <strong>
                            ${escapeHtml(product.sku || 'Chưa cập nhật')}
                        </strong>

                    </div>

                </section>
            `,

            safety: `
                <div class="safety-grid">

                    <div class="safety-notice">

                        <strong>Lưu ý an toàn:</strong>

                        Tuân thủ hướng dẫn của nhà sản xuất,
                        quy định PCCC và yêu cầu an toàn
                        tại địa điểm tổ chức.

                    </div>

                    <section class="info-card">

                        <h3>Kiểm tra thiết bị</h3>

                        <p>
                            Kiểm tra tình trạng thiết bị và phụ kiện
                            trước khi bàn giao, lắp đặt hoặc sử dụng.
                        </p>

                    </section>

                    <section class="info-card">

                        <h3>Nhân sự chuyên môn</h3>

                        <p>
                            Việc lắp đặt và vận hành cần được thực hiện
                            bởi nhân sự có chuyên môn phù hợp.
                        </p>

                    </section>

                </div>
            `,

            downloads: `
                <section class="info-card">

                    <h3>Tài liệu sản phẩm</h3>

                    <p>
                        Hiện chưa có tệp tài liệu được đính kèm
                        cho sản phẩm này.
                    </p>

                    <p>
                        Vui lòng liên hệ bộ phận tư vấn
                        để xác nhận tài liệu chính thức.
                    </p>

                </section>
            `

        };

        elements.tabPanel.innerHTML =
            contents[state.activeTab] || contents.specs;
    }

    // ==========================================
    // 9. SẢN PHẨM LIÊN QUAN
    // ==========================================

    function renderRelated() {

        if (!elements.related) {
            return;
        }

        // Sẽ kết nối danh sách sản phẩm liên quan
        // khi Controller truyền dữ liệu xuống ViewModel.

        elements.related.replaceChildren();
    }

    // ==========================================
    // 10. CHUYỂN ĐỔI MUA / THUÊ
    // ==========================================

    function setOrderType(type) {

        if (type !== 'Mua' && type !== 'Thue') {
            return;
        }

        if (type === 'Thue' && state.product.rentalPrice <= 0) {

            showToast(
                'Sản phẩm chưa được cập nhật giá thuê. Vui lòng liên hệ tư vấn.'
            );

            return;
        }

        state.orderType = type;

        // Đưa số ngày về 1 khi chuyển hình thức.
        if (type === 'Thue') {
            state.rentalDays = 1;
        }

        render();
    }

    // ==========================================
    // 11. HIỂN THỊ TOÀN BỘ SẢN PHẨM
    // ==========================================

    function render() {

        const product = state.product;

        if (elements.category) {
            elements.category.textContent = product.category;
        }

        if (elements.stock) {
            elements.stock.textContent = String(product.stock);
        }

        if (elements.sku) {
            elements.sku.textContent = product.sku;
        }

        if (elements.breadcrumbSku) {
            elements.breadcrumbSku.textContent = product.sku;
        }

        if (elements.name) {
            elements.name.textContent = product.name;
        }

        if (elements.description) {
            elements.description.textContent = product.description;
        }

        // Giá
        if (elements.priceLabel) {

            elements.priceLabel.textContent =
                state.orderType === 'Thue'
                    ? 'Đơn giá thuê / ngày:'
                    : 'Giá bán:';

        }

        if (elements.price) {
            elements.price.textContent = money.format(getUnitPrice());
        }

        // Số ngày thuê
        if (elements.rentalDaysWrap) {
            elements.rentalDaysWrap.hidden =
                state.orderType !== 'Thue';
        }

        if (elements.rentalDays) {
            elements.rentalDays.value = String(state.rentalDays);
        }

        // Chiết khấu B2B
        if (elements.volumePricing) {
            elements.volumePricing.hidden =
                state.orderType === 'Thue';
        }

        // Flightcase
        if (elements.flightcaseSection) {
            elements.flightcaseSection.hidden = false;
        }

        // Nút mua / thuê
        if (elements.buyMode) {

            const active = state.orderType === 'Mua';

            elements.buyMode.classList.toggle('active', active);

            elements.buyMode.setAttribute(
                'aria-pressed',
                String(active)
            );

        }

        if (elements.rentMode) {

            const active = state.orderType === 'Thue';

            elements.rentMode.classList.toggle('active', active);

            elements.rentMode.setAttribute(
                'aria-pressed',
                String(active)
            );

            elements.rentMode.disabled =
                product.rentalPrice <= 0;

        }

        // Số lượng
        if (elements.quantity) {

            elements.quantity.min = '1';
            elements.quantity.max = String(product.stock);
            elements.quantity.value = String(state.quantity);

            elements.quantity.disabled =
                product.stock <= 0;

        }

        if (elements.decreaseQuantity) {

            elements.decreaseQuantity.disabled =
                product.stock <= 0 || state.quantity <= 1;

        }

        if (elements.increaseQuantity) {

            elements.increaseQuantity.disabled =
                product.stock <= 0 ||
                state.quantity >= product.stock;

        }

        // Tạm tính
        if (elements.subtotal) {
            elements.subtotal.textContent =
                money.format(getSubtotal());
        }

        // Nút hành động
        if (elements.addToCart) {

            elements.addToCart.disabled =
                product.stock <= 0 || state.submitting;

        }

        if (elements.buyNow) {

            elements.buyNow.disabled =
                product.stock <= 0 || state.submitting;

            elements.buyNow.innerHTML =
                product.stock > 0
                    ? '<span class="material-symbols-outlined">bolt</span>Đặt mua ngay'
                    : 'Tạm hết hàng';

        }

        renderGallery();
        renderFlightcase();
        renderTabs();
        renderRelated();
    }

    // ==========================================
    // 12. CẬP NHẬT SỐ LƯỢNG
    // ==========================================

    function setQuantity(value) {

        const parsed = Number.parseInt(value, 10);

        if (state.product.stock <= 0) {

            state.quantity = 0;

        } else {

            const safeValue =
                Number.isFinite(parsed) ? parsed : 1;

            state.quantity = Math.max(
                1,
                Math.min(state.product.stock, safeValue)
            );

        }

        render();
    }

    // ==========================================
    // 13. KIỂM TRA DỮ LIỆU TRƯỚC KHI GỬI
    // ==========================================

    function validateOrder() {

        const product = state.product;

        if (product.stock <= 0) {

            showToast('Sản phẩm hiện đã hết hàng.');

            return false;

        }

        if (state.quantity < 1) {

            showToast('Số lượng phải lớn hơn 0.');

            return false;

        }

        if (state.quantity > product.stock) {

            showToast(
                `Số lượng vượt quá tồn kho. Hiện chỉ còn ${product.stock} máy.`
            );

            return false;

        }

        if (state.orderType === 'Thue') {

            if (product.rentalPrice <= 0) {

                showToast('Sản phẩm chưa có giá thuê.');

                return false;

            }

            if (
                state.rentalDays < 1 ||
                state.rentalDays > 30
            ) {

                showToast('Số ngày thuê phải từ 1 đến 30 ngày.');

                return false;

            }

        }

        return true;
    }

    // ==========================================
    // 14. ĐỒNG BỘ FORM VỚI CARTCONTROLLER
    // ==========================================

    function syncCartForm() {

        if (!elements.cartForm) {

            showToast(
                'Không tìm thấy form giỏ hàng. Hãy kiểm tra ProductDetail.cshtml.'
            );

            return false;

        }

        elements.cartProductId.value =
            String(state.product.id);

        elements.cartQuantity.value =
            String(state.quantity);

        elements.cartOrderType.value =
            state.orderType;

        elements.cartRentalDays.value =
            String(state.rentalDays);

        elements.cartFlightcase.value =
            state.coFlightcase ? 'true' : 'false';

        return true;
    }

    // ==========================================
    // 15. GỬI SẢN PHẨM VÀO GIỎ HÀNG SQL SERVER
    // ==========================================

    function submitCart() {

        if (state.submitting) {
            return;
        }

        if (!validateOrder()) {
            return;
        }

        if (!syncCartForm()) {
            return;
        }

        state.submitting = true;

        if (elements.addToCart) {
            elements.addToCart.disabled = true;
            elements.addToCart.textContent = 'Đang xử lý...';
        }

        if (elements.buyNow) {
            elements.buyNow.disabled = true;
        }

        showToast('Đang thêm sản phẩm vào giỏ hàng...');

        /*
         * Gửi POST trực tiếp đến CartController.Add.
         * Form có AntiForgeryToken nên MVC sẽ xác thực
         * yêu cầu trước khi xử lý.
         *
         * Không ghi dữ liệu vào localStorage.
         */

        elements.cartForm.submit();
    }

    // ==========================================
    // 16. GẮN SỰ KIỆN
    // ==========================================

    // Chuyển sang mua
    if (elements.buyMode) {

        elements.buyMode.addEventListener('click', () => {

            setOrderType('Mua');

        });

    }

    // Chuyển sang thuê
    if (elements.rentMode) {

        elements.rentMode.addEventListener('click', () => {

            setOrderType('Thue');

        });

    }

    // Thay đổi số ngày thuê
    if (elements.rentalDays) {

        elements.rentalDays.addEventListener('change', event => {

            const value = Number.parseInt(
                event.currentTarget.value,
                10
            );

            if (!Number.isFinite(value)) {

                state.rentalDays = 1;

            } else {

                state.rentalDays = Math.max(
                    1,
                    Math.min(30, value)
                );

            }

            render();

        });

    }

    // Nhập số lượng
    if (elements.quantity) {

        elements.quantity.addEventListener('change', event => {

            setQuantity(event.currentTarget.value);

        });

    }

    // Giảm số lượng
    if (elements.decreaseQuantity) {

        elements.decreaseQuantity.addEventListener('click', () => {

            setQuantity(state.quantity - 1);

        });

    }

    // Tăng số lượng
    if (elements.increaseQuantity) {

        elements.increaseQuantity.addEventListener('click', () => {

            setQuantity(state.quantity + 1);

        });

    }

    // Thêm vào giỏ
    if (elements.addToCart) {

        elements.addToCart.addEventListener('click', () => {

            submitCart();

        });

    }

    // Đặt mua ngay
    if (elements.buyNow) {

        elements.buyNow.addEventListener('click', () => {

            submitCart();

        });

    }

    // Các tab
    document.querySelectorAll('.tab-button').forEach(button => {

        button.addEventListener('click', () => {

            state.activeTab =
                button.dataset.tab || 'specs';

            renderTabs();

        });

    });

    // ==========================================
    // 17. KHỞI CHẠY
    // ==========================================

    render();

    // Thông báo nếu form chưa được thêm vào Razor.
    if (!elements.cartForm) {

        console.error(
            'Thiếu #add-to-cart-form. ' +
            'Hãy kiểm tra ProductDetail.cshtml.'
        );

    }

})();


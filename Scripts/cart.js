
'use strict';

/*
 * STAGEFX PRO - SHOPPING CART
 * Đồng bộ với Product Detail thông qua localStorage.
 * Phiên bản hiện tại: Demo localStorage, chưa lưu giỏ hàng vào SQL Server.
 */

// ======================================================
// 1. CẤU HÌNH
// ======================================================

const CART_STORAGE_KEY = 'stagefx-demo-cart';

const SHIPPING_THRESHOLD = 20000000;
const SHIPPING_ESTIMATE = 350000;
const RENTAL_DEPOSIT_RATE = 0.3;

const money = new Intl.NumberFormat('vi-VN', {
    style: 'currency',
    currency: 'VND',
    maximumFractionDigits: 0
});

const productImages = {
    'fog-200': 'https://images.unsplash.com/photo-1470229722913-7c0e2dbbafd3?auto=format&fit=crop&w=240&q=70',
    'fog-2000': 'https://images.unsplash.com/photo-1470229722913-7c0e2dbbafd3?auto=format&fit=crop&w=240&q=70',
    'haze-pro': 'https://images.unsplash.com/photo-1501386761578-eac5c94b800a?auto=format&fit=crop&w=240&q=70',
    'haze-1500': 'https://images.unsplash.com/photo-1501386761578-eac5c94b800a?auto=format&fit=crop&w=240&q=70',
    'co2-jet': 'https://images.unsplash.com/photo-1533174072545-7a4b6ad7a6c3?auto=format&fit=crop&w=240&q=70',
    'spark-300': 'https://images.unsplash.com/photo-1492684223066-81342ee5ff30?auto=format&fit=crop&w=240&q=70',
    'cold-spark': 'https://images.unsplash.com/photo-1492684223066-81342ee5ff30?auto=format&fit=crop&w=240&q=70',
    'flame-dmx': 'https://images.unsplash.com/photo-1506157786151-b8491531f063?auto=format&fit=crop&w=240&q=70',
    'confetti-pro': 'https://images.unsplash.com/photo-1533174072545-7a4b6ad7a6c3?auto=format&fit=crop&w=240&q=70',
    'dmx-controller': 'https://images.unsplash.com/photo-1524368535928-5b5e00ddc76b?auto=format&fit=crop&w=240&q=70'
};

// ======================================================
// 2. TRẠNG THÁI
// ======================================================

const state = {
    items: [],
    promoCode: '',
    discountPercent: 0,
    promoApplied: false,
    vatEnabled: true
};

// ======================================================
// 3. LẤY PHẦN TỬ HTML
// ======================================================

const elements = {
    description: document.querySelector('#cart-description'),
    clearCart: document.querySelector('#clear-cart'),

    emptyCart: document.querySelector('#empty-cart'),
    cartContent: document.querySelector('#cart-content'),
    cartItems: document.querySelector('#cart-items'),

    itemCount: document.querySelector('#item-count'),
    summaryItemCount: document.querySelector('#summary-item-count'),

    subtotal: document.querySelector('#subtotal'),

    discountRow: document.querySelector('#discount-row'),
    discountLabel: document.querySelector('#discount-label'),
    discountAmount: document.querySelector('#discount-amount'),

    shippingFee: document.querySelector('#shipping-fee'),

    vatEnabled: document.querySelector('#vat-enabled'),
    vatAmount: document.querySelector('#vat-amount'),

    depositRow: document.querySelector('#deposit-row'),
    depositAmount: document.querySelector('#deposit-amount'),

    grandTotal: document.querySelector('#grand-total'),
    estimateNote: document.querySelector('#estimate-note'),

    promoForm: document.querySelector('#promo-form'),
    promoCode: document.querySelector('#promo-code'),
    promoMessage: document.querySelector('#promo-message'),

    checkoutButton: document.querySelector('#checkout-button'),
    toast: document.querySelector('#toast')
};

// ======================================================
// 4. HÀM HỖ TRỢ
// ======================================================

function makeElement(tag, className = '', text) {
    const element = document.createElement(tag);

    if (className) {
        element.className = className;
    }

    if (text !== undefined) {
        element.textContent = text;
    }

    return element;
}

function parseMoney(value) {
    const amount = Number(value);

    return Number.isFinite(amount) && amount >= 0
        ? amount
        : 0;
}

function showToast(message) {
    if (!elements.toast) {
        console.info(message);
        return;
    }

    elements.toast.textContent = message;
    elements.toast.classList.add('visible');

    window.clearTimeout(showToast.timeoutId);

    showToast.timeoutId = window.setTimeout(() => {
        elements.toast.classList.remove('visible');
    }, 3200);
}

// ======================================================
// 5. CHUẨN HÓA SẢN PHẨM TRONG GIỎ
// ======================================================

function normalizeCartItem(item, index) {
    if (!item || typeof item !== 'object' || Array.isArray(item)) {
        return null;
    }

    const productId = String(
        item.productId || item.id || `item-${index}`
    );

    const productName =
        typeof item.productName === 'string' && item.productName.trim()
            ? item.productName.trim()
            : 'Thiết bị sân khấu';

    const flightcaseData =
        item.flightcaseOption &&
            typeof item.flightcaseOption === 'object'
            ? item.flightcaseOption
            : null;

    let flightcase = null;

    if (flightcaseData) {
        flightcase = {
            name: String(flightcaseData.name || 'Flightcase'),
            price: parseMoney(flightcaseData.price)
        };
    } else if (
        typeof item.flightcase === 'string' &&
        item.flightcase.trim()
    ) {
        flightcase = {
            name: item.flightcase.trim(),
            price: 0
        };
    }

    const quantity = Math.max(
        1,
        Math.min(999, Math.floor(Number(item.quantity) || 1))
    );

    const isRental = item.isRental === true;

    const rentalDays = isRental
        ? Math.max(
            1,
            Math.min(30, Math.floor(Number(item.rentalDays) || 1))
        )
        : 1;

    // null nghĩa là sản phẩm cũ chưa có dữ liệu tồn kho.
    const stock = item.stock == null
        ? null
        : Math.max(0, Math.floor(Number(item.stock) || 0));

    const fallbackImage =
        productImages[productId] ||
        'https://images.unsplash.com/photo-1501386761578-eac5c94b800a?auto=format&fit=crop&w=240&q=70';

    const productImage =
        typeof item.productImage === 'string' &&
            item.productImage.trim()
            ? item.productImage.trim()
            : fallbackImage;

    return {
        id: String(item.id || `${productId}-${index}`),
        productId,
        productName,

        productSku: String(
            item.productSku || item.sku || 'SKU'
        ),

        productImage,

        quantity,

        unitPrice: parseMoney(item.unitPrice),

        stock,

        isRental,
        rentalDays,

        flightcase
    };
}

// ======================================================
// 6. ĐỌC VÀ LƯU GIỎ HÀNG
// ======================================================

function loadCart() {
    try {
        const raw = localStorage.getItem(CART_STORAGE_KEY);

        if (!raw) {
            return [];
        }

        const parsed = JSON.parse(raw);

        if (!Array.isArray(parsed)) {
            throw new TypeError(
                'Dữ liệu giỏ hàng không đúng định dạng.'
            );
        }

        return parsed
            .map((item, index) => normalizeCartItem(item, index))
            .filter(Boolean);

    } catch (error) {
        console.error('Không thể đọc giỏ hàng:', error);

        showToast(
            'Không đọc được giỏ hàng đã lưu. Vui lòng kiểm tra dữ liệu.'
        );

        return [];
    }
}

function saveCart() {
    try {
        localStorage.setItem(
            CART_STORAGE_KEY,
            JSON.stringify(state.items)
        );

        window.dispatchEvent(
            new Event('stagefx:cart-change')
        );

        return true;

    } catch (error) {
        console.error('Không thể lưu giỏ hàng:', error);

        showToast(
            'Không lưu được thay đổi giỏ hàng trong trình duyệt.'
        );

        return false;
    }
}

// ======================================================
// 7. TÍNH TOÁN GIÁ TRỊ SẢN PHẨM
// ======================================================

function getItemBaseTotal(item) {
    const unitTotal =
        item.unitPrice + (item.flightcase?.price || 0);

    return (
        unitTotal *
        item.quantity *
        (item.isRental ? item.rentalDays : 1)
    );
}

function calculateFinancials() {
    const subtotal = state.items.reduce(
        (sum, item) => sum + getItemBaseTotal(item),
        0
    );

    const discountAmount = Math.round(
        subtotal * (state.discountPercent / 100)
    );

    const shippingFee =
        subtotal >= SHIPPING_THRESHOLD || subtotal === 0
            ? 0
            : SHIPPING_ESTIMATE;

    const taxableAmount = Math.max(
        0,
        subtotal - discountAmount + shippingFee
    );

    const vat = state.vatEnabled
        ? Math.round(taxableAmount * 0.1)
        : 0;

    const rentalSubtotal = state.items
        .filter(item => item.isRental)
        .reduce(
            (sum, item) => sum + getItemBaseTotal(item),
            0
        );

    const depositEstimate = Math.round(
        rentalSubtotal * RENTAL_DEPOSIT_RATE
    );

    return {
        subtotal,
        discountAmount,
        shippingFee,
        vat,
        depositEstimate,
        total: taxableAmount + vat
    };
}

// ======================================================
// 8. TẠO GIAO DIỆN MỘT SẢN PHẨM
// ======================================================

function createItem(item) {
    const row = makeElement('article', 'cart-item');

    // Hình ảnh
    const imageWrap = makeElement('div', 'item-image-wrap');

    const image = makeElement('img', 'item-image');

    image.src = item.productImage;
    image.alt = item.productName;
    image.loading = 'lazy';

    const sku = makeElement(
        'span',
        'item-sku',
        item.productSku
    );

    imageWrap.append(image, sku);

    // Thông tin sản phẩm
    const details = makeElement('div', 'item-details');

    const badges = makeElement('div', 'item-badges');

    const typeBadge = makeElement(
        'span',
        `item-type${item.isRental ? ' rental' : ''}`,
        item.isRental
            ? `Thuê • ${item.rentalDays} ngày`
            : 'Mua thiết bị'
    );

    badges.append(typeBadge);

    if (item.flightcase) {
        badges.append(
            makeElement(
                'span',
                'flightcase-badge',
                `+ ${item.flightcase.name}`
            )
        );
    }

    const name = makeElement(
        'h3',
        'item-name',
        item.productName
    );

    const priceLine = makeElement('p', 'item-unit-price');

    const unitPrice =
        item.unitPrice + (item.flightcase?.price || 0);

    priceLine.append(
        document.createTextNode('Đơn giá: '),

        makeElement(
            'strong',
            '',
            money.format(unitPrice)
        )
    );

    if (item.flightcase) {
        priceLine.append(
            makeElement(
                'span',
                'item-price-note',
                `(gồm flightcase ${money.format(item.flightcase.price)})`
            )
        );
    }

    details.append(badges, name, priceLine);

    // Điều khiển số lượng
    const controls = makeElement('div', 'item-controls');

    const quantity = makeElement(
        'div',
        'quantity-control'
    );

    const decrease = makeElement('button', '', '−');

    decrease.type = 'button';

    decrease.setAttribute(
        'aria-label',
        `Giảm số lượng ${item.productName}`
    );

    decrease.disabled = item.quantity <= 1;

    decrease.addEventListener('click', () => {
        updateQuantity(item.id, item.quantity - 1);
    });

    const count = makeElement(
        'output',
        '',
        String(item.quantity)
    );

    count.setAttribute('aria-label', 'Số lượng');

    const increase = makeElement('button', '', '+');

    increase.type = 'button';

    increase.setAttribute(
        'aria-label',
        `Tăng số lượng ${item.productName}`
    );

    increase.disabled =
        item.stock !== null &&
        (
            item.stock <= 0 ||
            item.quantity >= item.stock
        );

    increase.addEventListener('click', () => {
        updateQuantity(item.id, item.quantity + 1);
    });

    quantity.append(decrease, count, increase);

    // Tổng tiền
    const total = makeElement(
        'strong',
        'item-total',
        money.format(getItemBaseTotal(item))
    );

    // Xóa sản phẩm
    const remove = makeElement(
        'button',
        'remove-item'
    );

    remove.type = 'button';
    remove.title = 'Xóa thiết bị';

    remove.setAttribute(
        'aria-label',
        `Xóa ${item.productName} khỏi giỏ`
    );

    remove.append(
        makeElement(
            'span',
            'material-symbols-outlined',
            'close'
        )
    );

    remove.addEventListener('click', () => {
        removeItem(item.id);
    });

    controls.append(quantity, total, remove);

    row.append(imageWrap, details, controls);

    return row;
}

// ======================================================
// 9. HIỂN THỊ GIỎ HÀNG
// ======================================================

function render() {
    const totalQuantity = state.items.reduce(
        (sum, item) => sum + item.quantity,
        0
    );

    const itemCountText =
        `${state.items.length} hạng mục • ${totalQuantity} thiết bị`;

    if (elements.description) {
        elements.description.textContent = itemCountText;
    }

    if (elements.itemCount) {
        elements.itemCount.textContent = itemCountText;
    }

    if (elements.summaryItemCount) {
        elements.summaryItemCount.textContent =
            `${state.items.length} hạng mục`;
    }

    const isEmpty = state.items.length === 0;

    if (elements.emptyCart) {
        elements.emptyCart.hidden = !isEmpty;
    }

    if (elements.cartContent) {
        elements.cartContent.hidden = isEmpty;
    }

    if (elements.clearCart) {
        elements.clearCart.hidden = isEmpty;
    }

    if (isEmpty) {
        state.discountPercent = 0;
        state.promoApplied = false;
        state.promoCode = '';

        if (elements.promoCode) {
            elements.promoCode.value = '';
        }

        if (elements.promoMessage) {
            elements.promoMessage.hidden = true;
        }

        return;
    }

    // Danh sách sản phẩm
    if (elements.cartItems) {
        elements.cartItems.replaceChildren(
            ...state.items.map(createItem)
        );
    }

    // VAT
    if (elements.vatEnabled) {
        elements.vatEnabled.checked = state.vatEnabled;
    }

    const financials = calculateFinancials();

    if (elements.subtotal) {
        elements.subtotal.textContent =
            money.format(financials.subtotal);
    }

    if (elements.discountRow) {
        elements.discountRow.hidden =
            financials.discountAmount <= 0;
    }

    if (elements.discountLabel) {
        elements.discountLabel.textContent =
            `Ưu đãi đối tác (${state.discountPercent}%):`;
    }

    if (elements.discountAmount) {
        elements.discountAmount.textContent =
            `−${money.format(financials.discountAmount)}`;
    }

    if (elements.shippingFee) {
        elements.shippingFee.textContent =
            financials.shippingFee === 0
                ? 'Miễn phí*'
                : money.format(financials.shippingFee);
    }

    if (elements.vatAmount) {
        elements.vatAmount.textContent =
            money.format(financials.vat);
    }

    if (elements.depositRow) {
        elements.depositRow.hidden =
            financials.depositEstimate <= 0;
    }

    if (elements.depositAmount) {
        elements.depositAmount.textContent =
            money.format(financials.depositEstimate);
    }

    if (elements.grandTotal) {
        elements.grandTotal.textContent =
            money.format(financials.total);
    }

    if (elements.estimateNote) {
        elements.estimateNote.textContent =
            `Bản demo: phí giao hàng ${money.format(SHIPPING_ESTIMATE)} nếu tạm tính dưới ${money.format(SHIPPING_THRESHOLD)}; miễn phí từ ngưỡng này. Tiền cọc thuê tạm tính ${Math.round(RENTAL_DEPOSIT_RATE * 100)}% phần thuê. Giá và phí thực tế cần được nhân viên xác nhận. VAT tính trên tạm tính sau ưu đãi cộng phí giao hàng.`;
    }
}

// ======================================================
// 10. CẬP NHẬT SỐ LƯỢNG
// ======================================================

function updateQuantity(id, nextQuantity) {
    const item = state.items.find(
        entry => entry.id === id
    );

    if (!item) {
        return;
    }

    const quantity = Math.floor(Number(nextQuantity));

    if (!Number.isFinite(quantity) || quantity < 1) {
        showToast('Số lượng không hợp lệ.');
        return;
    }

    // Chỉ kiểm tra tồn kho nếu sản phẩm có dữ liệu tồn kho.
    if (
        item.stock !== null &&
        (
            item.stock <= 0 ||
            quantity > item.stock
        )
    ) {
        showToast(
            `Số lượng vượt quá tồn kho. Hiện chỉ còn ${item.stock} sản phẩm.`
        );

        return;
    }

    const previousQuantity = item.quantity;

    item.quantity = Math.min(999, quantity);

    if (!saveCart()) {
        item.quantity = previousQuantity;
        return;
    }

    render();
}

// ======================================================
// 11. XÓA SẢN PHẨM
// ======================================================

function removeItem(id) {
    const item = state.items.find(
        entry => entry.id === id
    );

    if (!item) {
        return;
    }

    const previousItems = [...state.items];

    state.items = state.items.filter(
        entry => entry.id !== id
    );

    if (!saveCart()) {
        state.items = previousItems;
        return;
    }

    render();

    showToast(
        `Đã xóa ${item.productName} khỏi giỏ.`
    );
}

// ======================================================
// 12. MÃ ƯU ĐÃI
// ======================================================

function showPromoMessage(message, isError) {
    if (!elements.promoMessage) {
        return;
    }

    elements.promoMessage.textContent = message;

    elements.promoMessage.classList.toggle(
        'error',
        isError
    );

    elements.promoMessage.hidden = false;
}

function applyPromo(event) {
    event.preventDefault();

    if (!elements.promoCode) {
        return;
    }

    const code = elements.promoCode.value
        .trim()
        .toUpperCase();

    if (!code) {
        showPromoMessage(
            'Vui lòng nhập mã ưu đãi.',
            true
        );

        return;
    }

    // Mã ưu đãi demo.
    const promotions = {
        B2BVIP: 10,
        TECHEVENT2025: 5
    };

    if (
        !Object.prototype.hasOwnProperty.call(
            promotions,
            code
        )
    ) {
        state.discountPercent = 0;
        state.promoApplied = false;
        state.promoCode = '';

        render();

        showPromoMessage(
            'Mã ưu đãi không hợp lệ hoặc đã hết hạn.',
            true
        );

        return;
    }

    state.promoCode = code;
    state.discountPercent = promotions[code];
    state.promoApplied = true;

    render();

    showPromoMessage(
        `Đã áp dụng ${code}: giảm ${state.discountPercent}% trên tạm tính mẫu.`,
        false
    );
}

// ======================================================
// 13. XÓA TOÀN BỘ GIỎ HÀNG
// ======================================================

function clearCart() {
    if (
        !window.confirm(
            'Bạn muốn xóa toàn bộ thiết bị khỏi giỏ hàng?'
        )
    ) {
        return;
    }

    const previousItems = state.items;

    state.items = [];

    if (!saveCart()) {
        state.items = previousItems;
        return;
    }

    state.discountPercent = 0;
    state.promoApplied = false;
    state.promoCode = '';

    render();

    showToast('Đã làm trống giỏ hàng.');
}

// ======================================================
// 14. CHUYỂN SANG THANH TOÁN
// ======================================================

function handleCheckout() {
    if (state.items.length === 0) {
        showToast(
            'Giỏ hàng đang trống. Vui lòng chọn thiết bị trước.'
        );

        return;
    }

    const checkoutUrl = window.STAGEFX_CHECKOUT_URL;

    if (
        typeof checkoutUrl !== 'string' ||
        !checkoutUrl.trim()
    ) {
        console.error(
            'Chưa cấu hình URL Checkout trong Razor View.'
        );

        showToast(
            'Không tìm thấy đường dẫn Checkout. Vui lòng tải lại trang.'
        );

        return;
    }

    window.location.href = checkoutUrl;
}

// ======================================================
// 15. GẮN SỰ KIỆN
// ======================================================

if (elements.promoForm) {
    elements.promoForm.addEventListener(
        'submit',
        applyPromo
    );
}

if (elements.vatEnabled) {
    elements.vatEnabled.addEventListener('change', () => {
        state.vatEnabled = elements.vatEnabled.checked;
        render();
    });
}

if (elements.clearCart) {
    elements.clearCart.addEventListener(
        'click',
        clearCart
    );
}

if (elements.checkoutButton) {
    elements.checkoutButton.addEventListener(
        'click',
        handleCheckout
    );
}

// ======================================================
// 16. KHỞI CHẠY
// ======================================================

state.items = loadCart();

render();

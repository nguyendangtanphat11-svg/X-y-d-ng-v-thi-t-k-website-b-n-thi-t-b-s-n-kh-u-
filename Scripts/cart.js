'use strict';

const CART_STORAGE_KEY = 'stagefx-demo-cart';
const SHIPPING_THRESHOLD = 20000000;
const SHIPPING_ESTIMATE = 350000;
const RENTAL_DEPOSIT_RATE = 0.3;
const money = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND', maximumFractionDigits: 0 });

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

const state = {
    items: [],
    promoCode: '',
    discountPercent: 0,
    promoApplied: false,
    vatEnabled: true
};

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

function makeElement(tag, className, text) {
    const element = document.createElement(tag);
    if (className) element.className = className;
    if (text !== undefined) element.textContent = text;
    return element;
}

function parseMoney(value) {
    const amount = Number(value);
    return Number.isFinite(amount) && amount >= 0 ? amount : 0;
}

function normalizeCartItem(item, index) {
    if (!item || typeof item !== 'object' || Array.isArray(item)) return null;

    const productName = typeof item.productName === 'string' && item.productName.trim()
        ? item.productName.trim()
        : 'Thiết bị sân khấu';
    const flightcaseData = item.flightcaseOption && typeof item.flightcaseOption === 'object'
        ? item.flightcaseOption
        : null;
    const flightcase = flightcaseData
        ? { name: String(flightcaseData.name || 'Flightcase'), price: parseMoney(flightcaseData.price) }
        : (typeof item.flightcase === 'string' && item.flightcase.trim() ? { name: item.flightcase.trim(), price: 0 } : null);
    const quantity = Math.max(1, Math.min(999, Math.floor(Number(item.quantity) || 1)));
    const rentalDays = item.isRental === true
        ? Math.max(1, Math.min(30, Math.floor(Number(item.rentalDays) || 1)))
        : 1;
    const productId = String(item.productId || item.id || `item-${index}`);

    return {
        id: String(item.id || `${productId}-${index}`),
        productId,
        productName,
        productSku: String(item.productSku || item.sku || 'SKU'),
        productImage: typeof item.productImage === 'string' && item.productImage
            ? item.productImage
            : (productImages[productId] || 'https://images.unsplash.com/photo-1501386761578-eac5c94b800a?auto=format&fit=crop&w=240&q=70'),
        quantity,
        unitPrice: parseMoney(item.unitPrice),
        isRental: item.isRental === true,
        rentalDays,
        flightcase
    };
}

function loadCart() {
    try {
        const raw = localStorage.getItem(CART_STORAGE_KEY);
        if (!raw) return [];
        const parsed = JSON.parse(raw);
        if (!Array.isArray(parsed)) {
            throw new TypeError('Dữ liệu giỏ hàng không đúng định dạng.');
        }
        return parsed.map(normalizeCartItem).filter(Boolean);
    } catch (error) {
        console.error('Không thể đọc giỏ hàng đã lưu:', error);
        showToast('Không đọc được giỏ hàng đã lưu. Vui lòng kiểm tra dữ liệu trong trình duyệt.');
        return [];
    }
}

function saveCart() {
    try {
        localStorage.setItem(CART_STORAGE_KEY, JSON.stringify(state.items));
        window.dispatchEvent(new Event('stagefx:cart-change'));
        return true;
    } catch (error) {
        console.error('Không thể lưu giỏ hàng:', error);
        showToast('Không lưu được thay đổi giỏ hàng trong trình duyệt.');
        return false;
    }
}

function getItemBaseTotal(item) {
    const unitTotal = item.unitPrice + (item.flightcase?.price || 0);
    return unitTotal * item.quantity * (item.isRental ? item.rentalDays : 1);
}

function calculateFinancials() {
    const subtotal = state.items.reduce((sum, item) => sum + getItemBaseTotal(item), 0);
    const discountAmount = Math.round(subtotal * (state.discountPercent / 100));
    const shippingFee = subtotal >= SHIPPING_THRESHOLD || subtotal === 0 ? 0 : SHIPPING_ESTIMATE;
    const taxableAmount = Math.max(0, subtotal - discountAmount + shippingFee);
    const vat = state.vatEnabled ? Math.round(taxableAmount * 0.1) : 0;
    const rentalSubtotal = state.items
        .filter((item) => item.isRental)
        .reduce((sum, item) => sum + getItemBaseTotal(item), 0);
    const depositEstimate = Math.round(rentalSubtotal * RENTAL_DEPOSIT_RATE);

    return {
        subtotal,
        discountAmount,
        shippingFee,
        vat,
        depositEstimate,
        total: taxableAmount + vat
    };
}

function createItem(item) {
    const row = makeElement('article', 'cart-item');
    const imageWrap = makeElement('div', 'item-image-wrap');
    const image = makeElement('img', 'item-image');
    image.src = item.productImage;
    image.alt = item.productName;
    image.loading = 'lazy';
    const sku = makeElement('span', 'item-sku', item.productSku);
    imageWrap.append(image, sku);

    const details = makeElement('div', 'item-details');
    const badges = makeElement('div', 'item-badges');
    badges.append(makeElement('span', `item-type${item.isRental ? ' rental' : ''}`, item.isRental ? `Thuê • ${item.rentalDays} ngày` : 'Mua thiết bị'));
    if (item.flightcase) badges.append(makeElement('span', 'flightcase-badge', `+ ${item.flightcase.name}`));
    const name = makeElement('h3', 'item-name', item.productName);
    const priceLine = makeElement('p', 'item-unit-price');
    const unitPrice = item.unitPrice + (item.flightcase?.price || 0);
    priceLine.append(document.createTextNode('Đơn giá: '), makeElement('strong', '', money.format(unitPrice)));
    if (item.flightcase) {
        priceLine.append(makeElement('span', 'item-price-note', `(gồm flightcase ${money.format(item.flightcase.price)})`));
    }
    details.append(badges, name, priceLine);

    const controls = makeElement('div', 'item-controls');
    const quantity = makeElement('div', 'quantity-control');
    const decrease = makeElement('button', '', '−');
    decrease.type = 'button';
    decrease.setAttribute('aria-label', `Giảm số lượng ${item.productName}`);
    decrease.addEventListener('click', () => updateQuantity(item.id, item.quantity - 1));
    const count = makeElement('output', '', String(item.quantity));
    count.setAttribute('aria-label', 'Số lượng');
    const increase = makeElement('button', '', '+');
    increase.type = 'button';
    increase.setAttribute('aria-label', `Tăng số lượng ${item.productName}`);
    increase.addEventListener('click', () => updateQuantity(item.id, item.quantity + 1));
    quantity.append(decrease, count, increase);

    const total = makeElement('strong', 'item-total', money.format(getItemBaseTotal(item)));
    const remove = makeElement('button', 'remove-item');
    remove.type = 'button';
    remove.title = 'Xóa thiết bị';
    remove.setAttribute('aria-label', `Xóa ${item.productName} khỏi giỏ`);
    remove.append(makeElement('span', 'material-symbols-outlined', 'close'));
    remove.addEventListener('click', () => removeItem(item.id));
    controls.append(quantity, total, remove);
    row.append(imageWrap, details, controls);
    return row;
}

function render() {
    const totalQuantity = state.items.reduce((sum, item) => sum + item.quantity, 0);
    const itemCountText = `${state.items.length} hạng mục • ${totalQuantity} thiết bị`;
    elements.description.textContent = itemCountText;
    elements.itemCount.textContent = itemCountText;
    elements.summaryItemCount.textContent = `${state.items.length} hạng mục`;

    const isEmpty = state.items.length === 0;
    elements.emptyCart.hidden = !isEmpty;
    elements.cartContent.hidden = isEmpty;
    elements.clearCart.hidden = isEmpty;
    if (isEmpty) {
        state.discountPercent = 0;
        state.promoApplied = false;
        elements.promoCode.value = '';
        elements.promoMessage.hidden = true;
        return;
    }

    elements.cartItems.replaceChildren(...state.items.map(createItem));
    elements.vatEnabled.checked = state.vatEnabled;
    const financials = calculateFinancials();
    elements.subtotal.textContent = money.format(financials.subtotal);
    elements.discountRow.hidden = financials.discountAmount <= 0;
    elements.discountLabel.textContent = `Ưu đãi đối tác (${state.discountPercent}%):`;
    elements.discountAmount.textContent = `−${money.format(financials.discountAmount)}`;
    elements.shippingFee.textContent = financials.shippingFee === 0 ? 'Miễn phí*' : money.format(financials.shippingFee);
    elements.vatAmount.textContent = money.format(financials.vat);
    elements.depositRow.hidden = financials.depositEstimate <= 0;
    elements.depositAmount.textContent = money.format(financials.depositEstimate);
    elements.grandTotal.textContent = money.format(financials.total);
    elements.estimateNote.textContent = `Bản demo: phí giao hàng ${money.format(SHIPPING_ESTIMATE)} nếu tạm tính dưới ${money.format(SHIPPING_THRESHOLD)}; miễn phí từ ngưỡng này. Tiền cọc thuê tạm tính ${Math.round(RENTAL_DEPOSIT_RATE * 100)}% phần thuê. Giá và phí thực tế cần được nhân viên xác nhận. VAT tính trên tạm tính sau ưu đãi cộng phí giao hàng.`;
}

function updateQuantity(id, nextQuantity) {
    const item = state.items.find((entry) => entry.id === id);
    if (!item) return;
    item.quantity = Math.max(1, Math.min(999, Math.floor(nextQuantity)));
    if (!saveCart()) return;
    render();
}

function removeItem(id) {
    const item = state.items.find((entry) => entry.id === id);
    if (!item) return;
    state.items = state.items.filter((entry) => entry.id !== id);
    if (!saveCart()) {
        state.items = loadCart();
        return;
    }
    render();
    showToast(`Đã xóa ${item.productName} khỏi giỏ.`);
}

function showToast(message) {
    elements.toast.textContent = message;
    elements.toast.classList.add('visible');
    window.clearTimeout(showToast.timeoutId);
    showToast.timeoutId = window.setTimeout(() => elements.toast.classList.remove('visible'), 3200);
}

function applyPromo(event) {
    event.preventDefault();
    const code = elements.promoCode.value.trim().toUpperCase();
    if (!code) {
        showPromoMessage('Vui lòng nhập mã ưu đãi.', true);
        return;
    }

    const promotions = { B2BVIP: 10, TECHEVENT2025: 5 };
    if (!Object.prototype.hasOwnProperty.call(promotions, code)) {
        state.discountPercent = 0;
        state.promoApplied = false;
        render();
        showPromoMessage('Mã ưu đãi không hợp lệ hoặc đã hết hạn.', true);
        return;
    }

    state.promoCode = code;
    state.discountPercent = promotions[code];
    state.promoApplied = true;
    render();
    showPromoMessage(`Đã áp dụng ${code}: giảm ${state.discountPercent}% trên tạm tính mẫu.`, false);
}

function showPromoMessage(message, isError) {
    elements.promoMessage.textContent = message;
    elements.promoMessage.classList.toggle('error', isError);
    elements.promoMessage.hidden = false;
}

function clearCart() {
    if (!window.confirm('Bạn muốn xóa toàn bộ thiết bị khỏi giỏ hàng?')) return;
    const previousItems = state.items;
    state.items = [];
    if (!saveCart()) {
        state.items = previousItems;
        return;
    }
    state.discountPercent = 0;
    state.promoApplied = false;
    render();
    showToast('Đã làm trống giỏ hàng.');
}


function handleCheckout() {
    if (state.items.length === 0) {
        showToast('Giỏ hàng đang trống. Vui lòng chọn thiết bị trước.');
        return;
    }

    const checkoutUrl = window.STAGEFX_CHECKOUT_URL;

    if (typeof checkoutUrl !== 'string' || !checkoutUrl) {
        console.error('Chưa cấu hình URL Checkout trong Razor View.');
        showToast('Không tìm thấy đường dẫn Checkout. Vui lòng tải lại trang.');
        return;
    }

    window.location.href = checkoutUrl;
}


elements.promoForm.addEventListener('submit', applyPromo);
elements.vatEnabled.addEventListener('change', () => {
    state.vatEnabled = elements.vatEnabled.checked;
    render();
});
elements.clearCart.addEventListener('click', clearCart);
elements.checkoutButton.addEventListener('click', handleCheckout);

state.items = loadCart();
render();

'use strict';

const CART_STORAGE_KEY = 'stagefx-demo-cart';
const SHIPPING_THRESHOLD = 20000000;
const SHIPPING_ESTIMATE = 350000;
const RENTAL_DEPOSIT_RATE = 0.3;
const money = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND', maximumFractionDigits: 0 });
const paymentLabels = {
    bank_transfer: 'Chuyển khoản',
    card: 'Thẻ doanh nghiệp',
    credit: 'Thỏa thuận công nợ'
};
const productImages = {
    'fog-200': 'https://images.unsplash.com/photo-1470229722913-7c0e2dbbafd3?auto=format&fit=crop&w=120&q=70',
    'fog-2000': 'https://images.unsplash.com/photo-1470229722913-7c0e2dbbafd3?auto=format&fit=crop&w=120&q=70',
    'haze-pro': 'https://images.unsplash.com/photo-1501386761578-eac5c94b800a?auto=format&fit=crop&w=120&q=70',
    'haze-1500': 'https://images.unsplash.com/photo-1501386761578-eac5c94b800a?auto=format&fit=crop&w=120&q=70',
    'co2-jet': 'https://images.unsplash.com/photo-1533174072545-7a4b6ad7a6c3?auto=format&fit=crop&w=120&q=70',
    'spark-300': 'https://images.unsplash.com/photo-1492684223066-81342ee5ff30?auto=format&fit=crop&w=120&q=70',
    'cold-spark': 'https://images.unsplash.com/photo-1492684223066-81342ee5ff30?auto=format&fit=crop&w=120&q=70',
    'flame-dmx': 'https://images.unsplash.com/photo-1506157786151-b8491531f063?auto=format&fit=crop&w=120&q=70'
};

const state = {
    items: [],
    step: 1
};

const elements = {
    emptyCart: document.querySelector('#empty-cart'),
    checkoutContent: document.querySelector('#checkout-content'),
    form: document.querySelector('#checkout-form'),
    formError: document.querySelector('#form-error'),
    summaryCount: document.querySelector('#summary-count'),
    summaryItems: document.querySelector('#summary-items'),
    subtotal: document.querySelector('#subtotal'),
    shipping: document.querySelector('#shipping'),
    vat: document.querySelector('#vat'),
    depositRow: document.querySelector('#deposit-row'),
    deposit: document.querySelector('#deposit'),
    grandTotal: document.querySelector('#grand-total'),
    previewDialog: document.querySelector('#preview-dialog'),
    previewOrderId: document.querySelector('#preview-order-id'),
    previewDetails: document.querySelector('#preview-details')
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

function normalizeItem(item, index) {
    if (!item || typeof item !== 'object' || Array.isArray(item)) return null;
    const productId = String(item.productId || item.id || `item-${index}`);
    const caseOption = item.flightcaseOption && typeof item.flightcaseOption === 'object'
        ? { name: String(item.flightcaseOption.name || 'Flightcase'), price: parseMoney(item.flightcaseOption.price) }
        : (typeof item.flightcase === 'string' && item.flightcase.trim()
            ? { name: item.flightcase.trim(), price: 0 }
            : null);
    const quantity = Math.max(1, Math.min(999, Math.floor(Number(item.quantity) || 1)));

    return {
        id: String(item.id || `${productId}-${index}`),
        productId,
        productName: typeof item.productName === 'string' && item.productName.trim() ? item.productName.trim() : 'Thiết bị sân khấu',
        productSku: String(item.productSku || item.sku || 'SKU'),
        productImage: typeof item.productImage === 'string' && item.productImage
            ? item.productImage
            : (productImages[productId] || 'https://images.unsplash.com/photo-1501386761578-eac5c94b800a?auto=format&fit=crop&w=120&q=70'),
        quantity,
        unitPrice: parseMoney(item.unitPrice),
        isRental: item.isRental === true,
        rentalDays: item.isRental === true ? Math.max(1, Math.min(30, Math.floor(Number(item.rentalDays) || 1))) : 1,
        flightcase: caseOption
    };
}

function loadCart() {
    try {
        const stored = localStorage.getItem(CART_STORAGE_KEY);
        if (!stored) return [];
        const parsed = JSON.parse(stored);
        if (!Array.isArray(parsed)) throw new TypeError('Dữ liệu giỏ hàng không đúng định dạng.');
        return parsed.map(normalizeItem).filter(Boolean);
    } catch (error) {
        console.error('Không thể đọc giỏ hàng đã lưu:', error);
        return [];
    }
}

function itemTotal(item) {
    return (item.unitPrice + (item.flightcase?.price || 0))
        * item.quantity
        * (item.isRental ? item.rentalDays : 1);
}

function calculateTotals() {
    const subtotal = state.items.reduce((sum, item) => sum + itemTotal(item), 0);
    const shipping = subtotal === 0 || subtotal >= SHIPPING_THRESHOLD ? 0 : SHIPPING_ESTIMATE;
    const vat = Math.round((subtotal + shipping) * 0.1);
    const rentalTotal = state.items
        .filter((item) => item.isRental)
        .reduce((sum, item) => sum + itemTotal(item), 0);
    const deposit = Math.round(rentalTotal * RENTAL_DEPOSIT_RATE);
    return { subtotal, shipping, vat, deposit, total: subtotal + shipping + vat };
}

function renderSummaryItems() {
    elements.summaryItems.replaceChildren(...state.items.map((item) => {
        const row = makeElement('div', 'summary-item');
        const image = makeElement('img');
        image.src = item.productImage;
        image.alt = '';
        image.loading = 'lazy';
        const details = makeElement('div', 'summary-item-details');
        details.append(
            makeElement('strong', '', item.productName),
            makeElement('span', '', `${item.productSku} • SL: ${item.quantity}${item.isRental ? ` • ${item.rentalDays} ngày` : ''}`)
        );
        row.append(image, details, makeElement('strong', 'summary-item-price', money.format(itemTotal(item))));
        return row;
    }));
}

function renderTotals() {
    const totals = calculateTotals();
    elements.subtotal.textContent = money.format(totals.subtotal);
    elements.shipping.textContent = totals.shipping === 0 ? 'Miễn phí*' : money.format(totals.shipping);
    elements.vat.textContent = money.format(totals.vat);
    elements.depositRow.hidden = totals.deposit <= 0;
    elements.deposit.textContent = money.format(totals.deposit);
    elements.grandTotal.textContent = money.format(totals.total);
}

function renderCart() {
    const totalQuantity = state.items.reduce((sum, item) => sum + item.quantity, 0);
    elements.emptyCart.hidden = state.items.length > 0;
    elements.checkoutContent.hidden = state.items.length === 0;
    if (state.items.length === 0) return;
    elements.summaryCount.textContent = `${state.items.length} mục • ${totalQuantity} thiết bị`;
    renderSummaryItems();
    renderTotals();
}

function updateStep() {
    document.querySelectorAll('.form-step').forEach((panel) => {
        panel.hidden = Number(panel.dataset.stepPanel) !== state.step;
    });
    document.querySelectorAll('.step-button').forEach((button) => {
        const buttonStep = Number(button.dataset.step);
        button.classList.toggle('active', buttonStep === state.step);
        button.classList.toggle('completed', buttonStep < state.step);
        if (buttonStep === state.step) button.setAttribute('aria-current', 'step');
        else button.removeAttribute('aria-current');
    });
    elements.formError.hidden = true;
    elements.formError.textContent = '';
    const heading = document.querySelector(`[data-step-panel="${state.step}"] h2`);
    if (heading) heading.focus({ preventScroll: true });
}

function validateCurrentStep() {
    const panel = document.querySelector(`[data-step-panel="${state.step}"]`);
    const requiredFields = [...panel.querySelectorAll('[required]')];
    const invalidField = requiredFields.find((field) => {
        if (field.type === 'radio') {
            return !panel.querySelector(`input[name="${field.name}"]:checked`);
        }
        return !field.checkValidity();
    });

    if (invalidField) {
        if (invalidField.type === 'radio') {
            elements.formError.textContent = 'Vui lòng chọn loại địa điểm trước khi tiếp tục.';
            elements.formError.hidden = false;
            panel.querySelector(`[name="${invalidField.name}"]`).focus();
        } else {
            invalidField.reportValidity();
            elements.formError.textContent = 'Vui lòng kiểm tra các trường bắt buộc được đánh dấu.';
            elements.formError.hidden = false;
        }
        return false;
    }
    return true;
}

function appendDetail(container, label, value) {
    const row = makeElement('div', 'preview-detail-row');
    row.append(makeElement('span', '', label), makeElement('strong', '', value || 'Chưa cung cấp'));
    container.append(row);
}

function createPreview() {
    const formData = new FormData(elements.form);
    const totals = calculateTotals();
    const orderId = `SFX-DEMO-${new Date().toISOString().slice(0, 10).replace(/-/g, '')}-${Math.random().toString(36).slice(2, 6).toUpperCase()}`;
    const venueType = formData.get('venueType') === 'indoor' ? 'Trong nhà' : 'Ngoài trời';
    const paymentMethod = paymentLabels[formData.get('paymentMethod')] || 'Chưa chọn';

    elements.previewOrderId.textContent = orderId;
    elements.previewDetails.replaceChildren();
    appendDetail(elements.previewDetails, 'Đơn vị', formData.get('company'));
    appendDetail(elements.previewDetails, 'Người liên hệ', formData.get('contactName'));
    appendDetail(elements.previewDetails, 'Sự kiện', formData.get('eventName'));
    appendDetail(elements.previewDetails, 'Ngày dự kiến', formData.get('eventDate'));
    appendDetail(elements.previewDetails, 'Địa điểm', `${venueType} • ${formData.get('eventLocation')}`);
    appendDetail(elements.previewDetails, 'Hình thức thanh toán', paymentMethod);
    appendDetail(elements.previewDetails, 'Tổng dự toán tham khảo', money.format(totals.total));
    elements.previewDialog.showModal();
}

document.querySelectorAll('[data-next]').forEach((button) => {
    button.addEventListener('click', () => {
        if (!validateCurrentStep()) return;
        state.step = Number(button.dataset.next);
        updateStep();
        window.scrollTo({ top: 0, behavior: 'smooth' });
    });
});

document.querySelectorAll('[data-previous]').forEach((button) => {
    button.addEventListener('click', () => {
        state.step = Number(button.dataset.previous);
        updateStep();
        window.scrollTo({ top: 0, behavior: 'smooth' });
    });
});

document.querySelectorAll('.step-button').forEach((button) => {
    button.addEventListener('click', () => {
        const targetStep = Number(button.dataset.step);
        if (targetStep <= state.step) {
            state.step = targetStep;
            updateStep();
            return;
        }
        if (targetStep === state.step + 1 && validateCurrentStep()) {
            state.step = targetStep;
            updateStep();
        }
    });
});

elements.form.addEventListener('submit', (event) => {
    event.preventDefault();
    if (!validateCurrentStep()) return;
    createPreview();
});

document.querySelectorAll('input[name="paymentMethod"]').forEach((input) => {
    input.addEventListener('change', () => {
        document.querySelectorAll('.payment-card').forEach((card) => {
            card.classList.toggle('selected', card.contains(input) && input.checked);
        });
    });
});

document.querySelector('#close-preview').addEventListener('click', () => elements.previewDialog.close());
document.querySelector('#close-preview-secondary').addEventListener('click', () => elements.previewDialog.close());
elements.previewDialog.addEventListener('click', (event) => {
    if (event.target === elements.previewDialog) elements.previewDialog.close();
});

const eventDate = document.querySelector('[name="eventDate"]');
const today = new Date();
eventDate.min = `${today.getFullYear()}-${String(today.getMonth() + 1).padStart(2, '0')}-${String(today.getDate()).padStart(2, '0')}`;
state.items = loadCart();
renderCart();
updateStep();

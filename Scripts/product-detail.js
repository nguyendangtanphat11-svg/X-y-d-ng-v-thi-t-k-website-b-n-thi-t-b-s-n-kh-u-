'use strict';

const products = [
    {
        id: 'fog-200',
        sku: 'FX-FOG-200',
        name: 'Professional Fog Machine 2000W',
        category: 'FOG / HAZE',
        description: 'Máy tạo khói công suất cao cho sân khấu, sự kiện và tour lưu diễn. Hỗ trợ điều khiển DMX, phù hợp phối hợp cùng hệ thống ánh sáng chuyên nghiệp.',
        image: 'https://images.unsplash.com/photo-1470229722913-7c0e2dbbafd3?auto=format&fit=crop&w=1200&q=85',
        gallery: [
            'https://images.unsplash.com/photo-1470229722913-7c0e2dbbafd3?auto=format&fit=crop&w=500&q=75',
            'https://images.unsplash.com/photo-1501386761578-eac5c94b800a?auto=format&fit=crop&w=500&q=75',
            'https://images.unsplash.com/photo-1492684223066-81342ee5ff30?auto=format&fit=crop&w=500&q=75'
        ],
        stock: 18,
        price: 18500000,
        rentalPricePerDay: 1800000,
        power: '2000W',
        voltage: 'AC 220V - 240V, 50/60Hz',
        dmxChannels: '3-pin & 5-pin DMX In/Out',
        weight: '14.5 kg',
        dimensions: '450 x 380 x 420 mm',
        flightcaseOptions: [
            { id: 'standard', name: 'Flightcase đơn', price: 2200000 },
            { id: 'touring', name: 'Flightcase touring', price: 3800000 }
        ]
    },
    {
        id: 'haze-pro',
        sku: 'FX-HAZE-1500',
        name: 'Touring Haze Machine 1500W',
        category: 'HAZE',
        description: 'Máy haze touring tạo lớp sương mịn, hỗ trợ làm nổi bật tia sáng và bố cục ánh sáng sân khấu.',
        image: 'https://images.unsplash.com/photo-1501386761578-eac5c94b800a?auto=format&fit=crop&w=1200&q=85',
        gallery: [
            'https://images.unsplash.com/photo-1501386761578-eac5c94b800a?auto=format&fit=crop&w=500&q=75',
            'https://images.unsplash.com/photo-1470229722913-7c0e2dbbafd3?auto=format&fit=crop&w=500&q=75'
        ],
        stock: 12,
        price: 24900000,
        rentalPricePerDay: 2200000,
        power: '1500W',
        voltage: 'AC 220V - 240V, 50/60Hz',
        dmxChannels: '3-pin & 5-pin DMX In/Out',
        weight: '18 kg',
        dimensions: '510 x 390 x 440 mm',
        flightcaseOptions: [{ id: 'haze-case', name: 'Flightcase touring', price: 3200000 }]
    },
    {
        id: 'co2-jet',
        sku: 'FX-CO2-120',
        name: 'CO₂ Jet Professional',
        category: 'CO₂ JET',
        description: 'Thiết bị hiệu ứng CO₂ dành cho sân khấu và sự kiện trực tiếp. Vận hành theo hướng dẫn của nhà sản xuất và quy trình an toàn tại địa điểm.',
        image: 'https://images.unsplash.com/photo-1533174072545-7a4b6ad7a6c3?auto=format&fit=crop&w=1200&q=85',
        gallery: [
            'https://images.unsplash.com/photo-1533174072545-7a4b6ad7a6c3?auto=format&fit=crop&w=500&q=75',
            'https://images.unsplash.com/photo-1492684223066-81342ee5ff30?auto=format&fit=crop&w=500&q=75'
        ],
        stock: 9,
        price: 16500000,
        rentalPricePerDay: 1600000,
        power: '800W',
        voltage: 'AC 220V - 240V, 50/60Hz',
        dmxChannels: 'DMX In/Out (tham khảo tài liệu hãng)',
        weight: '12 kg',
        dimensions: '400 x 320 x 390 mm',
        flightcaseOptions: [{ id: 'co2-case', name: 'Flightcase bảo vệ', price: 1900000 }]
    },
    {
        id: 'cold-spark',
        sku: 'FX-SPARK-300',
        name: 'Cold Spark Machine 300',
        category: 'COLD SPARK',
        description: 'Máy hiệu ứng cold spark cho chương trình biểu diễn và sự kiện. Cần được lắp đặt, kiểm tra và vận hành bởi nhân sự có chuyên môn.',
        image: 'https://images.unsplash.com/photo-1492684223066-81342ee5ff30?auto=format&fit=crop&w=1200&q=85',
        gallery: [
            'https://images.unsplash.com/photo-1492684223066-81342ee5ff30?auto=format&fit=crop&w=500&q=75',
            'https://images.unsplash.com/photo-1533174072545-7a4b6ad7a6c3?auto=format&fit=crop&w=500&q=75'
        ],
        stock: 6,
        price: 12800000,
        rentalPricePerDay: 1400000,
        power: '700W',
        voltage: 'AC 220V - 240V, 50/60Hz',
        dmxChannels: 'DMX In/Out (tham khảo tài liệu hãng)',
        weight: '10 kg',
        dimensions: '360 x 300 x 350 mm',
        flightcaseOptions: [{ id: 'spark-case', name: 'Flightcase touring', price: 1800000 }]
    }
];

function escapeHtml(value) {
    return String(value).replace(/[&<>"']/g, (character) => ({
        '&': '&amp;',
        '<': '&lt;',
        '>': '&gt;',
        '"': '&quot;',
        "'": '&#39;'
    })[character]);
}

function getCatalogProduct() {
    const productId = new URLSearchParams(window.location.search).get('id');
    if (!productId) return null;

    const detailProfiles = {
        'fog-2000': 'fog-200',
        'haze-1500': 'haze-pro',
        'fog-compact': 'fog-200',
        'spark-300': 'cold-spark',
        'spark-pro': 'cold-spark',
        'flame-dmx': 'cold-spark',
        'flame-stage': 'cold-spark',
        'co2-jet': 'co2-jet',
        'co2-accessory': 'co2-jet',
        'confetti-pro': 'cold-spark',
        'dmx-controller': 'cold-spark'
    };
    const profile = products.find((product) => product.id === detailProfiles[productId]);
    if (!profile) return null;

    let catalogProduct;
    try {
        catalogProduct = JSON.parse(sessionStorage.getItem('stagefx-selected-product') || 'null');
    } catch (error) {
        console.error('Không thể đọc thông tin sản phẩm đã chọn:', error);
        return profile;
    }

    if (!catalogProduct || catalogProduct.id !== productId) return profile;
    try {
        sessionStorage.removeItem('stagefx-selected-product');
    } catch (error) {
        console.error('Không thể xóa thông tin sản phẩm tạm thời:', error);
    }

    const text = (value, fallback) => typeof value === 'string' && value.trim() ? value.trim() : fallback;
    const price = Number(catalogProduct.b2bPrice);
    const stock = Number(catalogProduct.stock);
    const image = text(catalogProduct.image, profile.image);
    const gallery = [image, ...profile.gallery].filter((item, index, list) => list.indexOf(item) === index);

    return {
        ...profile,
        id: productId,
        sku: text(catalogProduct.sku, profile.sku),
        name: text(catalogProduct.name, profile.name),
        category: text(catalogProduct.categoryLabel, profile.category),
        description: text(catalogProduct.description, profile.description),
        image,
        gallery,
        stock: Number.isFinite(stock) && stock >= 0 ? stock : profile.stock,
        price: Number.isFinite(price) && price >= 0 ? price : profile.price,
        power: text(catalogProduct.power, profile.power),
        dmxChannels: text(catalogProduct.dmxChannels, profile.dmxChannels),
        voltage: 'Xác nhận theo tài liệu hãng',
        weight: 'Xác nhận theo tài liệu hãng',
        dimensions: 'Xác nhận theo tài liệu hãng',
        flightcaseOptions: ['fog-2000', 'haze-1500', 'spark-300', 'co2-jet'].includes(productId)
            ? profile.flightcaseOptions
            : []
    };
}

const money = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND', maximumFractionDigits: 0 });
const initialProduct = getCatalogProduct() || products[0];
const state = {
    product: initialProduct,
    orderType: 'buy',
    quantity: 1,
    rentalDays: 1,
    selectedFlightcaseId: initialProduct.flightcaseOptions[0]?.id || null,
    selectedImage: initialProduct.image,
    activeTab: 'specs'
};

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
    cartCount: document.querySelector('#cart-count'),
    toast: document.querySelector('#toast')
};

function getSelectedFlightcase() {
    return state.product.flightcaseOptions.find((option) => option.id === state.selectedFlightcaseId) || null;
}

function getTotal() {
    const basePrice = state.orderType === 'buy'
        ? state.product.price
        : (state.product.rentalPricePerDay || Math.round(state.product.price * 0.08));
    const flightcasePrice = state.orderType === 'buy' ? (getSelectedFlightcase()?.price || 0) : 0;
    return (basePrice + flightcasePrice) * state.quantity * (state.orderType === 'rent' ? state.rentalDays : 1);
}

function renderGallery() {
    const images = [state.product.image, ...state.product.gallery];
    elements.image.src = state.selectedImage;
    elements.image.alt = state.product.name;
    elements.gallery.replaceChildren(...images.map((image, index) => {
        const button = document.createElement('button');
        button.type = 'button';
        button.className = `thumbnail${image === state.selectedImage ? ' active' : ''}`;
        button.setAttribute('aria-label', `Xem ảnh ${index + 1} của ${state.product.name}`);
        button.setAttribute('aria-pressed', String(image === state.selectedImage));
        const thumbnail = document.createElement('img');
        thumbnail.src = image;
        thumbnail.alt = '';
        button.append(thumbnail);
        button.addEventListener('click', () => {
            state.selectedImage = image;
            renderGallery();
        });
        return button;
    }));
}

function renderFlightcaseOptions() {
    elements.flightcaseOptions.replaceChildren();
    const noCase = document.createElement('button');
    noCase.type = 'button';
    noCase.className = `flightcase-option${state.selectedFlightcaseId === null ? ' selected' : ''}`;
    noCase.innerHTML = '<strong>Không lấy Flightcase</strong><span>Thùng carton tiêu chuẩn</span>';
    noCase.addEventListener('click', () => {
        state.selectedFlightcaseId = null;
        render();
    });
    elements.flightcaseOptions.append(noCase);

    state.product.flightcaseOptions.forEach((option) => {
        const button = document.createElement('button');
        button.type = 'button';
        button.className = `flightcase-option${state.selectedFlightcaseId === option.id ? ' selected' : ''}`;
        button.innerHTML = `<strong>${option.name}</strong><span>+${money.format(option.price)}</span>`;
        button.setAttribute('aria-pressed', String(state.selectedFlightcaseId === option.id));
        button.addEventListener('click', () => {
            state.selectedFlightcaseId = option.id;
            render();
        });
        elements.flightcaseOptions.append(button);
    });
}

function renderTabs() {
    document.querySelectorAll('.tab-button').forEach((button) => {
        const active = button.dataset.tab === state.activeTab;
        button.classList.toggle('active', active);
        button.setAttribute('aria-selected', String(active));
        if (active) elements.tabPanel.setAttribute('aria-labelledby', button.id);
    });

    const product = state.product;
    const contents = {
        specs: `
      <div class="spec-grid">
        <section class="info-card"><h3>Thông số điện &amp; vận hành</h3>
          <div class="info-row"><span>Công suất tiêu thụ</span><strong>${escapeHtml(product.power)}</strong></div>
          <div class="info-row"><span>Điện áp hoạt động</span><strong>${escapeHtml(product.voltage)}</strong></div>
          <div class="info-row"><span>Kết nối tín hiệu</span><strong>${escapeHtml(product.dmxChannels)}</strong></div>
          <div class="info-row"><span>Cổng nguồn</span><strong>PowerCON In/Thru</strong></div>
        </section>
        <section class="info-card"><h3>Kích thước &amp; trọng lượng</h3>
          <div class="info-row"><span>Trọng lượng máy</span><strong>${escapeHtml(product.weight)}</strong></div>
          <div class="info-row"><span>Kích thước khung</span><strong>${escapeHtml(product.dimensions)}</strong></div>
          <div class="info-row"><span>Vật liệu vỏ</span><strong>Vỏ kim loại chuyên dụng</strong></div>
          <div class="info-row"><span>Cấp bảo vệ</span><strong>Theo tài liệu sản phẩm</strong></div>
        </section>
      </div>`,
        dmx: `
      <p class="tab-intro">Cấu hình DMX thay đổi theo model. Chỉ sử dụng fixture profile và hướng dẫn chính hãng; việc lắp đặt, cấu hình và vận hành thiết bị hiệu ứng cần do kỹ thuật viên đủ chuyên môn thực hiện theo quy trình an toàn tại địa điểm.</p>
      <section class="info-card"><h3>Hồ sơ điều khiển</h3>
        <div class="info-row"><span>Thiết bị</span><strong>${escapeHtml(product.sku)}</strong></div>
        <div class="info-row"><span>Kết nối</span><strong>${escapeHtml(product.dmxChannels)}</strong></div>
        <div class="info-row"><span>Thông số kênh / profile</span><strong>Tra cứu tài liệu chính hãng</strong></div>
      </section>`,
        safety: `
      <div class="safety-grid">
        <div class="safety-notice"><strong>Lưu ý trước khi sử dụng:</strong> Tuân thủ hướng dẫn của nhà sản xuất, quy định PCCC và khoảng cách an toàn được phê duyệt cho từng địa điểm. Luôn kiểm tra thiết bị trước show.</div>
        <section class="info-card"><h3>Kiểm tra thiết bị</h3><p>Kiểm tra dây dẫn, kết nối, vị trí lắp đặt và tình trạng thiết bị trước mỗi lần vận hành.</p></section>
        <section class="info-card"><h3>Nhân sự chuyên môn</h3><p>Chỉ giao việc lắp đặt và vận hành cho kỹ thuật viên đã được đào tạo phù hợp.</p></section>
        <section class="info-card"><h3>Hồ sơ an toàn</h3><p>Xác nhận hồ sơ kiểm định, phương án ứng phó và yêu cầu của địa điểm tổ chức.</p></section>
      </div>`,
        downloads: `
      <div class="download-grid">
        <button class="download-card" type="button" data-document="Datasheet và hướng dẫn sử dụng"><span class="material-symbols-outlined">picture_as_pdf</span><span><strong>Datasheet &amp; hướng dẫn</strong><small>PDF • Liên hệ để nhận tài liệu</small></span></button>
        <button class="download-card" type="button" data-document="DMX fixture profile"><span class="material-symbols-outlined">code</span><span><strong>DMX fixture profile</strong><small>Profile chính hãng theo model</small></span></button>
        <button class="download-card" type="button" data-document="Hồ sơ an toàn"><span class="material-symbols-outlined">verified</span><span><strong>Hồ sơ an toàn</strong><small>Liên hệ để xác nhận tài liệu</small></span></button>
      </div>`
    };
    elements.tabPanel.innerHTML = contents[state.activeTab];
    elements.tabPanel.querySelectorAll('[data-document]').forEach((button) => {
        button.addEventListener('click', () => showToast(`Chưa có tệp ${button.dataset.document}. Vui lòng liên hệ để nhận tài liệu chính thức.`));
    });
}

function renderRelated() {
    const relatedProducts = products.filter((product) => product.id !== state.product.id).slice(0, 3);
    elements.related.replaceChildren(...relatedProducts.map((product) => {
        const card = document.createElement('article');
        card.className = 'related-card';
        card.tabIndex = 0;
        card.setAttribute('role', 'button');
        card.setAttribute('aria-label', `Xem ${product.name}`);
        card.innerHTML = `<img src="${product.image}" alt=""><div class="related-card-body"><span>${product.sku}</span><h3>${product.name}</h3><strong>${money.format(product.price)}</strong></div>`;
        const selectProduct = () => {
            state.product = product;
            state.orderType = 'buy';
            state.quantity = 1;
            state.rentalDays = 1;
            state.selectedFlightcaseId = product.flightcaseOptions[0]?.id || null;
            state.selectedImage = product.image;
            state.activeTab = 'specs';
            render();
            window.scrollTo({ top: 0, behavior: 'smooth' });
        };
        card.addEventListener('click', selectProduct);
        card.addEventListener('keydown', (event) => {
            if (event.key === 'Enter' || event.key === ' ') {
                event.preventDefault();
                selectProduct();
            }
        });
        return card;
    }));
}

function render() {
    const product = state.product;
    elements.category.textContent = product.category;
    elements.stock.textContent = product.stock;
    elements.sku.textContent = product.sku;
    elements.breadcrumbSku.textContent = product.sku;
    elements.name.textContent = product.name;
    elements.description.textContent = product.description;
    elements.priceLabel.textContent = state.orderType === 'buy'
        ? 'Đơn giá niêm yết (đã gồm VAT):'
        : 'Đơn giá thuê / ngày:';
    const unitPrice = state.orderType === 'buy'
        ? product.price
        : (product.rentalPricePerDay || Math.round(product.price * 0.08));
    elements.price.textContent = money.format(unitPrice);
    elements.rentalDaysWrap.hidden = state.orderType !== 'rent';
    elements.volumePricing.hidden = state.orderType !== 'buy';
    elements.flightcaseSection.hidden = state.orderType !== 'buy' || product.flightcaseOptions.length === 0;
    document.querySelector('#buy-mode').classList.toggle('active', state.orderType === 'buy');
    document.querySelector('#buy-mode').setAttribute('aria-pressed', String(state.orderType === 'buy'));
    document.querySelector('#rent-mode').classList.toggle('active', state.orderType === 'rent');
    document.querySelector('#rent-mode').setAttribute('aria-pressed', String(state.orderType === 'rent'));
    elements.quantity.max = String(product.stock);
    elements.quantity.value = String(state.quantity);
    elements.rentalDays.value = String(state.rentalDays);
    elements.subtotal.textContent = money.format(getTotal());
    document.querySelector('#buy-now').innerHTML = `<span class="material-symbols-outlined">bolt</span>${state.orderType === 'buy' ? 'Đặt mua ngay' : 'Xác nhận đặt thuê'}`;
    renderGallery();
    renderFlightcaseOptions();
    renderTabs();
    renderRelated();
    updateCartCount();
}

function showToast(message) {
    elements.toast.textContent = message;
    elements.toast.classList.add('visible');
    window.clearTimeout(showToast.timeoutId);
    showToast.timeoutId = window.setTimeout(() => elements.toast.classList.remove('visible'), 3200);
}

function readCart() {
    try {
        const savedCart = JSON.parse(localStorage.getItem('stagefx-demo-cart') || '[]');
        return Array.isArray(savedCart) ? savedCart : [];
    } catch (error) {
        console.error('Không thể đọc giỏ hàng local:', error);
        return [];
    }
}

function updateCartCount() {
    const count = readCart().reduce((total, item) => total + item.quantity, 0);
    if (elements.cartCount) elements.cartCount.textContent = String(count);
    window.dispatchEvent(new Event('stagefx:cart-change'));
}

function addToCart() {
    const cart = readCart();
    const flightcase = state.orderType === 'buy' ? getSelectedFlightcase() : null;
    const cartItem = {
        productId: state.product.id,
        productName: state.product.name,
        sku: state.product.sku,
        quantity: state.quantity,
        unitPrice: state.orderType === 'buy' ? state.product.price : state.product.rentalPricePerDay,
        isRental: state.orderType === 'rent',
        rentalDays: state.orderType === 'rent' ? state.rentalDays : undefined,
        flightcase: flightcase ? flightcase.name : null
    };
    const existing = cart.find((item) => item.productId === cartItem.productId
        && item.isRental === cartItem.isRental
        && item.rentalDays === cartItem.rentalDays
        && item.flightcase === cartItem.flightcase);
    if (existing) existing.quantity += cartItem.quantity;
    else cart.push(cartItem);

    try {
        localStorage.setItem('stagefx-demo-cart', JSON.stringify(cart));
    } catch (error) {
        console.error('Không thể lưu giỏ hàng local:', error);
        showToast('Không lưu được giỏ hàng trên trình duyệt này.');
        return;
    }
    updateCartCount();
    showToast(`Đã thêm ${state.product.name} (x${state.quantity}) vào giỏ demo trên thiết bị.`);
}

function setQuantity(value) {
    const parsed = Number.parseInt(value, 10);
    state.quantity = Math.max(1, Math.min(state.product.stock, Number.isFinite(parsed) ? parsed : 1));
    render();
}

document.querySelector('#buy-mode').addEventListener('click', () => {
    state.orderType = 'buy';
    render();
});
document.querySelector('#rent-mode').addEventListener('click', () => {
    state.orderType = 'rent';
    render();
});
document.querySelector('#quantity').addEventListener('change', (event) => setQuantity(event.currentTarget.value));
document.querySelector('#decrease-quantity').addEventListener('click', () => setQuantity(state.quantity - 1));
document.querySelector('#increase-quantity').addEventListener('click', () => setQuantity(state.quantity + 1));
document.querySelector('#rental-days').addEventListener('change', (event) => {
    const parsed = Number.parseInt(event.currentTarget.value, 10);
    state.rentalDays = Math.max(1, Math.min(30, Number.isFinite(parsed) ? parsed : 1));
    render();
});
document.querySelector('#add-to-cart').addEventListener('click', addToCart);
document.querySelector('#buy-now').addEventListener('click', addToCart);
document.querySelectorAll('.tab-button').forEach((button) => {
    button.addEventListener('click', () => {
        state.activeTab = button.dataset.tab;
        renderTabs();
    });
});

render();


'use strict';

/* =========================================================
   STAGEFX PRO - CATALOG
   ASP.NET MVC 5 (.NET Framework)
   ========================================================= */

const products = [
    {
        id: 'fog-2000',
        sku: 'FX-FOG-2000',
        name: 'Máy tạo khói Touring 2000W',
        category: 'fog-haze',
        categoryLabel: 'Khói / Haze',
        searchTerms: 'máy khói fog machine',
        description: 'Máy tạo khói công suất cao cho sân khấu, sự kiện và tour lưu diễn.',
        image: 'https://images.unsplash.com/photo-1470229722913-7c0e2dbbafd3?auto=format&fit=crop&w=800&q=80',
        dmxChannels: 'DMX512 In/Out',
        power: '2000W',
        heightOrReach: 'Touring Grade',
        b2bPrice: 18500000,
        stock: 18,
        reviewCount: 28
    },
    {
        id: 'haze-1500',
        sku: 'FX-HAZE-1500',
        name: 'Máy tạo Haze sân khấu 1500W',
        category: 'fog-haze',
        categoryLabel: 'Khói / Haze',
        searchTerms: 'máy khói haze',
        description: 'Tạo lớp haze mịn, hỗ trợ làm nổi bật tia sáng sân khấu.',
        image: 'https://images.unsplash.com/photo-1501386761578-eac5c94b800a?auto=format&fit=crop&w=800&q=80',
        dmxChannels: 'DMX512 In/Out',
        power: '1500W',
        heightOrReach: 'Dung tích 3.5L',
        b2bPrice: 24900000,
        stock: 9,
        reviewCount: 21
    },
    {
        id: 'fog-compact',
        sku: 'FX-FOG-1200',
        name: 'Máy khói compact 1200W',
        category: 'fog-haze',
        categoryLabel: 'Khói / Haze',
        searchTerms: 'máy khói fog',
        description: 'Thiết kế gọn cho sân khấu vừa và nhỏ, điều khiển có dây.',
        image: 'https://images.unsplash.com/photo-1492684223066-81342ee5ff30?auto=format&fit=crop&w=800&q=80',
        dmxChannels: 'DMX512 tùy chọn',
        power: '1200W',
        heightOrReach: 'Dung tích 2.5L',
        b2bPrice: 8900000,
        stock: 0,
        reviewCount: 10
    },
    {
        id: 'spark-300',
        sku: 'FX-SPARK-300',
        name: 'Máy hiệu ứng Cold Spark 300',
        category: 'spark',
        categoryLabel: 'Cold Spark',
        searchTerms: 'pháo sáng sparkular',
        description: 'Thiết bị hiệu ứng sân khấu; cần vận hành theo hướng dẫn của hãng.',
        image: 'https://images.unsplash.com/photo-1533174072545-7a4b6ad7a6c3?auto=format&fit=crop&w=800&q=80',
        dmxChannels: 'DMX512 In/Out',
        power: '700W',
        heightOrReach: 'Hiệu ứng sân khấu',
        b2bPrice: 12800000,
        stock: 11,
        reviewCount: 37
    },
    {
        id: 'spark-pro',
        sku: 'FX-SPARK-PRO',
        name: 'Cold Spark Touring Pro',
        category: 'spark',
        categoryLabel: 'Cold Spark',
        searchTerms: 'pháo sáng sparkular',
        description: 'Phiên bản touring với vỏ bảo vệ chắc chắn và giao tiếp DMX.',
        image: 'https://images.unsplash.com/photo-1506157786151-b8491531f063?auto=format&fit=crop&w=800&q=80',
        dmxChannels: 'DMX512 In/Out',
        power: '900W',
        heightOrReach: 'Touring Grade',
        b2bPrice: 19800000,
        stock: 5,
        reviewCount: 25
    },
    {
        id: 'flame-dmx',
        sku: 'FX-FLAME-DMX',
        name: 'Thiết bị hiệu ứng Flame DMX',
        category: 'flame',
        categoryLabel: 'Máy phun lửa',
        searchTerms: 'phun lửa flame',
        description: 'Thiết bị hiệu ứng chuyên dụng. Cần được lắp đặt và vận hành bởi nhân sự có chuyên môn, theo hồ sơ an toàn được duyệt.',
        image: 'https://images.unsplash.com/photo-1506157786151-b8491531f063?auto=format&fit=crop&w=800&q=80',
        dmxChannels: 'DMX512 In/Out',
        power: 'Theo tài liệu hãng',
        heightOrReach: 'Thông số theo model',
        b2bPrice: 32500000,
        stock: 2,
        reviewCount: 16
    },
    {
        id: 'flame-stage',
        sku: 'FX-FLAME-STAGE',
        name: 'Bộ hiệu ứng Flame Stage',
        category: 'flame',
        categoryLabel: 'Máy phun lửa',
        searchTerms: 'phun lửa flame',
        description: 'Thiết bị biểu diễn chuyên dụng; cấu hình thực tế cần được xác nhận bởi nhà cung cấp.',
        image: 'https://images.unsplash.com/photo-1501386761578-eac5c94b800a?auto=format&fit=crop&w=800&q=80',
        dmxChannels: 'DMX512 tùy model',
        power: 'Theo tài liệu hãng',
        heightOrReach: 'Thông số theo model',
        b2bPrice: 42000000,
        stock: 0,
        reviewCount: 8
    },
    {
        id: 'co2-jet',
        sku: 'FX-CO2-JET',
        name: 'CO₂ Jet hiệu ứng sân khấu',
        category: 'co2',
        categoryLabel: 'CO₂ Jet',
        searchTerms: 'co2 khí carbon dioxide',
        description: 'Thiết bị hiệu ứng CO₂; tuân thủ hướng dẫn nhà sản xuất và quy trình an toàn tại địa điểm.',
        image: 'https://images.unsplash.com/photo-1524368535928-5b5e00ddc76b?auto=format&fit=crop&w=800&q=80',
        dmxChannels: 'DMX512 tùy model',
        power: '800W',
        heightOrReach: 'Thông số theo model',
        b2bPrice: 16500000,
        stock: 7,
        reviewCount: 19
    },
    {
        id: 'co2-accessory',
        sku: 'FX-CO2-KIT',
        name: 'Bộ phụ kiện kết nối CO₂',
        category: 'co2',
        categoryLabel: 'CO₂ Jet',
        searchTerms: 'co2 khí carbon dioxide',
        description: 'Phụ kiện tham khảo cho hệ thống CO₂ tương thích; xác nhận kết nối trước khi đặt hàng.',
        image: 'https://images.unsplash.com/photo-1524368535928-5b5e00ddc76b?auto=format&fit=crop&w=800&q=80',
        dmxChannels: 'Không áp dụng',
        power: 'Không áp dụng',
        heightOrReach: 'Phụ kiện',
        b2bPrice: 6500000,
        stock: 13,
        reviewCount: 6
    },
    {
        id: 'confetti-pro',
        sku: 'FX-CONFETTI-PRO',
        name: 'Máy bắn giấy kim tuyến Pro',
        category: 'confetti',
        categoryLabel: 'Máy bắn giấy',
        searchTerms: 'máy bắn giấy kim tuyến confetti',
        description: 'Máy tạo hiệu ứng giấy cho sân khấu, chương trình trực tiếp và sự kiện.',
        image: 'https://images.unsplash.com/photo-1533174072545-7a4b6ad7a6c3?auto=format&fit=crop&w=800&q=80',
        dmxChannels: 'DMX512 In/Out',
        power: '500W',
        heightOrReach: 'Thông số theo model',
        b2bPrice: 11200000,
        stock: 10,
        reviewCount: 14
    },
    {
        id: 'dmx-controller',
        sku: 'FX-DMX-CTRL',
        name: 'Bộ điều khiển DMX và phụ kiện',
        category: 'controller',
        categoryLabel: 'Điều khiển & phụ kiện',
        searchTerms: 'bộ điều khiển dmx phụ kiện',
        description: 'Thiết bị điều khiển tham khảo để tích hợp hệ thống hiệu ứng sân khấu.',
        image: 'https://images.unsplash.com/photo-1524368535928-5b5e00ddc76b?auto=format&fit=crop&w=800&q=80',
        dmxChannels: 'DMX512',
        power: 'Theo tài liệu hãng',
        heightOrReach: 'Phụ kiện',
        b2bPrice: 14500000,
        stock: 4,
        reviewCount: 12
    }
];

const itemsPerPage = 6;

const money = new Intl.NumberFormat('vi-VN', {
    style: 'currency',
    currency: 'VND',
    maximumFractionDigits: 0
});

const state = {
    searchQuery: new URLSearchParams(window.location.search).get('q') || '',
    selectedCategory: 'all',
    maxPrice: 50000000,
    stockOnly: false,
    dmxOnly: false,
    sortBy: 'bestseller',
    viewMode: 'grid',
    currentPage: 1
};

const elements = {
    searchForm: document.querySelector('#search-form'),
    searchInput: document.querySelector('#search-input'),
    clearSearch: document.querySelector('#clear-search'),
    skuCount: document.querySelector('#sku-count'),
    priceRange: document.querySelector('#price-range'),
    priceOutput: document.querySelector('#price-output'),
    stockOnly: document.querySelector('#stock-only'),
    dmxOnly: document.querySelector('#dmx-only'),
    sortSelect: document.querySelector('#sort-select'),
    productList: document.querySelector('#product-list'),
    resultsSummary: document.querySelector('#results-summary'),
    emptyState: document.querySelector('#empty-state'),
    pagination: document.querySelector('#pagination'),
    cartCount: document.querySelector('#cart-count'),
    dialog: document.querySelector('#product-dialog'),
    dialogContent: document.querySelector('#dialog-content'),
    toast: document.querySelector('#toast')
};

/* =========================================================
   UTILITIES
   ========================================================= */

function normalizeText(value) {
    return String(value ?? '')
        .normalize('NFD')
        .replace(/[\u0300-\u036f]/g, '')
        .replace(/₂/g, '2')
        .replace(/[đĐ]/g, 'd')
        .toLowerCase()
        .trim();
}

function makeElement(tag, className = '', text) {
    const element = document.createElement(tag);

    if (className) {
        element.className = className;
    }

    if (text !== undefined && text !== null) {
        element.textContent = String(text);
    }

    return element;
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
    }, 3000);
}

/* =========================================================
   MVC URL
   ========================================================= */

// URL template được Razor khai báo trong Views/Catalog/Index.cshtml.
// Ví dụ: /Product/Details/__PRODUCT_ID__
function getProductDetailUrl(productId) {
    const template = window.STAGEFX_PRODUCT_DETAILS_URL;

    if (typeof template === 'string' && template.length > 0) {
        return template.replace(
            '__PRODUCT_ID__',
            encodeURIComponent(productId)
        );
    }

    // Fallback nếu chưa khai báo template Razor.
    // Phù hợp khi ứng dụng chạy tại thư mục gốc website.
    return `/Product/Details/${encodeURIComponent(productId)}`;
}

function openProductDetails(product, detailUrl) {
    try {
        sessionStorage.setItem(
            'stagefx-selected-product',
            JSON.stringify(product)
        );
    } catch (error) {
        // Việc lưu dữ liệu mẫu không được ngăn điều hướng.
        console.warn('Không lưu được dữ liệu sản phẩm:', error);
    }

    window.location.assign(detailUrl);
}

/* =========================================================
   FILTER & SORT
   ========================================================= */

function getFilteredProducts() {
    const query = normalizeText(state.searchQuery);

    return products
        .filter((product) => {
            const searchableText = normalizeText([
                product.name,
                product.sku,
                product.description,
                product.categoryLabel,
                product.dmxChannels,
                product.power,
                product.searchTerms || ''
            ].join(' '));

            if (
                query &&
                !query.split(/\s+/).every((term) => searchableText.includes(term))
            ) {
                return false;
            }

            if (
                state.selectedCategory !== 'all' &&
                product.category !== state.selectedCategory
            ) {
                return false;
            }

            if (product.b2bPrice > state.maxPrice) {
                return false;
            }

            if (state.stockOnly && product.stock <= 0) {
                return false;
            }

            if (
                state.dmxOnly &&
                !normalizeText(product.dmxChannels).includes('dmx')
            ) {
                return false;
            }

            return true;
        })
        .sort((a, b) => {
            if (state.sortBy === 'price-asc') {
                return a.b2bPrice - b.b2bPrice;
            }

            if (state.sortBy === 'price-desc') {
                return b.b2bPrice - a.b2bPrice;
            }

            if (state.sortBy === 'name-az') {
                return a.name.localeCompare(b.name, 'vi');
            }

            return b.reviewCount - a.reviewCount;
        });
}

/* =========================================================
   PRODUCT CARD
   ========================================================= */

function createStockBadge(product) {
    const badge = makeElement(
        'span',
        `stock-badge${product.stock <= 0 ? ' out-of-stock' : ''}`
    );

    const dot = makeElement('i', 'stock-dot');
    dot.setAttribute('aria-hidden', 'true');

    badge.append(
        dot,
        document.createTextNode(
            product.stock > 0 ? 'CÒN HÀNG' : 'ĐẶT TRƯỚC'
        )
    );

    return badge;
}

function createProductCard(product) {
    const card = makeElement('article', 'product-card');

    const imageWrap = makeElement('div', 'product-card-image-wrap');
    const image = makeElement('img', 'product-card-image');

    image.src = product.image;
    image.alt = product.name;
    image.loading = 'lazy';

    const category = makeElement(
        'span',
        'image-category',
        product.categoryLabel
    );

    imageWrap.append(
        image,
        createStockBadge(product),
        category
    );

    const body = makeElement('div', 'product-card-body');
    const meta = makeElement('div', 'product-meta');

    meta.append(
        makeElement('span', '', product.sku),
        makeElement('span', '', `${product.reviewCount} đánh giá`)
    );

    const title = makeElement('h2', 'product-card-title');
    const detailUrl = getProductDetailUrl(product.id);

    const titleLink = makeElement(
        'a',
        'product-detail-link',
        product.name
    );

    titleLink.href = detailUrl;

    titleLink.addEventListener('click', (event) => {
        if (
            event.button !== 0 ||
            event.metaKey ||
            event.ctrlKey ||
            event.shiftKey ||
            event.altKey
        ) {
            return;
        }

        event.preventDefault();
        openProductDetails(product, detailUrl);
    });

    title.append(titleLink);

    const description = makeElement(
        'p',
        'product-description',
        product.description
    );

    const specs = makeElement('div', 'product-specs');

    specs.append(
        makeElement('span', '', `• ${product.dmxChannels}`),
        makeElement('span', '', `• ${product.power}`)
    );

    const price = makeElement('div', 'product-price-row');

    price.append(
        makeElement('span', '', 'Giá B2B mẫu'),
        makeElement('strong', '', money.format(product.b2bPrice))
    );

    const actions = makeElement('div', 'product-actions');

    const detailButton = makeElement(
        'a',
        'catalog-detail-button',
        'Xem chi tiết'
    );

    detailButton.href = detailUrl;

    detailButton.addEventListener('click', (event) => {
        if (
            event.button !== 0 ||
            event.metaKey ||
            event.ctrlKey ||
            event.shiftKey ||
            event.altKey
        ) {
            return;
        }

        event.preventDefault();
        openProductDetails(product, detailUrl);
    });

    const addButton = makeElement('button', 'add-cart-button');
    addButton.type = 'button';

    const cartIcon = makeElement(
        'span',
        'material-symbols-outlined',
        'add_shopping_cart'
    );

    cartIcon.setAttribute('aria-hidden', 'true');

    addButton.append(
        cartIcon,
        document.createTextNode('Thêm giỏ')
    );

    addButton.addEventListener('click', () => {
        addToCart(product);
    });

    actions.append(detailButton, addButton);

    body.append(
        meta,
        title,
        description,
        specs,
        price,
        actions
    );

    card.append(imageWrap, body);

    return card;
}

/* =========================================================
   PAGINATION
   ========================================================= */

function changePage(page) {
    const totalPages = Math.max(
        1,
        Math.ceil(getFilteredProducts().length / itemsPerPage)
    );

    state.currentPage = Math.max(
        1,
        Math.min(totalPages, page)
    );

    render();

    document.querySelector('#catalog-results')?.scrollIntoView({
        behavior: 'smooth',
        block: 'start'
    });
}

function renderPagination(totalItems, totalPages) {
    if (!elements.pagination) return;

    elements.pagination.replaceChildren();
    elements.pagination.hidden = totalItems === 0;

    if (totalItems === 0) return;

    const start = (state.currentPage - 1) * itemsPerPage + 1;
    const end = Math.min(
        state.currentPage * itemsPerPage,
        totalItems
    );

    const summary = makeElement('p', 'pagination-summary');

    summary.append(
        document.createTextNode('Hiển thị '),
        makeElement('strong', '', `${start}–${end}`),
        document.createTextNode(' / '),
        makeElement('strong', '', String(totalItems)),
        document.createTextNode(' thiết bị')
    );

    const buttons = makeElement('div', 'pagination-buttons');

    const previous = makeElement('button');
    previous.type = 'button';
    previous.disabled = state.currentPage <= 1;
    previous.setAttribute('aria-label', 'Trang trước');

    previous.append(
        makeElement('span', 'material-symbols-outlined', 'chevron_left')
    );

    previous.addEventListener('click', () => {
        changePage(state.currentPage - 1);
    });

    buttons.append(previous);

    for (let page = 1; page <= totalPages; page += 1) {
        const button = makeElement(
            'button',
            page === state.currentPage ? 'current' : '',
            String(page)
        );

        button.type = 'button';
        button.setAttribute('aria-label', `Trang ${page}`);

        if (page === state.currentPage) {
            button.setAttribute('aria-current', 'page');
        }

        button.addEventListener('click', () => {
            changePage(page);
        });

        buttons.append(button);
    }

    const next = makeElement('button');
    next.type = 'button';
    next.disabled = state.currentPage >= totalPages;
    next.setAttribute('aria-label', 'Trang tiếp theo');

    next.append(
        makeElement('span', 'material-symbols-outlined', 'chevron_right')
    );

    next.addEventListener('click', () => {
        changePage(state.currentPage + 1);
    });

    buttons.append(next);
    elements.pagination.append(summary, buttons);
}

/* =========================================================
   RENDER CATALOG
   ========================================================= */

function render() {
    // Tránh lỗi nếu file JS vô tình được tải ở trang khác.
    if (
        !elements.searchInput ||
        !elements.productList ||
        !elements.resultsSummary
    ) {
        return;
    }

    const filtered = getFilteredProducts();

    const totalPages = Math.max(
        1,
        Math.ceil(filtered.length / itemsPerPage)
    );

    state.currentPage = Math.min(
        state.currentPage,
        totalPages
    );

    const startIndex = (state.currentPage - 1) * itemsPerPage;

    const pageItems = filtered.slice(
        startIndex,
        startIndex + itemsPerPage
    );

    elements.searchInput.value = state.searchQuery;

    if (elements.clearSearch) {
        elements.clearSearch.hidden = state.searchQuery.length === 0;
    }

    if (elements.skuCount) {
        elements.skuCount.textContent = `${filtered.length} SKU`;
    }

    if (elements.priceRange) {
        elements.priceRange.value = String(state.maxPrice);
    }

    if (elements.priceOutput) {
        elements.priceOutput.textContent = money.format(state.maxPrice);
    }

    if (elements.stockOnly) {
        elements.stockOnly.checked = state.stockOnly;
    }

    if (elements.dmxOnly) {
        elements.dmxOnly.checked = state.dmxOnly;
    }

    if (elements.sortSelect) {
        elements.sortSelect.value = state.sortBy;
    }

    const categoryInput = document.querySelector(
        `input[name="category"][value="${state.selectedCategory}"]`
    );

    if (categoryInput) {
        categoryInput.checked = true;
    }

    const firstItem = filtered.length === 0 ? 0 : startIndex + 1;
    const lastItem = Math.min(
        startIndex + itemsPerPage,
        filtered.length
    );

    elements.resultsSummary.replaceChildren(
        document.createTextNode('Đang hiển thị '),
        makeElement('strong', '', `${firstItem}–${lastItem}`),
        document.createTextNode(` trong ${filtered.length} sản phẩm`)
    );

    elements.productList.className =
        `product-list ${state.viewMode}-view`;

    elements.productList.replaceChildren(
        ...pageItems.map(createProductCard)
    );

    elements.productList.hidden = filtered.length === 0;

    if (elements.emptyState) {
        elements.emptyState.hidden = filtered.length !== 0;
    }

    renderPagination(filtered.length, totalPages);

    document.querySelectorAll('.view-button').forEach((button) => {
        const active = button.dataset.view === state.viewMode;

        button.classList.toggle('active', active);
        button.setAttribute('aria-pressed', String(active));
    });

    updateCartCount();
}

/* =========================================================
   FILTER RESET
   ========================================================= */

function resetFilters() {
    state.searchQuery = '';
    state.selectedCategory = 'all';
    state.maxPrice = 50000000;
    state.stockOnly = false;
    state.dmxOnly = false;
    state.sortBy = 'bestseller';
    state.currentPage = 1;

    render();
}

/* =========================================================
   DEMO CART - LOCAL STORAGE
   ========================================================= */

function readCart() {
    try {
        const savedCart = JSON.parse(
            localStorage.getItem('stagefx-demo-cart') || '[]'
        );

        if (!Array.isArray(savedCart)) {
            throw new TypeError('Giỏ hàng không đúng định dạng.');
        }

        return savedCart;
    } catch (error) {
        console.error('Không thể đọc giỏ hàng demo:', error);
        return [];
    }
}

function updateCartCount() {
    const count = readCart().reduce((total, item) => {
        const quantity = Number(item.quantity);

        return total + (
            Number.isFinite(quantity) && quantity > 0
                ? quantity
                : 0
        );
    }, 0);

    if (elements.cartCount) {
        elements.cartCount.textContent = String(count);
    }

    window.dispatchEvent(new Event('stagefx:cart-change'));
}

function addToCart(product) {
    if (product.stock <= 0) {
        showToast('Thiết bị hiện hết hàng mẫu. Vui lòng liên hệ tư vấn.');
        return;
    }

    const cart = readCart();

    const existing = cart.find(
        (item) =>
            item.productId === product.id &&
            item.isRental !== true
    );

    if (existing) {
        existing.quantity = Number(existing.quantity || 0) + 1;
    } else {
        cart.push({
            productId: product.id,
            productName: product.name,
            sku: product.sku,
            productImage: product.image,
            quantity: 1,
            unitPrice: product.b2bPrice,
            isRental: false
        });
    }

    try {
        localStorage.setItem(
            'stagefx-demo-cart',
            JSON.stringify(cart)
        );
    } catch (error) {
        console.error('Không thể lưu giỏ hàng demo:', error);
        showToast('Không lưu được giỏ hàng trên trình duyệt này.');
        return;
    }

    updateCartCount();
    showToast(`Đã thêm ${product.name} vào giỏ hàng mẫu.`);
}

/* =========================================================
   CATALOG CSV EXPORT
   ========================================================= */

function downloadCatalog() {
    const headers = [
        'SKU',
        'Tên sản phẩm',
        'Danh mục',
        'Giá tham khảo (VND)',
        'Tồn kho mẫu',
        'DMX',
        'Công suất'
    ];

    const rows = products.map((product) => [
        product.sku,
        product.name,
        product.categoryLabel,
        product.b2bPrice,
        product.stock,
        product.dmxChannels,
        product.power
    ]);

    const csv = [headers, ...rows]
        .map((row) =>
            row.map((value) =>
                `"${String(value).replace(/"/g, '""')}"`
            ).join(',')
        )
        .join('\r\n');

    const blob = new Blob(
        ['\uFEFF', csv],
        { type: 'text/csv;charset=utf-8' }
    );

    const downloadUrl = URL.createObjectURL(blob);
    const link = document.createElement('a');

    link.href = downloadUrl;
    link.download = 'stagefx-catalog-mau.csv';

    document.body.append(link);
    link.click();
    link.remove();

    URL.revokeObjectURL(downloadUrl);
}

/* =========================================================
   EVENT LISTENERS
   ========================================================= */

if (elements.searchForm) {
    elements.searchForm.addEventListener('submit', (event) => {
        event.preventDefault();

        state.searchQuery = elements.searchInput.value;
        state.currentPage = 1;

        render();
    });
}

if (elements.searchInput) {
    elements.searchInput.addEventListener('input', () => {
        state.searchQuery = elements.searchInput.value;
        state.currentPage = 1;

        render();
    });
}

if (elements.clearSearch) {
    elements.clearSearch.addEventListener('click', () => {
        state.searchQuery = '';
        state.currentPage = 1;

        render();
        elements.searchInput?.focus();
    });
}

document.querySelectorAll('[data-query]').forEach((button) => {
    button.addEventListener('click', () => {
        state.searchQuery = button.dataset.query || '';
        state.currentPage = 1;

        render();
        elements.searchInput?.focus();
    });
});

document.querySelectorAll('input[name="category"]').forEach((input) => {
    input.addEventListener('change', () => {
        state.selectedCategory = input.value;
        state.currentPage = 1;

        render();
    });
});

if (elements.priceRange) {
    elements.priceRange.addEventListener('input', () => {
        state.maxPrice = Number(elements.priceRange.value);
        state.currentPage = 1;

        render();
    });
}

document.querySelectorAll('[data-max-price]').forEach((button) => {
    button.addEventListener('click', () => {
        state.maxPrice = Number(button.dataset.maxPrice);
        state.currentPage = 1;

        render();
    });
});

if (elements.stockOnly) {
    elements.stockOnly.addEventListener('change', () => {
        state.stockOnly = elements.stockOnly.checked;
        state.currentPage = 1;

        render();
    });
}

if (elements.dmxOnly) {
    elements.dmxOnly.addEventListener('change', () => {
        state.dmxOnly = elements.dmxOnly.checked;
        state.currentPage = 1;

        render();
    });
}

if (elements.sortSelect) {
    elements.sortSelect.addEventListener('change', () => {
        state.sortBy = elements.sortSelect.value;
        state.currentPage = 1;

        render();
    });
}

document.querySelectorAll('.view-button').forEach((button) => {
    button.addEventListener('click', () => {
        state.viewMode = button.dataset.view;
        render();
    });
});

document.querySelector('#reset-filters')?.addEventListener(
    'click',
    resetFilters
);

document.querySelector('#empty-reset')?.addEventListener(
    'click',
    resetFilters
);

document.querySelector('#download-catalog')?.addEventListener(
    'click',
    downloadCatalog
);

/* =========================================================
   INITIALIZE
   ========================================================= */

render();

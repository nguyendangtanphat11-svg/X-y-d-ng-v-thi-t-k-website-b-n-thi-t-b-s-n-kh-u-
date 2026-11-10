
"use strict";

/* =========================================================
   STAGEFX PRO - CATALOG
   ASP.NET MVC 5 - SQL SERVER
   Giỏ hàng được lưu thông qua CartController
   ========================================================= */

/* =========================================================
   DATA & CONFIGURATION
   ========================================================= */

function normalizeProduct(raw) {
    const specifications = String(raw.specifications || "");
    const dmxSupported = /dmx/i.test(specifications);

    return {
        id: Number(raw.id),
        sku: raw.sku || "",
        name: raw.name || "Sản phẩm chưa có tên",
        categoryId: String(raw.categoryId ?? ""),
        category: String(raw.categoryId ?? ""),
        categoryLabel: raw.categoryLabel || "Chưa phân loại",
        description: raw.description || "",
        detail: raw.detail || "",
        specifications,
        image: raw.image || "",
        b2bPrice: Number(raw.b2bPrice || 0),
        stock: Number(raw.stock || 0),
        dmxChannels: dmxSupported
            ? "Có thông tin DMX512"
            : "Xem thông số kỹ thuật",
        power: "Xem thông số kỹ thuật",
        reviewCount: 0,
        searchTerms: specifications
    };
}

const products = Array.isArray(window.STAGEFX_PRODUCTS)
    ? window.STAGEFX_PRODUCTS.map(normalizeProduct)
    : [];

const itemsPerPage = 6;

const money = new Intl.NumberFormat("vi-VN", {
    style: "currency",
    currency: "VND",
    maximumFractionDigits: 0
});

const maximumProductPrice = products.reduce(
    (max, product) => Math.max(max, product.b2bPrice),
    0
);

const defaultMaxPrice = Math.max(50000000, maximumProductPrice);

/* =========================================================
   STATE
   ========================================================= */

const state = {
    searchQuery: new URLSearchParams(window.location.search).get("q") || "",
    selectedCategory: "all",
    maxPrice: defaultMaxPrice,
    stockOnly: false,
    dmxOnly: false,
    sortBy: "bestseller",
    viewMode: "grid",
    currentPage: 1
};

/* =========================================================
   DOM ELEMENTS
   ========================================================= */

const elements = {
    searchForm: document.querySelector("#search-form"),
    searchInput: document.querySelector("#search-input"),
    clearSearch: document.querySelector("#clear-search"),

    skuCount: document.querySelector("#sku-count"),
    priceRange: document.querySelector("#price-range"),
    priceOutput: document.querySelector("#price-output"),

    stockOnly: document.querySelector("#stock-only"),
    dmxOnly: document.querySelector("#dmx-only"),
    sortSelect: document.querySelector("#sort-select"),

    productList: document.querySelector("#product-list"),
    resultsSummary: document.querySelector("#results-summary"),
    emptyState: document.querySelector("#empty-state"),
    pagination: document.querySelector("#pagination"),

    dialog: document.querySelector("#product-dialog"),
    dialogContent: document.querySelector("#dialog-content"),
    toast: document.querySelector("#toast"),

    // Form gửi sản phẩm vào giỏ hàng SQL Server
    cartForm: document.querySelector("#catalog-cart-form"),
    cartProductId: document.querySelector("#catalog-product-id"),
    cartQuantity: document.querySelector("#catalog-quantity"),
    cartOrderType: document.querySelector("#catalog-order-type"),
    cartRentalDays: document.querySelector("#catalog-rental-days"),
    cartFlightcase: document.querySelector("#catalog-flightcase")
};

/* =========================================================
   UTILITIES
   ========================================================= */

function normalizeText(value) {
    return String(value ?? "")
        .normalize("NFD")
        .replace(/[\u0300-\u036f]/g, "")
        .replace(/₂/g, "2")
        .replace(/[đĐ]/g, "d")
        .toLowerCase()
        .trim();
}

function makeElement(tag, className = "", text) {
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
    elements.toast.classList.add("visible");

    window.clearTimeout(showToast.timeoutId);

    showToast.timeoutId = window.setTimeout(() => {
        elements.toast.classList.remove("visible");
    }, 3000);
}

function getProductDetailUrl(productId) {
    const template = window.STAGEFX_PRODUCT_DETAILS_URL;

    if (typeof template === "string" && template.length > 0) {
        return template.replace(
            "__PRODUCT_ID__",
            encodeURIComponent(productId)
        );
    }

    return `/Product/Details/${encodeURIComponent(productId)}`;
}

function openProductDetails(product, detailUrl) {
    try {
        sessionStorage.setItem(
            "stagefx-selected-product",
            JSON.stringify(product)
        );
    } catch (error) {
        console.warn("Không lưu được dữ liệu sản phẩm:", error);
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
                product.detail,
                product.categoryLabel,
                product.specifications
            ].join(" "));

            if (
                query &&
                !query
                    .split(/\s+/)
                    .every((term) => searchableText.includes(term))
            ) {
                return false;
            }

            if (
                state.selectedCategory !== "all" &&
                product.categoryId !== state.selectedCategory
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
                !/dmx/i.test(product.specifications)
            ) {
                return false;
            }

            return true;
        })
        .sort((a, b) => {
            if (state.sortBy === "price-asc") {
                return a.b2bPrice - b.b2bPrice;
            }

            if (state.sortBy === "price-desc") {
                return b.b2bPrice - a.b2bPrice;
            }

            if (state.sortBy === "name-az") {
                return a.name.localeCompare(b.name, "vi");
            }

            // Giữ thứ tự do Controller trả về.
            return 0;
        });
}

/* =========================================================
   PRODUCT CARD
   ========================================================= */

function createStockBadge(product) {
    const badge = makeElement(
        "span",
        `stock-badge${product.stock <= 0 ? " out-of-stock" : ""}`
    );

    const dot = makeElement("i", "stock-dot");

    dot.setAttribute("aria-hidden", "true");

    badge.append(
        dot,
        document.createTextNode(
            product.stock > 0
                ? `CÒN ${product.stock}`
                : "HẾT HÀNG"
        )
    );

    return badge;
}

function createProductCard(product) {
    const card = makeElement("article", "product-card");

    const imageWrap = makeElement("div", "product-card-image-wrap");
    const image = makeElement("img", "product-card-image");

    if (product.image) {
        image.src = product.image;
    } else {
        image.hidden = true;
        imageWrap.classList.add("no-product-image");
    }

    image.alt = product.name;
    image.loading = "lazy";

    image.onerror = () => {
        image.hidden = true;
        imageWrap.classList.add("no-product-image");
    };

    const category = makeElement(
        "span",
        "image-category",
        product.categoryLabel
    );

    imageWrap.append(
        image,
        createStockBadge(product),
        category
    );

    const body = makeElement("div", "product-card-body");

    const meta = makeElement("div", "product-meta");
    meta.append(makeElement("span", "", product.sku));

    const title = makeElement("h2", "product-card-title");

    const detailUrl = getProductDetailUrl(product.id);

    const titleLink = makeElement(
        "a",
        "product-detail-link",
        product.name
    );

    titleLink.href = detailUrl;

    titleLink.addEventListener("click", (event) => {
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
        "p",
        "product-description",
        product.description
    );

    const specs = makeElement("div", "product-specs");

    specs.append(
        makeElement("span", "", `• ${product.dmxChannels}`),
        makeElement("span", "", `• ${product.power}`)
    );

    const price = makeElement("div", "product-price-row");

    price.append(
        makeElement("span", "", "Giá bán"),
        makeElement("strong", "", money.format(product.b2bPrice))
    );

    const actions = makeElement("div", "product-actions");

    // Nút xem chi tiết
    const detailButton = makeElement(
        "a",
        "catalog-detail-button",
        "Xem chi tiết"
    );

    detailButton.href = detailUrl;

    detailButton.addEventListener("click", (event) => {
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

    // Nút thêm giỏ hàng
    const addButton = makeElement("button", "add-cart-button");

    addButton.type = "button";

    const cartIcon = makeElement(
        "span",
        "material-symbols-outlined",
        "add_shopping_cart"
    );

    cartIcon.setAttribute("aria-hidden", "true");

    addButton.append(
        cartIcon,
        document.createTextNode("Thêm giỏ")
    );

    addButton.disabled = product.stock <= 0;

    if (product.stock <= 0) {
        addButton.title = "Sản phẩm hiện đã hết hàng";
    }

    addButton.addEventListener("click", () => {
        addToCart(product, addButton);
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

    document.querySelector("#catalog-results")?.scrollIntoView({
        behavior: "smooth",
        block: "start"
    });
}

function renderPagination(totalItems, totalPages) {
    if (!elements.pagination) {
        return;
    }

    elements.pagination.replaceChildren();
    elements.pagination.hidden = totalItems === 0;

    if (totalItems === 0) {
        return;
    }

    const start = (state.currentPage - 1) * itemsPerPage + 1;

    const end = Math.min(
        state.currentPage * itemsPerPage,
        totalItems
    );

    const summary = makeElement("p", "pagination-summary");

    summary.append(
        document.createTextNode("Hiển thị "),
        makeElement("strong", "", `${start}–${end}`),
        document.createTextNode(" / "),
        makeElement("strong", "", String(totalItems)),
        document.createTextNode(" thiết bị")
    );

    const buttons = makeElement("div", "pagination-buttons");

    // Trang trước
    const previous = makeElement("button");

    previous.type = "button";
    previous.disabled = state.currentPage <= 1;

    previous.setAttribute("aria-label", "Trang trước");

    previous.append(
        makeElement(
            "span",
            "material-symbols-outlined",
            "chevron_left"
        )
    );

    previous.addEventListener("click", () => {
        changePage(state.currentPage - 1);
    });

    buttons.append(previous);

    // Các trang
    for (let page = 1; page <= totalPages; page += 1) {
        const button = makeElement(
            "button",
            page === state.currentPage ? "current" : "",
            String(page)
        );

        button.type = "button";

        button.setAttribute(
            "aria-label",
            `Trang ${page}`
        );

        if (page === state.currentPage) {
            button.setAttribute("aria-current", "page");
        }

        button.addEventListener("click", () => {
            changePage(page);
        });

        buttons.append(button);
    }

    // Trang tiếp theo
    const next = makeElement("button");

    next.type = "button";
    next.disabled = state.currentPage >= totalPages;

    next.setAttribute("aria-label", "Trang tiếp theo");

    next.append(
        makeElement(
            "span",
            "material-symbols-outlined",
            "chevron_right"
        )
    );

    next.addEventListener("click", () => {
        changePage(state.currentPage + 1);
    });

    buttons.append(next);

    elements.pagination.append(summary, buttons);
}

/* =========================================================
   RENDER CATALOG
   ========================================================= */

function render() {
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
        elements.clearSearch.hidden =
            state.searchQuery.length === 0;
    }

    if (elements.skuCount) {
        elements.skuCount.textContent =
            `${filtered.length} SKU`;
    }

    if (elements.priceRange) {
        elements.priceRange.max = String(defaultMaxPrice);

        elements.priceRange.value = String(
            Math.min(state.maxPrice, defaultMaxPrice)
        );
    }

    if (elements.priceOutput) {
        elements.priceOutput.textContent =
            money.format(state.maxPrice);
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

    document.querySelectorAll('input[name="category"]').forEach((input) => {
        input.checked =
            input.value === state.selectedCategory;
    });

    const firstItem = filtered.length === 0
        ? 0
        : startIndex + 1;

    const lastItem = Math.min(
        startIndex + itemsPerPage,
        filtered.length
    );

    elements.resultsSummary.replaceChildren(
        document.createTextNode("Đang hiển thị "),
        makeElement("strong", "", `${firstItem}–${lastItem}`),
        document.createTextNode(
            ` trong ${filtered.length} sản phẩm`
        )
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

    document.querySelectorAll(".view-button").forEach((button) => {
        const active =
            button.dataset.view === state.viewMode;

        button.classList.toggle("active", active);

        button.setAttribute(
            "aria-pressed",
            String(active)
        );
    });
}

/* =========================================================
   RESET FILTERS
   ========================================================= */

function resetFilters() {
    state.searchQuery = "";
    state.selectedCategory = "all";
    state.maxPrice = defaultMaxPrice;
    state.stockOnly = false;
    state.dmxOnly = false;
    state.sortBy = "bestseller";
    state.currentPage = 1;

    render();
}

/* =========================================================
   CART - SAVE TO SQL SERVER
   ========================================================= */

function addToCart(product, button) {
    // Kiểm tra tồn kho
    if (product.stock <= 0) {
        showToast("Sản phẩm hiện đã hết hàng.");
        return;
    }

    // Kiểm tra mã sản phẩm
    if (
        !Number.isInteger(product.id) ||
        product.id <= 0
    ) {
        showToast("Mã sản phẩm không hợp lệ.");

        console.error(
            "Mã sản phẩm không hợp lệ:",
            product.id
        );

        return;
    }

    // Kiểm tra form POST
    const requiredElements = [
        elements.cartForm,
        elements.cartProductId,
        elements.cartQuantity,
        elements.cartOrderType,
        elements.cartRentalDays,
        elements.cartFlightcase
    ];

    if (requiredElements.some((element) => !element)) {
        showToast(
            "Không tìm thấy form giỏ hàng. Vui lòng tải lại trang."
        );

        console.error(
            "Thiếu form hoặc input gửi dữ liệu giỏ hàng.",
            elements
        );

        return;
    }

    // Kiểm tra form có AntiForgeryToken
    const token = elements.cartForm.querySelector(
        'input[name="__RequestVerificationToken"]'
    );

    if (!token || !token.value) {
        showToast(
            "Không tìm thấy mã bảo mật. Vui lòng tải lại trang."
        );

        console.error(
            "Thiếu AntiForgeryToken trong catalog-cart-form."
        );

        return;
    }

    // Gán dữ liệu gửi lên CartController
    elements.cartProductId.value = String(product.id);
    elements.cartQuantity.value = "1";
    elements.cartOrderType.value = "Mua";
    elements.cartRentalDays.value = "1";
    elements.cartFlightcase.value = "false";

    // Chống bấm nhiều lần
    if (button) {
        button.disabled = true;
        button.dataset.originalText =
            button.textContent.trim();

        button.textContent = "Đang xử lý...";
    }

    // Gửi POST đến CartController.Add()
    try {
        elements.cartForm.submit();
    } catch (error) {
        console.error(
            "Lỗi gửi sản phẩm vào giỏ hàng:",
            error
        );

        if (button) {
            button.disabled = false;

            button.textContent =
                button.dataset.originalText || "Thêm giỏ";
        }

        showToast(
            "Không thể gửi yêu cầu thêm sản phẩm vào giỏ hàng."
        );
    }
}

/* =========================================================
   CSV EXPORT - DATABASE PRODUCTS
   ========================================================= */

function downloadCatalog() {
    if (products.length === 0) {
        showToast("Chưa có sản phẩm để xuất danh mục.");
        return;
    }

    const headers = [
        "Mã sản phẩm",
        "SKU",
        "Tên sản phẩm",
        "Danh mục",
        "Giá bán (VND)",
        "Số lượng tồn",
        "Thông số kỹ thuật"
    ];

    const rows = products.map((product) => [
        product.id,
        product.sku,
        product.name,
        product.categoryLabel,
        product.b2bPrice,
        product.stock,
        product.specifications
    ]);

    const csv = [headers, ...rows]
        .map((row) =>
            row.map((value) =>
                `"${String(value ?? "").replace(/"/g, '""')}"`
            ).join(",")
        )
        .join("\r\n");

    const blob = new Blob(
        ["\uFEFF", csv],
        { type: "text/csv;charset=utf-8" }
    );

    const downloadUrl = URL.createObjectURL(blob);

    const link = document.createElement("a");

    link.href = downloadUrl;
    link.download = "stagefx-catalog.csv";

    document.body.append(link);
    link.click();
    link.remove();

    URL.revokeObjectURL(downloadUrl);
}

/* =========================================================
   EVENTS
   ========================================================= */

// Tìm kiếm
if (elements.searchForm) {
    elements.searchForm.addEventListener("submit", (event) => {
        event.preventDefault();

        state.searchQuery = elements.searchInput.value;
        state.currentPage = 1;

        render();
    });
}

if (elements.searchInput) {
    elements.searchInput.addEventListener("input", () => {
        state.searchQuery = elements.searchInput.value;
        state.currentPage = 1;

        render();
    });
}

// Xóa tìm kiếm
if (elements.clearSearch) {
    elements.clearSearch.addEventListener("click", () => {
        state.searchQuery = "";
        state.currentPage = 1;

        render();

        elements.searchInput?.focus();
    });
}

// Tìm kiếm nhanh
document.querySelectorAll("[data-query]").forEach((button) => {
    button.addEventListener("click", () => {
        state.searchQuery = button.dataset.query || "";
        state.currentPage = 1;

        render();

        elements.searchInput?.focus();
    });
});

// Lọc danh mục
document.querySelectorAll('input[name="category"]').forEach((input) => {
    input.addEventListener("change", () => {
        state.selectedCategory = input.value;
        state.currentPage = 1;

        render();
    });
});

// Lọc giá
if (elements.priceRange) {
    elements.priceRange.addEventListener("input", () => {
        state.maxPrice = Number(elements.priceRange.value);
        state.currentPage = 1;

        render();
    });
}

// Lọc giá nhanh
document.querySelectorAll("[data-max-price]").forEach((button) => {
    button.addEventListener("click", () => {
        state.maxPrice = Number(button.dataset.maxPrice);
        state.currentPage = 1;

        render();
    });
});

// Chỉ hiện sản phẩm còn hàng
if (elements.stockOnly) {
    elements.stockOnly.addEventListener("change", () => {
        state.stockOnly = elements.stockOnly.checked;
        state.currentPage = 1;

        render();
    });
}

// Lọc sản phẩm hỗ trợ DMX
if (elements.dmxOnly) {
    elements.dmxOnly.addEventListener("change", () => {
        state.dmxOnly = elements.dmxOnly.checked;
        state.currentPage = 1;

        render();
    });
}

// Sắp xếp sản phẩm
if (elements.sortSelect) {
    elements.sortSelect.addEventListener("change", () => {
        state.sortBy = elements.sortSelect.value;
        state.currentPage = 1;

        render();
    });
}

// Chuyển chế độ hiển thị Grid/List
document.querySelectorAll(".view-button").forEach((button) => {
    button.addEventListener("click", () => {
        state.viewMode = button.dataset.view || "grid";

        render();
    });
});

// Đặt lại bộ lọc
document.querySelector("#reset-filters")?.addEventListener(
    "click",
    resetFilters
);

document.querySelector("#empty-reset")?.addEventListener(
    "click",
    resetFilters
);

// Xuất CSV
document.querySelector("#download-catalog")?.addEventListener(
    "click",
    downloadCatalog
);

/* =========================================================
   INITIALIZE
   ========================================================= */

render();

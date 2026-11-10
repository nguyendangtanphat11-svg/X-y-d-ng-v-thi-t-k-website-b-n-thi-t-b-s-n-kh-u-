
document.addEventListener("DOMContentLoaded", function () {
    const modal = document.getElementById("inventoryModal");
    const form = document.getElementById("inventoryTransactionForm");

    if (!modal || !form) return;

    const productId = document.getElementById("inventoryProductId");
    const productName = document.getElementById("inventoryProductName");
    const currentStock = document.getElementById("inventoryCurrentStock");
    const quantity = document.getElementById("inventoryQuantity");
    const quantityLabel = document.getElementById("inventoryQuantityLabel");
    const note = document.getElementById("inventoryNote");
    const title = document.getElementById("inventoryModalTitle");

    const submitButton = document.getElementById("inventorySubmit");

    const routes = {
        import: "/AdminInventory/ImportStock",
        export: "/AdminInventory/ExportStock",
        adjust: "/AdminInventory/AdjustStock"
    };

    function closeModal() {
        modal.classList.remove("is-open");
        modal.setAttribute("aria-hidden", "true");
        form.reset();
    }

    function openModal(button) {
        const action = button.dataset.action;
        const id = button.dataset.id;
        const name = button.dataset.name;
        const stock = Number(button.dataset.stock);

        if (!routes[action] || !id || !Number.isFinite(stock)) {
            return;
        }

        form.action = routes[action];

        productId.value = id;
        productName.value = name;
        currentStock.value = stock.toLocaleString("vi-VN");

        quantity.value = "";
        quantity.min = action === "adjust" ? "0" : "1";

        if (action === "import") {
            title.textContent = "Nhập kho";
            quantityLabel.textContent = "Số lượng nhập";
            quantity.name = "quantity";
            submitButton.textContent = "Xác nhận nhập kho";
        } else if (action === "export") {
            title.textContent = "Xuất kho";
            quantityLabel.textContent = "Số lượng xuất";
            quantity.name = "quantity";
            submitButton.textContent = "Xác nhận xuất kho";
        } else {
            title.textContent = "Điều chỉnh tồn kho";
            quantityLabel.textContent = "Số lượng tồn thực tế";
            quantity.name = "actualQuantity";
            quantity.value = stock;
            submitButton.textContent = "Xác nhận điều chỉnh";
        }

        modal.classList.add("is-open");
        modal.setAttribute("aria-hidden", "false");
        quantity.focus();
    }

    document.querySelectorAll("[data-action]").forEach(function (button) {
        button.addEventListener("click", function () {
            openModal(button);
        });
    });

    modal.querySelectorAll("[data-close-modal]").forEach(function (button) {
        button.addEventListener("click", closeModal);
    });

    document.addEventListener("keydown", function (event) {
        if (event.key === "Escape" && modal.classList.contains("is-open")) {
            closeModal();
        }
    });

    form.addEventListener("submit", function (event) {
        const value = Number(quantity.value);

        if (!Number.isInteger(value) || value < Number(quantity.min)) {
            event.preventDefault();
            alert("Vui lòng nhập số lượng nguyên hợp lệ.");
            quantity.focus();
            return;
        }

        const action = form.action;

        if (action.endsWith("/ExportStock")) {
            const available = Number(
                currentStock.value.replace(/[^\d]/g, "")
            );

            if (value > available) {
                event.preventDefault();
                alert("Số lượng xuất vượt quá tồn kho.");
                quantity.focus();
                return;
            }
        }

        const confirmed = window.confirm(
            "Bạn có chắc chắn muốn thực hiện giao dịch kho này?"
        );

        if (!confirmed) {
            event.preventDefault();
            return;
        }

        submitButton.disabled = true;
        submitButton.textContent = "Đang xử lý...";
    });
});


document.addEventListener("DOMContentLoaded", function () {

    // ==========================================
    // 1. XÁC NHẬN TRƯỚC KHI XÓA DANH MỤC
    // ==========================================
    document.querySelectorAll(".delete-category-form").forEach(function (form) {
        form.addEventListener("submit", function (event) {
            const confirmed = window.confirm(
                "Bạn có chắc chắn muốn xóa danh mục này không?\n\n" +
                "Lưu ý: Danh mục đang có sản phẩm sẽ không thể bị xóa."
            );

            if (!confirmed) {
                event.preventDefault();
            }
        });
    });


    // ==========================================
    // 2. XÁC NHẬN THAY ĐỔI TRẠNG THÁI
    // ==========================================
    document.querySelectorAll(
        'form.category-inline-form:not(.delete-category-form)'
    ).forEach(function (form) {
        form.addEventListener("submit", function (event) {
            const button = form.querySelector("button");

            const icon = button
                ? button.querySelector(".material-symbols-outlined")
                : null;

            const isActive = icon &&
                icon.textContent.trim() === "toggle_on";

            const message = isActive
                ? "Bạn có muốn ngừng hoạt động danh mục này không?"
                : "Bạn có muốn kích hoạt danh mục này không?";

            if (!window.confirm(message)) {
                event.preventDefault();
            }
        });
    });


    // ==========================================
    // 3. XEM TRƯỚC ẢNH DANH MỤC
    // ==========================================
    const imageInput = document.getElementById("AnhUpload");
    const imagePreview = document.getElementById("categoryImagePreview");
    const fileName = document.getElementById("categoryImageFileName");
    const imageError = document.getElementById("categoryImageError");

    if (imageInput && imagePreview) {

        const allowedTypes = [
            "image/jpeg",
            "image/png",
            "image/webp"
        ];

        const maxFileSize = 5 * 1024 * 1024;

        let currentPreviewUrl = null;

        imageInput.addEventListener("change", function () {

            const file = this.files && this.files[0];

            if (imageError) {
                imageError.textContent = "";
                imageError.style.display = "none";
            }

            if (!file) {
                if (fileName) {
                    fileName.textContent = "";
                }

                return;
            }

            // Kiểm tra định dạng
            if (!allowedTypes.includes(file.type)) {
                showError("Vui lòng chọn ảnh JPG, JPEG, PNG hoặc WEBP.");
                this.value = "";
                return;
            }

            // Kiểm tra dung lượng
            if (file.size > maxFileSize) {
                showError("Ảnh không được vượt quá 5MB.");
                this.value = "";
                return;
            }

            // Giải phóng URL xem trước cũ
            if (currentPreviewUrl) {
                URL.revokeObjectURL(currentPreviewUrl);
            }

            // Tạo URL ảnh mới
            currentPreviewUrl = URL.createObjectURL(file);

            // Hiển thị ảnh
            imagePreview.innerHTML = "";

            const image = document.createElement("img");
            image.src = currentPreviewUrl;
            image.alt = "Ảnh xem trước danh mục";
            image.id = "categoryPreviewImage";

            imagePreview.appendChild(image);

            // Hiển thị tên file
            if (fileName) {
                fileName.textContent = file.name;
            }
        });

        function showError(message) {
            if (imageError) {
                imageError.textContent = message;
                imageError.style.display = "block";
            } else {
                window.alert(message);
            }

            if (fileName) {
                fileName.textContent = "";
            }
        }

        // Giải phóng bộ nhớ khi rời trang
        window.addEventListener("beforeunload", function () {
            if (currentPreviewUrl) {
                URL.revokeObjectURL(currentPreviewUrl);
            }
        });
    }

});

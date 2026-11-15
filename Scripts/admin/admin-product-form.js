
document.addEventListener('DOMContentLoaded', function () {
    'use strict';

    const MAX_FILE_SIZE = 5 * 1024 * 1024;
    const MAX_FILES = 10;
    const ALLOWED_EXTENSIONS = ['.jpg', '.jpeg', '.png', '.webp'];
    const ALLOWED_MIME_TYPES = ['image/jpeg', 'image/png', 'image/webp'];

    const forms = document.querySelectorAll(
        '.product-create-form, .product-edit-form'
    );

    forms.forEach(function (form) {
        const fileInput = form.querySelector('#NewImages');
        const preview = form.querySelector('#productImagePreview');
        const mainImageIndex = form.querySelector('#MainImageNewIndex');

        if (!fileInput) return;

        let previewUrls = [];

        // Giải phóng URL ảnh preview cũ
        function clearPreview() {
            previewUrls.forEach(function (url) {
                URL.revokeObjectURL(url);
            });

            previewUrls = [];

            if (preview) {
                preview.innerHTML = '';
            }

            if (mainImageIndex) {
                mainImageIndex.value = '';
            }
        }

        // Lấy phần mở rộng của file
        function getExtension(fileName) {
            const index = (fileName || '').lastIndexOf('.');

            return index >= 0
                ? fileName.substring(index).toLowerCase()
                : '';
        }

        // Kiểm tra file ảnh
        function validateFile(file) {
            if (!file || file.size <= 0) {
                return 'Tệp ảnh không hợp lệ.';
            }

            if (file.size > MAX_FILE_SIZE) {
                return 'Mỗi ảnh không được vượt quá 5 MB.';
            }

            if (!ALLOWED_EXTENSIONS.includes(getExtension(file.name))) {
                return 'Chỉ hỗ trợ JPG, JPEG, PNG và WEBP.';
            }

            if (
                file.type &&
                !ALLOWED_MIME_TYPES.includes(file.type.toLowerCase())
            ) {
                return 'Định dạng nội dung ảnh không hợp lệ.';
            }

            return null;
        }

        // Hiển thị preview ảnh mới
        function renderPreview(files) {
            clearPreview();

            if (!preview) return;

            files.forEach(function (file, index) {
                const card = document.createElement('div');
                card.className = 'new-image-preview';

                const image = document.createElement('img');
                image.alt = file.name;
                image.loading = 'lazy';

                const url = URL.createObjectURL(file);
                previewUrls.push(url);
                image.src = url;

                const info = document.createElement('div');
                info.className = 'new-image-preview-info';

                const name = document.createElement('span');
                name.className = 'new-image-name';
                name.textContent = file.name;

                const size = document.createElement('small');
                size.textContent =
                    (file.size / (1024 * 1024)).toFixed(2) + ' MB';

                const label = document.createElement('label');
                label.className = 'new-image-main-option';

                const radio = document.createElement('input');
                radio.type = 'radio';
                radio.name = 'MainImageNewIndexSelector';
                radio.value = String(index);

                radio.addEventListener('change', function () {
                    if (!radio.checked) return;

                    if (mainImageIndex) {
                        mainImageIndex.value = String(index);
                    }

                    form.querySelectorAll('input[name="MainImageId"]')
                        .forEach(function (item) {
                            item.checked = false;
                        });

                    preview.querySelectorAll('.new-image-preview')
                        .forEach(function (item) {
                            item.classList.remove('is-main');
                        });

                    card.classList.add('is-main');
                });

                const text = document.createElement('span');
                text.textContent = 'Chọn làm ảnh chính';

                label.appendChild(radio);
                label.appendChild(text);

                info.appendChild(name);
                info.appendChild(size);
                info.appendChild(label);

                card.appendChild(image);
                card.appendChild(info);

                preview.appendChild(card);
            });
        }

        // Khi người dùng chọn ảnh mới
        fileInput.addEventListener('change', function () {
            const files = Array.from(fileInput.files || []);

            if (files.length > MAX_FILES) {
                window.alert('Chỉ được chọn tối đa 10 ảnh.');
                fileInput.value = '';
                clearPreview();
                return;
            }

            for (const file of files) {
                const error = validateFile(file);

                if (error) {
                    window.alert(file.name + '\n' + error);
                    fileInput.value = '';
                    clearPreview();
                    return;
                }
            }

            renderPreview(files);
        });

        // Chọn ảnh cũ làm ảnh chính
        form.querySelectorAll('input[name="MainImageId"]')
            .forEach(function (radio) {
                radio.addEventListener('change', function () {
                    if (!radio.checked) return;

                    if (mainImageIndex) {
                        mainImageIndex.value = '';
                    }

                    if (preview) {
                        preview.querySelectorAll('input[type="radio"]')
                            .forEach(function (item) {
                                item.checked = false;
                            });

                        preview.querySelectorAll('.new-image-preview')
                            .forEach(function (item) {
                                item.classList.remove('is-main');
                            });
                    }
                });
            });

        // Kiểm tra form trước khi gửi
        form.addEventListener('submit', function (event) {
            if (!form.checkValidity()) {
                event.preventDefault();
                form.reportValidity();
                return;
            }

            form.querySelectorAll('.product-form-submit')
                .forEach(function (button) {
                    button.disabled = true;
                    button.textContent = 'Đang lưu...';
                });
        });

        // Giải phóng URL ảnh tạm khi rời trang
        window.addEventListener('pagehide', clearPreview);
    });
});

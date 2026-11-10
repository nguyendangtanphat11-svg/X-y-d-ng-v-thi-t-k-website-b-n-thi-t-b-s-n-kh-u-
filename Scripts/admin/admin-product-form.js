
document.addEventListener('DOMContentLoaded', function () {
    'use strict';

    const MAX_FILE_SIZE = 5 * 1024 * 1024;
    const MAX_FILES = 10;

    const ALLOWED_EXTENSIONS = [
        '.jpg', '.jpeg', '.png', '.webp'
    ];

    document.querySelectorAll(
        '.product-create-form, .product-edit-form'
    ).forEach(function (form) {

        const fileInput = form.querySelector('#NewImages');
        const preview = form.querySelector('#productImagePreview');
        const mainImageIndex = form.querySelector('#MainImageNewIndex');

        if (!fileInput) return;

        let objectUrls = [];

        function clearPreview() {
            objectUrls.forEach(function (url) {
                URL.revokeObjectURL(url);
            });

            objectUrls = [];

            if (preview) {
                preview.replaceChildren();
            }

            if (mainImageIndex) {
                mainImageIndex.value = '';
            }
        }

        function getExtension(name) {
            const index = name.lastIndexOf('.');
            return index >= 0
                ? name.substring(index).toLowerCase()
                : '';
        }

        function validate(file) {
            if (!file || file.size <= 0) {
                return 'Tệp ảnh không hợp lệ.';
            }

            if (file.size > MAX_FILE_SIZE) {
                return 'Mỗi ảnh không được vượt quá 5 MB.';
            }

            if (!ALLOWED_EXTENSIONS.includes(
                getExtension(file.name)
            )) {
                return 'Chỉ hỗ trợ JPG, JPEG, PNG và WEBP.';
            }

            return null;
        }

        function render(files) {
            if (!preview) return;

            clearPreview();

            files.forEach(function (file, index) {
                const card = document.createElement('div');
                card.className = 'new-image-preview';

                const image = document.createElement('img');
                image.alt = file.name;
                image.loading = 'lazy';

                const url = URL.createObjectURL(file);
                objectUrls.push(url);

                image.src = url;

                const info = document.createElement('div');
                info.className = 'new-image-preview-info';

                const name = document.createElement('span');
                name.className = 'new-image-name';
                name.textContent = file.name;

                const size = document.createElement('small');
                size.textContent =
                    (file.size / 1048576).toFixed(2) + ' MB';

                const label = document.createElement('label');
                label.className = 'new-image-main-option';

                const radio = document.createElement('input');
                radio.type = 'radio';
                radio.name = 'MainImageNewIndexSelector';
                radio.value = index;

                radio.addEventListener('change', function () {
                    if (!radio.checked) return;

                    if (mainImageIndex) {
                        mainImageIndex.value = String(index);
                    }

                    form.querySelectorAll(
                        'input[name="MainImageId"]'
                    ).forEach(function (item) {
                        item.checked = false;
                    });

                    preview.querySelectorAll(
                        '.new-image-preview'
                    ).forEach(function (item) {
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

        fileInput.addEventListener('change', function () {
            const files = Array.from(fileInput.files || []);

            clearPreview();

            if (!files.length) return;

            if (files.length > MAX_FILES) {
                alert('Chỉ được chọn tối đa 10 ảnh.');
                fileInput.value = '';
                return;
            }

            for (const file of files) {
                const error = validate(file);

                if (error) {
                    alert(file.name + '\n' + error);
                    fileInput.value = '';
                    clearPreview();
                    return;
                }
            }

            render(files);
        });

        form.querySelectorAll(
            'input[name="MainImageId"]'
        ).forEach(function (radio) {
            radio.addEventListener('change', function () {
                if (radio.checked && mainImageIndex) {
                    mainImageIndex.value = '';
                }

                if (preview) {
                    preview.querySelectorAll(
                        '.new-image-preview'
                    ).forEach(function (card) {
                        card.classList.remove('is-main');
                    });

                    preview.querySelectorAll(
                        'input[type="radio"]'
                    ).forEach(function (item) {
                        item.checked = false;
                    });
                }
            });
        });

        form.addEventListener('submit', function (event) {
            if (!form.checkValidity()) {
                event.preventDefault();
                form.reportValidity();
                return;
            }

            const buttons = form.querySelectorAll(
                '.product-form-submit'
            );

            buttons.forEach(function (button) {
                button.disabled = true;
                button.textContent = 'Đang lưu...';
            });
        });

        window.addEventListener('pagehide', clearPreview);
    });
});


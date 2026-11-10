
document.addEventListener('DOMContentLoaded', function () {
    'use strict';

    // =====================================================
    // 1. CONFIGURATION
    // =====================================================

    const MAX_FILE_SIZE = 5 * 1024 * 1024;
    const MAX_FILES = 10;

    const ALLOWED_EXTENSIONS = [
        '.jpg',
        '.jpeg',
        '.png',
        '.webp'
    ];

    const ALLOWED_MIME_TYPES = [
        'image/jpeg',
        'image/png',
        'image/webp'
    ];

    // Giới hạn kích thước ảnh để hạn chế ảnh có độ phân giải
    // quá lớn gây áp lực bộ nhớ khi trình duyệt hiển thị preview.
    const MAX_IMAGE_WIDTH = 8000;
    const MAX_IMAGE_HEIGHT = 8000;
    const MAX_IMAGE_PIXELS = 40000000;

    // =====================================================
    // 2. ELEMENTS
    // =====================================================

    const forms = document.querySelectorAll(
        '.product-create-form, .product-edit-form'
    );

    if (!forms.length) {
        return;
    }

    // =====================================================
    // 3. FILE VALIDATION
    // =====================================================

    function validateFile(file) {
        if (!file) {
            return 'Không tìm thấy tệp ảnh.';
        }

        if (file.size <= 0) {
            return 'Tệp ảnh bị rỗng.';
        }

        if (file.size > MAX_FILE_SIZE) {
            return 'Dung lượng ảnh không được vượt quá 5 MB.';
        }

        const fileName = file.name || '';
        const extension = fileName.includes('.')
            ? fileName.substring(fileName.lastIndexOf('.')).toLowerCase()
            : '';

        if (!ALLOWED_EXTENSIONS.includes(extension)) {
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

    // =====================================================
    // 4. IMAGE DIMENSION VALIDATION
    // =====================================================

    function validateImageDimensions(file) {
        return new Promise(function (resolve) {
            const objectUrl = URL.createObjectURL(file);
            const image = new Image();

            let completed = false;

            function finish(result) {
                if (completed) return;

                completed = true;

                image.onload = null;
                image.onerror = null;

                URL.revokeObjectURL(objectUrl);

                resolve(result);
            }

            image.onload = function () {
                const width = image.naturalWidth;
                const height = image.naturalHeight;
                const pixels = width * height;

                if (
                    width <= 0 ||
                    height <= 0 ||
                    width > MAX_IMAGE_WIDTH ||
                    height > MAX_IMAGE_HEIGHT ||
                    pixels > MAX_IMAGE_PIXELS
                ) {
                    finish(
                        'Kích thước ảnh quá lớn. Vui lòng chọn ảnh có kích thước tối đa 8000 x 8000 pixel và không vượt quá 40 megapixel.'
                    );

                    return;
                }

                finish(null);
            };

            image.onerror = function () {
                finish('Không thể đọc ảnh. Tệp có thể bị lỗi hoặc không phải ảnh hợp lệ.');
            };

            image.src = objectUrl;
        });
    }

    // =====================================================
    // 5. PREVIEW MANAGEMENT
    // =====================================================

    function createImagePreviewManager(container, hiddenInput) {
        let previewUrls = [];
        let currentFiles = [];

        function revokePreviewUrls() {
            previewUrls.forEach(function (url) {
                try {
                    URL.revokeObjectURL(url);
                } catch (error) {
                    console.warn(
                        '[STAGEFX] Không thể giải phóng preview URL:',
                        error
                    );
                }
            });

            previewUrls = [];
        }

        function clearPreview() {
            revokePreviewUrls();

            if (container) {
                container.innerHTML = '';
            }

            currentFiles = [];

            if (hiddenInput) {
                hiddenInput.value = '';
            }
        }

        function clearNewMainImage() {
            if (hiddenInput) {
                hiddenInput.value = '';
            }

            if (!container) return;

            container
                .querySelectorAll('.new-image-preview')
                .forEach(function (card) {
                    card.classList.remove('is-main');

                    const radio = card.querySelector(
                        'input[type="radio"]'
                    );

                    if (radio) {
                        radio.checked = false;
                    }
                });
        }

        function clearExistingMainImage() {
            document.querySelectorAll(
                'input[name="MainImageId"]'
            ).forEach(function (radio) {
                radio.checked = false;
            });
        }

        function selectMainImage(index, card) {
            if (hiddenInput) {
                hiddenInput.value = String(index);
            }

            clearExistingMainImage();

            if (!container) return;

            container
                .querySelectorAll('.new-image-preview')
                .forEach(function (item) {
                    item.classList.remove('is-main');
                });

            card.classList.add('is-main');
        }

        function renderPreview(files) {
            if (!container) return;

            clearPreview();

            currentFiles = files.slice();

            files.forEach(function (file, index) {
                const card = document.createElement('div');

                card.className = 'new-image-preview';

                // IMAGE
                const image = document.createElement('img');

                image.alt = file.name;
                image.loading = 'lazy';
                image.decoding = 'async';

                const objectUrl = URL.createObjectURL(file);

                previewUrls.push(objectUrl);

                image.src = objectUrl;

                image.addEventListener('error', function () {
                    card.classList.add('is-image-error');
                    image.removeAttribute('src');
                });

                // INFORMATION
                const info = document.createElement('div');

                info.className = 'new-image-preview-info';

                const name = document.createElement('span');

                name.className = 'new-image-name';
                name.textContent = file.name;

                const size = document.createElement('small');

                size.textContent =
                    (file.size / (1024 * 1024)).toFixed(2) + ' MB';

                // MAIN IMAGE RADIO
                const mainLabel = document.createElement('label');

                mainLabel.className = 'new-image-main-option';

                const radio = document.createElement('input');

                radio.type = 'radio';
                radio.name = 'MainImageNewIndexSelector';
                radio.value = String(index);

                radio.addEventListener('change', function () {
                    if (radio.checked) {
                        selectMainImage(index, card);
                    }
                });

                const radioText = document.createElement('span');

                radioText.textContent = 'Chọn làm ảnh chính';

                mainLabel.appendChild(radio);
                mainLabel.appendChild(radioText);

                // APPEND
                info.appendChild(name);
                info.appendChild(size);
                info.appendChild(mainLabel);

                card.appendChild(image);
                card.appendChild(info);

                container.appendChild(card);
            });
        }

        function destroy() {
            revokePreviewUrls();
            currentFiles = [];
        }

        return {
            clearPreview: clearPreview,
            clearNewMainImage: clearNewMainImage,
            clearExistingMainImage: clearExistingMainImage,
            renderPreview: renderPreview,
            destroy: destroy,
            getCurrentFiles: function () {
                return currentFiles;
            }
        };
    }

    // =====================================================
    // 6. INITIALIZE EACH FORM
    // =====================================================

    forms.forEach(function (form) {
        const fileInput = form.querySelector('#NewImages');

        const previewContainer = form.querySelector(
            '#productImagePreview'
        );

        const mainImageNewIndex = form.querySelector(
            '#MainImageNewIndex'
        );

        if (!fileInput) {
            console.warn(
                '[STAGEFX] Không tìm thấy input NewImages trong form.'
            );

            return;
        }

        const preview = createImagePreviewManager(
            previewContainer,
            mainImageNewIndex
        );

        let validating = false;

        // =================================================
        // 7. EXISTING IMAGE EVENTS
        // =================================================

        form.querySelectorAll(
            'input[name="MainImageId"]'
        ).forEach(function (radio) {
            radio.addEventListener('change', function () {
                if (radio.checked) {
                    preview.clearNewMainImage();
                }
            });
        });

        // =================================================
        // 8. FILE INPUT CHANGE
        // =================================================

        fileInput.addEventListener('change', async function () {
            if (validating) return;

            const files = Array.from(fileInput.files || []);

            preview.clearPreview();

            if (files.length === 0) {
                return;
            }

            // CHECK NUMBER OF FILES
            if (files.length > MAX_FILES) {
                window.alert(
                    'Chỉ được chọn tối đa ' + MAX_FILES + ' ảnh.'
                );

                fileInput.value = '';

                return;
            }

            // CHECK FILE TYPE AND SIZE
            for (const file of files) {
                const error = validateFile(file);

                if (error) {
                    window.alert(
                        'Ảnh không hợp lệ: ' +
                        file.name +
                        '\n\n' +
                        error
                    );

                    fileInput.value = '';

                    return;
                }
            }

            // CHECK IMAGE DIMENSIONS
            validating = true;
            fileInput.disabled = true;

            try {
                for (const file of files) {
                    const error = await validateImageDimensions(file);

                    if (error) {
                        window.alert(
                            'Không thể sử dụng ảnh: ' +
                            file.name +
                            '\n\n' +
                            error
                        );

                        fileInput.value = '';

                        preview.clearPreview();

                        return;
                    }
                }

                // RENDER ONLY AFTER ALL FILES PASS VALIDATION
                preview.renderPreview(files);

            } catch (error) {
                console.error(
                    '[STAGEFX] Image preview error:',
                    error
                );

                fileInput.value = '';

                preview.clearPreview();

                window.alert(
                    'Không thể xử lý ảnh. Vui lòng thử một ảnh khác.'
                );

            } finally {
                validating = false;
                fileInput.disabled = false;
            }
        });

        // =================================================
        // 9. FORM SUBMISSION
        // =================================================

        form.addEventListener('submit', function (event) {
            if (validating) {
                event.preventDefault();

                window.alert(
                    'Đang kiểm tra ảnh. Vui lòng chờ hoàn tất.'
                );

                return;
            }

            if (!form.checkValidity()) {
                event.preventDefault();

                form.reportValidity();

                return;
            }

            const files = Array.from(fileInput.files || []);

            if (files.length > MAX_FILES) {
                event.preventDefault();

                window.alert(
                    'Chỉ được tải lên tối đa ' + MAX_FILES + ' ảnh.'
                );

                return;
            }

            for (const file of files) {
                const error = validateFile(file);

                if (error) {
                    event.preventDefault();

                    window.alert(
                        'Ảnh không hợp lệ: ' +
                        file.name +
                        '\n\n' +
                        error
                    );

                    return;
                }
            }

            // PREVENT DOUBLE SUBMISSION
            const submitButtons = form.querySelectorAll(
                '.product-form-submit'
            );

            submitButtons.forEach(function (button) {
                button.disabled = true;
                button.classList.add('is-loading');

                button.innerHTML =
                    '<span class="material-symbols-outlined">' +
                    'hourglass_top' +
                    '</span> Đang lưu...';
            });
        });

        // =================================================
        // 10. CLEANUP
        // =================================================

        window.addEventListener('beforeunload', function () {
            preview.destroy();
        });
    });

});

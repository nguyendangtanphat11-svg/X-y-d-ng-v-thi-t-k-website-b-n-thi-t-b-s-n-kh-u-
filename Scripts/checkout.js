document.addEventListener('DOMContentLoaded', function () {

    // ==========================================
    // 1. CẤU HÌNH VÀ TRẠNG THÁI
    // ==========================================

    const state = {
        currentStep: 1,
        totalSteps: 3,
        submitting: false
    };

    const elements = {
        form: document.querySelector('#checkout-form'),
        formError: document.querySelector('#form-error'),
        submitButton: document.querySelector('#submit-order'),
        eventDate: document.querySelector('[name="EventDate"]')
    };

    // Không chạy nếu không phải trang Checkout.
    if (!elements.form) {
        console.warn('[STAGEFX] Không tìm thấy form Checkout.');
        return;
    }

    // ==========================================
    // 2. HÀM HỖ TRỢ
    // ==========================================

    function getStepPanel(step) {
        return document.querySelector(
            '.form-step[data-step-panel="' + step + '"]'
        );
    }

    function showError(message) {
        if (!elements.formError) {
            console.warn('[STAGEFX Checkout]', message);
            return;
        }

        elements.formError.textContent = message;
        elements.formError.hidden = false;

        elements.formError.scrollIntoView({
            behavior: 'smooth',
            block: 'center'
        });
    }

    function clearError() {
        if (!elements.formError) return;

        elements.formError.textContent = '';
        elements.formError.hidden = true;
    }

    function scrollToTop() {
        window.scrollTo({
            top: 0,
            behavior: 'smooth'
        });
    }

    // ==========================================
    // 3. CẬP NHẬT GIAO DIỆN CÁC BƯỚC
    // ==========================================

    function updateStep() {

        document.querySelectorAll('.form-step').forEach(function (panel) {

            const panelStep = Number(panel.dataset.stepPanel);

            panel.hidden = panelStep !== state.currentStep;
        });

        document.querySelectorAll('.step-button').forEach(function (button) {

            const buttonStep = Number(button.dataset.step);

            button.classList.toggle(
                'active',
                buttonStep === state.currentStep
            );

            button.classList.toggle(
                'completed',
                buttonStep < state.currentStep
            );

            if (buttonStep === state.currentStep) {
                button.setAttribute('aria-current', 'step');
            } else {
                button.removeAttribute('aria-current');
            }
        });

        clearError();

        const panel = getStepPanel(state.currentStep);
        const heading = panel ? panel.querySelector('h2') : null;

        if (heading) {
            heading.setAttribute('tabindex', '-1');
            heading.focus({ preventScroll: true });
        }
    }

    // ==========================================
    // 4. KIỂM TRA RADIO BUTTON
    // ==========================================

    function validateRadioGroup(field, panel) {

        const groupName = field.name;

        const checked = panel.querySelector(
            'input[type="radio"][name="' + groupName + '"]:checked'
        );

        if (checked) {
            return true;
        }

        showError('Vui lòng chọn một phương án bắt buộc.');

        const firstRadio = panel.querySelector(
            'input[type="radio"][name="' + groupName + '"]'
        );

        if (firstRadio) {
            firstRadio.focus();
        }

        return false;
    }

    // ==========================================
    // 5. KIỂM TRA CHECKBOX
    // ==========================================

    function validateCheckbox(field) {

        if (field.checked) {
            return true;
        }

        showError('Vui lòng xác nhận các nội dung bắt buộc.');

        field.focus();

        return false;
    }

    // ==========================================
    // 6. KIỂM TRA BƯỚC HIỆN TẠI
    // ==========================================

    function validateCurrentStep() {

        const panel = getStepPanel(state.currentStep);

        if (!panel) {
            showError(
                'Không tìm thấy nội dung bước ' + state.currentStep + '.'
            );

            return false;
        }

        const requiredFields = panel.querySelectorAll('[required]');

        for (const field of requiredFields) {

            if (field.disabled) {
                continue;
            }

            // Radio button
            if (field.type === 'radio') {

                if (!validateRadioGroup(field, panel)) {
                    return false;
                }

                continue;
            }

            // Checkbox
            if (field.type === 'checkbox') {

                if (!validateCheckbox(field)) {
                    return false;
                }

                continue;
            }

            // Input, select, textarea
            if (!field.checkValidity()) {

                field.reportValidity();

                showError(
                    'Vui lòng kiểm tra và nhập đầy đủ thông tin.'
                );

                field.focus();

                return false;
            }
        }

        clearError();

        return true;
    }

    // ==========================================
    // 7. KIỂM TRA TOÀN BỘ 3 BƯỚC
    // ==========================================

    function validateAllSteps() {

        const previousStep = state.currentStep;

        for (let step = 1; step <= state.totalSteps; step++) {

            state.currentStep = step;

            updateStep();

            if (!validateCurrentStep()) {

                const panel = getStepPanel(step);

                if (panel) {
                    panel.scrollIntoView({
                        behavior: 'smooth',
                        block: 'start'
                    });
                }

                return false;
            }
        }

        // Khôi phục bước ban đầu nếu tất cả đều hợp lệ.
        state.currentStep = previousStep;

        updateStep();

        return true;
    }

    // ==========================================
    // 8. NÚT TIẾP TỤC
    // ==========================================

    document.querySelectorAll('[data-next]').forEach(function (button) {

        button.addEventListener('click', function () {

            if (!validateCurrentStep()) {
                return;
            }

            const nextStep = Number(button.dataset.next);

            if (
                !Number.isInteger(nextStep) ||
                nextStep < 1 ||
                nextStep > state.totalSteps
            ) {
                return;
            }

            state.currentStep = nextStep;

            updateStep();

            scrollToTop();
        });
    });

    // ==========================================
    // 9. NÚT QUAY LẠI
    // ==========================================

    document.querySelectorAll('[data-previous]').forEach(function (button) {

        button.addEventListener('click', function () {

            const previousStep = Number(button.dataset.previous);

            if (
                !Number.isInteger(previousStep) ||
                previousStep < 1 ||
                previousStep >= state.currentStep
            ) {
                return;
            }

            state.currentStep = previousStep;

            updateStep();

            scrollToTop();
        });
    });

    // ==========================================
    // 10. CLICK STEPPER
    // ==========================================

    document.querySelectorAll('.step-button').forEach(function (button) {

        button.addEventListener('click', function () {

            const targetStep = Number(button.dataset.step);

            if (
                !Number.isInteger(targetStep) ||
                targetStep < 1 ||
                targetStep > state.totalSteps
            ) {
                return;
            }

            // Cho phép quay lại bước trước.
            if (targetStep < state.currentStep) {

                state.currentStep = targetStep;

                updateStep();

                return;
            }

            // Chỉ cho phép tiến từng bước.
            if (targetStep === state.currentStep + 1) {

                if (!validateCurrentStep()) {
                    return;
                }

                state.currentStep = targetStep;

                updateStep();
            }
        });
    });

    // ==========================================
    // 11. CẬP NHẬT THẺ THANH TOÁN
    // ==========================================

    function updatePaymentCards() {

        document.querySelectorAll('.payment-card').forEach(function (card) {

            const radio = card.querySelector(
                'input[name="PaymentMethod"]'
            );

            card.classList.toggle(
                'selected',
                Boolean(radio && radio.checked)
            );
        });
    }

    document.querySelectorAll(
        'input[name="PaymentMethod"]'
    ).forEach(function (input) {

        input.addEventListener('change', updatePaymentCards);
    });

    updatePaymentCards();

    // ==========================================
    // 12. CẬP NHẬT THẺ LOẠI ĐỊA ĐIỂM
    // ==========================================

    function updateVenueCards() {

        document.querySelectorAll(
            '.venue-field .choice-card'
        ).forEach(function (card) {

            const radio = card.querySelector(
                'input[name="VenueType"]'
            );

            card.classList.toggle(
                'selected',
                Boolean(radio && radio.checked)
            );
        });
    }

    document.querySelectorAll(
        'input[name="VenueType"]'
    ).forEach(function (input) {

        input.addEventListener('change', updateVenueCards);
    });

    updateVenueCards();

    // ==========================================
    // 13. CẬP NHẬT THẺ KỸ THUẬT VIÊN
    // ==========================================

    function updateTechnicianCards() {

        document.querySelectorAll(
            'input[name="SiteTechnician"]'
        ).forEach(function (input) {

            const group = input.closest('.field');

            if (!group) {
                return;
            }

            group.querySelectorAll('.choice-card').forEach(function (card) {

                const radio = card.querySelector(
                    'input[name="SiteTechnician"]'
                );

                card.classList.toggle(
                    'selected',
                    Boolean(radio && radio.checked)
                );
            });
        });
    }

    document.querySelectorAll(
        'input[name="SiteTechnician"]'
    ).forEach(function (input) {

        input.addEventListener('change', updateTechnicianCards);
    });

    updateTechnicianCards();

    // ==========================================
    // 14. GIỚI HẠN NGÀY TỔ CHỨC
    // ==========================================

    function setMinimumEventDate() {

        if (!elements.eventDate) {
            return;
        }

        const today = new Date();

        const year = today.getFullYear();

        const month = String(
            today.getMonth() + 1
        ).padStart(2, '0');

        const day = String(
            today.getDate()
        ).padStart(2, '0');

        // Định dạng yyyy-MM-dd, không có khoảng trắng.
        const minimumDate = year + '-' + month + '-' + day;

        elements.eventDate.min = minimumDate;
    }

    setMinimumEventDate();

    // ==========================================
    // 15. XỬ LÝ SUBMIT ĐẶT HÀNG
    // ==========================================

    elements.form.addEventListener('submit', function (event) {

        console.log('[STAGEFX] Submit event triggered.');

        // Chặn gửi lặp.
        if (state.submitting) {

            event.preventDefault();

            console.warn('[STAGEFX] Form đang được gửi.');

            return;
        }

        // Kiểm tra cả 3 bước.
        if (!validateAllSteps()) {

            event.preventDefault();

            console.warn('[STAGEFX] Validation failed.');

            return;
        }

        console.log(
            '[STAGEFX] Validation passed. Sending order to server.'
        );

        // Không preventDefault ở đây.
        // Trình duyệt sẽ POST đến Checkout/PlaceOrder.

        state.submitting = true;

        if (elements.submitButton) {

            elements.submitButton.disabled = true;

            const submitText = elements.submitButton.querySelector(
                '.submit-text'
            );

            if (submitText) {

                submitText.textContent = 'Đang gửi yêu cầu...';

            } else {

                elements.submitButton.textContent =
                    'Đang gửi yêu cầu...';
            }
        }
    });

    // ==========================================
    // 16. KHỞI TẠO CHECKOUT
    // ==========================================

    updateStep();

    console.log('[STAGEFX] Checkout JavaScript initialized.');

});

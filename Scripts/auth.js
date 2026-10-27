
'use strict';

document.addEventListener('DOMContentLoaded', () => {
    initializePasswordToggles();
    initializeAuthForms();
});

/**
 * Khởi tạo chức năng hiện/ẩn mật khẩu.
 */
function initializePasswordToggles() {
    const toggleButtons = document.querySelectorAll('[data-password-toggle]');

    toggleButtons.forEach((button) => {
        button.addEventListener('click', () => {
            const inputId = button.dataset.passwordToggle;
            const passwordInput = document.getElementById(inputId);

            if (!(passwordInput instanceof HTMLInputElement)) {
                console.error(`Không tìm thấy trường mật khẩu: ${inputId}`);
                return;
            }

            const shouldShowPassword = passwordInput.type === 'password';

            passwordInput.type = shouldShowPassword ? 'text' : 'password';

            button.setAttribute('aria-pressed', String(shouldShowPassword));
            button.setAttribute(
                'aria-label',
                shouldShowPassword ? 'Ẩn mật khẩu' : 'Hiện mật khẩu'
            );

            const icon = button.querySelector('.material-symbols-outlined');

            if (icon) {
                icon.textContent = shouldShowPassword
                    ? 'visibility_off'
                    : 'visibility';
            }
        });
    });
}

/**
 * Khởi tạo và kiểm tra biểu mẫu đăng nhập/đăng ký.
 */
function initializeAuthForms() {
    const authForms = document.querySelectorAll('[data-auth-form]');

    authForms.forEach((form) => {
        form.addEventListener('submit', (event) => {
            event.preventDefault();

            clearAuthMessage(form);

            if (!form.reportValidity()) {
                return;
            }

            if (form.dataset.authForm === 'register') {
                const isPasswordValid = validatePasswordConfirmation(form);

                if (!isPasswordValid) {
                    return;
                }
            }

            showAuthMessage(
                form,
                'Biểu mẫu hợp lệ. Đây là giao diện demo; dữ liệu chưa được gửi lên máy chủ.'
            );
        });
    });
}

/**
 * Kiểm tra mật khẩu và xác nhận mật khẩu.
 * Trả về true nếu hai trường khớp nhau.
 */
function validatePasswordConfirmation(form) {
    const passwordInput = form.querySelector('#register-password');
    const confirmationInput = form.querySelector('#register-confirm-password');

    if (
        !(passwordInput instanceof HTMLInputElement) ||
        !(confirmationInput instanceof HTMLInputElement)
    ) {
        console.error('Không tìm thấy trường mật khẩu hoặc xác nhận mật khẩu.');
        showAuthMessage(
            form,
            'Không thể kiểm tra mật khẩu. Vui lòng tải lại trang.',
            true
        );
        return false;
    }

    confirmationInput.setCustomValidity('');

    if (passwordInput.value !== confirmationInput.value) {
        confirmationInput.setCustomValidity('Mật khẩu xác nhận chưa khớp.');
        confirmationInput.reportValidity();
        confirmationInput.focus();
        return false;
    }

    return true;
}

/**
 * Xóa thông báo cũ và lỗi xác nhận mật khẩu khi người dùng nhập lại.
 */
function clearAuthMessage(form) {
    const message = form.querySelector('.auth-message');

    if (message instanceof HTMLElement) {
        message.textContent = '';
        message.classList.remove('visible', 'error');
    }
}

document.addEventListener('input', (event) => {
    const target = event.target;

    if (!(target instanceof HTMLInputElement)) {
        return;
    }

    if (target.id === 'register-confirm-password') {
        target.setCustomValidity('');
    }
});

/**
 * Hiển thị thông báo cho người dùng.
 */
function showAuthMessage(form, text, isError = false) {
    const message = form.querySelector('.auth-message');

    if (!(message instanceof HTMLElement)) {
        console.error('Không tìm thấy vùng thông báo của biểu mẫu tài khoản.');
        return;
    }

    message.textContent = text;
    message.classList.toggle('error', isError);
    message.classList.add('visible');
}


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
 * Kiểm tra biểu mẫu trước khi gửi lên máy chủ.
 */
function initializeAuthForms() {
    const authForms = document.querySelectorAll('.auth-form');

    authForms.forEach((form) => {
        form.addEventListener('submit', (event) => {
            clearAuthMessage(form);

            // Kiểm tra các trường HTML có required, type...
            if (!form.reportValidity()) {
                event.preventDefault();
                return;
            }

            // Kiểm tra mật khẩu xác nhận khi đăng ký.
            const isRegisterForm =
                form.id === 'registerForm' ||
                form.dataset.authForm === 'register';

            if (isRegisterForm) {
                if (!validatePasswordConfirmation(form)) {
                    event.preventDefault();
                    return;
                }
            }

            // Không gọi preventDefault ở đây.
            // Form sẽ gửi POST đến AccountController.
        });
    });
}

/**
 * Kiểm tra mật khẩu và xác nhận mật khẩu.
 */
function validatePasswordConfirmation(form) {
    const passwordInput = form.querySelector('#register-password');
    const confirmationInput = form.querySelector('#register-confirm-password');

    if (
        !(passwordInput instanceof HTMLInputElement) ||
        !(confirmationInput instanceof HTMLInputElement)
    ) {
        console.error('Không tìm thấy trường mật khẩu hoặc xác nhận mật khẩu.');
        return true;
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
 * Xóa lỗi xác nhận mật khẩu khi người dùng nhập lại.
 */
document.addEventListener('input', (event) => {
    const target = event.target;

    if (!(target instanceof HTMLInputElement)) {
        return;
    }

    if (
        target.id === 'register-confirm-password' ||
        target.id === 'register-password'
    ) {
        const confirmationInput =
            document.getElementById('register-confirm-password');

        if (confirmationInput instanceof HTMLInputElement) {
            confirmationInput.setCustomValidity('');
        }
    }
});

/**
 * Xóa thông báo cũ.
 */
function clearAuthMessage(form) {
    const message = form.querySelector('.auth-message');

    if (message instanceof HTMLElement) {
        message.textContent = '';
        message.classList.remove('visible', 'error');
    }
}

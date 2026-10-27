'use strict';

// Mở/đóng menu trên màn hình nhỏ
const menuToggle = document.querySelector('.menu-toggle');
const mainNav = document.querySelector('#mainNav');
if (menuToggle && mainNav) {
    menuToggle.addEventListener('click', () => {
        const isOpen = mainNav.classList.toggle('open');
        menuToggle.setAttribute('aria-expanded', String(isOpen));
        menuToggle.textContent = isOpen ? '✕' : '☰';
    });
    mainNav.querySelectorAll('a, button').forEach((item) => {
        item.addEventListener('click', () => {
            mainNav.classList.remove('open');
            menuToggle.setAttribute('aria-expanded', 'false');
            menuToggle.textContent = '☰';
        });
    });
}

// Cuộn đến phần tương ứng từ các nút CTA
document.querySelectorAll('[data-scroll]').forEach((button) => {
    button.addEventListener('click', () => {
        const target = document.querySelector(button.dataset.scroll);
        if (target) target.scrollIntoView({ behavior: 'smooth', block: 'start' });
    });
});

// Lọc sản phẩm minh họa
const filterButtons = document.querySelectorAll('[data-filter]');
const productCards = document.querySelectorAll('[data-product-category]');
filterButtons.forEach((button) => {
    button.addEventListener('click', () => {
        filterButtons.forEach((item) => item.classList.toggle('active', item === button));
        const filter = button.dataset.filter;
        productCards.forEach((card) => {
            card.hidden = filter !== 'all' && card.dataset.productCategory !== filter;
        });
    });
});

// Chọn sản phẩm sẽ điền tên vào form tư vấn
const requestedProduct = document.querySelector('#requestedProduct');
document.querySelectorAll('[data-product]').forEach((button) => {
    button.addEventListener('click', () => {
        if (requestedProduct) requestedProduct.value = button.dataset.product || '';
        const form = document.querySelector('#consultForm');
        if (form) {
            form.scrollIntoView({ behavior: 'smooth', block: 'center' });
            document.querySelector('#companyName')?.focus({ preventScroll: true });
        }
    });
});

// Form giao diện: chưa gửi dữ liệu lên máy chủ/API
const consultForm = document.querySelector('#consultForm');
const formMessage = document.querySelector('#formMessage');
if (consultForm && formMessage) {
    consultForm.addEventListener('submit', (event) => {
        event.preventDefault();
        if (!consultForm.reportValidity()) return;
        const company = document.querySelector('#companyName').value.trim();
        formMessage.textContent = `Đã kiểm tra biểu mẫu của ${company}. Đây là bản giao diện mẫu; cần kết nối backend để lưu và gửi yêu cầu thật.`;
    });
}

const year = document.querySelector('#year');
if (year) year.textContent = new Date().getFullYear();

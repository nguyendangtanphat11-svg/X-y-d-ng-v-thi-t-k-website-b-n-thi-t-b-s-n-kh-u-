(function () {
    'use strict';

    function initializeDashboard() {
        var dashboard = document.querySelector('[data-admin-dashboard]');
        if (!dashboard) {
            return;
        }

        var rows = dashboard.querySelectorAll('[data-status-count]');
        var maximum = 0;

        Array.prototype.forEach.call(rows, function (row) {
            var count = Number(row.getAttribute('data-status-count')) || 0;
            maximum = Math.max(maximum, count);
        });

        Array.prototype.forEach.call(rows, function (row) {
            var count = Number(row.getAttribute('data-status-count')) || 0;
            var percentage = maximum > 0 ? (count / maximum) * 100 : 0;
            var bar = row.querySelector('.admin-status-track span');

            if (bar) {
                bar.style.width = percentage + '%';
            }
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initializeDashboard);
    } else {
        initializeDashboard();
    }
}());

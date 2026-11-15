
document.addEventListener("DOMContentLoaded", function () {
    const fromDate = document.getElementById("fromDate");
    const toDate = document.getElementById("toDate");

    if (!fromDate || !toDate) return;

    fromDate.addEventListener("change", function () {
        toDate.min = fromDate.value;

        if (toDate.value && toDate.value < fromDate.value) {
            toDate.value = fromDate.value;
        }
    });

    toDate.addEventListener("change", function () {
        fromDate.max = toDate.value;

        if (fromDate.value && fromDate.value > toDate.value) {
            fromDate.value = toDate.value;
        }
    });

    const form = fromDate.closest("form");

    if (form) {
        form.addEventListener("submit", function (event) {
            if (
                fromDate.value &&
                toDate.value &&
                fromDate.value > toDate.value
            ) {
                event.preventDefault();
                alert("Ngày bắt đầu không được lớn hơn ngày kết thúc.");
            }
        });
    }
});

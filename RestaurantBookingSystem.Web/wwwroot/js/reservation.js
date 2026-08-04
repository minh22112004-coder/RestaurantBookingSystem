document.querySelectorAll("[data-reservation-form]").forEach((form) => {
    const tableSelect = form.querySelector("select[name$='.TableId']");
    const guestInput = form.querySelector("input[name$='.GuestCount']");
    const startInput = form.querySelector("input[name$='.StartTime']");
    const endInput = form.querySelector("input[name$='.EndTime']");
    const capacityHint = form.querySelector("[data-capacity-hint]");

    const updateCapacity = () => {
        if (!tableSelect || !guestInput) return;
        const option = tableSelect.selectedOptions[0];
        const capacity = Number(option?.dataset.capacity || 0);
        guestInput.max = capacity > 0 ? String(capacity) : "100";
        guestInput.setCustomValidity(
            capacity > 0 && Number(guestInput.value) > capacity
                ? `This table seats a maximum of ${capacity} guests.`
                : "");
        if (capacityHint) {
            capacityHint.textContent = capacity > 0 ? `Selected table capacity: ${capacity}` : "";
        }
    };

    const updateTimeValidity = () => {
        if (!startInput || !endInput) return;
        endInput.setCustomValidity(
            startInput.value && endInput.value && endInput.value <= startInput.value
                ? "The end time must be later than the start time."
                : "");
    };

    tableSelect?.addEventListener("change", updateCapacity);
    guestInput?.addEventListener("input", updateCapacity);
    startInput?.addEventListener("input", updateTimeValidity);
    endInput?.addEventListener("input", updateTimeValidity);
    form.addEventListener("submit", () => {
        updateCapacity();
        updateTimeValidity();
    }, true);

    updateCapacity();
    updateTimeValidity();
});

document.querySelectorAll("[data-table-choice]").forEach((button) => {
    button.addEventListener("click", () => {
        const booking = document.getElementById("booking");
        const tableSelect = booking?.querySelector("select[name$='.TableId']");
        if (!booking || !tableSelect) return;
        tableSelect.value = button.dataset.tableChoice || "";
        tableSelect.dispatchEvent(new Event("change", { bubbles: true }));
        booking.scrollIntoView({ behavior: "smooth", block: "start" });
        tableSelect.focus({ preventScroll: true });
    });
});

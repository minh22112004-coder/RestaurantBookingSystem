document.querySelectorAll("[data-nav-toggle]").forEach((button) => {
    button.addEventListener("click", () => {
        const navigation = document.getElementById(button.getAttribute("aria-controls"));
        if (!navigation) return;
        const isOpen = navigation.classList.toggle("is-open");
        button.setAttribute("aria-expanded", String(isOpen));
    });
});

document.querySelectorAll("[data-sidebar-toggle]").forEach((button) => {
    button.addEventListener("click", () => {
        const sidebar = document.getElementById(button.getAttribute("aria-controls"));
        if (!sidebar) return;
        const isOpen = sidebar.classList.toggle("is-open");
        button.setAttribute("aria-expanded", String(isOpen));
        button.setAttribute("aria-label", isOpen ? "Close administration menu" : "Open administration menu");
    });
});

document.querySelectorAll("[data-password-toggle]").forEach((button) => {
    button.addEventListener("click", () => {
        const input = button.parentElement?.querySelector("input");
        if (!input) return;
        const shouldShow = input.type === "password";
        input.type = shouldShow ? "text" : "password";
        button.textContent = shouldShow ? "Hide" : "Show";
        button.setAttribute("aria-label", shouldShow ? "Hide password" : "Show password");
    });
});

document.querySelectorAll("[data-flash-dismiss]").forEach((button) => {
    button.addEventListener("click", () => button.closest("[data-flash-message]")?.remove());
});

document.querySelectorAll("[data-loading-form], form[method='post']:not([data-confirm-form])").forEach((form) => {
    form.addEventListener("submit", () => {
        form.setAttribute("aria-busy", "true");
        form.querySelectorAll("button[type='submit'], input[type='submit']").forEach((button) => {
            button.setAttribute("aria-disabled", "true");
        });
        const loadingState = document.querySelector("[data-loading-state]");
        if (loadingState) loadingState.hidden = false;
    });
});

const confirmDialog = document.querySelector("[data-confirm-dialog]");
if (confirmDialog) {
    let pendingForm = null;
    document.querySelectorAll("[data-confirm-form]").forEach((form) => {
        form.addEventListener("submit", (event) => {
            if (form.dataset.confirmed === "true") {
                delete form.dataset.confirmed;
                return;
            }

            event.preventDefault();
            pendingForm = form;
            const title = confirmDialog.querySelector("[data-confirm-title]");
            const message = confirmDialog.querySelector("[data-confirm-message]");
            if (title) title.textContent = form.dataset.confirmTitle || "Are you sure?";
            if (message) message.textContent = form.dataset.confirmMessage || "This action will change stored data.";
            confirmDialog.showModal();
        });
    });

    confirmDialog.addEventListener("close", () => {
        if (confirmDialog.returnValue === "confirm" && pendingForm) {
            pendingForm.dataset.confirmed = "true";
            pendingForm.setAttribute("aria-busy", "true");
            pendingForm.querySelectorAll("button[type='submit']").forEach((button) => {
                button.setAttribute("aria-disabled", "true");
            });
            pendingForm.requestSubmit();
        }
        pendingForm = null;
    });
}

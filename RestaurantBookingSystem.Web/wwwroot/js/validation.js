document.querySelectorAll("[data-validated-form]").forEach((form) => {
    const fields = [...form.querySelectorAll("input:not([type='hidden']), select, textarea")];

    const validateField = (field) => {
        const message = form.querySelector(`[data-valmsg-for="${field.name}"]`);
        let error = "";

        if (field.required && !field.value.trim()) {
            error = field.dataset.requiredMessage || "Please complete this field.";
        } else if (field.dataset.trimRequired !== undefined && !field.value.trim()) {
            error = field.dataset.requiredMessage || "This field cannot contain only spaces.";
        } else if (field.type === "email" && field.validity.typeMismatch) {
            error = "Enter a valid email address.";
        } else if (field.minLength > 0 && field.value.length < field.minLength) {
            error = `Enter at least ${field.minLength} characters.`;
        } else if (!field.validity.valid) {
            error = field.dataset.invalidMessage || field.validationMessage || "Please review this field.";
        } else if (field.dataset.match) {
            const source = form.elements.namedItem(field.dataset.match);
            if (source && field.value !== source.value) {
                error = field.dataset.matchMessage || "The confirmation does not match.";
            }
        }

        field.setAttribute("aria-invalid", String(Boolean(error)));
        if (message) message.textContent = error;
        return !error;
    };

    fields.forEach((field) => {
        field.addEventListener("input", () => validateField(field));
        field.addEventListener("blur", () => validateField(field));
    });

    form.addEventListener("submit", (event) => {
        const firstInvalidField = fields.find((field) => !validateField(field));
        if (firstInvalidField) {
            event.preventDefault();
            firstInvalidField.focus();
        }
    });
});

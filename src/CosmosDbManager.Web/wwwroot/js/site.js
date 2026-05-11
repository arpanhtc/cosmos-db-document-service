(function () {
    function renumberPatchRows(container) {
        const rows = Array.from(container.querySelectorAll("[data-patch-row]"));

        rows.forEach((row, rowIndex) => {
            const fields = {
                operationType: row.querySelector("[data-operation-type]"),
                path: row.querySelector("[data-operation-path]"),
                value: row.querySelector("[data-operation-value]")
            };

            if (fields.operationType) {
                fields.operationType.name = `Operations[${rowIndex}].OperationType`;
                fields.operationType.id = `Operations_${rowIndex}__OperationType`;
            }

            if (fields.path) {
                fields.path.name = `Operations[${rowIndex}].Path`;
                fields.path.id = `Operations_${rowIndex}__Path`;
            }

            if (fields.value) {
                fields.value.name = `Operations[${rowIndex}].Value`;
                fields.value.id = `Operations_${rowIndex}__Value`;
            }
        });
    }

    function syncPatchRowVisibility(row) {
        const operationType = row.querySelector("[data-operation-type]");
        const valueWrap = row.querySelector("[data-value-wrap]");
        const valueInput = row.querySelector("[data-operation-value]");

        if (!operationType || !valueWrap || !valueInput) {
            return;
        }

        const isRemove = operationType.value === "Remove";
        valueWrap.classList.toggle("d-none", isRemove);
        if (isRemove) {
            valueInput.value = "";
        }
    }

    function initPatchBuilder() {
        const container = document.querySelector("[data-patch-operations]");
        const template = document.getElementById("patch-operation-template");

        if (!container || !template) {
            return;
        }

        const addButton = document.querySelector("[data-add-operation]");

        const refresh = () => {
            renumberPatchRows(container);
            container.querySelectorAll("[data-patch-row]").forEach(syncPatchRowVisibility);
        };

        refresh();

        container.addEventListener("change", (event) => {
            const target = event.target;
            if (target && target.matches("[data-operation-type]")) {
                const row = target.closest("[data-patch-row]");
                if (row) {
                    syncPatchRowVisibility(row);
                }
            }
        });

        container.addEventListener("click", (event) => {
            const target = event.target;
            if (!target || !target.matches("[data-remove-operation]")) {
                return;
            }

            const row = target.closest("[data-patch-row]");
            if (!row) {
                return;
            }

            if (container.querySelectorAll("[data-patch-row]").length === 1) {
                const rowType = row.querySelector("[data-operation-type]");
                const rowPath = row.querySelector("[data-operation-path]");
                const rowValue = row.querySelector("[data-operation-value]");
                if (rowType) rowType.value = "Set";
                if (rowPath) rowPath.value = "";
                if (rowValue) rowValue.value = "";
                syncPatchRowVisibility(row);
                return;
            }

            row.remove();
            refresh();
        });

        if (addButton) {
            addButton.addEventListener("click", () => {
                const fragment = template.content.cloneNode(true);
                container.appendChild(fragment);
                refresh();
            });
        }
    }

    function initCopyButtons() {
        document.querySelectorAll("[data-copy-json]").forEach((button) => {
            button.addEventListener("click", async () => {
                const selector = button.getAttribute("data-target");
                if (!selector) {
                    return;
                }

                const source = document.querySelector(selector);
                if (!source) {
                    return;
                }

                try {
                    await navigator.clipboard.writeText(source.textContent || "");
                    const original = button.textContent;
                    button.textContent = "Copied";
                    window.setTimeout(() => button.textContent = original, 1000);
                }
                catch {
                    button.textContent = "Copy failed";
                }
            });
        });
    }

    document.addEventListener("DOMContentLoaded", () => {
        if (window.cosmosDbManager && typeof window.cosmosDbManager.formatJsonBlocks === "function") {
            window.cosmosDbManager.formatJsonBlocks();
        }

        initPatchBuilder();
        initCopyButtons();
    });
})();

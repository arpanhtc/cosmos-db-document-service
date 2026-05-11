window.cosmosDbManager = window.cosmosDbManager || {};

window.cosmosDbManager.formatJsonBlocks = function () {
    document.querySelectorAll("[data-json-block]").forEach((element) => {
        const raw = element.textContent || "";
        const trimmed = raw.trim();

        if (!trimmed) {
            return;
        }

        try {
            const parsed = JSON.parse(trimmed);
            element.textContent = JSON.stringify(parsed, null, 2);
        }
        catch {
            element.textContent = raw;
        }
    });
};

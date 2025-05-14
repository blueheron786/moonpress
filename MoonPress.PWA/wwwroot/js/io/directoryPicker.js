/**
 * Shows a folder picker. Lists all .md files within that folder (non-recursively).
 * @returns {Promise<{error}|*[]>}
 */
window.moonpress = {
    async showFolderPicker() {
        try {
            const dirHandle = await window.showDirectoryPicker();
            return {
                success: true,
                name: dirHandle.name
                // You can't pass the full handle directly to Blazor, it's a JS object.
                // You'll need to cache it or operate on it within JS.
            };
        } catch (err) {
            return {
                success: false,
                error: err.message
            };
        }
    }
};


/**
 * Shows a folder picker. Lists all .md files within that folder (non-recursively).
 * @returns {Promise<{error}|*[]>}
 */
window.pickFolderAndListFiles = async () => {
    try {
        const dirHandle = await window.showDirectoryPicker();
        const files = [];
        
        for await (const [name, handle] of dirHandle.entries()) {
            if (handle.kind === "file" && name.endsWith(".md")) {
                const file = await handle.getFile();
                const content = await file.text();
                files.push({ name, content });
            }
        }
        return files;
    } catch (e) {
        return { error: e.message };
    }
};

window.copyToClipboard = (text) => {
    navigator.clipboard.writeText(text).then(() => {
        console.log("Testo copiato: " + text);
    }).catch(err => {
        console.error("Errore nella copia:", err);
    });
}

window.openNewTab = (url, title) => {
    let newTab = window.open();
    newTab.document.title = title;  // Imposta dinamicamente il titolo
    newTab.document.body.innerHTML = '<img src="' + url + '" style="width:100%; height:auto;">';  // Scrive l'immagine nel body
}

window.viewFile = (base64Data, mediaType, filename) => {
    const bytes = atob(base64Data);
    const arr = new Uint8Array(bytes.length);
    for (let i = 0; i < bytes.length; i++) arr[i] = bytes.charCodeAt(i);
    const blob = new Blob([arr], { type: mediaType || 'application/octet-stream' });
    const url = URL.createObjectURL(blob);
    const viewable = mediaType && (mediaType.startsWith('image/') || mediaType === 'application/pdf' || mediaType.startsWith('text/'));
    if (viewable) {
        window.open(url, '_blank');
    } else {
        const a = document.createElement('a');
        a.href = url;
        a.download = filename || 'documento';
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        setTimeout(() => URL.revokeObjectURL(url), 1000);
    }
};

window.downloadFile = (fileDataUrl, filename) => {
    const a = document.createElement("a");
    a.href = fileDataUrl;
    a.download = filename;
    a.click();
}

window.downloadTextAsFile = (filename, text, mimeType) => {
    const blob = new Blob([text], { type: mimeType || 'text/html;charset=utf-8' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
};

window.downloadBytesAsFile = (filename, bytes) => {
    const blob = new Blob([new Uint8Array(bytes)], { type: 'application/zip' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
};

window.setIframeSrcdoc = (iframeId, html) => {
    const iframe = document.getElementById(iframeId);
    if (iframe) iframe.srcdoc = html;
};

window.initResizablePanel = (panelId, handleId, tabId, editorSelector, minWidth) => {
    const panel = document.getElementById(panelId);
    const handle = document.getElementById(handleId);
    if (!panel || !handle) return;

    let dragging = false, startX, startWidth;

    handle.addEventListener('mousedown', e => {
        dragging = true;
        startX = e.clientX;
        startWidth = panel.offsetWidth;
        document.body.style.cursor = 'ew-resize';
        document.body.style.userSelect = 'none';
        e.preventDefault();
    });

    document.addEventListener('mousemove', e => {
        if (!dragging) return;
        const newWidth = Math.max(startWidth + (startX - e.clientX), minWidth);
        panel.style.width = newWidth + 'px';
        const tab = document.getElementById(tabId);
        if (tab) tab.style.right = newWidth + 'px';
        const editor = document.querySelector(editorSelector);
        if (editor) editor.style.marginRight = newWidth + 'px';
    });

    document.addEventListener('mouseup', () => {
        if (!dragging) return;
        dragging = false;
        document.body.style.cursor = '';
        document.body.style.userSelect = '';
    });
};

window.resetPanelPosition = (tabId, editorSelector) => {
    const tab = document.getElementById(tabId);
    const editor = document.querySelector(editorSelector);
    if (tab) tab.style.right = '';
    if (editor) editor.style.marginRight = '';
};

window.avatarSetTheme = (theme) => {
    const html = document.documentElement;
    if (theme === 'auto') {
        const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
        html.setAttribute('data-bs-theme', prefersDark ? 'dark' : 'light');
    } else {
        html.setAttribute('data-bs-theme', theme);
    }
    try { localStorage.setItem('blazor-bs-theme', theme); } catch {}
};

window.avatarGetTheme = () => {
    try { return localStorage.getItem('blazor-bs-theme') || 'auto'; } catch { return 'auto'; }
};

window.initTableDropZone = function (container) {
    const input = container.querySelector('input[type="file"]');
    if (!input) return;

    // Drop fires on the container (input has pointer-events:none).
    // Forward the files by assigning them to the input and firing a synthetic change,
    // which Blazor's InputFile picks up via its native change listener.
    container.addEventListener('drop', function (e) {
        if (!e.dataTransfer?.files?.length) return;
        const dt = new DataTransfer();
        for (const file of e.dataTransfer.files) dt.items.add(file);
        input.files = dt.files;
        input.dispatchEvent(new Event('change', { bubbles: true }));
    }, true); // capture: fires before Blazor's ondrop handler
};
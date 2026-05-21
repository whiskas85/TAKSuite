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

window.viewFile = (base64Data, mediaType) => {
    const bytes = atob(base64Data);
    const arr = new Uint8Array(bytes.length);
    for (let i = 0; i < bytes.length; i++) arr[i] = bytes.charCodeAt(i);
    const blob = new Blob([arr], { type: mediaType || 'application/octet-stream' });
    const url = URL.createObjectURL(blob);
    window.open(url, '_blank');
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
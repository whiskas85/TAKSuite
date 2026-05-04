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
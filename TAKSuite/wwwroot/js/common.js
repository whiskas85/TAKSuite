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
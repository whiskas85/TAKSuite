export function returnArrayAsync() {
    DotNet.invokeMethodAsync('BlazorSample', 'Pollo')
        .then(data => {
            console.log(data);
        });
}
function dragstartHandler(ev) {
    ev.dataTransfer.setData("text", ev.target.id);
}

function dragoverHandler(ev) {
    ev.preventDefault();
}

function dropHandler(ev) {
    ev.preventDefault();
    const data = ev.dataTransfer.getData("text");
    ev.target.appendChild(document.getElementById(data));
}


function addDragAndDropEvents() {
    const treeNode = document.querySelector(".teams-treenode");
    if (!treeNode) return;

    // Aggiunge attributi agli elementi <a>
    treeNode.querySelectorAll("a").forEach(a => {
        a.setAttribute("draggable", "true");
        a.setAttribute("ondragstart", "dragstartHandler(event)");
    });

    // Aggiunge attributi agli elementi <div>
    treeNode.querySelectorAll("div").forEach(div => {
        div.setAttribute("ondrop", "dropHandler(event)");
        div.setAttribute("ondragover", "dragoverHandler(event)");
    });
}
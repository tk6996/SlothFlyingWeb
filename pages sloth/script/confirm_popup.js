function offPopUp() {
    const body = document.querySelector("body");
    body.style.overflowY = "scroll";
    const confirm_popup = document.querySelector("#plane");
    confirm_popup.className = "plane-off";
}

function onPopUp(inner_text, obj) {
    const body = document.querySelector("body");
    body.style.overflowY = "hidden";
    const confirm_popup = document.querySelector("#plane");
    confirm_popup.className = "plane";
    const text = document.querySelector("#text");
    text.innerHTML = inner_text;
    const data_input = document.querySelector("#data");
    if (typeof obj === "object" && Object.entries(obj).length > 0) {
        const [key, value] = Object.entries(obj)[0];
        data_input.setAttribute("name", key);
        data_input.setAttribute("value", value);
    }
}

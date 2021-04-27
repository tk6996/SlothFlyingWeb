/**
@param {string} method
@param {string} url
@param {object} body
@param {object} header
 */
function makeRequest(method, url, body, header) {
  return new Promise((resolve, reject) => {
    let xhr = new XMLHttpRequest();
    xhr.open(method, url);
    for (const key in header) {
      if (Object.hasOwnProperty.call(header, key)) {
        const value = header[key];
        xhr.setRequestHeader(key, value);
      }
    }
    xhr.onload = function () {
      if (this.status >= 200 && this.status < 300) {
        resolve(xhr.response);
      } else {
        reject({
          status: this.status,
          statusText: this.statusText,
        });
      }
    };
    xhr.onerror = function () {
      reject({
        status: this.status,
        statusText: this.statusText,
      });
    };
    xhr.send(body);
  });
}
// off modal
function confirmPopUpOff() {
  const body = document.querySelector("body");
  body.style.overflowY = "auto";
  const confirm_popup = document.querySelector("#modal");
  confirm_popup.className = "modal-off";
}
// open modal
function confirmPopUpOn() {
  const body = document.querySelector("body");
  body.style.overflowY = "hidden";
  const confirm_popup = document.querySelector("#modal");
  confirm_popup.className = "modal";
}

// send object to server by form
function confirmPopUpOnForm(object) {
  confirmPopUpOn();
  const inputSection = document.querySelector("#input-section");
  inputSection.innerHTML = "";
  for (const key in object) {
    if (Object.hasOwnProperty.call(object, key)) {
      const input = document.createElement("input");
      input.setAttribute("type", "hidden");
      input.name = key;
      input.value = object[key];
      inputSection.appendChild(input);
    }
  }
}
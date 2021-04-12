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
          error: xhr.response
        });
      }
    };
    xhr.onerror = function () {
      reject({
        status: this.status,
        error: xhr.response
      });
    };
    xhr.send(body);
  });
}

function confirmPopUpOff() {
  const body = document.querySelector("body");
  body.style.overflowY = "scroll";
  const confirm_popup = document.querySelector("#plane");
  confirm_popup.className = "plane-off";
}

function confirmPopUpOn(obj) {
  const body = document.querySelector("main");
  body.style.overflowY = "hidden";
  const confirm_popup = document.querySelector("#plane");
  confirm_popup.className = "plane";
  const data_input = document.querySelector("#data");
  if (typeof obj === "object" && Object.entries(obj).length > 0) {
      const [key, value] = Object.entries(obj)[0];
      data_input.setAttribute("name", key);
      data_input.setAttribute("value", value);
  }
}
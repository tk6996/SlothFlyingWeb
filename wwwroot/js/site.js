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
          error: xhr.response,
        });
      }
    };
    xhr.onerror = function () {
      reject({
        status: this.status,
        error: xhr.response,
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

// send object to server by json
function confirmPopUpOnJson(object) {
  return new Promise((resolve, reject) => {
    confirmPopUpOn();
    const submit = document.querySelector("#submit");
    const cancel = document.querySelector("#cancel");
    const clickedSubmitEventHandler = (event) => {
      event.preventDefault();
      submit.disabled = true;
      cancel.disabled = true;

      let token = document.querySelector(
        'input[name="__RequestVerificationToken"]'
      ).value;

      makeRequest("POST", window.location.pathname, JSON.stringify(object), {
        RequestVerificationToken: token,
        "Content-Type": "application/json;charset=utf-8",
      })
        .then((response) => {
          submit.disabled = false;
          cancel.disabled = false;
          resolve(response);
        })
        .catch((error) => {
          submit.disabled = false;
          cancel.disabled = false;
          reject(error);
        });
    };
    submit.addEventListener("click", clickedSubmitEventHandler);
    cancel.addEventListener("click", () => {
      submit.removeEventListener("click", clickedSubmitEventHandler);
    });
  });
}

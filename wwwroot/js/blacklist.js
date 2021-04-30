/**
 * @param {{
 *  id : number,
 *  imageUrl : string,
 *  name : string,
 *  email : string,
 *  phone : string,
 * }} user
 * @param {number} index
 */
function appendList(user, index) {
  const tr = document.createElement("tr");
  const indexCell = document.createElement("td");
  const idCell = document.createElement("td");
  const nameWithImage = document.createElement("td");
  const imageCell = document.createElement("img");
  const nameCell = document.createTextNode(user.name);
  const emailCell = document.createElement("td");
  const phoneCell = document.createElement("td");
  const buttonCell = document.createElement("td");

  indexCell.innerHTML = index;
  idCell.innerHTML = String(user.id).padStart(8, "0");
  imageCell.src = user.imageUrl;
  imageCell.alt = "user_image";
  emailCell.innerHTML = user.email;
  phoneCell.innerHTML = user.phone;
  buttonCell.className = "icon";
  const link = document.createElement("a");
  link.href = `/Search/UserProfile/${user.id}`;
  const viewIcon = document.createElement("i");
  viewIcon.className = "fas fa-eye view-button";
  link.appendChild(viewIcon);
  const ejectIcon = document.createElement("i");
  ejectIcon.className = "fas fa-times-circle cancel-button";
  ejectIcon.onclick = () => confirmPopUpOnForm({ id: user.id });
  buttonCell.appendChild(link);
  buttonCell.appendChild(ejectIcon);

  tr.appendChild(indexCell);
  tr.appendChild(idCell);
  nameWithImage.appendChild(imageCell);
  nameWithImage.appendChild(nameCell);
  tr.appendChild(nameWithImage);
  tr.appendChild(emailCell);
  tr.appendChild(phoneCell);
  tr.appendChild(buttonCell);
  document
    .querySelector("tbody")
    .insertBefore(tr, document.querySelector(".bottom"));
}

/**
 *
 * @param {number} round
 */
async function fetchList(round) {
  try {
    let booklists = await makeRequest(
      "GET",
      `${window.location.pathname}/${round}`
    );
    booklists = JSON.parse(booklists);
    if (booklists.length === 0) {
      document.querySelector("tbody").removeEventListener("scroll", loadScroll);
      document.querySelector(".bottom").remove();
    } else {
      for (let i = 0; i < booklists.length; i++) {
        appendList(booklists[i], round * 10 + i + 1);
      }
    }
  } catch (error) {
    console.error(error);
  }
}

let round = 0;
let enableLoad = true;

/**
 * @this HTMLElement
 */
async function loadScroll() {
  if (enableLoad === true) {
    const bottom = document.querySelector(".bottom");
    if (
      this.scrollTop + this.offsetHeight >=
      this.scrollHeight - bottom.clientHeight
    ) {
      enableLoad = false;
      await fetchList(++round);
      enableLoad = true;
    }
  }
}

document.querySelector("tbody").addEventListener("scroll", loadScroll);

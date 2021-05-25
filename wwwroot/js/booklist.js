/**
 * @typedef {number} StatusType
 * @enum {StatusType}
 */
const StatusType = {
  USING: 0,
  COMING: 1,
  FINISHED: 2,
  CANCEL: 3,
  EJECT: 4,
};
/**
 * @param {{
 *  id : number,
 *  labId : number,
 *  itemName : string,
 *  date : string,
 *  from : string,
 *  to : string,
 *  status : StatusType
 * }} booklist
 * @param {number} index
 */
function appendList(booklist, index) {
  const tr = document.createElement("tr");
  const indexCell = document.createElement("td");
  const labCell = document.createElement("td");
  const dateCell = document.createElement("td");
  const fromCell = document.createElement("td");
  const toCell = document.createElement("td");
  const statusCell = document.createElement("td");
  const cancelCell = document.createElement("td");

  indexCell.innerHTML = index;
  labCell.innerHTML = `Lab ${booklist.labId} ${booklist.itemName}`;
  dateCell.innerHTML = booklist.date;
  fromCell.innerHTML = booklist.from;
  toCell.innerHTML = booklist.to;
  statusCell.className = "status";
  const statusBar = document.createElement("span");
  switch (booklist.status) {
    case StatusType.USING:
      statusBar.className = "using";
      statusCell.appendChild(statusBar).innerHTML = "Using";
      break;
    case StatusType.COMING:
      statusBar.className = "coming";
      statusCell.appendChild(statusBar).innerHTML = "Coming";
      break;
    case StatusType.FINISHED:
      statusCell.appendChild(statusBar).innerHTML = "Finished";
      break;
    case StatusType.CANCEL:
      statusCell.appendChild(statusBar).innerHTML = "Cancel";
      break;
    case StatusType.EJECT:
      statusCell.appendChild(statusBar).innerHTML = "Eject";
      break;
    default:
      break;
  }
  if (
    booklist.status == StatusType.USING ||
    booklist.status == StatusType.COMING
  ) {
    cancelDiv = document.createElement("div");
    cancelDiv.className = "icon";
    const icon = document.createElement("i");
    icon.className = "fas fa-times-circle";
    icon.addEventListener("click", () =>
      confirmPopUpOnForm({
        id: booklist.id,
      })
    );
    icon.addEventListener("mouseover", setOverPosition);
    icon.addEventListener("onmouseout", resetTooltip);
    const tooltip = document.createElement("span");
    tooltip.innerHTML = "Cancel";
    tooltip.className = "tooltip";
    cancelCell.appendChild(cancelDiv).appendChild(icon);
    cancelDiv.appendChild(tooltip);
  }

  tr.appendChild(indexCell);
  tr.appendChild(labCell);
  tr.appendChild(dateCell);
  tr.appendChild(fromCell);
  tr.appendChild(toCell);
  tr.appendChild(statusCell);
  tr.appendChild(cancelCell);
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

/**
 * @param {Event} event
 */
function setOverPosition(event) {
  const scrollTable = document.querySelector(".scroll-table");
  const tbody = document.querySelector("tbody");
  const icon = event.target;
  const iconContainer = event.target.parentElement;
  const tooltip = event.target.parentElement.children[1];
  iconContainer.style.position = "static";
  tooltip.style.visibility = "visible";
  tooltip.style.top =
    getPosition(icon)[1] - tbody.scrollTop - tooltip.offsetHeight - 65 + "px";
  tooltip.style.left = tooltip.style.left =
    getPosition(icon)[0] -
    scrollTable.scrollLeft +
    icon.offsetWidth / 2 -
    tooltip.offsetWidth / 2 +
    "px";
}

/**
 * @param {Event} event
 */
function resetTooltip(event) {
  const tooltip = event.target.parentElement.children[1];
  const iconContainer = event.target.parentElement;
  tooltip.style = null;
  iconContainer.style = null;
}

if (document.querySelector(".bottom")) {
  document.querySelector("tbody").addEventListener("scroll", loadScroll);
}

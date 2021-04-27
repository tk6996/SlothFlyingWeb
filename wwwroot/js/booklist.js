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
    cancelCell.className = "icon";
    const icon = document.createElement("i");
    icon.className = "fas fa-times-circle";
    icon.addEventListener("onclick", () =>
      confirmPopUpOnForm({
        id: booklist.id,
      })
    );
    cancelCell.appendChild(icon);
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
    let booklists = await makeRequest("GET", `${window.location.pathname}/${round}`);
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

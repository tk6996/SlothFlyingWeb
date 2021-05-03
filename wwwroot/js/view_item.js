async function showResult(event) {
  event.stopPropagation();
  try {
    const row = event.target.id.slice(4, 5);
    const col = event.target.id.slice(6);
    const labId = Number(document.querySelector("#LabId").value);
    const date =
      Number(col) * 24 * 60 * 60 * 1000 +
      Number(document.querySelector("#startDateString").value);
    const timeslot = Number(row) + 1;
    let result = await makeRequest(
      "GET",
      `/LabAdmin/UserBookList?labId=${labId}&date=${date}&timeslot=${timeslot}`
    );
    result = JSON.parse(result);
    //console.log(result);

    let bl_table = document.querySelector(".booklist-table");
    bl_table.innerHTML = "";
    bl_table.appendChild(createBooklistTable(result, new Date(date), timeslot));
    bl_table.scrollIntoView(true);
  } catch (error) {
    console.error(error);
    let bl_table = document.querySelector(".booklist-table");
    bl_table.innerHTML = "";
    const newElement = document.createElement("p");
    newElement.style.color = "var(--red)";
    bl_table.appendChild(newElement).innerHTML =
      "Request error , You should to refresh pages.";
  }
}

function createBooklistTable(booklist, date, timeslot) {
  const day = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];
  const month = [
    "Jan",
    "Feb",
    "Mar",
    "Apr",
    "May",
    "Jun",
    "Jul",
    "Aug",
    "Sep",
    "Oct",
    "Nov",
    "Dec",
  ];
  const status = ["Using", "Coming", "Finished", "Cancel", "Eject"];
  const table_colname = ["#", "User Id", "Name", "From", "To", "Status", ""];

  let result = document.createElement("div");
  result.className = "book-list-range";
  let title = document.createElement("h2");
  title.innerHTML = `${day[date.getDay()]} ${date.getDate()} ${month[date.getMonth()]
    } @ ${String(timeslot + 7).padStart(2, "0")}.00 - ${String(
      timeslot + 8
    ).padStart(2, "0")}.00`;
  result.appendChild(title);

  if (booklist.length) {
    let table = document.createElement("table");
    let thead = document.createElement("thead");
    let tbody = document.createElement("tbody");
    let trHead = document.createElement("tr");

    // thead
    for (let i = 0; i < table_colname.length; i++) {
      trHead.appendChild(document.createElement("th")).innerHTML =
        table_colname[i];
    }
    thead.appendChild(trHead);

    // tbody
    for (let i = 0; i < booklist.length; i++) {
      let trBody = document.createElement("tr");
      //index number
      trBody.appendChild(document.createElement("td")).innerHTML = i + 1;
      //user ID
      trBody.appendChild(document.createElement("td")).innerHTML = String(
        booklist[i].userId
      ).padStart(8, "0");
      //create a user image element
      user_img = document.createElement("img");
      user_img.setAttribute("src", booklist[i].userImageUrl);
      user_img.className = "user-img";
      user_name = document.createElement("td");
      trBody.appendChild(user_name).appendChild(user_img);
      //user fullname
      user_name.appendChild(document.createTextNode(booklist[i].fullName));
      //from
      trBody.appendChild(document.createElement("td")).innerHTML = `${String(
        booklist[i].from
      ).padStart(2, "0")}.00`;
      //to
      trBody.appendChild(document.createElement("td")).innerHTML = `${String(
        booklist[i].to
      ).padStart(2, "0")}.00`;
      //status
      td_status = document.createElement("td");
      td_status.className = "status";
      span = document.createElement("span");
      trBody.appendChild(td_status).appendChild(span).innerHTML =
        status[booklist[i].status];
      if (status[booklist[i].status] == "Using") {
        span.className = "using";
      } else if (status[booklist[i].status] == "Coming") {
        span.className = "coming";
      }
      //icon
      tdIcon = document.createElement("td");
      //view user icon
      if (booklist[i].userId < 1000000000) {
        v_icon = document.createElement("i");
        v_icon.className = "fas fa-eye view-button";
        v_icon.onclick = () => {
          window.location.pathname = `Search/UserProfile/${booklist[i].userId}`;
        };
        v_icon_div = document.createElement("div");
        v_icon_div.className = "icon";
        v_tooltip = document.createElement("span");
        v_tooltip.innerHTML = "View";
        v_tooltip.className = "tooltip";
        tdIcon.appendChild(v_icon_div).appendChild(v_icon);
        v_icon_div.appendChild(v_tooltip);
      }

      //cancel icon
      c_icon = document.createElement("i");
      if (
        status[booklist[i].status] == "Using" ||
        status[booklist[i].status] == "Coming"
      ) {
        c_icon.className = "fas fa-times-circle cancel-button";
        c_icon.onclick = function () {
          confirmPopUpOnForm({
            id: booklist[i].booklistId,
            api: booklist[i].userId >= 1000000000,
          });
        };
        c_tooltip_div = document.createElement("div");
        c_tooltip_div.className = "icon"
        c_tooltip = document.createElement("span");
        c_tooltip.innerHTML = "Eject";
        c_tooltip.className = "tooltip";
        tdIcon.appendChild(c_tooltip_div).appendChild(c_icon);
        c_tooltip_div.appendChild(c_tooltip);
      }

      tbody.appendChild(trBody).appendChild(tdIcon);
    }

    table.appendChild(thead);
    table.appendChild(tbody);
    result.appendChild(table);
  } else {
    text = document.createElement("h3");
    text.innerHTML = "No Booklist.";
    text.className = "no-booklist";
    result.appendChild(text);
  }

  return result;
}

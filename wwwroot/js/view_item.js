async function showResult(e, labId, date, timeslot) {
    e.stopPropagation();

    try {
        // get users
        let result = await makeRequest(
            "GET",
            `/LabAdmin/UserBookList?labId=${encodeURI(labId)}&date=${encodeURI(Date.parse(date))}&timeslot=${encodeURI(timeslot)}`
        );
        result = JSON.parse(result);
        //console.log(result);

        let bl_table = document.querySelector(".booklist-table");
        bl_table.innerHTML = "";
        bl_table.appendChild(createBooklistTable(result, new Date(date), timeslot));

    } catch (error) {
        console.error(error);
    }
};


function createBooklistTable(booklist, date, timeslot) {
    const day = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];
    const month = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
    const status = ["Using", "Coming", "Finished", "Cancel", "Eject"]
    const table_colname = ["#", "User Id", "Name", "From", "To", "status", ""];

    let result = document.createElement("div");
    result.className = "book-list-range";
    let title = document.createElement("h2");
    title.innerHTML = `${day[date.getDay()]} ${date.getDate()} ${month[date.getMonth()]} @ ${String(timeslot + 7).padStart(2, "0")}.00 - ${String(timeslot + 8).padStart(2, "0")}.00`;
    result.appendChild(title);

    if (booklist.length) {
        let table = document.createElement("table");
        let thead = document.createElement("thead");
        let tbody = document.createElement("tbody");
        let trHead = document.createElement("tr");

        // thead
        for (let i = 0; i < table_colname.length; i++) {
            trHead.appendChild(document.createElement("th")).innerHTML = table_colname[i];
        }
        thead.appendChild(trHead);

        // tbody
        for (let i = 0; i < booklist.length; i++) {
            let trBody = document.createElement("tr");
            //index number
            trBody.appendChild(document.createElement("td")).innerHTML = i + 1;
            //user ID
            trBody.appendChild(document.createElement("td")).innerHTML = String(booklist[i].userId).padStart(8, "0");
            //create a user image element
            user_img = document.createElement("img");
            user_img.setAttribute("src", booklist[i].userImageUrl);
            user_img.className = "user-img";
            user_name = document.createElement("td")
            trBody.appendChild(user_name).appendChild(user_img);
            //user fullname
            user_name.appendChild(document.createTextNode(booklist[i].fullname));
            //from
            trBody.appendChild(document.createElement("td")).innerHTML = `${String(booklist[i].from).padStart(2, "0")}.00`;
            //to
            trBody.appendChild(document.createElement("td")).innerHTML = `${String(booklist[i].to).padStart(2, "0")}.00`;
            //status
            td_status = document.createElement("td");
            td_status.className = "status";
            span = document.createElement("span");
            trBody.appendChild(td_status).appendChild(span).innerHTML = status[booklist[i].status];
            if (status[booklist[i].status] == "Using") {
                span.className = "using";
            } else if (status[booklist[i].status] == "Coming") {
                span.className = "coming";
            }
            //cancel icon
            icon = document.createElement("i");
            if (status[booklist[i].status] == "Using" || status[booklist[i].status] == "Coming") {
                icon.className = "fas fa-times-circle";
                icon.onclick = function () {
                    confirmPopUpOnForm({ id: booklist[i].booklistId });
                    console.log(booklist[i].booklistId);
                }
            }
            trBody.appendChild(document.createElement("td")).appendChild(icon);

            tbody.appendChild(trBody);
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
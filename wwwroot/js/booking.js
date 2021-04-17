let count = 0;

function duplicateBookInline() {
  count++; //count up
  const bookInline = document.getElementById("book-inline").cloneNode(true);
  // change attribute to attr_value[count]
  // book-inline change
  bookInline.id = bookInline.id + count;
  // date change
  const dateField = bookInline.children[0];
  // <label for="date">Date</label>
  dateField.children[0].setAttribute(
    "for",
    dateField.children[0].getAttribute("for") + count
  );
  // <input type="date" id="date" name="date"/>
  dateField.children[1].id = dateField.children[1].id + count;
  dateField.children[1].name = dateField.children[1].name + count;
  dateField.children[1].value = "";
  // from change
  const fromField = bookInline.children[1];
  // <label for="from">From</label>
  fromField.children[0].setAttribute(
    "for",
    fromField.children[0].getAttribute("for") + count
  );
  // <select name="from" id="from"></select>
  fromField.children[1].name = fromField.children[1].name + count;
  fromField.children[1].id = fromField.children[1].id + count;
  // from change
  const toField = bookInline.children[2];
  // <label for="to">To</label>
  toField.children[0].setAttribute(
    "for",
    toField.children[0].getAttribute("for") + count
  );
  // <select name="to" id="to"></select>
  toField.children[1].name = toField.children[1].name + count;
  toField.children[1].id = toField.children[1].id + count;
  // remove button
  const remove = bookInline.children[3].children[0];
  // <i class="fas fa-minus remove"></i>
  remove.className = "fas fa-minus remove";
  remove.onclick = () => bookInline.remove();
  // remove if has message-error
  if (bookInline.querySelector(".message-error") != null) {
    bookInline.children[4].remove();
  }
  // add to book
  document.getElementById("book").appendChild(bookInline);
}

async function validation() {
  let valid = true;
  // start date
  let startDate = Number(document.getElementById("startDateString").value);

  //createBookSlot Table
  let bookSlotTable = new Array(14);
  for (let index = 0; index < bookSlotTable.length; index++) {
    bookSlotTable[index] = new Array(9).fill(0);
  }

  // loop book-inline
  for (let index = 0; index <= count; index++) {
    const bookInline = document.getElementById(
      "book-inline" + (index ? index : "")
    );
    // validation deleted book-inline
    if (bookInline == null) {
      continue;
    }

    if (bookInline.querySelector(".message-error") != null) {
      bookInline.children[4].remove();
    }

    // message error template
    // <div class="message-error">
    //     <div>
    //         <i class="fas fa-exclamation-circle"></i>
    //         Lorem ipsum dolor sit amet.
    //     </div>
    // </div>

    const messageError = document.createElement("div");
    messageError.appendChild(document.createElement("div"));
    messageError.children[0].appendChild(document.createElement("i"));
    messageError.children[0].appendChild(document.createElement("span"));
    messageError.className = "message-error";
    messageError.children[0].children[0].className =
      "fas fa-exclamation-circle";
    messageError.children[0].children[0].style.marginRight = "10px";

    let dateValue = bookInline.children[0].children[1].value;
    let fromValue = bookInline.children[1].children[1].value;
    let toValue = bookInline.children[2].children[1].value;

    // validation parital null
    if (dateValue === "" || fromValue === "0" || toValue === "0") {
      // error all field required
      valid = false;
      //console.log("error all field required " + index);
      messageError.children[0].children[1].innerHTML =
        "Please complete all field.";
      bookInline.appendChild(messageError);
      continue;
    }

    dateValue = Date.parse(bookInline.children[0].children[1].value);
    fromValue = Number(bookInline.children[1].children[1].value);
    toValue = Number(bookInline.children[2].children[1].value);

    // validation date before and after interval
    if (
      dateValue < startDate ||
      dateValue >= startDate + 14 * 24 * 60 * 60 * 1000
    ) {
      // message error this date is't booking
      valid = false;
      //console.log("message error this date can't booking " + index);
      messageError.children[0].children[1].innerHTML =
        "The date is out of the boundary that you can book.";
      bookInline.appendChild(messageError);
      continue;
    }

    if (new Date(dateValue).getDay() % 6 == 0) {
      // message error you can't booking in weekend
      valid = false;
      //console.log("message error you can't booking in weekend " + index);
      messageError.children[0].children[1].innerHTML =
        "You cannot book an item on Weekend.";
      bookInline.appendChild(messageError);
      continue;
    }

    if (fromValue > toValue) {
      // message error 'from' to 'to' incorrect
      valid = false;
      //console.log("message error 'from' to 'to' incorrect range " + index);
      messageError.children[0].children[1].innerHTML =
        "You entered the wrong period.";
      bookInline.appendChild(messageError);
      continue;
    }

    if (dateValue == startDate && new Date().getHours() > fromValue + 7) {
      // message error time late
      valid = false;
      messageError.children[0].children[1].innerHTML =
        "The time is out of the boundary that you can book.";
      bookInline.appendChild(messageError);
      continue;
    }

    // loop assign into bookslot table
    for (let slot = fromValue - 1; slot < toValue; slot++) {
      // -----------------
      if (
        bookSlotTable[
          Math.floor((dateValue - startDate) / (24 * 60 * 60 * 1000))
        ][slot] == 1
      ) {
        if (bookInline.querySelector(".message-error") == null) {
          // message error time collapse
          valid = false;
          //console.log("message error time overlap " + index);
          messageError.children[0].children[1].innerHTML =
            "You cannot enter the period that overlaps.";
          bookInline.appendChild(messageError);
        }
        continue;
      }
      // -----------------
      if (
        document.getElementById(
          `slot${slot}_${Math.floor(
            (dateValue - startDate) / (24 * 60 * 60 * 1000)
          )}`
        ).className === "booked"
      ) {
        if (bookInline.querySelector(".message-error") == null) {
          // message error amount <= 0
          valid = false;
          //console.log("message error time overlap " + index);
          messageError.children[0].children[1].innerHTML =
            "You have already booked this period.";
          bookInline.appendChild(messageError);
        }
        continue;
      }
      // -----------------
      if (
        Number(
          document.getElementById(
            `slot${slot}_${Math.floor(
              (dateValue - startDate) / (24 * 60 * 60 * 1000)
            )}`
          ).innerHTML
        ) <= 0
      ) {
        if (bookInline.querySelector(".message-error") == null) {
          // message error amount <= 0
          valid = false;
          //console.log("message error time overlap " + index);
          messageError.children[0].children[1].innerHTML =
            "You cannot enter the period that items full.";
          bookInline.appendChild(messageError);
        }
        continue;
      }
      bookSlotTable[
        Math.floor((dateValue - startDate) / (24 * 60 * 60 * 1000))
      ][slot] = 1;
    }
  }

  if (!valid) return;
  //convert bookslot table to book-list
  let bookRanges = new Array(0);
  for (let date = 0; date < 14; date++) {
    let start = 0;
    let end = 0;
    for (let timeslot = 0; timeslot < 9; timeslot++) {
      if (bookSlotTable[date][timeslot] == 1 && start == 0) {
        start = timeslot + 8;
      }
      if (bookSlotTable[date][timeslot] == 1 && start > 0) {
        end = timeslot + 9;
      }
      if (bookSlotTable[date][timeslot] == 0 && start > 0) {
        bookRanges.push({
          date: date * 24 * 60 * 60 * 1000 + startDate,
          from: start,
          to: end,
        });
        start = end = 0;
      }
    }
    if (start > 0) {
      bookRanges.push({
        date: date * 24 * 60 * 60 * 1000 + startDate,
        from: start,
        to: end,
      });
    }
  }
  updateBodyBookList(bookRanges);
  let response = await confirmPopUpOnJson(bookRanges);
  if (response.success) window.location.href = "/User/Booklist";
  else if (response.error) console.error(response.error);
}

function updateBodyBookList(bookRanges) {
  const body = document.querySelector("#body-list");
  body.innerHTML = "";
  for (const bookRange of bookRanges) {
    const row = document.createElement("tr");
    const createTr = (value) => {
      const column = document.createElement("td");
      column.innerHTML = value;
      return column;
    };
    const dateTransform = (date_ms) => {
      const date = new Date(date_ms);
      const DAYS = Object.freeze([
        "Sunday",
        "Monday",
        "Tuesday",
        "Wednesday",
        "Thursday",
        "Friday",
        "Saturday",
      ]);
      const MONTHS = Object.freeze([
        "January",
        "February",
        "March",
        "April",
        "May",
        "June",
        "July",
        "August",
        "September",
        "October",
        "November",
        "December",
      ]);
      return `${DAYS[date.getDay()].slice(0, 3)} ${date.getDate()} ${MONTHS[
        date.getMonth()
      ].slice(0, 3)} ${date.getFullYear()}`;
    };
    const timeTransform = (time) => (time < 10 ? `0${time}.00` : `${time}.00`);
    row.appendChild(createTr(dateTransform(bookRange.date)));
    row.appendChild(createTr(timeTransform(bookRange.from)));
    row.appendChild(createTr(timeTransform(bookRange.to)));
    body.appendChild(row);
  }
}

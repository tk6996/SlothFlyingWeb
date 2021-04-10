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
  const remove = bookInline.children[4].children[0];
  remove.className = "fas fa-minus remove";
  // <i class="fas fa-minus remove"></i>
  // add to book
  document.getElementById("book").appendChild(bookInline);
}

function removeBookInline(nodeElement) {
  nodeElement.parentElement.parentElement.remove();
}

function validation() {
  let startDate = new Date(document.getElementById("startDateString").value);
  for (let index = 0; index <= count; index++) {
      const bookInline = document.getElementById("book-inline" + (index ? index : ""));
      if(bookInline == null) 
        continue;
      const dateValue = bookInline.children[0].children[1].value;
      const fromValue = bookInline.children[1].children[1].value;
      const toValue = bookInline.children[2].children[1].value;
      if(dateValue == "" || fromValue == 0 || toValue == 0)
        continue;
  }
}

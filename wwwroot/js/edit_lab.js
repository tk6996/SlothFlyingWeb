window.onkeydown = (event) => {
  if (event.keyCode == 13) {
    event.preventDefault();
    return false;
  }
};

const beforeAmount = Number(document.getElementById("Amount").value);

function readUrl(input) {
  if (input.files && input.files[0]) {
    let reader = new FileReader();

    reader.onload = (event) => {
      document
        .getElementById("lab_image")
        .setAttribute("src", event.target.result);
    };

    reader.readAsDataURL(input.files[0]);
  }
}

function updateAmount(amount) {
  document.getElementById("Amount").value = amount;
  if (amount < beforeAmount) {
    console.log("a");
    document.getElementById("Amount").style.color = "var(--red)";
  } else if (amount > beforeAmount) {
    console.log("b");
    document.getElementById("Amount").style.color = "var(--green)";
  } else {
    console.log("c");
    document.getElementById("Amount").style.color = "#000000";
  }
}

function decreseAmount() {
  let amount = Number(document.getElementById("Amount").value);
  if (amount > 0) {
    amount--;
    updateAmount(amount);
  }
}
function increseAmount() {
  let amount = Number(document.getElementById("Amount").value);
  if (amount < 999) {
    amount++;
    updateAmount(amount);
  }
}

function changeAmount() {
  let amount = Number(document.getElementById("Amount").value);
  if (amount < 0) {
    amount = 0;
  } else if (amount > 999) {
    amount = 999;
  }
  updateAmount(amount);
}

function resetAmount() {
  updateAmount(beforeAmount);
}

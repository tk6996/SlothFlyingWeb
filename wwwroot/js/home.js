var modal = document.getElementById("myModal");

// Get the button that opens the modal
var btn = document.getElementById("myBtn");

// Get the <span class="close"> element that closes the modal
var close = document.getElementById("close");

// When the user clicks the button, open the modal
function visibleModal () {
  modal.className = "modal";
};

// When the user clicks on <span class="close"> (x), close the modal
function invisibleModal() {
  modal.className = "modal off";
};

// When the user clicks anywhere outside of the modal, close it
window.onclick = function (event) {
  if (event.target == modal) {
    modal.className = "modal off";
  }
};

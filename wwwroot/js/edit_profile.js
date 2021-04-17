function readUrl(input) {
  if (input.files && input.files[0]) {
    let reader = new FileReader();

    reader.onload = (event) => {
      document
        .getElementById("user_image")
        .setAttribute("src", event.target.result);
    };

    reader.readAsDataURL(input.files[0]);
  }
}

document.getElementById("Id").value = String(
  document.getElementById("Id").value
).padStart(8, "0");

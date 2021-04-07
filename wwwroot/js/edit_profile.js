function readURL(input) {
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

function numberZeroPadding(number, padding) {
  let numstr = String(number);
  console.log(typeof numstr);
  for (let i = numstr.length; i < padding; i++) {
    numstr = "0".concat(numstr);
  }
  return numstr;
}

document.getElementById("Id").value = numberZeroPadding(
  document.getElementById("Id").value,
  8
);

// Number with leading zero 8 places
const zeroPad = (num, places) => String(num).padStart(places, '0');

// Get the loader
const loader = document.getElementById("loader");
// Set to do not display the loader
loader.style.display = "none";

// Get the input element
var input = document.getElementById("input");

// Trigger button click on enter key
input.addEventListener("keyup", function (event) {
    if (event.keyCode === 13) {
        event.preventDefault();
        document.getElementById("search-button").click();
    }
});

// Remove all childnodes
function removeAllChildNodes(parent) {
    while (parent.firstChild) {
        parent.removeChild(parent.firstChild);
    }
}

function post(name, value, method = 'post') {
    const form = document.createElement('form');
    form.method = method;

    const hiddenField = document.createElement('input');
    hiddenField.type = 'hidden';
    hiddenField.name = name;
    hiddenField.value = value;

    form.appendChild(hiddenField);
    document.body.appendChild(form);
    form.submit();
}

// Create search results
function createResultElement(parent, user) {
    let result = document.createElement("div");
    result.className = "user-container";
    user_img = document.createElement("img");
    user_img.setAttribute("src", user.imageUrl != "" ? user.imageUrl.replace("~", "..") : "../assets/images/brand.jpg");
    user_img.className = "user-img";
    result.appendChild(user_img);
    result.appendChild(document.createElement("div")).innerHTML = zeroPad(user.id, 8);
    result.appendChild(document.createElement("div")).innerHTML = user.firstName
    result.appendChild(document.createElement("div")).innerHTML = user.lastName
    parent.appendChild(result);
    result.onclick = function () { post("id", user.id) };
}

// Search results
function SearchResult(users) {
    let ans = false;
    let input_value = document.getElementById("input").value.toLowerCase();
    let all_result = document.getElementById("search-result");
    all_result.style.display = "none";
    removeAllChildNodes(all_result);
    input_arr = input_value.split(" ").filter(e => e.trim().length > 0);

    for (let i = 0; i < users.length; i++) {
        let firstname = users[i].firstName.toLowerCase();
        let lastname = users[i].lastName.toLowerCase();
        let id = users[i].id.toString();
        let full_id = zeroPad(users[i].id, 8);

        if (input_arr.length === 1 && (firstname.startsWith(input_arr[0])
            || lastname.startsWith(input_arr[0]) || id.startsWith(input_arr[0])
            || full_id.startsWith(input_arr[0]))) {
            createResultElement(all_result, users[i]);
            ans = true;
        } else if (input_arr.length === 2
            && ((firstname.startsWith(input_arr[0]) && lastname.startsWith(input_arr[1]))
                || (lastname.startsWith(input_arr[0]) && firstname.startsWith(input_arr[1])))) {
            createResultElement(all_result, users[i]);
            ans = true;
        }
    }

    if (ans === false) {
        all_result.appendChild(document.createElement("h1")).innerHTML = "No Results"
    }
}

// Show the loader
function showLoader() {
    myVar = setTimeout(showResult, 500);
    loader.style.display = "";
}

// Show search results
function showResult() {
    loader.style.display = "none";
    document.getElementById("search-result").style.display = "";
}
// Create search results
function createResultElement(user) {
  let result = document.createElement("div");
  result.className = "user-container";
  user_img = document.createElement("img");
  user_img.setAttribute("src", user.imageUrl);
  user_img.className = "user-img";
  result.appendChild(user_img);
  result.appendChild(document.createElement("div")).innerHTML = String(
    user.id
    // Number with leading zero 8 places
  ).padStart(8, "0");
  result.appendChild(document.createElement("div")).innerHTML = user.firstName;
  result.appendChild(document.createElement("div")).innerHTML = user.lastName;
  result.onclick = () => {
    window.location.pathname = `Search/UserProfile/${user.id}`;
  };
  return result;
}

// Search results
async function SearchResult() {
  let input_value = document.getElementById("input").value;

  if (input_value === "") {
    input_value.className = "empty";
  } else {
    input_value.className = "";
  }

  let searchResult = document.getElementById("search-result");
  try {
    // get users
    let result = await makeRequest(
      "GET",
      `/Search/UserList?search=${encodeURI(input_value)}`
    );
    result = JSON.parse(result);
    // clear result
    searchResult.innerHTML = "";
    // if no result
    if (result.length === 0) {
      searchResult.appendChild(document.createElement("h1")).innerHTML =
        "No Results";
    } else {
      for (const user of result) {
        searchResult.appendChild(createResultElement(user));
      }
    }
  } catch (error) {
    console.error(error);
  }
}

function slideBar() {
    var nav = document.getElementById("Topnav");
    if (nav.className === "menu") {
        nav.className += " responsive";
        document.getElementById("Sidenav").style.width = "250px";
    } else {
        nav.className = "menu";
        document.getElementById("Sidenav").style.width = "0";
    }
}
function registMenu() {
    $('.menu-trigger').on('click', menuBtClick);
}

function menuBtClick() {
    $(this).toggleClass("active");
    $("#menu-container").toggleClass("top-menu-bt-cover-click");
    $("#menu").toggleClass("menu-cover-click");
    $("#body-content").toggleClass("body-content-click");
}
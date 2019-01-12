function registMenu() {
    $('.menu-trigger').on('click', menuBtClick);
}

function menuBtClick() {
    $(this).toggleClass("active");
    $("#top-menu-bt-cover").toggleClass("top-menu-bt-cover-click");
    $("#menu").toggleClass("menu-cover-click");
}
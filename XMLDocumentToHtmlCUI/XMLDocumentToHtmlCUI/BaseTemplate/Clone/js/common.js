
/*
 *
 * Copyright (C) 2018 - 2020 Aona Suzutsuki.
 * 
 */

function registMenu() {
    $('.menu-trigger').on('click', menuBtClick);
}

function menuBtClick() {
    $(this).toggleClass("active");
    $("body").toggleClass("overflow-hidden");
    $("#menu-container").toggleClass("top-menu-bt-cover-click");
    $("#menu").toggleClass("menu-cover-click");
    $("#body-content").toggleClass("body-content-click");
}
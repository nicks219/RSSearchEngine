export function hideMenu(cssProperty: string): any {
    if (cssProperty !== "none") {
        cssProperty = "none";
        // внешняя зависимость
        (document.getElementById("login")as HTMLElement).style.display = "none";
        return cssProperty;
    }

    cssProperty = "block";
    return cssProperty;
}
// Засена видимости меню на противоположное, см. login: MessageOn - MessageOff и hideMenu по коду
export function menuHandler(cssProperty: string): string {
    if (cssProperty !== "none") {
        cssProperty = "none";
        (document.getElementById("login")as HTMLElement).style.display = "none";
        return cssProperty;
    }

    cssProperty = "block";
    return cssProperty;
}

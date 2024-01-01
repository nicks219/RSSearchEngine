// login: MessageOn - MessageOff ?
export function menuHandler(cssProperty: string): any {
    if (cssProperty !== "none") {
        cssProperty = "none";
        (document.getElementById("login")as HTMLElement).style.display = "none";
        return cssProperty;
    }

    cssProperty = "block";
    return cssProperty;
}

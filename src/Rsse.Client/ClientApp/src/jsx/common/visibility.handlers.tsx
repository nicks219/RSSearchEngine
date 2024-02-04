import {createRoot} from "react-dom/client";
import {FunctionComponentStateWrapper} from "./state.wrappers.tsx";

/** Изменить видимость контейнера с меню на противоположную, см. по коду LoginBoxHandler: Visible/Invisible + hideMenu */
export const toggleMenuVisibility = (cssProperty: string): string => {
    if (cssProperty !== "none") {
        cssProperty = "none";
        (document.getElementById("login") as HTMLElement).style.display = "none";
        return cssProperty;
    }

    cssProperty = "block";
    return cssProperty;
}

/** Функционал изменения видимости контейнера с логином */
export class LoginBoxHandler {
    static login = false;
    static loginMessageElement = document.querySelector("#loginMessage") ?? document.createElement('loginMessage');
    static loginMessageRoot = createRoot(this.loginMessageElement);

    static SetInvisible = () => {
        if (!LoginBoxHandler.login) return;
        (document.getElementById("loginMessage") as HTMLElement).style.display = "none";
    }

    static SetVisible<T>(stateWrapper: FunctionComponentStateWrapper<T>, url: string) {
        window.stateWrapperStorage = stateWrapper;
        window.urlStorage = url;

        (document.getElementById("loginMessage") as HTMLElement).style.display = "block";
        (document.getElementById("login") as HTMLElement).style.display = "block";
        LoginBoxHandler.login = true;
        LoginBoxHandler.loginMessageRoot.render(
            <h1>Login please</h1>
        );
    }
}

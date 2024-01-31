import {createRoot} from "react-dom/client";
import {Component} from "react";
import {IMountedComponent} from "./contracts.tsx";
import {FunctionComponentStateWrapper} from "./state.wrappers.tsx";
import {getPageNumber} from "./dto.handlers.tsx";
import {CatalogResponseDto} from "../dto/request.response.dto.tsx";

/** Изменить видимость контейнера с меню на противоположную, см. login: Visible/Invisible и hideMenu() по коду */
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

    static SetVisible<T>(component: (Component & IMountedComponent)|FunctionComponentStateWrapper<T>, url: string) {
        window.temp = component;
        window.url = url;

        // FC / IMounted switch:
        if (component instanceof FunctionComponentStateWrapper) {
            const castedComponent = component as FunctionComponentStateWrapper<T>;
            window.pageNumber = getPageNumber(castedComponent.data as CatalogResponseDto);
        }
        else {
            const state: Readonly<object> = (component as (Component & IMountedComponent)).state;
            if (Object.prototype.hasOwnProperty.call(state, 'data')) {
                const data = 'data' as keyof typeof state;
                window.pageNumber = getPageNumber(state[data]);
            }
        }

        (document.getElementById("loginMessage") as HTMLElement).style.display = "block";
        (document.getElementById("login") as HTMLElement).style.display = "block";
        LoginBoxHandler.login = true;
        LoginBoxHandler.loginMessageRoot.render(
            <h1>
                LOGIN PLEASE
            </h1>
        );
    }
}

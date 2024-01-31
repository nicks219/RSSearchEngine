import * as React from 'react';
import { Loader } from "./loader.tsx";
import { getPageNumber } from "../dto/handler.catalog.tsx";
import { createRoot } from "react-dom/client";
import { Component, useState } from "react";
import { IMountedComponent } from "../contracts/i.mounted.tsx";

declare global {
    interface Window { textId: number, temp: Component & IMountedComponent, url: string, pageNumber: number|undefined }
}

export class LoginBoxHandler {
    static login = false;
    static loginMessageElement = document.querySelector("#loginMessage") ?? document.createElement('loginMessage');
    static loginMessageRoot = createRoot(this.loginMessageElement);

    static Invisible = () => {
        if (!LoginBoxHandler.login) return;
        (document.getElementById("loginMessage") as HTMLElement).style.display = "none";
    }

    static Visible = (component: Component & IMountedComponent, url: string) => {
        window.temp = component;
        window.url = url;
        const state: Readonly<object> = component.state;
        if (Object.prototype.hasOwnProperty.call(state, 'data')) {
            const data = 'data' as keyof typeof state;
            window.pageNumber = getPageNumber(state[data]);
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

export const LoginComponent = () => {
    const [style, setStyle] = useState("submitStyle");

    let loginElement = document.getElementById("login") as HTMLElement ?? document.createElement('login');
    loginElement.style.display = "block";

    const submit = (e: React.SyntheticEvent) => {
        e.preventDefault();
        let email = "test@email";
        let password = "password";
        let emailElement = document.getElementById('email') as HTMLInputElement;
        let passwordElement = document.getElementById('password') as HTMLInputElement;
        if (emailElement) email = emailElement.value;
        if (passwordElement) password = passwordElement.value;

        let query = "?email=" + String(email) + "&password=" + String(password);
        let callback = (response: Response) => response.ok ? loginOk() : loginErr();
        Loader.fireAndForgetWithQuery(Loader.loginUrl, query, callback, null);
    }

    const loginErr = () => {
        document.cookie = 'rsse_auth = false';
        console.log("Login error");
    }

    const loginOk = () => {
        document.cookie = 'rsse_auth = true';
        console.log("Login ok.");
        setStyle("submitStyleGreen");
        continueLoading();
        setTimeout(() => {
            (document.getElementById("login") as HTMLElement).style.display = "none";
        }, 1500);
    }

    // восстанавливаем данные (но не последнее действие), не полученные из-за ошибки авторизации:
    const continueLoading = () => {
        let component = window.temp;

        if (component) {
            // login для update:
            if (window.url === Loader.updateUrl) {
                // Loader в случае ошибки вызовет MessageOn()
                Loader.unusedPromise = Loader.getDataById(component, window.textId, window.url);
                // login для catalog:
            } else if (window.url === Loader.catalogUrl) {
                Loader.unusedPromise = Loader.getDataById(component, getPageNumber(window), window.url);
            }
            // login для остальных компонентов, кроме случая когда последним лействием было logout:
            else if (window.url !== Loader.logoutUrl) {
                Loader.unusedPromise = Loader.getData(component, window.url);
            }
        }

        LoginBoxHandler.Invisible();
    }

    return (
        <div>
            <div id={style}>
                <input type="checkbox" id="loginButton" className="regular-checkbox" onClick={submit}/>
                <label htmlFor="loginButton">Войти</label>
            </div>
            &nbsp;&nbsp;&nbsp;&nbsp;
            <span>
                    <input type="text" id="email" name="email" autoComplete={"on"}/>
                </span>
            &nbsp;&nbsp;&nbsp;&nbsp;
            <span>
                    <input type="text" id="password" name="password" autoComplete={"on"}/>
                </span>
        </div>
    );
}

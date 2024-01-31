import * as React from 'react';
import { Component, useState } from "react";
import { Loader } from "../common/loader.tsx";
import { IMountedComponent } from "../common/contracts.tsx";
import { FunctionComponentStateWrapper } from "../common/state.wrappers.tsx";
import { getPageNumber } from "../common/dto.handlers.tsx";
import { LoginBoxHandler } from "../common/visibility.handlers.tsx";

declare global {
    // избавляйся от <any>: тип данных для смены стейта знает только компонент отображения (и Loader) - нужно ли продолжение загрузки?
    interface Window { textId: number, temp: (Component & IMountedComponent)|FunctionComponentStateWrapper<any>, url: string, pageNumber: number|undefined }
}

export const LoginComponent = () => {
    const [style, setStyle] = useState("submitStyle");

    let loginElement = document.getElementById("login") as HTMLElement ?? document.createElement('login');
    loginElement.style.display = "block";

    const onSubmit = (e: React.SyntheticEvent) => {
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

    // загружаем в компонент данные, не отданные сервисом из-за ошибки авторизации:
    const continueLoading = () => {
        let component = window.temp;

        if (component) {
            // продолжение для update:
            if (window.url === Loader.updateUrl) {
                // Loader в случае ошибки вызовет MessageOn()
                Loader.unusedPromise = Loader.getDataById(component, window.textId, window.url);
            // продолжение для catalog:
            } else if (window.url === Loader.catalogUrl) {
                Loader.unusedPromise = Loader.getDataById(component, getPageNumber(window), window.url);
            }
            // продолжение для остальных компонентов, кроме случая когда последним лействием было logout:
            else if (window.url !== Loader.logoutUrl) {
                Loader.unusedPromise = Loader.getData(component, window.url);
            }
        }

        LoginBoxHandler.SetInvisible();
    }

    return (
        <div>
            <div id={style}>
                <input type="checkbox" id="loginButton" className="regular-checkbox" onClick={onSubmit}/>
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

import * as React from 'react';
import {useContext, useState} from "react";
import {Loader} from "../common/loader";
import {LoginBoxVisibility} from "../common/visibility.handlers";
import {CommonContext} from "../common/context.provider";

export const LoginComponent = () => {
    const style = useState("submitStyle");
    const context = useContext(CommonContext);

    const loginElement = document.getElementById("login") as HTMLElement ?? document.createElement('login');
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
        style[1]("submitStyleGreen");
        continueLoading();
        setTimeout(() => {
            const loginElement = document.getElementById("login") as HTMLElement;
            loginElement.style.display = "none";
        }, 1500);
    }

    // загружаем в компонент данные, не полученные из-за ошибки авторизации:
    const continueLoading = () => {
        const stateWrapper = context.stateWrapper;

        if (stateWrapper) {
            // продолжение для update:
            if (context.commonString === Loader.updateUrl) {
                // Loader в случае ошибки вызовет MessageOn()
                Loader.unusedPromise = Loader.getDataById(stateWrapper, context.commonNumber, context.commonString);
            // продолжение для catalog: загрузка первой страницы:
            } else if (context.commonString === Loader.catalogUrl) {
                const id = 1;
                Loader.unusedPromise = Loader.getDataById(stateWrapper, id, context.commonString);
            }
            // продолжение для остальных компонентов, кроме случая когда последним лействием было logout:
            else if (context.commonString !== Loader.logoutUrl) {
                Loader.unusedPromise = Loader.getData(stateWrapper, context.commonString);
            }
        }

        LoginBoxVisibility(false);
    }

    return (
        <div id="login">
            <div id={style[0]}>
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

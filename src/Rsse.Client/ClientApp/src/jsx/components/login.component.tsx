import * as React from 'react';
import {useState} from "react";
import {Loader} from "../common/loader";
import {LoginBoxHandler} from "../common/visibility.handlers";
import {CommonStateStorage} from "../common/state.wrappers";

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
        const stateWrapper = CommonStateStorage.stateWrapperStorage;

        if (stateWrapper) {
            // продолжение для update:
            if (CommonStateStorage.commonString === Loader.updateUrl) {
                // Loader в случае ошибки вызовет MessageOn()
                Loader.unusedPromise = Loader.getDataById(stateWrapper, CommonStateStorage.noteIdStorage, CommonStateStorage.commonString);
            // продолжение для catalog: загрузка первой страницы:
            } else if (CommonStateStorage.commonString === Loader.catalogUrl) {
                const id = 1;
                Loader.unusedPromise = Loader.getDataById(stateWrapper, id, CommonStateStorage.commonString);
            }
            // продолжение для остальных компонентов, кроме случая когда последним лействием было logout:
            else if (CommonStateStorage.commonString !== Loader.logoutUrl) {
                Loader.unusedPromise = Loader.getData(stateWrapper, CommonStateStorage.commonString);
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

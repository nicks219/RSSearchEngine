import * as React from 'react';
import {useContext, useState} from "react";
import {Loader} from "../common/loader";
import {LoginBoxVisibility} from "../common/visibility.handlers";
import {CommonContext, RecoveryContext} from "../common/context.provider";
import {LoginView} from "./login.view";

export const LoginContainer = () => {
    const style = useState("submitStyle");
    const commonContext = useContext(CommonContext);
    const recoveryContext = useContext(RecoveryContext);

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
        console.log("Login error.");
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
        const stateWrapper = recoveryContext.recoveryStateWrapper;

        // TODO: замени на свич
        if (stateWrapper) {
            switch (recoveryContext.recoveryString) {
                // продолжение для update:
                case Loader.updateUrl: {
                    const id = commonContext.commonNumber;
                    Loader.unusedPromise = Loader.getDataById(stateWrapper, id, Loader.updateUrl);
                    break;
                }
                // продолжение для delete (catalog): загрузка первой страницы:
                case Loader.catalogUrl: {
                    const id = 1;
                    Loader.unusedPromise = Loader.getDataById(stateWrapper, id, Loader.catalogUrl);
                    break;
                }
                case Loader.migrationRestoreUrl:
                    Loader.unusedPromise = Loader.getData(stateWrapper, Loader.migrationRestoreUrl);
                    break;
                case Loader.migrationCreateUrl:
                    Loader.unusedPromise = Loader.getData(stateWrapper, Loader.migrationCreateUrl);
                    break;
                case Loader.createUrl:
                    Loader.unusedPromise = Loader.getData(stateWrapper, Loader.createUrl);
                    break;
                default:
                    throw new Error(`Unknown recovery url saved: ${recoveryContext.recoveryString}`);
            }
        }

        LoginBoxVisibility(false);
    }

    return(<LoginView id={style[0]} onClick={onSubmit} />)
}

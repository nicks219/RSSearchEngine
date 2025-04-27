import * as React from 'react';
import {useContext, useEffect, useState} from "react";
import {Loader} from "../common/loader";
import {setLoginBoxVisibility} from "../common/visibility.handlers";
import {CommonContext, RecoveryContext} from "../common/context.provider";
import {LoginView} from "./login.view";
import {Doms, Messages, SystemConstants} from "../dto/doms.tsx";

export const LoginContainer = () => {
    const [style, setStyle] = useState(Doms.submitStyle);

    const commonContext = useContext(CommonContext);
    const recoveryContext = useContext(RecoveryContext);

    commonContext.stringState = setStyle;

    const loginElement = document.getElementById(Doms.loginName) as HTMLElement ?? document.createElement(Doms.loginName);
    loginElement.style.display = SystemConstants.block;

    useEffect(() => {
        onMount();
        return;
    }, []);

    const onMount = () => {
        let callback = (response: Response) => response.ok ? loginOk() : loginErr();
        Loader.fireAndForgetWithQuery(Loader.checkAuth, "", callback, null);
        document.querySelector('#main')?.classList.remove('footer-hidden');
    }

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
        console.log(Messages.loginError);
        localStorage.setItem('isAuth', 'false');
    }

    const loginOk = () => {
        document.cookie = 'rsse_auth = true';
        console.log(Messages.loginOk);
        localStorage.setItem('isAuth', 'true');

        // todo: SetVisible - после log out без обновления страницы стейт и dom расходятся
        // todo: следует сделать полноценный компонент авторизации и отвязать видимость от css

        continueLoadingAfterLogin();
        setStyle(Doms.submitStyleGreen);

        setTimeout(() => {
            const loginElement = document.getElementById(Doms.loginName) as HTMLElement;
            if (loginElement) {
                loginElement.style.display = SystemConstants.none;
                document.querySelector('#main')?.classList.add('footer-hidden');
            }
        }, 1500);
    }

    // загружаем в компонент данные, не полученные из-за ошибки авторизации:
    const continueLoadingAfterLogin = () => {
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
                case Loader.logoutUrl:
                    break;
                default:
                    throw new Error(`Unknown recovery url saved: ${recoveryContext.recoveryString}`);
            }
        }

        setLoginBoxVisibility(false);
    }

    // return(localStorage.getItem('isAuth') === 'false' && <LoginView id={style} onClick={onSubmit} />)
    return(<LoginView id={style} onClick={onSubmit} />)
}

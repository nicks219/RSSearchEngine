import * as React from 'react';
import { LoaderComponent } from "./loader.component.tsx";
import { getPageNumber } from "../dto/dto.catalog.tsx";
import {createRoot} from "react-dom/client";

interface IState {
    style: any;
}

declare global {
    interface Window { textId: number, temp: any, url: string, pageNumber: number }
}

interface IProps {
    subscription: any;
    formId: any;
    jsonStorage: any;
    id: any;
}

export class LoginBoxHandler {
    static login = false;
    static loginMessageElement = document.querySelector("#loginMessage") ?? new Element();
    static loginMessageRoot = createRoot(this.loginMessageElement);

    // восстанавливаем данные (но не последнее действие), не полученные из-за ошибки авторизации:
    static ContinueLoading() {
        let component = window.temp;

        if (component) {
            // login для update:
            if (window.url === LoaderComponent.updateUrl) {
                // Loader в случае ошибки вызовет MessageOn()
                LoaderComponent.unusedPromise = LoaderComponent.getDataById(component, window.textId, window.url);
                // login для catalog:
            } else if (window.url === LoaderComponent.catalogUrl) {
                LoaderComponent.unusedPromise = LoaderComponent.getDataById(component, getPageNumber(window), window.url);
            }
            // login для остальных компонентов, кроме случая когда последним лействием было logout:
            else if (window.url !== LoaderComponent.logoutUrl) {
                LoaderComponent.unusedPromise = LoaderComponent.getData(component, window.url);
            }
        }

        this.Invisible();
    }

    static Invisible() {
        if (!this.login) return;
        (document.getElementById("loginMessage")as HTMLElement).style.display = "none";
    }

    // hideMenu ?
    static Visible(component: any, url: string) {
        window.temp = component;
        window.url = url;
        window.pageNumber = getPageNumber(component.state.data);

        (document.getElementById("loginMessage")as HTMLElement).style.display = "block";
        (document.getElementById("login")as HTMLElement).style.display = "block";
        this.login = true;
        LoginBoxHandler.loginMessageRoot.render(
            <h1>
                LOGIN PLEASE
            </h1>
        );
    }
}

export class LoginComponent extends React.Component<IProps, IState> {

    public state: IState = {
        style: "submitStyle"
    }

    constructor(props: any) {
        super(props);
        this.submit = this.submit.bind(this);

        (document.getElementById("login")as HTMLElement).style.display = "block";
    }

    submit(e: any) {
        e.preventDefault();
        let email = "test@email";
        let password = "password";
        let emailElement = document.getElementById('email') as HTMLInputElement;
        let passwordElement = document.getElementById('password') as HTMLInputElement;
        if (emailElement) email = emailElement.value;
        if (passwordElement) password = passwordElement.value;

        let query = "?email=" + String(email) + "&password=" + String(password);
        let callback = (response: Response) => response.ok ? this.loginOk() : this.loginErr();

        LoaderComponent.fireAndForgetWithQuery(LoaderComponent.loginUrl, query, callback, null);
    }

    loginErr = () => {
        document.cookie = 'rsse_auth = false';
        console.log("Login error");
    }

    loginOk = () => {
        document.cookie = 'rsse_auth = true';
        console.log("Login ok.");
        this.setState({ style: "submitStyleGreen" });
        LoginBoxHandler.ContinueLoading();
        setTimeout(() => {
            (document.getElementById("login")as HTMLElement).style.display = "none";
        }, 1500);
    }

    componentWillUnmount() {
        // отменяй подписки и асинхронную загрузку
    }

    render() {
        return (
            <div>
                <div id={this.state.style}>
                    <input type="checkbox" id="loginButton" className="regular-checkbox" onClick={this.submit} />
                    <label htmlFor="loginButton">Войти</label>
                </div>
                {/*<div id={this.state.style}>*/}
                &nbsp;&nbsp;&nbsp;&nbsp;
                <span>
                    <input type="text" id="email" name="email" autoComplete={ "on" } />
                </span>
                {/*<div id={this.state.style}>*/}
                &nbsp;&nbsp;&nbsp;&nbsp;
                <span>
                    <input type="text" id="password" name="password" autoComplete={ "on" } />
                </span>
            </div>
        );
    }
}

import * as React from 'react';
import * as ReactDOM from 'react-dom';

import { Loader } from "./loader";

interface IState {
    style: any;
}

declare global {
    interface Window { textId: number, temp: any, url: string }
}

interface IProps {
    listener: any;
    formId: any;
    jsonStorage: any;
    id: any;
}

export class LoginRequired {
    // восстанавливаем данные (но не последнее действие), не полученные из-за ошибки авторизации
    // [TODO] сейчас у компонентов нет component.url - их надо докидывать отдельно
    static ContinueLoading() {
        let component = window.temp;
        if (component) {
            if (component.url === Loader.updateUrl) {
                // Loader в случае ошибки вызовет MessageOn()
                Loader.getDataById(component, window.textId, window.url);
            } else if (component.url === Loader.catalogUrl){
                Loader.getDataById(component, component.state.data.pageNumber, window.url);
            }
            else {
                Loader.getData(component, window.url);
            }
        }
        
        this.MessageOff();
    }

    static MessageOff() {
        (document.getElementById("loginMessage")as HTMLElement).style.display = "none";
    }

    static MessageOn(component: any, url: string) {
        window.temp = component;
        window.url = url;
        
        (document.getElementById("loginMessage")as HTMLElement).style.display = "block";
        (document.getElementById("login")as HTMLElement).style.display = "block";
        ReactDOM.render(
            <h1>
                LOGIN PLEASE
            </h1>
            , document.querySelector("#loginMessage")
        );
    }
}

export class Login extends React.Component<IProps, IState> {

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
        let email = "test_e";
        let password = "test_p";
        let emailElement = document.getElementById('email') as HTMLInputElement;
        let passwordElement = document.getElementById('password') as HTMLInputElement;
        if (emailElement) email = emailElement.value;
        if (passwordElement) password = passwordElement.value;

        let query = "?email=" + String(email) + "&password=" + String(password);
        let callback = (response: Response) => response.ok ? this.loginOk() : this.loginErr();

        Loader.getWithQuery(Loader.loginUrl, query, callback, null);
    }
    
    loginErr = () => {
        // установим локальные куки
        document.cookie = 'rsse_auth = false';
        console.log("Login error");
    }

    loginOk = () => {
        // установим локальные куки
        document.cookie = 'rsse_auth = true';
        console.log("Login ok");
        this.setState({ style: "submitStyleGreen" });
        LoginRequired.ContinueLoading();
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
                    <input type="text" id="email" name="email" />
                </span>
                {/*<div id={this.state.style}>*/}
                &nbsp;&nbsp;&nbsp;&nbsp;
                <span>
                    <input type="text" id="password" name="password" />
                </span>
            </div>
        );
    }
}
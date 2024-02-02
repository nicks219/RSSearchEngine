import { LoginBoxHandler } from "./visibility.handlers.tsx";
import { Component } from "react";
import { IMountedComponent } from "./contracts.tsx";
import { FunctionComponentStateWrapper } from "./state.wrappers.tsx";
import {IDataTimeState} from "../components/update.component.tsx";
import {NoteResponseDto} from "../dto/request.response.dto.tsx";

export class Loader {
    static createUrl: string = "/api/create";
    static readUrl: string = "/api/read";
    static readTitleUrl: string = "/api/read/title";
    static updateUrl: string = "/api/update";
    static catalogUrl: string = "/api/catalog";
    static loginUrl: string = "/account/login";
    static logoutUrl: string = "/account/logout";
    static complianceIndicesUrl: string = "/api/compliance/indices";

    static migrationCreateUrl: string = "/migration/create";
    static migrationRestoreUrl: string = "/migration/restore";

    static corsCredentialsPolicy: "omit" | "same-origin" | "include" = "same-origin";
    static corsServiceBaseUrl: string = "";
    static redirectHostSchema = "http";

    static unusedPromise: Promise<void>;

    static setupDevEnvironment = () => {
        if (process.env.NODE_ENV === "development") {
            this.corsCredentialsPolicy = "include";
            this.corsServiceBaseUrl = "http://127.0.0.1:5000";
            this.redirectHostSchema = "https";
        }
    }

    static redirectToMenu = (url: string) => {
        Loader.setupDevEnvironment();
        LoginBoxHandler.SetInvisible();
        try {
            let redirectTo = this.redirectHostSchema + "://" + window.location.host + url;
            console.log("Redirect to: " + redirectTo);
            window.location.href = redirectTo;
        } catch {
            console.log("Loader: redirect exception");
        }
    }

    // TODO: после модификации create.component переписать функцию:
    static async processResponse<T>(response: Response,
                                    component: (Component & IMountedComponent)|FunctionComponentStateWrapper<T>,
                                    url: string,
                                    error: string,
                                    time?: string): Promise<void> {
        try {
            // FC / IMounted switch:
            let data: T;
            let mounted: boolean;
            let setComponentState: (data: T) => void;

            // весь postData - с полем time:
            const isFunctionalComponent = component instanceof FunctionComponentStateWrapper;
            switch (isFunctionalComponent) {
                case true: {
                    const castedFcComponent = component as FunctionComponentStateWrapper<T>;
                    mounted = castedFcComponent.mounted[0];
                    if (castedFcComponent.setComplexData) {
                        // FC + multi state:
                        setComponentState = (data: T) => {
                            // предопределенный тип:
                            const complexState: IDataTimeState = {data: data as NoteResponseDto, time: Number(time)};
                            castedFcComponent.setComplexData!(complexState);// {data: T,time: number}
                        };
                    } else {
                        setComponentState = (data: T) => castedFcComponent.setData!(data);// {data: T}
                    }
                    data = await response.json().catch(() => LoginBoxHandler.SetVisible(castedFcComponent, url));
                    break;
                }
                default: {
                    const castedComponent = component as (Component & IMountedComponent);
                    mounted = castedComponent.mounted;
                    if (time) {
                        setComponentState = (data: T) => castedComponent.setState({data, time});
                    } else {
                        setComponentState = (data: T) => castedComponent.setState({data});
                    }
                    data = await response.json().catch(() => LoginBoxHandler.SetVisible(castedComponent, url));
                    break;
                }
            }

            if (mounted) {
                setComponentState(data);
            }

        } catch(ex) {
            console.log(error);
            console.log(ex);
        }
    }

    // GET request: /api/controller
    static async getData<T>(component: (Component & IMountedComponent)|FunctionComponentStateWrapper<T>,
                            url: string): Promise<void> {
        Loader.setupDevEnvironment();
        LoginBoxHandler.SetInvisible();
        const error: string = "Loader: get exception";

        try {
            const response = await fetch(this.corsServiceBaseUrl + url, {
                credentials: this.corsCredentialsPolicy, redirect: "follow"
            });

            await this.processResponse(response, component, url, error);
        } catch {
            console.log(error);
        }
    }

    // GET request: /api/controller?id=
    static async getDataById<T>(component: (Component & IMountedComponent)|FunctionComponentStateWrapper<T>,
                                requestId: number|undefined,
                                url: string): Promise<void> {
        Loader.setupDevEnvironment();
        LoginBoxHandler.SetInvisible();
        const error: string = "Loader: getById exception";

        try {
            const response = await fetch(this.corsServiceBaseUrl + url + "?id=" + String(requestId), {credentials: this.corsCredentialsPolicy});

            await this.processResponse(response, component, url, error);
        } catch {
            console.log(error);
        }
    }

    // POST request: /api/controller
    static async postData<T>(component: (Component & IMountedComponent)|FunctionComponentStateWrapper<T>,
                             requestBody: string,
                             url: string,
                             id: number|string|null = null): Promise<void> {
        Loader.setupDevEnvironment();
        LoginBoxHandler.SetInvisible();
        const error: string = "Loader: post exception";

        try {
            const response = await fetch(this.corsServiceBaseUrl + url + "?id=" + String(id), {
                method: "POST",
                headers: {'Content-Type': "application/json;charset=utf-8"},
                body: requestBody,
                credentials: this.corsCredentialsPolicy
            });

            let time = String(Date.now());
            await this.processResponse(response, component, url, error, time);
        } catch {
            console.log(error);
        }
    }

    // DELETE request: /api/controller?id=
    static async deleteDataById<T>(component: (Component & IMountedComponent)|FunctionComponentStateWrapper<T>,
                                   requestId: number,
                                   url: string,
                                   pageNumber?: number): Promise<void> {
        Loader.setupDevEnvironment();
        LoginBoxHandler.SetInvisible();
        const error: string = "Loader: delete exception";

        try {
            const response = await fetch(
                this.corsServiceBaseUrl + url + "?id=" + String(requestId) + "&pg=" + String(pageNumber), {
                    method: "DELETE",
                    credentials: this.corsCredentialsPolicy
                });

            await this.processResponse(response, component, url, error);
        } catch {
            console.log(error);
        }
    }

    // LOGIN & LOGOUT request: /account/login?email= &password= or /account/logout
    static fireAndForgetWithQuery<T>(url: string,
                                     query: string,
                                     callback: (v: Response)=>Response|PromiseLike<Response>|void,
                                     component: (Component&IMountedComponent)|FunctionComponentStateWrapper<T>|null): void {
        Loader.setupDevEnvironment();
        const error: string = "Loader: login/logout exception";

        try {
            fetch(this.corsServiceBaseUrl + url + query, {credentials: this.corsCredentialsPolicy}).then(callback);
            if (component !== null) {
                LoginBoxHandler.SetVisible(component, url);
            }
        } catch {
            console.log(error);
        }
    }

    // CREATE: /api/find?text= or /api/read/title?id=
    static getWithPromise = async(url: string, query: string, callback: (data: Response)=>Promise<void>|void): Promise<void> => {
        Loader.setupDevEnvironment();
        const error: string = "Loader: promise exception";

        try {
            const response = await fetch(this.corsServiceBaseUrl + url + query, {credentials: this.corsCredentialsPolicy});
            return response.json().then(callback);
        } catch {
            console.log(error);
        }
    }
}

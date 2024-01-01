import {LoginBoxHandler} from "./login.component.tsx";
import {Component} from "react";

export class LoaderComponent {
    static createUrl: string = "/api/create";
    static readUrl: string = "/api/read";
    static readTitleUrl: string = "/api/read/title";
    static updateUrl: string = "/api/update";
    static catalogUrl: string = "/api/catalog";
    static loginUrl: string = "/account/login";
    static logoutUrl: string = "/account/logout";
    static findUrl: string = "/api/find";

    static backupCreateUrl: string = "/create";
    static backupRestoreUrl: string = "/restore";

    static corsCredentialsPolicy: "omit" | "same-origin" | "include" = "same-origin";
    static corsServiceBaseUrl: string = "";
    static redirectHostSchema = "http";

    static unusedPromise: any;

    static setDevelopmentCredos() {
        if (process.env.NODE_ENV === "development") {
            this.corsCredentialsPolicy = "include";
            this.corsServiceBaseUrl = "http://127.0.0.1:5000";
            this.redirectHostSchema = "https";
        }
    }

    static redirectToMenu(url: string) {
        LoaderComponent.setDevelopmentCredos();
        LoginBoxHandler.Invisible();
        try {
            let redirectTo = this.redirectHostSchema + "://" + window.location.host + url;
            console.log("Redirect to: " + redirectTo);
            window.location.href = redirectTo;
        } catch {
            console.log("Loader: redirect exception");
        }
    }

    // GET request: /api/controller
    static async getData(component: any, url: any) {
        LoaderComponent.setDevelopmentCredos();
        LoginBoxHandler.Invisible();

        try {
            const response = await fetch(this.corsServiceBaseUrl + url, {credentials: this.corsCredentialsPolicy});
            const data = await response.json().catch(() => LoginBoxHandler.Visible(component, url));
            if (component.mounted) component.setState({data});
        } catch {
            console.log("Loader: get exception");
        }
    }

    // GET request: /api/controller?id=
    static async getDataById(component: any, requestId: any, url: any) {
        LoaderComponent.setDevelopmentCredos();
        LoginBoxHandler.Invisible();

        try {
            const response = await fetch(this.corsServiceBaseUrl + url + "?id=" + String(requestId), {credentials: this.corsCredentialsPolicy});
            const data = await response.json().catch(() => LoginBoxHandler.Visible(component, url));
            if (component.mounted) component.setState({data});
        } catch {
            console.log("Loader: getById exception");
        }
    }

    // POST request: /api/controller
    static async postData(component: any, requestBody: any, url: any, id: any = null) {
        LoaderComponent.setDevelopmentCredos();
        let time = String(Date.now());
        LoginBoxHandler.Invisible();

        try {
            const response = await fetch(this.corsServiceBaseUrl + url + "?id=" + String(id), {
                method: "POST",
                headers: {'Content-Type': "application/json;charset=utf-8"},
                body: requestBody,
                credentials: this.corsCredentialsPolicy
            });

            const data = await response.json().catch(() => LoginBoxHandler.Visible(component, url));
            if (component.mounted) component.setState({data, time});
        } catch {
            console.log("Loader: post exception");
        }
    }

    // DELETE request: /api/controller?id=
    static async deleteDataById(component: any, requestId: any, url: any, pageNumber: any) {
        LoaderComponent.setDevelopmentCredos();
        LoginBoxHandler.Invisible();

        try {
            const response = await fetch(
                this.corsServiceBaseUrl + url + "?id=" + String(requestId) + "&pg=" + String(pageNumber), {
                    method: "DELETE",
                    credentials: this.corsCredentialsPolicy
                });

            const data = await response.json().catch(() => LoginBoxHandler.Visible(component, url));
            if (component.mounted) component.setState({data});
        } catch {
            console.log("Loader: delete exception");
        }
    }

    // LOGIN & LOGOUT request: /account/login?email= &password= or /account/logout
    static fireAndForgetWithQuery(url: string, query: string, callback: any, component: Component | null) {
        LoaderComponent.setDevelopmentCredos();

        try {
            fetch(this.corsServiceBaseUrl + url + query, {credentials: this.corsCredentialsPolicy}).then(callback);
            if (component !== null) {
                LoginBoxHandler.Visible(component, url);
            }
        } catch {
            console.log("Loader: login/logout exception");
        }
    }

    // CREATE: /api/find?text= or /api/read/title?id=
    static async getWithPromise(url: string, query: string, callback: any): Promise<any> {
        LoaderComponent.setDevelopmentCredos();

        try {
            const response = await fetch(this.corsServiceBaseUrl + url + query, {credentials: this.corsCredentialsPolicy});
            return response.json().then(callback);
        } catch {
            console.log("Loader: promise exception");
        }
    }
}

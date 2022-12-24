import {LoginRequired} from "./login";
import {Component} from "react";

export class Loader {
    static createUrl: string = "/api/create";
    static readUrl: string = "/api/read";
    static readTitleUrl: string = "/api/read/title";
    static updateUrl: string = "/api/update";
    // deleteUrl = "/api/catalog" with DELETE verb
    static catalogUrl: string = "/api/catalog";
    static loginUrl: string = "/account/login";
    static logoutUrl: string = "/account/logout";
    static findUrl: string = "/api/find";

    static backupCreateUrl: string = "/create";
    static backupRestoreUrl: string = "/restore";

    // для прода:
    static credos: "omit" | "same-origin" | "include" = "same-origin";
    static corsAddress: string = "";

    static setDevelopmentCredos() {
        if (process.env.NODE_ENV === "development") {
            this.credos = "include";
            // для разработки:
            // куки чувствительны к Origin ('localhost' != '127.0.0.1')
            this.corsAddress = "http://127.0.0.1:5000";
        }
    }

    //GET request: /api/controller
    static async getData(component: any, url: any) {
        Loader.setDevelopmentCredos();
        LoginRequired.MessageOff();

        // url = this.corsAddress + url;

        /*try {
            window.fetch(this.corsAddress + url, {
                credentials: this.credos
            })
                .then(response => response.ok ? response.json() : Promise.reject(response))
                .then(data => {
                    if (component.mounted) component.setState({data})
                })
                .catch((e) => LoginRequired.MessageOn(component, url));//
        } catch (err) {
            console.log("Loader: get exception");
        }*/

        try {
            const response = await fetch(this.corsAddress + url, {credentials: this.credos});
            const data = await response.json().catch(() => LoginRequired.MessageOn(component, url));
            if (component.mounted) component.setState({data});
        } catch {
            console.log("Loader: get exception");
        }
    }

    // GET request: /api/controller?id=
    static async getDataById(component: any, requestId: any, url: any) {
        Loader.setDevelopmentCredos();
        LoginRequired.MessageOff();

        // url = this.corsAddress + url;

        /*try {
            window.fetch(this.corsAddress + url + "?id=" + String(requestId), {
                credentials: this.credos
            })
                .then(response => response.ok ? response.json() : Promise.reject(response))
                .then(data => { if (component.mounted) component.setState({ data }) })
                .catch((e) => LoginRequired.MessageOn(component, url));
        } catch (err) {
            console.log("Loader: getById exception");
        }*/

        try {
            const response = await fetch(this.corsAddress + url + "?id=" + String(requestId), {credentials: this.credos});
            const data = await response.json().catch(() => LoginRequired.MessageOn(component, url));
            if (component.mounted) component.setState({data});
        } catch {
            console.log("Loader: getById exception");
        }
    }

    // POST request: /api/controller
    static async postData(component: any, requestBody: any, url: any, id: any = null) {
        // [obsolte] из 1го цикла: при пустых areChecked чекбоксах внешний вид компонента <Сheckboxes> не менялся (после "ошибки" POST)
        // при этом все данные были  правильные и рендеринг/обновление проходили успешно (в компоненте <UpdateView>)
        // решение: уникальный key <Checkbox key={"checkbox " + i + this.state.time} ...>
        Loader.setDevelopmentCredos();
        let time = String(Date.now());
        LoginRequired.MessageOff();

        // url = this.corsAddress + url;

        /*try {
            window.fetch(this.corsAddress + url + "?id=" + String(id), {
                method: "POST",
                headers: { 'Content-Type': "application/json;charset=utf-8" },
                body: requestBody,
                credentials: this.credos
            })
                .then(response => response.ok ? response.json() : Promise.reject(response))
                .then(data => component.setState({ data, time }))
                .catch((e) => LoginRequired.MessageOn(component, url));
        } catch (err) {
            console.log("Loader: post exception");
        }*/

        try {
            const response = await fetch(this.corsAddress + url + "?id=" + String(id), {
                method: "POST",
                headers: {'Content-Type': "application/json;charset=utf-8"},
                body: requestBody,
                credentials: this.credos
            });

            const data = await response.json().catch(() => LoginRequired.MessageOn(component, url));
            if (component.mounted) component.setState({data, time});
        } catch {
            console.log("Loader: post exception");
        }
    }

    // DELETE request: /api/controller?id=
    static async deleteDataById(component: any, requestId: any, url: any, pageNumber: any) {
        Loader.setDevelopmentCredos();
        LoginRequired.MessageOff();

        // url = this.corsAddress + url;

        /*try {
            window.fetch(this.corsAddress + url + "?id=" + String(requestId) + "&pg=" + String(pageNumber), {
                method: "DELETE",
                credentials: this.credos
            })
                .then(response => response.ok ? response.json() : Promise.reject(response))
                .then(data => { if (component.mounted) component.setState({ data }) })
                .catch((e) => LoginRequired.MessageOn(component, url));
        } catch (err) {
            console.log("Loader: delete exception");
        }*/

        try {
            const response = await fetch(
                this.corsAddress + url + "?id=" + String(requestId) + "&pg=" + String(pageNumber), {
                    method: "DELETE",
                    credentials: this.credos
                });

            const data = await response.json().catch(() => LoginRequired.MessageOn(component, url));
            if (component.mounted) component.setState({data});
        } catch {
            console.log("Loader: delete exception");
        }
    }

    // LOGIN & LOGOUT request: /account/login?email= &password= or /account/logout
    static fireAndForgetWithQuery(url: string, query: string, callback: any, component: Component | null) {
        Loader.setDevelopmentCredos();

        // url = this.corsAddress + url;

        /*try{
            window.fetch(this.corsAddress + url + query,
                {credentials: this.credos})
                .then(callback);
            if (component !== null)
            {
                LoginRequired.MessageOn(component, url);
            }
        } catch (err) {
            console.log("Loader: login/logout exception");
        }*/

        try {
            fetch(this.corsAddress + url + query, {credentials: this.credos}).then(callback);
            if (component !== null) {
                LoginRequired.MessageOn(component, url);
            }
        } catch {
            console.log("Loader: login/logout exception");
        }
    }

    // CREATE: /api/find?text= or /api/read/title?id=
    static async getWithPromise(url: string, query: string, callback: any): Promise<any> {
        Loader.setDevelopmentCredos();

        // url = this.corsAddress + url;

        /*let promise: Promise<any>;
        
        try{
            promise = window.fetch(this.corsAddress + url + query,
                {credentials: this.credos})
                .then(response => response.ok ? response.json() : Promise.reject(response))
                .then(callback);

            return promise;
        } catch (err) {
            console.log("Loader: promise exception");
        }*/

        try {
            const response = await fetch(this.corsAddress + url + query, {credentials: this.credos});
            return response.json().then(callback);
        } catch {
            console.log("Loader: promise exception");
        }
    }
}
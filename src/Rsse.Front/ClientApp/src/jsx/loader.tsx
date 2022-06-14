import { LoginRequired } from "./login";
import {Component} from "react";

export class Loader {
    static createUrl:string = "/api/create";
    static readUrl: string = "/api/read";
    static readTitleUrl: string = "/api/read/title";
    static updateUrl: string = "/api/update";
    // deleteUrl = "/api/catalog" with DELETE verb
    static catalogUrl: string = "/api/catalog";
    static loginUrl: string = "/account/login";
    static logoutUrl: string = "/account/logout";
    static findUrl: string = "/api/find";
    
    // для прода:
    static credos: "omit" | "same-origin" | "include" = "same-origin"; 
    static corsAddress: string = ""; 
    
    static isDevelopment() {
        if (process.env.NODE_ENV === "development") 
        {
            this.credos = "include";
            // для разработки:
            // куки чувствительны к Origin ('localhost' != '127.0.0.1')
            this.corsAddress = "http://localhost:5000";
        }
    }
    
    //GET request: /api/controller
    static getData(component: any, url: any) {
        Loader.isDevelopment();
        LoginRequired.MessageOff();

        // url = this.corsAddress + url;

        try {
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
        }
    }

    // GET request: /api/controller?id=
    static getDataById(component: any, requestId: any, url: any) {
        Loader.isDevelopment();
        LoginRequired.MessageOff();
        
        // url = this.corsAddress + url;
        
        try {
            window.fetch(this.corsAddress + url + "?id=" + String(requestId), {
                credentials: this.credos
            })
                .then(response => response.ok ? response.json() : Promise.reject(response))
                .then(data => { if (component.mounted) component.setState({ data }) })
                .catch((e) => LoginRequired.MessageOn(component, url));
        } catch (err) {
            console.log("Loader: getById exception");
        }
    }

    // POST request: /api/controller
    static postData(component: any, requestBody: any, url: any) {
        // [obsolte] из 1го цикла: при пустых areChecked чекбоксах внешний вид компонента <Сheckboxes> не менялся (после "ошибки" POST)
        // при этом все данные были  правильные и рендеринг/обновление проходили успешно (в компоненте <UpdateView>)
        // решение: уникальный key <Checkbox key={"checkbox " + i + this.state.time} ...>
        Loader.isDevelopment();
        let time = String(Date.now());
        LoginRequired.MessageOff();
        
        // url = this.corsAddress + url;
        
        try {
            window.fetch(this.corsAddress + url, {
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
        }
    }

    // DELETE request: /api/controller?id=
    static deleteDataById(component: any, requestId: any, url: any, pageNumber: any) {
        Loader.isDevelopment();
        LoginRequired.MessageOff();
        
        // url = this.corsAddress + url;
        
        try {
            window.fetch(this.corsAddress + url + "?id=" + String(requestId) + "&pg=" + String(pageNumber), {
                method: "DELETE",
                credentials: this.credos
            })
                .then(response => response.ok ? response.json() : Promise.reject(response))
                .then(data => { if (component.mounted) component.setState({ data }) })
                .catch((e) => LoginRequired.MessageOn(component, url));
        } catch (err) {
            console.log("Loader: delete exception");
        }
    }
    
    // LOGIN & LOGOUT request: /account/login?email= &password= or /account/logout
    static getWithQuery(url: string, query: string, callback: any, component: Component | null)
    {
        Loader.isDevelopment();
        
        // url = this.corsAddress + url;
        
        try{
            window.fetch(this.corsAddress + url + query,
                {credentials: this.credos})
                .then(callback);
            if (component !== null)
            {
                LoginRequired.MessageOn(component, url);
            }
        } catch (err) {
            console.log("Loader: login/logout exception");
        }
    }

    // CREATE: /api/find?text= or /api/read/title?id=
    static getWithPromise(url: string, query: string, callback: any): Promise<any> | undefined
    {
        Loader.isDevelopment();

        url = this.corsAddress + url;

        let promise: Promise<any>;
        
        try{
            promise = window.fetch(url + query,
                {credentials: this.credos})
                .then(response => response.ok ? response.json() : Promise.reject(response))
                .then(callback);

            return promise;
        } catch (err) {
            console.log("Loader: promise exception");
        }
    }
}
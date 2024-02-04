import {LoginBoxHandler} from "./visibility.handlers.tsx";
import {FunctionComponentStateWrapper} from "./state.wrappers.tsx";

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

    static async processResponse<T>(response: Response,
                                    stateWrapper: FunctionComponentStateWrapper<T>,
                                    url: string,
                                    error: string): Promise<void> {
        try {
            const mounted = stateWrapper.mounted[0];
            const setComponentState = (data: T) => stateWrapper.setData(data);
            const data: T = await response.json().catch(() => LoginBoxHandler.SetVisible(stateWrapper, url));

            if (mounted) {
                setComponentState(data);
            }

        } catch (exception) {
            console.log(error);
            console.log(exception);
        }
    }

    // GET request: /api/controller
    static async getData<T>(stateWrapper: FunctionComponentStateWrapper<T>,
                            url: string): Promise<void> {
        Loader.setupDevEnvironment();
        LoginBoxHandler.SetInvisible();
        const error: string = "Loader: get exception";

        try {
            const response = await fetch(this.corsServiceBaseUrl + url, {
                credentials: this.corsCredentialsPolicy, redirect: "follow"
            });

            await this.processResponse(response, stateWrapper, url, error);
        } catch {
            console.log(error);
        }
    }

    // GET request: /api/controller?id=
    static async getDataById<T>(stateWrapper: FunctionComponentStateWrapper<T>,
                                requestId: number|undefined,
                                url: string): Promise<void> {
        Loader.setupDevEnvironment();
        LoginBoxHandler.SetInvisible();
        const error: string = "Loader: getById exception";

        try {
            const response = await fetch(this.corsServiceBaseUrl + url + "?id=" + String(requestId), {credentials: this.corsCredentialsPolicy});

            await this.processResponse(response, stateWrapper, url, error);
        } catch {
            console.log(error);
        }
    }

    // POST request: /api/controller
    static async postData<T>(stateWrapper: FunctionComponentStateWrapper<T>,
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

            await this.processResponse(response, stateWrapper, url, error);
        } catch {
            console.log(error);
        }
    }

    // DELETE request: /api/controller?id=
    static async deleteDataById<T>(stateWrapper: FunctionComponentStateWrapper<T>,
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

            await this.processResponse(response, stateWrapper, url, error);
        } catch {
            console.log(error);
        }
    }

    // LOGIN & LOGOUT request: /account/login?email= &password= or /account/logout
    static fireAndForgetWithQuery<T>(url: string,
                                     query: string,
                                     callback: (v: Response)=>Response|PromiseLike<Response>|void,
                                     stateWrapper: FunctionComponentStateWrapper<T>|null): void {
        Loader.setupDevEnvironment();
        const error: string = "Loader: login/logout exception";

        try {
            fetch(this.corsServiceBaseUrl + url + query, {credentials: this.corsCredentialsPolicy}).then(callback);
            if (stateWrapper !== null) {
                LoginBoxHandler.SetVisible(stateWrapper, url);
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

import {LoginBoxVisibility} from "./visibility.handlers";
import {CommonStateStorage, FunctionComponentStateWrapper, StateTypesAlias} from "./state.wrappers";
import {CatalogResponseDto, NoteResponseDto} from "../dto/request.response.dto.tsx";

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
        const error = `${Loader.name}: redirectToMenu exception`;
        Loader.setupDevEnvironment();
        LoginBoxVisibility(false);

        try {
            let redirectTo = this.redirectHostSchema + "://" + window.location.host + url;
            console.log("Redirect to: " + redirectTo);
            window.location.href = redirectTo;
        } catch {
            console.log(error);
        }
    }

    static async processResponse(response: Response,
                                    stateWrapper: FunctionComponentStateWrapper<StateTypesAlias>,
                                    url: string,
                                    error: string,
                                    recoveryContext?: CommonStateStorage<StateTypesAlias>): Promise<void> {
        try {
            const mounted = stateWrapper.mounted[0];
            const setComponentState = (data: StateTypesAlias) => stateWrapper.setData(data);
            const data: StateTypesAlias = await response.json().catch(() => LoginBoxVisibility(true, stateWrapper, url, recoveryContext));

            if (mounted) {
                setComponentState(data);
            }

        } catch (exception) {
            console.log(error);
            console.log(exception);
        }
    }

    // GET request: /api/controller
    static async getData(stateWrapper: FunctionComponentStateWrapper<StateTypesAlias>,
                            url: string,
                            recoveryContext?: CommonStateStorage<StateTypesAlias>): Promise<void> {
        const error: string = `${Loader.name}: getData exception`;
        Loader.setupDevEnvironment();
        LoginBoxVisibility(false);

        try {
            const response = await fetch(this.corsServiceBaseUrl + url, {
                credentials: this.corsCredentialsPolicy, redirect: "follow"
            });

            await this.processResponse(response, stateWrapper, url, error, recoveryContext);
        } catch {
            console.log(error);
        }
    }

    // GET request: /api/controller?id=
    static async getDataById(stateWrapper: FunctionComponentStateWrapper<StateTypesAlias>,
                                requestId: number|undefined,
                                url: string): Promise<void> {
        const error: string = `${Loader.name}: getDataById exception`;
        Loader.setupDevEnvironment();
        LoginBoxVisibility(false);

        try {
            const response = await fetch(this.corsServiceBaseUrl + url + "?id=" + String(requestId), {credentials: this.corsCredentialsPolicy});

            await this.processResponse(response, stateWrapper, url, error);
        } catch {
            console.log(error);
        }
    }

    // POST request: /api/controller
    static async postData(stateWrapper: FunctionComponentStateWrapper<StateTypesAlias>,
                             requestBody: string,
                             url: string,
                             id: number|string|null = null,
                             recoveryContext?: CommonStateStorage<NoteResponseDto|CatalogResponseDto>): Promise<void> {
        const error: string = `${Loader.name}: postData exception`;
        Loader.setupDevEnvironment();
        LoginBoxVisibility(false);

        try {
            const response = await fetch(this.corsServiceBaseUrl + url + "?id=" + String(id), {
                method: "POST",
                headers: {'Content-Type': "application/json;charset=utf-8"},
                body: requestBody,
                credentials: this.corsCredentialsPolicy
            });

            await this.processResponse(response, stateWrapper, url, error, recoveryContext);
        } catch {
            console.log(error);
        }
    }

    // DELETE request: /api/controller?id=
    static async deleteDataById(stateWrapper: FunctionComponentStateWrapper<StateTypesAlias>,
                                   requestId: number,
                                   url: string,
                                   pageNumber?: number,
                                   recoveryContext?: CommonStateStorage<StateTypesAlias>): Promise<void> {
        const error: string = `${Loader.name}: deleteDataById exception`;
        Loader.setupDevEnvironment();
        LoginBoxVisibility(false);

        try {
            const response = await fetch(
                this.corsServiceBaseUrl + url + "?id=" + String(requestId) + "&pg=" + String(pageNumber), {
                    method: "DELETE",
                    credentials: this.corsCredentialsPolicy
                });

            await this.processResponse(response, stateWrapper, url, error, recoveryContext);
        } catch {
            console.log(error);
        }
    }

    // LOGIN & LOGOUT request: /account/login?email= &password= or /account/logout
    static fireAndForgetWithQuery(url: string,
                                     query: string,
                                     callback: (v: Response)=>Response|PromiseLike<Response>|void,
                                     stateWrapper: FunctionComponentStateWrapper<StateTypesAlias>|null,
                                     recoveryContext?: CommonStateStorage<StateTypesAlias>): void {
        const error: string = `${Loader.name}: FnF or login/logout exception`;
        Loader.setupDevEnvironment();

        try {
            fetch(this.corsServiceBaseUrl + url + query, {credentials: this.corsCredentialsPolicy}).then(callback);
            if (stateWrapper !== null) {
                LoginBoxVisibility(true, stateWrapper, url, recoveryContext);
            }
        } catch(exception) {
            console.log(error);
            console.log(exception);
        }
    }

    // CREATE: /api/find?text= or /api/read/title?id=
    static getWithPromise = async(url: string, query: string, callback: (data: Response)=>Promise<void>|void): Promise<void> => {
        const error: string = `${Loader.name}: promise exception`;
        Loader.setupDevEnvironment();

        try {
            const response = await fetch(this.corsServiceBaseUrl + url + query, {credentials: this.corsCredentialsPolicy});
            return response.json().then(callback);
        } catch {
            console.log(error);
        }
    }
}

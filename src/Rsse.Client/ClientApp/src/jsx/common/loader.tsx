import {setLoginBoxVisibility} from "./visibility.handlers";
import {
    FunctionComponentStateWrapper,
    RecoveryStateStorage,
    StateTypesAlias
} from "./state.handlers";
import {RouteConstants} from "../../api-routes.tsx";

export class Loader {
    static readTagsForCreateAuthGetUrl: string = RouteConstants.readTagsForCreateAuthGetUrl;// дубль readGetTagsUrl, но под авторизацией
    static createNotePostUrl: string = RouteConstants.createNotePostUrl;
    static readTagsGetUrl: string = RouteConstants.readTagsGetUrl;
    static readNotePostUrl: string = RouteConstants.readNotePostUrl;
    static readTitleGetUrl: string = RouteConstants.readTitleGetUrl;
    static redNoteWithTagsForUpdateAuthGetUrl: string = RouteConstants.redNoteWithTagsForUpdateAuthGetUrl;
    static updateNotePostUrl: string = RouteConstants.updateNotePostUrl;
    static catalogPageGetUrl: string = RouteConstants.catalogPageGetUrl;
    static catalogNavigatePostUrl: string = RouteConstants.catalogNavigatePostUrl;
    static deleteNoteUrl: string = RouteConstants.deleteNoteUrl;
    static loginUrl: string = RouteConstants.accountLoginGetUrl;
    static logoutUrl: string = RouteConstants.accountLogoutGetUrl;
    static checkAuth: string = RouteConstants.accountCheckGetUrl;
    static complianceIndicesUrl: string = RouteConstants.complianceIndicesGetUrl;

    static migrationCreateUrl: string = RouteConstants.migrationCreateGetUrl;
    static migrationRestoreUrl: string = RouteConstants.migrationRestoreGetUrl;
    static migrationDownloadUrl: string = RouteConstants.migrationDownloadGetUrl;

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
        setLoginBoxVisibility(false);

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
                                 recoveryContext?: RecoveryStateStorage<StateTypesAlias>): Promise<void> {
        try {
            const mounted = stateWrapper.mounted[0];
            const setComponentState = (data: StateTypesAlias) => stateWrapper.setData(data);
            const data: StateTypesAlias = await response.json().catch(() => setLoginBoxVisibility(true, stateWrapper, url, recoveryContext));

            if (mounted) {
                setComponentState(data);
            }

        } catch (exception) {
            console.log(error);
            console.log(exception);
        }
    }

    // GET file from web root: /migration/download
    static async getFile(fileName: string): Promise<Response|undefined> {
        const error: string = `${Loader.name}: getFile exception`;
        Loader.setupDevEnvironment();

        try {
            return await fetch(`${this.corsServiceBaseUrl}${this.migrationDownloadUrl}?filename=${fileName}`, {
                credentials: this.corsCredentialsPolicy
            });
        } catch {
            console.log(error);
        }
    }

    // GET request: /api/controller
    static async getData(stateWrapper: FunctionComponentStateWrapper<StateTypesAlias>,
                         url: string,
                         recoveryContext?: RecoveryStateStorage<StateTypesAlias>): Promise<void> {
        const error: string = `${Loader.name}: getData exception`;
        Loader.setupDevEnvironment();
        setLoginBoxVisibility(false);

        try {
            const response = await fetch(`${this.corsServiceBaseUrl}${url}`, {
                credentials: this.corsCredentialsPolicy,
                redirect: "follow"
            });

            await this.processResponse(response, stateWrapper, url, error, recoveryContext);
        } catch {
            console.log(error);
        }
    }

    // GET request: /api/controller?id=
    static async getDataById(stateWrapper: FunctionComponentStateWrapper<StateTypesAlias>,
                             requestId: number|undefined,
                             url: string,
                             recoveryContext?: RecoveryStateStorage<StateTypesAlias>): Promise<void> {
        const error: string = `${Loader.name}: getDataById exception`;
        Loader.setupDevEnvironment();
        setLoginBoxVisibility(false);

        try {
            const response = await fetch(`${this.corsServiceBaseUrl}${url}?id=${String(requestId)}`, {
                credentials: this.corsCredentialsPolicy
                });

            await this.processResponse(response, stateWrapper, url, error, recoveryContext);
        } catch {
            console.log(error);
        }
    }

    // POST request: /api/controller
    static async postData(stateWrapper: FunctionComponentStateWrapper<StateTypesAlias>,
                          requestBody: string,
                          url: string,
                          id: number|string|null = null,
                          recoveryContext?: RecoveryStateStorage<StateTypesAlias>): Promise<void> {
        const error: string = `${Loader.name}: postData exception`;
        Loader.setupDevEnvironment();
        setLoginBoxVisibility(false);

        try {
            const response = await fetch(`${this.corsServiceBaseUrl}${url}?id=${String(id)}`, {
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
                                recoveryContext?: RecoveryStateStorage<StateTypesAlias>): Promise<void> {
        const error: string = `${Loader.name}: deleteDataById exception`;
        Loader.setupDevEnvironment();
        setLoginBoxVisibility(false);

        try {
            const response = await fetch(
                `${this.corsServiceBaseUrl}${url}?id=${String(requestId)}&pg=${String(pageNumber)}`, {
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
                                  callback: (v: Response) => Response|PromiseLike<Response>|void,
                                  stateWrapper: FunctionComponentStateWrapper<StateTypesAlias>|null,
                                  recoveryContext?: RecoveryStateStorage<StateTypesAlias>): void {
        const error: string = `${Loader.name}: FnF or login/logout/check exception`;
        Loader.setupDevEnvironment();

        try {
            fetch(`${this.corsServiceBaseUrl}${url}${query}`, {
                credentials: this.corsCredentialsPolicy
            }).then(callback);

            if (stateWrapper !== null) {
                setLoginBoxVisibility(true, stateWrapper, url, recoveryContext);
            }
        } catch(exception) {
            console.log(error);
            console.log(exception);
        }
    }

    // CREATE: /api/find?text= or /api/read/title?id=
    static getWithPromise = async(url: string, query: string, callback: (data: Response) => Promise<void>|void): Promise<void> => {
        const error: string = `${Loader.name}: promise exception`;
        Loader.setupDevEnvironment();

        try {
            const response = await fetch(`${this.corsServiceBaseUrl}${url}${query}`, {
                credentials: this.corsCredentialsPolicy
            });

            return response.json().then(callback);
        } catch {
            console.log(error);
        }
    }
}

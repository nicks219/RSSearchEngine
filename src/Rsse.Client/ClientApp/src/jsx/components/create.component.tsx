import * as React from 'react';
import {Loader} from "../common/loader.tsx";
import {
    getCommonNoteId, getStructuredTagsListResponse, getTagsCheckedUncheckedResponse,
    getTextRequest, getTextResponse, getTitleRequest,
    getTitleResponse, setTextResponse, setTitleResponse
} from "../common/dto.handlers.tsx";
import {ISimpleProps, ISubscribed} from "../common/contracts.tsx";
import {NoteResponseDto, ComplianceResponseDto} from "../dto/request.response.dto.tsx";
import {useEffect, useReducer, useRef, useState} from "react";
import {FunctionComponentStateWrapper, StateStorageWrapper} from "../common/state.wrappers.tsx";
import {IDataTimeState} from "./update.component.tsx";

interface ISubscribeProps extends ISimpleProps, ISubscribed<FunctionComponentStateWrapper<NoteResponseDto>> {}

export const CreateView = () => {
    const [data, setState] = useState<IDataTimeState | null>({data: null, time: null, stateStorage: null});
    const mounted = useState(true);
    const stateWrapper = new FunctionComponentStateWrapper<NoteResponseDto>(mounted, null, null, setState);
    const refObject: React.MutableRefObject<HTMLFormElement | undefined> = useRef();
    let formId: HTMLFormElement | undefined = refObject.current;

    const componentDidMount = () => {
        formId = refObject.current;
        Loader.unusedPromise = Loader.getData(stateWrapper, Loader.createUrl);
    }
    const componentWillUnmount = () => {
        // переход на update при несохраненной заметке не приведёт к ошибке 400 (сервер не понимает NaN):
        if (isNaN(window.textId)) window.textId = 0;
        mounted[0] = false;
    }

    useEffect(() => {
        componentDidMount();
        return componentWillUnmount;
    }, []);

    const componentDidUpdate = () => {
        if (data?.stateStorage) {
            console.log("Redirected: " + data?.stateStorage);
            Loader.redirectToMenu("/#/read/" + data?.stateStorage);
        }

        let id = 0;
        if (data?.data) {
            id = Number(getCommonNoteId(data?.data));
        }

        if (id !== 0) {
            window.textId = id;
        }
    }

    useEffect(() => {
        componentDidUpdate();
    }, [data]);

    let checkboxes = [];
    if (data && getStructuredTagsListResponse(data.data) != null) {
        for (let i = 0; i < getStructuredTagsListResponse(data.data).length; i++) {
            let time = String(Date.now());
            // subscription={stateWrapper} дублируются для SubmitButton (изначально) и Checkbox (перенесены из SubmitButton):
            checkboxes.push(<Checkbox key={`checkbox ${i}${time}`} id={String(i)}
                                      jsonStorage={data.data!} formId={undefined} subscription={stateWrapper}/>);
        }
    }

    let jsonStorage = data?.data;
    if (jsonStorage) {
        if (!getTextResponse(jsonStorage)) setTextResponse(jsonStorage, "");
        if (!getTitleResponse(jsonStorage)) setTitleResponse(jsonStorage, "");
    }

    const castedRefObject = refObject as React.LegacyRef<HTMLFormElement> | undefined;
    return (
        <div id="renderContainer">
            <form ref={castedRefObject}
                  id="dizzy">
                {checkboxes}
                {/** subscription={stateWrapper} дублируются для SubmitButton (изначально) и Checkbox (перенесены из SubmitButton): */}
                {data?.data != null &&
                    <SubmitButton subscription={stateWrapper} formId={formId} id={undefined} jsonStorage={undefined}/>}
            </form>
            {data?.data != null && <Message formId={formId} jsonStorage={jsonStorage!} id={undefined}/>}
        </div>
    );
}

const Checkbox = (props: ISubscribeProps) => {
    const checked = getTagsCheckedUncheckedResponse(props) === "checked";

    const getTagName = (i: number) => {
        return getStructuredTagsListResponse(props.jsonStorage)[i];
    };

    const getTagId = (i: number) => {
        if (props.jsonStorage?.tagIdsInternal && props.jsonStorage?.tagIdsInternal.length > 0) {
            return props.jsonStorage.tagIdsInternal[i];
        } else if (props.jsonStorage?.structuredTagsListResponse) {
            return props.jsonStorage.structuredTagsListResponse[i];
        } else {
            return "";
        }
    };

    const loadNoteOnClick = (e: React.SyntheticEvent) => {
        if (StateStorageWrapper.submitStateStorage !== undefined) {
            let title = e.currentTarget.innerHTML.valueOf();
            // item(1) это аттрибут about, в неём должен храниться id заметки, на который указывает данный чекбокс:
            let id = e.currentTarget.attributes.item(1)?.nodeValue;

            // subscription на компонент create.
            console.log("Submitted: " + StateStorageWrapper.submitStateStorage + " " + title + " " + id);

            // установка stateStorage приведет к вызову редиректа на перерисовке CreateView:
            StateStorageWrapper.submitStateStorage = undefined;
            props.subscription.setComplexData!({data: null, time: null, stateStorage: id});// переименуй в id storage // ???
        }
    }

    return (
        <div id="checkboxStyle">
            <input name="chkButton" value={props.id} type="checkbox" id={props.id}
                   className="regular-checkbox"
                   defaultChecked={checked}/>
            <label htmlFor={props.id}
                   onClick={loadNoteOnClick}
                   about={getTagId(Number(props.id))}>{getTagName(Number(props.id))}</label>
        </div>
    );
}

const Message = (props: ISimpleProps) => {
    const [, forceUpdate] = useReducer(x => x + 1, 0);

    const textHandler = (e: string) => {
        setTextResponse(props.jsonStorage, e);
        forceUpdate();
    }

    const titleHandler = (e: string) => {
        setTitleResponse(props.jsonStorage, e);
        forceUpdate();
    }

    return (
        <div>
            <p/>
            {props.jsonStorage != null ?
                <div>
                    <h5>
                        <textarea name="ttl" cols={66} rows={1} form="dizzy"
                                  value={getTitleResponse(props.jsonStorage)}
                                  onChange={e => titleHandler(e.target.value)}/>
                    </h5>
                    <h5>
                        <textarea name="msg" cols={66} rows={30} form="dizzy"
                                  value={getTextResponse(props.jsonStorage)}
                                  onChange={e => textHandler(e.target.value)}/>
                    </h5>
                </div>
                : "loading.."}
        </div>
    );
}

const SubmitButton = (props: ISubscribeProps) => {
    let requestBody: string = "";// храним во внешнем стейте
    let storage: string[] = [];
    const storageId: string[] = [];

    const cancel = (e: React.SyntheticEvent) => {
        e.preventDefault();
        let buttonElement = (document.getElementById('cancelButton') as HTMLInputElement);
        buttonElement.style.display = "none";

        // отмена - сохраняем текст и название:
        StateStorageWrapper.submitStateStorage = undefined;
        StateStorageWrapper.setState(0);
        // восстановим requestBody из внешнего стейта
        if (requestBody == "") requestBody = StateStorageWrapper.requestBody;

        let text = getTextRequest(JSON.parse(requestBody));
        let title = getTitleRequest(JSON.parse(requestBody));

        requestBody = JSON.stringify({
            "tagsCheckedRequest": [],
            "textRequest": text,
            "titleRequest": title
        });
        Loader.unusedPromise = Loader.postData(props.subscription, requestBody, Loader.createUrl);
    }

    const componentDidMount = () => {
        let buttonElement = (document.getElementById('cancelButton') as HTMLInputElement);
        buttonElement.style.display = "none";
    }
    const componentWillUnmount = () => {
        // перед выходом восстанавливаем состояние обёртки:
        StateStorageWrapper.setState(0);
        StateStorageWrapper.submitStateStorage = undefined;
        StateStorageWrapper.requestBody = "";
    }

    useEffect(() => {
        componentDidMount();
        return componentWillUnmount;
    }, []);

    const submit = async (e: React.SyntheticEvent) => {
        e.preventDefault();

        let buttonElement = (document.getElementById('cancelButton') as HTMLInputElement);
        buttonElement.style.display = "none";
        StateStorageWrapper.submitStateStorage = undefined;

        if (StateStorageWrapper.getState() === 1) {
            // подтверждение в режиме "подтверждение/отмена":
            StateStorageWrapper.setState(0);
            Loader.unusedPromise = Loader.postData(props.subscription, requestBody, Loader.createUrl);
            return;
        }

        let formData = new FormData(props.formId);
        let checkboxesArray = (formData.getAll('chkButton')).map(a => Number(a) + 1);
        let formMessage = formData.get('msg');
        let formTitle = formData.get('ttl');
        const item = {
            "tagsCheckedRequest": checkboxesArray,
            "textRequest": formMessage,
            "titleRequest": formTitle
        };
        requestBody = JSON.stringify(item);
        // сохраним requestBody во внешний стейт
        StateStorageWrapper.requestBody = requestBody;

        storage = [];
        let promise = findSimilarNotes(formMessage, formTitle);
        await promise;

        if (storage.length > 0) {
            // переключение в режим "подтверждение/отмена":
            StateStorageWrapper.submitStateStorage = StateStorageWrapper.getState();
            buttonElement.style.display = "block";
            StateStorageWrapper.setState(1);
            return;
        }

        // совпадения не обнаружены, сохраняем заметку (стандртный режим):
        Loader.unusedPromise = Loader.postData(props.subscription, requestBody, Loader.createUrl);
    }

    const findSimilarNotes = async (formMessage: string | File | null, formTitle: string | File | null) => {
        let promise;
        if (typeof formMessage === "string") {
            formMessage = formMessage.replace(/\r\n|\r|\n/g, " ");
        }

        let callback = (data: Response) => getNoteTitles(data);
        let query = "?text=" + formMessage + " " + formTitle;
        try {
            promise = Loader.getWithPromise(Loader.complianceIndicesUrl, query, callback);
        } catch (err) {
            console.log("Find when create: try-catch err");
        }

        if (promise !== undefined) {
            await promise;
        }
    }

    const getNoteTitles = async (data: Response) => {
        let responseDto = data as unknown as ComplianceResponseDto;
        let response = responseDto.res;
        if (response === undefined) {
            return;
        }

        let array: number[][] = Object.keys(response).map((key) => [Number(key), response[Number(key)]]);
        array.sort(function (a, b) {
            return b[1] - a[1]
        });

        let result = [];
        for (let index in array) {
            result.push(array[index]);
        }

        if (result.length === 0) {
            return;
        }

        for (let ind = 0; ind < result.length; ind++) {
            // лучше сделать reject:
            if (storage.length >= 10) {
                continue;
            }

            let i = String(result[ind][0]);
            // получаем имена возможных совпадений: i:string зто id заметки, можно вместо time его выставлять:
            let promise;
            let callback = (data: Response) => getTitle(data, i);
            let query = "?id=" + i;
            try {
                promise = Loader.getWithPromise(Loader.readTitleUrl, query, callback);
            } catch (err) {
                console.log("Find when create: try-catch err");
            }

            if (promise !== undefined) {
                await promise;
            }
        }
    }

    const getTitle = (input: Response, i: string) => {
        let responseDto = input as unknown as ComplianceResponseDto;
        storage.push((responseDto.res + '\r\n'));
        storageId.push(i);

        // stub:
        let data = {
            "structuredTagsListResponse": storage,
            "tagsCheckedUncheckedResponse": [],
            "textResponse": getTextRequest(JSON.parse(requestBody)),
            "titleResponse": getTitleRequest(JSON.parse(requestBody)),
            "tagIdsInternal": storageId
        };
        let time = Date.now();
        // subscription - на CreateView:
        props.subscription.setComplexData!({data, time});
    }

    return (
        <div id="submitStyle">
            <input type="checkbox" id="submitButton" className="regular-checkbox"/>
            <label htmlFor="submitButton" onClick={submit}>Создать</label>
            <div id="cancelButton">
                <input type="checkbox" id="submitButtonDuplicate" className="regular-checkbox"/>
                <label htmlFor="submitButton" onClick={cancel}>Отменить</label>
            </div>
        </div>
    );
}

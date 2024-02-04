import * as React from 'react';
import {Loader} from "../common/loader.tsx";
import {
    getCommonNoteId, getStructuredTagsListResponse, getTagsCheckedUncheckedResponse,
    getTextRequest, getTextResponse, getTitleRequest,
    getTitleResponse, setTextResponse, setTitleResponse
} from "../common/dto.handlers.tsx";
import {ISimpleProps, IComplexProps} from "../common/contracts.tsx";
import {NoteResponseDto, ComplianceResponseDto} from "../dto/request.response.dto.tsx";
import {useEffect, useReducer, useRef, useState} from "react";
import {FunctionComponentStateWrapper, StateStorageWrapper} from "../common/state.wrappers.tsx";

export const CreateView = () => {
    const [data, setState] = useState<NoteResponseDto|null>(null);
    const mounted = useState(true);
    const stateWrapper = new FunctionComponentStateWrapper<NoteResponseDto>(mounted, setState);
    const refObject: React.MutableRefObject<HTMLFormElement | undefined> = useRef();
    let formId: HTMLFormElement | undefined = refObject.current;

    const componentDidMount = () => {
        formId = refObject.current;
        Loader.unusedPromise = Loader.getData(stateWrapper, Loader.createUrl);
    }
    const componentWillUnmount = () => {
        // переход на update при несохраненной заметке не приведёт к ошибке 400 (сервер не понимает NaN):
        if (isNaN(window.noteIdStorage)) window.noteIdStorage = 0;
        mounted[0] = false;
    }

    useEffect(() => {
        componentDidMount();
        return componentWillUnmount;
    }, []);

    const componentDidUpdate = () => {
        const redirectId = data?.commonNoteID;
        // TODO: в commonNoteID записывается также id созданной заметки, поправь:
        const okResponse: string = "[OK]";
        if (redirectId && data?.titleResponse !== okResponse) {
            console.log("Redirected: " + redirectId);
            Loader.redirectToMenu("/#/read/" + redirectId);
        }

        let id = 0;
        if (data) {
            id = Number(getCommonNoteId(data));
        }

        if (id !== 0) {
            window.noteIdStorage = id;
        }
    }

    useEffect(() => {
        componentDidUpdate();
    }, [data]);

    let checkboxes = [];
    if (data && getStructuredTagsListResponse(data)) {
        for (let i = 0; i < getStructuredTagsListResponse(data).length; i++) {
            let time = String(Date.now());
            // subscription={stateWrapper} дублируются для SubmitButton (изначально) и Checkbox (перенесены из SubmitButton):
            checkboxes.push(<Checkbox key={`checkbox ${i}${time}`} id={String(i)}
                                      jsonStorage={data} formId={undefined} subscriber={stateWrapper}/>);
        }
    }

    if (data) {
        if (!getTextResponse(data)) setTextResponse(data, "");
        if (!getTitleResponse(data)) setTitleResponse(data, "");
    }

    const castedRefObject = refObject as React.LegacyRef<HTMLFormElement> | undefined;
    return (
        <div id="renderContainer">
            <form ref={castedRefObject}
                  id="dizzy">
                {checkboxes}
                {/** subscription={stateWrapper} дублируются для SubmitButton (изначально) и Checkbox (перенесены из SubmitButton): */}
                {data && <SubmitButton subscriber={stateWrapper} formId={formId} id={undefined} jsonStorage={undefined}/>}
            </form>
            {data && <Message formId={formId} jsonStorage={data} id={undefined}/>}
        </div>
    );
}

const Checkbox = (props: IComplexProps) => {
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
            console.log(`Submitted & redirected: ${StateStorageWrapper.submitStateStorage} ${title} ${id}`);
            StateStorageWrapper.submitStateStorage = undefined;

            const noteResponseDto = new NoteResponseDto();
            // установка commonNoteID приведет к вызову редиректа на перерисовке CreateView:
            // commonNoteID также выставляется при сохранении новой заметки:
            noteResponseDto.commonNoteID = Number(id);
            props.subscriber.setData(noteResponseDto);
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
            {props.jsonStorage ?
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

const SubmitButton = (props: IComplexProps) => {
    let requestBody: string = "";// храним во внешнем стейте
    let similarNoteNameStorage: string[] = [];
    const similarNotesIdStorage: string[] = [];

    const cancel = (e: React.SyntheticEvent) => {
        e.preventDefault();
        let buttonElement = (document.getElementById('cancelButton') as HTMLInputElement);
        buttonElement.style.display = "none";

        // отмена - сохраняем текст и название:
        StateStorageWrapper.submitStateStorage = undefined;
        StateStorageWrapper.setState(0);
        // восстанавливаем requestBody - из внешнего стейта:
        if (requestBody == "") requestBody = StateStorageWrapper.requestBodyStorage;

        let text = getTextRequest(JSON.parse(requestBody));
        let title = getTitleRequest(JSON.parse(requestBody));

        requestBody = JSON.stringify({
            "tagsCheckedRequest": [],
            "textRequest": text,
            "titleRequest": title
        });
        Loader.unusedPromise = Loader.postData(props.subscriber, requestBody, Loader.createUrl);
    }

    const componentDidMount = () => {
        let buttonElement = (document.getElementById('cancelButton') as HTMLInputElement);
        buttonElement.style.display = "none";
    }
    const componentWillUnmount = () => {
        // перед выходом восстанавливаем состояние обёртки:
        StateStorageWrapper.setState(0);
        StateStorageWrapper.submitStateStorage = undefined;
        StateStorageWrapper.requestBodyStorage = "";
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
            // восстановим requestBody из внешнего стейта:
            if (requestBody == "") requestBody = StateStorageWrapper.requestBodyStorage;
            Loader.unusedPromise = Loader.postData(props.subscriber, requestBody, Loader.createUrl);
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
        // сохраним requestBody во внешний стейт:
        StateStorageWrapper.requestBodyStorage = requestBody;

        similarNoteNameStorage = [];
        let promise = findSimilarNotes(formMessage, formTitle);
        await promise;

        if (similarNoteNameStorage.length > 0) {
            // переключение в режим "подтверждение/отмена":
            StateStorageWrapper.submitStateStorage = StateStorageWrapper.getState();
            buttonElement.style.display = "block";
            StateStorageWrapper.setState(1);
            return;
        }

        // совпадения не обнаружены, сохраняем заметку ("стандартный" режим):
        Loader.unusedPromise = Loader.postData(props.subscriber, requestBody, Loader.createUrl);
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
            console.log("Find similar notes on create: err in try-catch scope");
        }

        if (promise) {
            await promise;
        }
    }

    const getNoteTitles = async (data: Response) => {
        let responseDto = data as unknown as ComplianceResponseDto;
        let response = responseDto.res;
        if (!response) {
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
            if (similarNoteNameStorage.length >= 10) {
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

            if (promise) {
                await promise;
            }
        }
    }

    const getTitle = (input: Response, i: string) => {
        let responseDto = input as unknown as ComplianceResponseDto;
        similarNoteNameStorage.push((responseDto.res + '\r\n'));
        similarNotesIdStorage.push(i);

        // stub:
        let data = {
            "structuredTagsListResponse": similarNoteNameStorage,
            "tagsCheckedUncheckedResponse": [],
            "textResponse": getTextRequest(JSON.parse(requestBody)),
            "titleResponse": getTitleRequest(JSON.parse(requestBody)),
            "tagIdsInternal": similarNotesIdStorage
        };
        // subscriber: CreateView:
        props.subscriber.setData(data);
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

import * as React from 'react';
import {Dispatch, SetStateAction, useEffect, useReducer, useRef, useState} from "react";
import {Loader} from "../common/loader.tsx";
import {
    getCommonNoteId, getStructuredTagsListResponse, getTagsCheckedUncheckedResponse,
    getTextRequest, getTextResponse, getTitleRequest,
    getTitleResponse, setTextResponse, setTitleResponse
} from "../common/dto.handlers.tsx";
import {NoteResponseDto, ComplianceResponseDto} from "../dto/request.response.dto.tsx";
import {FunctionComponentStateWrapper, CommonStateStorage} from "../common/state.wrappers.tsx";

export const CreateView = () => {
    const [data, setState] = useState<NoteResponseDto|null>(null);
    const mounted = useState(true);
    const stateWrapper = new FunctionComponentStateWrapper<NoteResponseDto>(mounted, setState);
    const refObject: React.MutableRefObject<HTMLFormElement|undefined> = useRef();
    let formElement: HTMLFormElement|undefined = refObject.current;

    const componentDidMount = () => {
        formElement = refObject.current;
        Loader.unusedPromise = Loader.getData(stateWrapper, Loader.createUrl);
    }
    const componentWillUnmount = () => {
        // переход на update при несохраненной заметке не приведёт к ошибке 400 (сервер не понимает NaN):
        if (isNaN(CommonStateStorage.noteIdStorage)) CommonStateStorage.noteIdStorage = 0;
        // перед выходом восстанавливаем состояние обёртки:
        CommonStateStorage.init();
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
            // по сути это переход на другой компонент, поэтому сбросим общий стейт:
            CommonStateStorage.init();
            Loader.redirectToMenu("/#/read/" + redirectId);
        }

        let id = 0;
        if (data) {
            id = Number(getCommonNoteId(data));
        }

        if (id !== 0) {
            CommonStateStorage.noteIdStorage = id;
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
            checkboxes.push(<Checkbox key={`checkbox ${i}${time}`} id={String(i)} noteDto={data} onClick={stateWrapper.setData}/>);
        }
    }

    if (data) {
        if (!getTextResponse(data)) setTextResponse(data, "");
        if (!getTitleResponse(data)) setTitleResponse(data, "");
    }

    const castedRefObject = refObject as React.LegacyRef<HTMLFormElement>|undefined;
    return (
        <div id="renderContainer">
            <form ref={castedRefObject}
                  id="textbox">
                {checkboxes}
                {/** subscription={stateWrapper} дублируются для SubmitButton (изначально) и Checkbox (перенесены из SubmitButton): */}
                {data && <SubmitButton formElement={formElement} stateWrapper={stateWrapper} />}
            </form>
            {data && <Note noteDto={data} />}
        </div>
    );
}

const Checkbox = (props: {noteDto: NoteResponseDto, id: string, onClick: Dispatch<SetStateAction<NoteResponseDto|null>>}) => {
    const checked = getTagsCheckedUncheckedResponse(props) === "checked";

    const getTagName = (i: number) => {
        return getStructuredTagsListResponse(props.noteDto)[i];
    };

    const getTagId = (i: number) => {
        if (props.noteDto.tagIdsInternal && props.noteDto.tagIdsInternal.length > 0) {
            return props.noteDto.tagIdsInternal[i];
        } else if (props.noteDto.structuredTagsListResponse) {
            return props.noteDto.structuredTagsListResponse[i];
        } else {
            return "";
        }
    };

    const loadNoteOnClick = (e: React.SyntheticEvent) => {
        if (CommonStateStorage.commonState == 1) {
            let title = e.currentTarget.innerHTML.valueOf();
            // item(1) это аттрибут about, в неём должен храниться id заметки, на который указывает данный чекбокс:
            let id = e.currentTarget.attributes.item(1)?.nodeValue;
            console.log(`Submitted & redirected: state: ${CommonStateStorage.commonState} title: ${title} id: ${id}`);

            const noteResponseDto = new NoteResponseDto();
            // установка commonNoteID приведет к вызову редиректа на перерисовке CreateView:
            // commonNoteID также выставляется при сохранении новой заметки:
            noteResponseDto.commonNoteID = Number(id);
            props.onClick(noteResponseDto);
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

const Note = (props: {noteDto: NoteResponseDto}) => {
    const [, forceUpdate] = useReducer(x => x + 1, 0);

    const textHandler = (e: string) => {
        setTextResponse(props.noteDto, e);
        forceUpdate();
    }

    const titleHandler = (e: string) => {
        setTitleResponse(props.noteDto, e);
        forceUpdate();
    }

    return (
        <div>
            <p/>
            {props.noteDto ?
                <div>
                    <h5>
                        <textarea name="ttl" cols={66} rows={1} form="textbox"
                                  value={getTitleResponse(props.noteDto)}
                                  onChange={e => titleHandler(e.target.value)}/>
                    </h5>
                    <h5>
                        <textarea name="msg" cols={66} rows={30} form="textbox"
                                  value={getTextResponse(props.noteDto)}
                                  onChange={e => textHandler(e.target.value)}/>
                    </h5>
                </div>
                : "loading.."}
        </div>
    );
}

const SubmitButton = (props: {formElement?: HTMLFormElement, stateWrapper: FunctionComponentStateWrapper<NoteResponseDto>}) => {
    let jsonString: string = "";// храним во внешнем стейте
    let similarNoteNameStorage: string[] = [];
    const similarNotesIdStorage: string[] = [];

    const cancel = (e: React.SyntheticEvent) => {
        e.preventDefault();
        const buttonElement = (document.getElementById('cancelButton') as HTMLInputElement);
        buttonElement.style.display = "none";
        CommonStateStorage.commonState = 0;

        // отмена - сохраняем текст и название: восстанавливаем requestBody из внешнего стейта:
        if (jsonString == "") jsonString = CommonStateStorage.jsonStringStorage;
        const text = getTextRequest(JSON.parse(jsonString));
        const title = getTitleRequest(JSON.parse(jsonString));
        jsonString = JSON.stringify({
            "tagsCheckedRequest": [],
            "textRequest": text,
            "titleRequest": title
        });

        Loader.unusedPromise = Loader.postData(props.stateWrapper, jsonString, Loader.createUrl);
    }

    const componentDidMount = () => {
        let buttonElement = (document.getElementById('cancelButton') as HTMLInputElement);
        buttonElement.style.display = "none";
    }
    const componentWillUnmount = () => {
        // перед выходом восстанавливаем состояние обёртки:
        //StateStorage.commonState = 0;
        //StateStorage.requestBodyStorage = "";
        CommonStateStorage.init();
    }

    useEffect(() => {
        componentDidMount();
        return componentWillUnmount;
    }, []);

    const submit = async (e: React.SyntheticEvent) => {
        e.preventDefault();

        const buttonElement = (document.getElementById('cancelButton') as HTMLInputElement);
        buttonElement.style.display = "none";

        if (CommonStateStorage.commonState === 1) {
            // подтверждение: режим "подтверждение/отмена": восстанавливаем requestBody из внешнего стейта:
            CommonStateStorage.commonState = 0;
            if (jsonString == "") jsonString = CommonStateStorage.jsonStringStorage;
            Loader.unusedPromise = Loader.postData(props.stateWrapper, jsonString, Loader.createUrl);
            return;
        }

        const formData = new FormData(props.formElement);
        const checkboxesArray = (formData.getAll('chkButton')).map(a => Number(a) + 1);
        const formMessage = formData.get('msg');
        const formTitle = formData.get('ttl');
        const item = {
            "tagsCheckedRequest": checkboxesArray,
            "textRequest": formMessage,
            "titleRequest": formTitle
        };
        jsonString = JSON.stringify(item);
        similarNoteNameStorage = [];
        await findSimilarNotes(formMessage, formTitle);
        if (similarNoteNameStorage.length > 0) {
            // сохраним requestBody во внешний стейт:
            CommonStateStorage.jsonStringStorage = jsonString;
            // переключение в режим "подтверждение/отмена":
            buttonElement.style.display = "block";
            CommonStateStorage.commonState = 1;
            return;
        }

        // совпадения не обнаружены, сохраняем заметку ("стандартный" режим):
        Loader.unusedPromise = Loader.postData(props.stateWrapper, jsonString, Loader.createUrl);
    }

    const findSimilarNotes = async (formMessage: FormDataEntryValue|null, formTitle: FormDataEntryValue|null) => {
        if (typeof formMessage === "string") {
            formMessage = formMessage.replace(/\r\n|\r|\n/g, " ");
        }

        const query = "?text=" + formMessage + " " + formTitle;
        const callback = (data: Response) => getNoteTitles(data);
        try {
            await Loader.getWithPromise(Loader.complianceIndicesUrl, query, callback);
        } catch (err) {
            console.log(`Find similar notes on create: ${err} in try-catch scope`);
        }
    }

    const getNoteTitles = async (response: Response) => {
        const responseDto = response as unknown as ComplianceResponseDto;
        const responseResult = responseDto.res;
        if (!responseResult) {
            return;
        }

        const array: number[][] = Object.keys(responseResult).map((key) => [Number(key), responseResult[Number(key)]]);
        array.sort(function (a, b) {
            return b[1] - a[1]
        });

        const result = [];
        for (let index in array) {
            result.push(array[index]);
        }

        if (result.length === 0) {
            return;
        }

        for (let index = 0; index < result.length; index++) {
            // лучше сделать reject:
            if (similarNoteNameStorage.length >= 10) {
                continue;
            }

            // получаем имена возможных совпадений: i:string зто id заметки, можно вместо time его выставлять:
            const id = String(result[index][0]);
            const query = "?id=" + id;
            const callback = (data: Response) => getTitle(data, id);
            try {
                await Loader.getWithPromise(Loader.readTitleUrl, query, callback);
            } catch (err) {
                console.log(`Get note titles on create: ${err} in try-catch scope`);
            }
        }
    }

    const getTitle = (response: Response, id: string) => {
        const responseDto = response as unknown as ComplianceResponseDto;
        similarNoteNameStorage.push((responseDto.res + '\r\n'));
        similarNotesIdStorage.push(id);

        const data = {
            "structuredTagsListResponse": similarNoteNameStorage,
            "tagsCheckedUncheckedResponse": [],
            "textResponse": getTextRequest(JSON.parse(jsonString)),
            "titleResponse": getTitleRequest(JSON.parse(jsonString)),
            "tagIdsInternal": similarNotesIdStorage
        };
        props.stateWrapper.setData(data);
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

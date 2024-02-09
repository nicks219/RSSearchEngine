import * as React from 'react';
import {Dispatch, SetStateAction, useContext, useEffect, useReducer, useRef, useState} from "react";
import {Loader} from "../common/loader";
import {
    getCommonNoteId, getStructuredTagsListResponse, getTagsCheckedUncheckedResponse,
    getTextRequest, getTextResponse, getTitleRequest,
    getTitleResponse, setTextResponse, setTitleResponse
} from "../common/dto.handlers";
import {NoteResponseDto, ComplianceResponseDto} from "../dto/request.response.dto";
import {CreateComponentMode, FunctionComponentStateWrapper} from "../common/state.wrappers";
import {CommonContext, RecoveryContext} from "../common/context.provider";

export const CreateContainer = () => {
    const [data, setState] = useState<NoteResponseDto|null>(null);
    const mounted = useState(true);
    const stateWrapper = new FunctionComponentStateWrapper<NoteResponseDto>(mounted, setState);
    const commonContext = useContext(CommonContext);
    const recoveryContext = useContext(RecoveryContext);
    const refObject: React.MutableRefObject<HTMLFormElement|undefined> = useRef();
    let formElement: HTMLFormElement|undefined = refObject.current;

    const onMount = () => {
        formElement = refObject.current;
        Loader.unusedPromise = Loader.getData(stateWrapper, Loader.createUrl, recoveryContext);
    }
    const onUnmount = () => {
        // переход на update при несохраненной заметке не приведёт к ошибке 400 (сервер не понимает NaN):
        if (isNaN(commonContext.commonNumber)) commonContext.commonNumber = 0;
        // перед выходом восстанавливаем состояние обёртки:
        // TODO: состояние обертки можно восстанавливать после монтирования компонента, а не на выходе:
        commonContext.init();
        mounted[0] = false;
    }

    useEffect(() => {
        onMount();
        return onUnmount;
    }, []);

    const componentDidUpdate = () => {
        const redirectId = data?.commonNoteID;
        // TODO: в commonNoteID записывается также id созданной заметки, поправь:
        const okResponse: string = "[OK]";
        if (redirectId && data?.titleResponse !== okResponse) {
            console.log("Redirected: " + redirectId);
            // по сути это переход на другой компонент, поэтому сбросим общий стейт:
            commonContext.init();
            Loader.redirectToMenu("/#/read/" + redirectId);
        }

        if (data) {
            const id = Number(getCommonNoteId(data));
            // if (id !== 0) {
            if (id !== 0) {
                commonContext.commonNumber = id;
            }
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
    const context = useContext(CommonContext);
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
        if (context.createComponentMode == CreateComponentMode.ExtendedMode) {
            let title = e.currentTarget.innerHTML.valueOf();
            // item(1) это аттрибут about, в нём должен храниться id заметки, на который указывает данный чекбокс:
            let id = e.currentTarget.attributes.item(1)?.nodeValue;
            console.log(`Submitted & redirected: state: ${context.createComponentMode} title: ${title} id: ${id}`);

            const noteResponseDto = new NoteResponseDto();
            // установка commonNoteID приведет к вызову редиректа после перерисовки CreateView:
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

    // TODO: напиши новый вариант ввода, без апдейта на каждый символ:
    const textHandler = (e: string) => {
        setTextResponse(props.noteDto, e);
        forceUpdate();
    }

    // TODO: напиши новый вариант ввода, без апдейта на каждый символ:
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
    let jsonString: string = "";
    let similarNoteNameStorage: string[] = [];
    const similarNotesIdStorage: string[] = [];

    const context = useContext(CommonContext);
    const cancel = (e: React.SyntheticEvent) => {
        e.preventDefault();
        const buttonElement = (document.getElementById('cancelButton') as HTMLInputElement);
        buttonElement.style.display = "none";
        context.createComponentMode = CreateComponentMode.ClassicMode;

        // отмена: восстанавливаем текст и название заметки, при необходимости из внешнего стейта:
        if (jsonString == "") jsonString = context.createComponentString;
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
        context.init();
    }

    useEffect(() => {
        componentDidMount();
        return componentWillUnmount;
    }, []);

    const submit = async (e: React.SyntheticEvent) => {
        e.preventDefault();

        const buttonElement = (document.getElementById('cancelButton') as HTMLInputElement);
        buttonElement.style.display = "none";

        if (context.createComponentMode === CreateComponentMode.ExtendedMode) {
            // подтверждение: режим "подтверждение/отмена": при необходимости восстанавливаем заметку из внешнего стейта:
            context.createComponentMode = CreateComponentMode.ClassicMode;
            if (jsonString == "") jsonString = context.createComponentString;
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
            context.createComponentString = jsonString;
            // переключение в режим "подтверждение/отмена":
            buttonElement.style.display = "block";
            context.createComponentMode = CreateComponentMode.ExtendedMode;
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

            // получаем имена возможных совпадений: id: string зто id заметки, можно вместо time его выставлять:
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

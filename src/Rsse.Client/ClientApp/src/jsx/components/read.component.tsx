import * as React from 'react';
import {Loader} from "../common/loader.tsx";
import {useParams} from "react-router-dom";
import {
    getCommonNoteId, getStructuredTagsListResponse,
    getTextResponse, getTitleResponse
} from "../common/dto.handlers.tsx";
import {createRoot, Root} from "react-dom/client";
import {NoteResponseDto} from "../dto/request.response.dto.tsx";
import {toggleMenuVisibility} from "../common/visibility.handlers.tsx";
import {useEffect, useRef, useState} from "react";
import {FunctionComponentStateWrapper, StateStorageWrapper} from "../common/state.wrappers.tsx";

export const ReadView = () => {
    const params = useParams();
    return <ReadViewParametrized noteId={params.textId} />;
}

class SearchButtonContainer {
    static searchButtonOneElement = document.querySelector("#searchButton1") ?? document.createElement('searchButton1');
    private static _root?: Root = createRoot(this.searchButtonOneElement);
    static getRoot: Root = (this._root)? this._root : createRoot(this.searchButtonOneElement);
}

const ReadViewParametrized = (props: {noteId?: string}) => {
    const [data, setData] = useState<NoteResponseDto|null>(null);
    const mounted = useState(true);
    const stateWrapper = new FunctionComponentStateWrapper(mounted, setData);

    const refObject:  React.MutableRefObject<HTMLFormElement|undefined> = useRef();
    let formElement: HTMLFormElement|undefined = refObject.current;

    const componentDidMount = () => {
        formElement = refObject.current;
        if (!StateStorageWrapper.redirectCall) {
            Loader.unusedPromise = Loader.getData(stateWrapper, Loader.readUrl);
        } else {
            // при редиректе из каталога (read/id) не обновляем содержимое компонента и убираем чекбоксы с логином:
            if (formElement) formElement.style.display = "none";
            (document.getElementById("login") as HTMLElement).style.display = "none";
        }

        StateStorageWrapper.redirectCall = false;
    }

    const componentWillUnmount = () => {
        mounted[0] = false;
        // перед выходом восстанавливаем состояние обёртки:
        StateStorageWrapper.renderedAfterRedirect = false;
        StateStorageWrapper.redirectCall = false;
        // убираем отображение кнопки "Поиск":
        SearchButtonContainer.getRoot.render(
            <div>
            </div>
        );
    }

    useEffect(() => {
        componentDidMount();
        return componentWillUnmount;
    }, []);

    const componentDidUpdate = () => {
        SearchButtonContainer.getRoot.render(
            <div>
                <SubmitButton stateWrapper={stateWrapper} formElement={formElement} />
            </div>
        );
        (document.getElementById("header") as HTMLElement).style.backgroundColor = "#405060";//"#e9ecee"
    }

    useEffect(() => {
        componentDidUpdate();
    }, [data]);

    if (props.noteId && !StateStorageWrapper.renderedAfterRedirect) {
        // редирект из каталога:
        console.log(`Get text id from path params: ${props.noteId}`);

        const item = {
            "tagsCheckedRequest": []
        };
        let requestBody = JSON.stringify(item);

        StateStorageWrapper.redirectCall = true;
        StateStorageWrapper.renderedAfterRedirect = true;
        Loader.unusedPromise = Loader.postData(stateWrapper, requestBody, Loader.readUrl, props.noteId);
    }

    let checkboxes = [];
    if (data) {
        for (let i = 0; i < getStructuredTagsListResponse(data).length; i++) {
            checkboxes.push(<Checkbox key={`checkbox ${i}`} id={String(i)} noteDto={data} />);
        }
    }

    const castedRefObject = refObject as React.LegacyRef<HTMLFormElement>|undefined;
    return (
        <div>
            <form ref={castedRefObject} id="dizzy">{checkboxes}</form>
            <div id="messageBox">
                {data && getTextResponse(data) && <Note formElement={formElement} noteDto={data} />}
            </div>
        </div>
    );
}

const Checkbox = (props: {id: string, noteDto: NoteResponseDto}) => {
    let getTagName = (i: number) => getStructuredTagsListResponse(props.noteDto)[i];
    return (
        <div id="checkboxStyle">
            <input name="chkButton" value={props.id} type="checkbox" id={props.id}
                   className="regular-checkbox"
                   defaultChecked={false}/>
            <label htmlFor={props.id}>{getTagName(Number(props.id))}</label>
        </div>
    );
}

const Note = (props: {formElement?: HTMLFormElement, noteDto: NoteResponseDto}) => {
    const hideMenu = () => {
        if (props.formElement) {
            props.formElement.style.display = toggleMenuVisibility(props.formElement.style.display);
        }
    }

    if (props.noteDto && Number(getCommonNoteId(props.noteDto)) !== 0) {
        window.noteIdStorage = Number(getCommonNoteId(props.noteDto));
    }

    return (
        <span>
                {props.noteDto ? (getTextResponse(props.noteDto) ?
                        <span>
                        <div id="songTitle" onClick={hideMenu}>
                            {getTitleResponse(props.noteDto)}
                        </div>
                        <div id="songBody">
                            <NoteTextSupportsLinks noteText={getTextResponse(props.noteDto) ?? ""}/>
                        </div>
                    </span>
                        : "select tag please")
                    : ""}
            </span>
    );
}

const NoteTextSupportsLinks = (props: {noteText: string}): JSX.Element => {
    // deprecated: JSX
    let res: (string | JSX.Element)[] = [];
    // https://css-tricks.com/almanac/properties/o/overflow-wrap/#:~:text=overflow%2Dwrap%20is%20generally%20used,%2C%20and%20Korean%20(CJK).
    props && props.noteText.replace(
        /((?:https?:\/\/|ftps?:\/\/|\bwww\.)(?:(?![.,?!;:()]*(?:\s|$))[^\s]){2,})|(\n+|(?:(?!(?:https?:\/\/|ftp:\/\/|\bwww\.)(?:(?![.,?!;:()]*(?:\s|$))[^\s]){2,}).)+)/gim,
        (_: string, link: string, text: string): string => {
            res.push(link ? <a href={(link[0] === "w" ? "//" : "") + link} key={res.length}>{link}</a> : text);
            return "";
        })

    return <div className="user-text">{res}</div>
}

const SubmitButton = (props: {formElement?: HTMLFormElement, stateWrapper: FunctionComponentStateWrapper<NoteResponseDto>}) => {
    const submit = (e: React.SyntheticEvent) => {
        e.preventDefault();
        (document.getElementById("login") as HTMLElement).style.display = "none";
        let formData = new FormData(props.formElement);
        let checkboxesArray = (formData.getAll("chkButton")).map(a => Number(a) + 1);
        const item = {
            "tagsCheckedRequest": checkboxesArray
        };
        let requestBody = JSON.stringify(item);
        Loader.unusedPromise = Loader.postData(props.stateWrapper, requestBody, Loader.readUrl);
        (document.getElementById("header") as HTMLElement).style.backgroundColor = "slategrey";
    }

    return (
        <div id="submitStyle">
            <input form="dizzy" type="checkbox" id="submitButton" className="regular-checkbox" onClick={submit}/>
            <label htmlFor="submitButton">Поиск</label>
        </div>
    );
}

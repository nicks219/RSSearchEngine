import * as React from 'react';
import {useContext, useEffect, useRef, useState} from "react";
import {useParams} from "react-router-dom";
import {createRoot, Root} from "react-dom/client";

import {Loader} from "../common/loader";
import {getCommonNoteId, getStructuredTagsListResponse, getTextResponse, getTitleResponse} from "../common/dto.handlers";
import {NoteResponseDto} from "../dto/request.response.dto";
import {toggleMenuVisibility} from "../common/visibility.handlers";
import {FunctionComponentStateWrapper} from "../common/state.wrappers";
import {CommonContext} from "../common/context.provider";

export const ReadContainer = () => {
    const params = useParams();
    return <ReadContainerParametrized noteId={params.textId} />;
}

class SearchButtonRoot {
    static searchButtonOneElement = document.querySelector("#searchButton1") ?? document.createElement('searchButton1');
    private static _root?: Root = createRoot(this.searchButtonOneElement);
    static getRoot: Root = (this._root)? this._root : createRoot(this.searchButtonOneElement);
}

const ReadContainerParametrized = (props: {noteId?: string}) => {
    const [data, setData] = useState<NoteResponseDto|null>(null);
    const mounted = useState(true);
    const stateWrapper = new FunctionComponentStateWrapper(mounted, setData);

    const refObject:  React.MutableRefObject<HTMLFormElement|undefined> = useRef();
    let formElement: HTMLFormElement|undefined = refObject.current;

    const onMount = () => {
        formElement = refObject.current;
        if (props.noteId && !data) {
            // при редиректе из каталога или из Create (read/id) не обновляем содержимое компонента и убираем чекбоксы с логином:
            // TODO: вынести style=none в отдельную функцию
            if (formElement) formElement.style.display = "none";
            (document.getElementById("login") as HTMLElement).style.display = "none";
        } else {
            Loader.unusedPromise = Loader.getData(stateWrapper, Loader.readUrl);
        }
    }

    const onUnmount = () => {
        mounted[0] = false;
        // убираем отображение кнопки "Поиск":
        SearchButtonRoot.getRoot.render(<div></div>);
    }

    useEffect(() => {
        onMount();
        return onUnmount;
    }, []);

    const componentDidUpdate = () => {
        SearchButtonRoot.getRoot.render(
            <div>
                <SubmitButton stateWrapper={stateWrapper} formElement={formElement} />
            </div>
        );
        (document.getElementById("header") as HTMLElement).style.backgroundColor = "#405060";//"#e9ecee"
    }

    useEffect(() => {
        componentDidUpdate();
    }, [data]);

    // если data не определена, значит fetch для id ещё не выполнялся:
    if (props.noteId && !data) {
        // редирект из каталога:
        console.log(`Get text id from path params: ${props.noteId}`);
        const item = {"tagsCheckedRequest": []};
        const requestBody = JSON.stringify(item);

        Loader.unusedPromise = Loader.postData(stateWrapper, requestBody, Loader.readUrl, props.noteId);
    }

    let checkboxes = [];
    if (data) {
        for (let index = 0; index < getStructuredTagsListResponse(data).length; index++) {
            checkboxes.push(<Checkbox key={`checkbox ${index}`} id={String(index)} noteDto={data} />);
        }
    }

    const castedRefObject = refObject as React.LegacyRef<HTMLFormElement>|undefined;
    return (
        <div>
            <form ref={castedRefObject} id="textbox">{checkboxes}</form>
            <div id="messageBox">
                {data && getTextResponse(data) && <Note formElement={formElement} noteDto={data} />}
            </div>
        </div>
    );
}

const Checkbox = (props: {id: string, noteDto: NoteResponseDto}) => {
    const getTagName = (i: number) => getStructuredTagsListResponse(props.noteDto)[i];
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
    const commonContext = useContext(CommonContext);
    const hideMenu = () => {
        if (props.formElement) {
            props.formElement.style.display = toggleMenuVisibility(props.formElement.style.display);
        }
    }

    if (props.noteDto && Number(getCommonNoteId(props.noteDto)) !== 0) {
        commonContext.commonNumber = Number(getCommonNoteId(props.noteDto));
    }

    return (
        <span>
            {props.noteDto ? (getTextResponse(props.noteDto) ?
                <span>
                    <div id="noteTitle" onClick={hideMenu}>
                        {getTitleResponse(props.noteDto)}
                    </div>
                    <div id="noteText">
                        <TextSupportsLinks text={getTextResponse(props.noteDto) ?? ""}/>
                    </div>
                </span>
                : "select tag please")
            : ""}
        </span>
    );
}

const TextSupportsLinks = (props: {text: string}): JSX.Element => {
    const result: (string | JSX.Element)[] = [];
    // https://css-tricks.com/almanac/properties/o/overflow-wrap/#:~:text=overflow%2Dwrap%20is%20generally%20used,%2C%20and%20Korean%20(CJK).
    props && props.text.replace(
        /((?:https?:\/\/|ftps?:\/\/|\bwww\.)(?:(?![.,?!;:()]*(?:\s|$))[^\s]){2,})|(\n+|(?:(?!(?:https?:\/\/|ftp:\/\/|\bwww\.)(?:(?![.,?!;:()]*(?:\s|$))[^\s]){2,}).)+)/gim,
        (_: string, link: string, text: string): string => {
            result.push(link ? <a href={(link[0] === "w" ? "//" : "") + link} key={result.length}>{link}</a> : text);
            return "";
        })

    return <div className="user-text">{result}</div>
}

const SubmitButton = (props: {formElement?: HTMLFormElement, stateWrapper: FunctionComponentStateWrapper<NoteResponseDto>}) => {
    const submit = (e: React.SyntheticEvent) => {
        e.preventDefault();
        (document.getElementById("login") as HTMLElement).style.display = "none";
        const formData = new FormData(props.formElement);
        const checkboxesArray = (formData.getAll("chkButton")).map(a => Number(a) + 1);
        const item = {
            "tagsCheckedRequest": checkboxesArray
        };
        const requestBody = JSON.stringify(item);
        Loader.unusedPromise = Loader.postData(props.stateWrapper, requestBody, Loader.readUrl);
        (document.getElementById("header") as HTMLElement).style.backgroundColor = "slategrey";
    }

    return (
        <div id="submitStyle">
            <input form="textbox" type="checkbox" id="submitButton" className="regular-checkbox" onClick={submit}/>
            <label htmlFor="submitButton">Поиск</label>
        </div>
    );
}

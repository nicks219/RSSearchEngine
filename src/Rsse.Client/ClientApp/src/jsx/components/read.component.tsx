import * as React from 'react';
import {Loader} from "../common/loader.tsx";
import {useParams} from "react-router-dom";
import {
    getCommonNoteId, getStructuredTagsListResponse,
    getTextResponse, getTitleResponse
} from "../common/dto.handlers.tsx";
import {createRoot, Root} from "react-dom/client";
import {NoteResponseDto} from "../dto/request.response.dto.tsx";
import {ISimpleProps, IComplexProps} from "../common/contracts.tsx";
import {toggleMenuVisibility} from "../common/visibility.handlers.tsx";
import {useEffect, useRef, useState} from "react";
import {FunctionComponentStateWrapper, StateStorageWrapper} from "../common/state.wrappers.tsx";

export const ReadView = () => {
    const params = useParams();
    return <ReadViewParametrized textId={params.textId} />;
}

class SearchButtonContainer {
    static searchButtonOneElement = document.querySelector("#searchButton1") ?? document.createElement('searchButton1');
    static getRoot: Root = createRoot(this.searchButtonOneElement);
}

const ReadViewParametrized = (props: {textId?: string}) => {
    const [data, setData] = useState<NoteResponseDto|null>(null);
    const mounted = useState(true);
    const stateWrapper = new FunctionComponentStateWrapper(mounted, setData);

    const refObject:  React.MutableRefObject<HTMLFormElement|undefined> = useRef();
    let formId: HTMLFormElement|undefined = refObject.current;

    const componentDidMount = () => {
        formId = refObject.current;
        if (!StateStorageWrapper.redirectCall) {
            Loader.unusedPromise = Loader.getData(stateWrapper, Loader.readUrl);
        } else {
            // при редиректе из каталога (read/id) не обновляем содержимое компонента и убираем чекбоксы с логином:
            if (formId) formId.style.display = "none";
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
                <SubmitButton subscriber={stateWrapper} formId={formId} jsonStorage={undefined} id={undefined}/>
            </div>
        );
        (document.getElementById("header") as HTMLElement).style.backgroundColor = "#405060";//"#e9ecee"
    }

    useEffect(() => {
        componentDidUpdate();
    }, [data]);

    if (props.textId && !StateStorageWrapper.renderedAfterRedirect) {
        // редирект из каталога:
        console.log(`Get text id from path params: ${props.textId}`);

        const item = {
            "tagsCheckedRequest": []
        };
        let requestBody = JSON.stringify(item);

        StateStorageWrapper.redirectCall = true;
        StateStorageWrapper.renderedAfterRedirect = true;
        Loader.unusedPromise = Loader.postData(stateWrapper, requestBody, Loader.readUrl, props.textId);
    }

    let checkboxes = [];
    if (data) {
        for (let i = 0; i < getStructuredTagsListResponse(data).length; i++) {
            checkboxes.push(<Checkbox key={`checkbox ${i}`} id={String(i)} jsonStorage={data} formId={undefined}/>);
        }
    }

    const castedRefObject = refObject as React.LegacyRef<HTMLFormElement> | undefined;
    return (
        <div>
            <form ref={castedRefObject} id="dizzy">{checkboxes}</form>
            <div id="messageBox">
                {data && getTextResponse(data) && <Message formId={formId} jsonStorage={data} id={undefined}/>}
            </div>
        </div>
    );
}

const Checkbox = (props: ISimpleProps) => {
    let getTagName = (i: number) => {
        return getStructuredTagsListResponse(props.jsonStorage)[i];
    };
    return (
        <div id="checkboxStyle">
            <input name="chkButton" value={props.id} type="checkbox" id={props.id}
                   className="regular-checkbox"
                   defaultChecked={false}/>
            <label htmlFor={props.id}>{getTagName(Number(props.id))}</label>
        </div>
    );
}

const Message = (props: ISimpleProps) => {
    const hideMenu = () => {
        if (props.formId) {
            props.formId.style.display = toggleMenuVisibility(props.formId.style.display);
        }
    }

    if (props.jsonStorage && Number(getCommonNoteId(props.jsonStorage)) !== 0) {
        window.noteIdStorage = Number(getCommonNoteId(props.jsonStorage));
    }

    return (
        <span>
                {props.jsonStorage ? (getTextResponse(props.jsonStorage) ?
                        <span>
                        <div id="songTitle" onClick={hideMenu}>
                            {getTitleResponse(props.jsonStorage)}
                        </div>
                        <div id="songBody">
                            <NoteTextSupportsLinks text={getTextResponse(props.jsonStorage) ?? ""}/>
                        </div>
                    </span>
                        : "select tag please")
                    : ""}
            </span>
    );
}

const NoteTextSupportsLinks = (props: {text:string}): JSX.Element => {
    // deprecated: JSX
    let res: (string | JSX.Element)[] = [];
    // https://css-tricks.com/almanac/properties/o/overflow-wrap/#:~:text=overflow%2Dwrap%20is%20generally%20used,%2C%20and%20Korean%20(CJK).
    props && props.text.replace(
        /((?:https?:\/\/|ftps?:\/\/|\bwww\.)(?:(?![.,?!;:()]*(?:\s|$))[^\s]){2,})|(\n+|(?:(?!(?:https?:\/\/|ftp:\/\/|\bwww\.)(?:(?![.,?!;:()]*(?:\s|$))[^\s]){2,}).)+)/gim,
        (_: string, link: string, text: string): string => {
            res.push(link ? <a href={(link[0] === "w" ? "//" : "") + link} key={res.length}>{link}</a> : text);
            return "";
        })

    return <div className="user-text">{res}</div>
}

const SubmitButton = (props: IComplexProps) => {
    const submit = (e: React.SyntheticEvent) => {
        e.preventDefault();
        (document.getElementById("login") as HTMLElement).style.display = "none";
        let formData = new FormData(props.formId);
        let checkboxesArray = (formData.getAll("chkButton")).map(a => Number(a) + 1);
        const item = {
            "tagsCheckedRequest": checkboxesArray
        };
        let requestBody = JSON.stringify(item);
        Loader.unusedPromise = Loader.postData(props.subscriber, requestBody, Loader.readUrl);
        (document.getElementById("header") as HTMLElement).style.backgroundColor = "slategrey";
    }

    return (
        <div id="submitStyle">
            <input form="dizzy" type="checkbox" id="submitButton" className="regular-checkbox" onClick={submit}/>
            <label htmlFor="submitButton">Поиск</label>
        </div>
    );
}

import * as React from 'react';
import {Loader} from "../common/loader.tsx";
import {
    getStructuredTagsListResponse, getTagsCheckedUncheckedResponse, getTextResponse,
    getTitleResponse, setTextResponse
} from "../common/dto.handlers.tsx";
import {NoteResponseDto} from "../dto/request.response.dto.tsx";
import {toggleMenuVisibility} from '../common/visibility.handlers.tsx';
import {useEffect, useReducer, useRef, useState} from "react";
import {FunctionComponentStateWrapper} from "../common/state.wrappers.tsx";

export const UpdateView = () => {
    const [data, setData] = useState<NoteResponseDto|null>(null);
    const mounted = useState(true);
    const stateWrapper = new FunctionComponentStateWrapper<NoteResponseDto>(mounted, setData);

    const refObject: React.MutableRefObject<HTMLFormElement|undefined> = useRef();
    let formElement: HTMLFormElement|undefined = refObject.current;

    const componentDidMount = () => {
        formElement = refObject.current;
        Loader.unusedPromise = Loader.getDataById(stateWrapper, window.noteIdStorage, Loader.updateUrl);
    }

    const componentWillUnmount = () => {
        mounted[0] = false;
    }

    useEffect(() => {
        componentDidMount();
        return componentWillUnmount;
    }, []);

    const checkboxes = [];
    if (data) {
        for (let i = 0; i < getStructuredTagsListResponse(data).length; i++) {
            // без уникального ключа ${i}${this.state.time} при снятии последнего чекбокса он не перендерится после загрузки данных:
            // можно создавать time в коде перед добавлением компонента:
            let time = String(Date.now());
            checkboxes.push(<Checkbox key={`checkbox ${i}${time}`}
                                      id={String(i)}
                                      noteDto={data} />);
        }
    }

    const castedRefObject = refObject as React.LegacyRef<HTMLFormElement>|undefined;
    return (
        <div id="renderContainer">
            <form ref={castedRefObject} id="dizzy">
                {checkboxes}
                {data && <SubmitButton stateWrapper={stateWrapper} formElement={formElement} noteDto={data} />}
            </form>
            {data && getTextResponse(data) && <Note formElement={formElement} noteDto={data} />}
        </div>
    );
}

const Checkbox = (props: {noteDto: NoteResponseDto, id: string}) => {
    const checked = getTagsCheckedUncheckedResponse(props) === "checked";
    const getTagName = (i: number) => {
        return getStructuredTagsListResponse(props.noteDto)[i];
    };
    return (
        <div id="checkboxStyle">
            <input name="chkButton" value={props.id} type="checkbox" id={props.id}
                   className="regular-checkbox"
                   defaultChecked={checked}/>
            <label htmlFor={props.id}>{getTagName(Number(props.id))}</label>
        </div>
    );
}

const Note = (props: {formElement?: HTMLFormElement, noteDto: NoteResponseDto}) => {
    const [, forceUpdate] = useReducer(x => x + 1, 0);

    useEffect(() => {
        getCookie();
    }, []);

    // именование кук ASP.NET: ".AspNetCore.Cookies"
    // учитывая изменения в работе с куками со стороны браузера, вопрос: зачем?
    const getCookie = () => {
        // куки выставляются в компоненте Login:
        const name = "rsse_auth";
        const matches = document.cookie.match(new RegExp(
            "(?:^|; )" + name.replace(/([.$?*|{}()\[\]\\\/+^])/g, '\\$1') + "=([^;]*)"
        ));

        if (matches == null || decodeURIComponent(matches[1]) === 'false') {
            hideMenu();
        }
    }

    const hideMenu = () => {
        if (props.formElement) props.formElement.style.display = toggleMenuVisibility(props.formElement.style.display);
        (document.getElementById("login") as HTMLElement).style.display = "block";
    }

    const inputText = (e: string) => {
        setTextResponse(props.noteDto, e);
        forceUpdate();
    }

    return (
        <div>
            <p/>
            {props.noteDto != null ? (getTextResponse(props.noteDto) != null ?
                    <div>
                        <h1 onClick={hideMenu}>
                            {getTitleResponse(props.noteDto)}
                        </h1>
                        <h5>
                            <textarea name="msg" cols={66} rows={30} form="dizzy"
                                      value={getTextResponse(props.noteDto)}
                                      onChange={e => inputText(e.target.value)}/>
                        </h5>
                    </div>
                    : "выберите заметку")
                : "loading.."}
        </div>
    );
}

const SubmitButton = (props: {formElement?: HTMLFormElement, noteDto: NoteResponseDto, stateWrapper: FunctionComponentStateWrapper<NoteResponseDto>}) => {
    const submit = (e: React.SyntheticEvent) => {
        e.preventDefault();
        const formData = new FormData(props.formElement);
        const checkboxesArray =
            (formData.getAll("chkButton"))
            .map(a => Number(a) + 1);
        const formMessage = formData.get("msg");
        const item = {
            "tagsCheckedRequest": checkboxesArray,
            "textRequest": formMessage,
            "titleRequest": getTitleResponse(props.noteDto),
            "commonNoteID": window.noteIdStorage
        };
        const requestBody = JSON.stringify(item);
        Loader.unusedPromise = Loader.postData(props.stateWrapper, requestBody, Loader.updateUrl);
    }

    return (
        <div id="submitStyle">
            <input type="checkbox" id="submitButton" className="regular-checkbox" onClick={submit}/>
            <label htmlFor="submitButton">Сохранить</label>
        </div>
    );
}


import * as React from 'react';
import {useEffect, useRef, useState} from "react";
import {useParams} from "react-router-dom";
import {createRoot, Root} from "react-dom/client";

import {Loader} from "../common/loader";
import {getStructuredTagsListResponse, getTextResponse} from "../common/dto.handlers";
import {NoteResponseDto} from "../dto/request.response.dto";
import {FunctionComponentStateWrapper} from "../common/state.handlers";
import {ReadNote} from "./read.note";
import {ReadCheckbox} from "./read.checkbox";
import {ReadSubmitButton} from "./read.submit";

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
                <ReadSubmitButton stateWrapper={stateWrapper} formElement={formElement} />
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
            checkboxes.push(<ReadCheckbox key={`checkbox ${index}`} id={String(index)} noteDto={data} />);
        }
    }

    const castedRefObject = refObject as React.LegacyRef<HTMLFormElement>|undefined;
    return (
        <div>
            <form ref={castedRefObject} id="textbox">{checkboxes}</form>
            <div id="messageBox">
                {data && getTextResponse(data) && <ReadNote formElement={formElement} noteDto={data} />}
            </div>
        </div>
    );
}

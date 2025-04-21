import * as React from 'react';
import {useEffect, useRef, useState} from "react";
import {useParams} from "react-router-dom";

import {Loader} from "../common/loader";
import {getStructuredTagsListResponse, getTextResponse} from "../common/dto.handlers";
import {NoteResponseDto} from "../dto/request.response.dto";
import {FunctionComponentStateWrapper} from "../common/state.handlers";
import {ReadNote} from "./read.note";
import {ReadCheckbox} from "./read.checkbox";
import {ReadSubmitButton} from "./read.submit";
import {Doms, SystemConstants} from "../dto/doms.tsx";
import {createPortal} from "react-dom";

export interface ButtonAnchorProps {
    buttonRef: React.RefObject<HTMLDivElement>;
}
interface ReadContainerParametrizedProps extends ButtonAnchorProps {
    noteId?: string;
}

export const ReadContainer = ({buttonRef}: ButtonAnchorProps) => {
    const params = useParams<{ textId?: string }>();
    return <ReadContainerParametrized noteId={params.textId} buttonRef={buttonRef} />;
}

const ReadContainerParametrized = (props: ReadContainerParametrizedProps) => {
    const [data, setData] = useState<NoteResponseDto|null>(null);
    const mounted = useState(true);
    const stateWrapper = new FunctionComponentStateWrapper(mounted, setData);

    const refObject:  React.MutableRefObject<HTMLFormElement|undefined> = useRef();
    let formElement: HTMLFormElement|undefined = refObject.current;

    useEffect(() => {
        onMount();
        return onUnmount;
    }, []);

    // component did update: на изменении data отрендерится портал с кнопкой
    useEffect(() => {}, [data]);

    const onMount = () => {
        formElement = refObject.current;
        if (props.noteId && !data) {
            // при редиректе из каталога или из Create (read/id) не обновляем содержимое компонента и убираем чекбоксы с логином:
            // TODO: вынести style=none в отдельную функцию
            if (formElement) formElement.style.display = SystemConstants.none;
            (document.getElementById(Doms.loginName) as HTMLElement).style.display = SystemConstants.none;
        } else {
            Loader.unusedPromise = Loader.getData(stateWrapper, Loader.readUrl);
        }
    }

    const onUnmount = () => {
        mounted[0] = false;
    }

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
        <div id={Doms.mainContent}>
            <form ref={castedRefObject} id={Doms.textbox}>{checkboxes}</form>
            <div id={Doms.messageBox}>
                {data && getTextResponse(data) && <ReadNote formElement={formElement} noteDto={data} />}
            </div>
            {props.buttonRef.current
                && createPortal(
                    <ReadSubmitButton stateWrapper={stateWrapper} formElement={formElement} />, props.buttonRef.current
                )}
        </div>
    );
};

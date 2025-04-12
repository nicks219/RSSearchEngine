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
import {Doms, SystemConstants} from "../dto/doms.tsx";

interface ReadContainerProps {
    buttonRef: React.RefObject<HTMLDivElement>;
}
interface ReadContainerParametrizedProps extends ReadContainerProps {
    noteId?: string;
}

export const ReadContainer = ({buttonRef}: ReadContainerProps) => {
    const params = useParams<{ textId?: string }>();
    return <ReadContainerParametrized noteId={params.textId} buttonRef={buttonRef} />;
}

const ReadContainerParametrized = (props: ReadContainerParametrizedProps) => {
    const rootRef = useRef<Root|null>();

    const [data, setData] = useState<NoteResponseDto|null>(null);
    const mounted = useState(true);
    const stateWrapper = new FunctionComponentStateWrapper(mounted, setData);

    const refObject:  React.MutableRefObject<HTMLFormElement|undefined> = useRef();
    let formElement: HTMLFormElement|undefined = refObject.current;

    useEffect(() => {
        onMount();
        return onUnmount;
    }, []);

    useEffect(() => {
        componentDidUpdate();
    }, [data]);

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
        // убираем отображение кнопки "Поиск":
        // SearchButtonRoot.getRoot.render(<div></div>);
        rootRef.current?.render(<div></div>);
        setTimeout(() => {
            rootRef.current?.unmount();
            rootRef.current = null;
        }, 0);
    }

    const componentDidUpdate = () => {
        if (!props.buttonRef.current) return;
        if (!rootRef.current) {rootRef.current = createRoot(props.buttonRef.current);}
        rootRef.current.render(
            <div>
                <ReadSubmitButton stateWrapper={stateWrapper} formElement={formElement} />
            </div>
        );
        (document.getElementById(Doms.header) as HTMLElement).style.backgroundColor = SystemConstants.color_405060;//"#e9ecee"
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
        </div>
    );
};

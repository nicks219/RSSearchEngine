import * as React from 'react';
import {useContext, useEffect, useRef, useState} from "react";
import {Loader} from "../common/loader";
import {
    getCommonNoteId, getStructuredTagsListResponse, getTextResponse,
    getTitleResponse, setTextResponse, setTitleResponse
} from "../common/dto.handlers";
import {NoteResponseDto} from "../dto/request.response.dto";
import {FunctionComponentStateWrapper} from "../common/state.handlers";
import {CommonContext, RecoveryContext} from "../common/context.provider";
import {CreateNote} from "./create.note";
import {CreateSubmitButton} from "./create.submit";
import {CreateCheckbox} from "./create.checkbox";
import {Doms} from "../dto/doms.tsx";
import {ButtonAnchorProps} from "./read.container";
import {createPortal} from "react-dom";

export const CreateContainer = ({buttonRef}: ButtonAnchorProps) => {
    const [data, setState] = useState<NoteResponseDto|null>(null);
    const mounted = useState(true);
    const stateWrapper = new FunctionComponentStateWrapper<NoteResponseDto>(mounted, setState);
    const commonContext = useContext(CommonContext);
    const recoveryContext = useContext(RecoveryContext);
    const refObject = useRef<HTMLFormElement>(null);
    let formElement: HTMLFormElement|null = refObject.current;

    const onMount = () => {
        formElement = refObject.current;
        Loader.unusedPromise = Loader.getData(stateWrapper, Loader.createGetTagsUrl, recoveryContext);
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
            // onClick={stateWrapper.setData} дублируются для SubmitButton (изначально) и Checkbox (перенесены из SubmitButton):
            checkboxes.push(<CreateCheckbox key={`checkbox ${i}${time}`} id={String(i)} noteDto={data} onClick={stateWrapper.setData}/>);
        }
    }

    if (data) {
        if (!getTextResponse(data)) setTextResponse(data, "");
        if (!getTitleResponse(data)) setTitleResponse(data, "");
    }

    const castedRefObject = refObject as React.LegacyRef<HTMLFormElement>|undefined;
    return (
        <div id={Doms.mainContent}>
            <form ref={castedRefObject}
                  id={Doms.textbox}>
                {checkboxes}
                {data && buttonRef.current && createPortal(
                    <CreateSubmitButton formElement={formElement} stateWrapper={stateWrapper} />, buttonRef.current
                )}
            </form>
            {data && <CreateNote noteDto={data} />}
        </div>
    );
}

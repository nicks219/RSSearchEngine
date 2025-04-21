import * as React from 'react';
import {useContext, useEffect, useRef, useState} from "react";
import {Loader} from "../common/loader";
import {getStructuredTagsListResponse, getTextResponse,} from "../common/dto.handlers";
import {NoteResponseDto} from "../dto/request.response.dto";
import {FunctionComponentStateWrapper} from "../common/state.handlers";
import {CommonContext, RecoveryContext} from "../common/context.provider";
import {UpdateSubmitButton} from "./update.submit";
import {UpdateCheckbox} from "./update.checkbox";
import {UpdateNote} from "./update.note";
import {Doms} from "../dto/doms.tsx";
import {ButtonAnchorProps} from "./read.container";
import {createPortal} from "react-dom";

export const UpdateContainer = ({buttonRef}: ButtonAnchorProps) => {
    const [data, setData] = useState<NoteResponseDto|null>(null);
    const mounted = useState(true);
    const stateWrapper = new FunctionComponentStateWrapper<NoteResponseDto>(mounted, setData);
    const commonContext = useContext(CommonContext);
    const recoveryContext = useContext(RecoveryContext);

    const refObject: React.MutableRefObject<HTMLFormElement|undefined> = useRef();
    let formElement: HTMLFormElement|undefined = refObject.current;

    const onMount = () => {
        formElement = refObject.current;
        Loader.unusedPromise = Loader.getDataById(stateWrapper, commonContext.commonNumber, Loader.updateUrl, recoveryContext);
    }

    const onUnmount = () => {
        mounted[0] = false;
    }

    useEffect(() => {
        onMount();
        return onUnmount;
    }, []);

    const checkboxes = [];
    if (data) {
        for (let i = 0; i < getStructuredTagsListResponse(data).length; i++) {
            // без уникального ключа ${i}${this.state.time} при снятии последнего чекбокса он не перендерится после загрузки данных:
            let time = String(Date.now());
            checkboxes.push(<UpdateCheckbox key={`checkbox ${i}${time}`}
                                      id={String(i)}
                                      noteDto={data} />);
        }
    }

    const castedRefObject = refObject as React.LegacyRef<HTMLFormElement>|undefined;
    return (
        <div id={Doms.mainContent}>
            <form ref={castedRefObject} id={Doms.textbox}>
                {checkboxes}
                {data && buttonRef.current && createPortal(
                    <UpdateSubmitButton stateWrapper={stateWrapper} formElement={formElement} noteDto={data} />, buttonRef.current
                )}
            </form>
            {data && getTextResponse(data) && <UpdateNote formElement={formElement} noteDto={data} />}
        </div>
    );
}

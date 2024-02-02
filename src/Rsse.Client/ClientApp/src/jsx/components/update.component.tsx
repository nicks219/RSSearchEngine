import * as React from 'react';
import {Loader} from "../common/loader.tsx";
import {
    getStructuredTagsListResponse, getTagsCheckedUncheckedResponse, getTextResponse,
    getTitleResponse, setTextResponse
} from "../common/dto.handlers.tsx";
import {ISimpleProps, ISubscribed} from "../common/contracts.tsx";
import {NoteResponseDto} from "../dto/request.response.dto.tsx";
import {toggleMenuVisibility} from '../common/visibility.handlers.tsx';
import {useEffect, useReducer, useRef, useState} from "react";
import {FunctionComponentStateWrapper} from "../common/state.wrappers.tsx";

interface ISubscribeProps extends ISimpleProps, ISubscribed<FunctionComponentStateWrapper<NoteResponseDto>> {
}

export interface IDataTimeState {
    data: NoteResponseDto | null;
    time?: number | null;
}

export const UpdateView = () => {
    // используются два типа: результат фетча NoteResponseDto, и тип стейта IDataTimeState с полями {data,time}
    const [data, setData] = useState<IDataTimeState | null>({data: null, time: null});// I.
    const mounted = useState(true);
    // где используется поле data для wrapper?
    const stateWrapper = new FunctionComponentStateWrapper<NoteResponseDto>(mounted, null, null, setData);// II.

    const refObject: React.MutableRefObject<HTMLFormElement | undefined> = useRef();
    let formId: HTMLFormElement | undefined = refObject.current;

    const componentDidMount = () => {
        formId = refObject.current;
        Loader.unusedPromise = Loader.getDataById(stateWrapper, window.textId, Loader.updateUrl);// зачем тут {data,state} из stateWrapper?
    }

    const componentWillUnmount = () => {
        mounted[0] = false;
    }

    useEffect(() => {
        componentDidMount();
        return componentWillUnmount;
    }, []);

    let checkboxes = [];
    if (data != null && data.data != null) {
        for (let i = 0; i < getStructuredTagsListResponse(data.data).length; i++) {
            // без уникального ключа ${i}${this.state.time} при снятии последнего чекбокса он не перендерится после загрузки данных:
            // TODO: попробоавть не записывать time лоадером в стейт: data.time, а создавать в коде перед добавлением компонента.
            // разберись: ре-рендерингу требуется key или уникальное поле в стейте?
            checkboxes.push(<Checkbox key={`checkbox ${i}${data.time}`}
                                      id={String(i)}
                                      jsonStorage={data.data}
                                      formId={undefined}/>);
        }
    }

    const castedRefObject = refObject as React.LegacyRef<HTMLFormElement> | undefined;
    return (
        <div id="renderContainer">
            <form ref={castedRefObject} id="dizzy">
                {checkboxes}
                {data != null && data.data != null &&
                    <SubmitButton subscription={stateWrapper} formId={formId} jsonStorage={data.data} id={undefined}/>// III.
                }
            </form>
            {data != null && data.data != null && getTextResponse(data.data) != null &&
                <Message formId={formId} jsonStorage={data.data} id={undefined}/>
            }
        </div>
    );
}

const Checkbox = (props: ISimpleProps) => {
    let checked = getTagsCheckedUncheckedResponse(props) === "checked";
    let getTagName = (i: number) => {
        return getStructuredTagsListResponse(props.jsonStorage)[i];
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

const Message = (props: ISimpleProps) => {
    const [, forceUpdate] = useReducer(x => x + 1, 0);

    useEffect(() => {
        getCookie();
    }, []);

    // именование кук ASP.NET: ".AspNetCore.Cookies"
    const getCookie = () => {
        // куки выставляются в компоненте Login:
        const name = "rsse_auth";
        let matches = document.cookie.match(new RegExp(
            "(?:^|; )" + name.replace(/([.$?*|{}()\[\]\\\/+^])/g, '\\$1') + "=([^;]*)"
        ));

        if (matches == null || decodeURIComponent(matches[1]) === 'false') {
            hideMenu();
        }
    }

    const hideMenu = () => {
        if (props.formId) props.formId.style.display = toggleMenuVisibility(props.formId.style.display);
        (document.getElementById("login") as HTMLElement).style.display = "block";
    }

    const inputText = (e: string) => {
        setTextResponse(props.jsonStorage, e);
        forceUpdate();
    }

    return (
        <div>
            <p/>
            {props.jsonStorage != null ? (getTextResponse(props.jsonStorage) != null ?
                    <div>
                        <h1 onClick={hideMenu}>
                            {getTitleResponse(props.jsonStorage)}
                        </h1>
                        <h5>
                            <textarea name="msg" cols={66} rows={30} form="dizzy"
                                      value={getTextResponse(props.jsonStorage)}
                                      onChange={e => inputText(e.target.value)}/>
                        </h5>
                    </div>
                    : "выберите заметку")
                : "loading.."}
        </div>
    );
}

const SubmitButton = (props: ISubscribeProps) => {
    const submit = (e: React.SyntheticEvent) => {
        e.preventDefault();
        let formData = new FormData(props.formId);
        let checkboxesArray = (formData.getAll("chkButton")).map(a => Number(a) + 1);
        let formMessage = formData.get("msg");
        const item = {
            "tagsCheckedRequest": checkboxesArray,
            "textRequest": formMessage,
            "titleRequest": getTitleResponse(props.jsonStorage),
            "commonNoteID": window.textId
        };
        let requestBody = JSON.stringify(item);
        Loader.unusedPromise = Loader.postData(props.subscription, requestBody, Loader.updateUrl);
    }

    return (
        <div id="submitStyle">
            <input type="checkbox" id="submitButton" className="regular-checkbox" onClick={submit}/>
            <label htmlFor="submitButton">Сохранить</label>
        </div>
    );
}


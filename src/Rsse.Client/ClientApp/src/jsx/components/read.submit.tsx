import * as React from "react";
import {FunctionComponentStateWrapper} from "../common/state.handlers";
import {NoteResponseDto} from "../dto/request.response.dto";
import {Loader} from "../common/loader";
import {Doms, SystemConstants} from "../dto/doms.tsx";

export const ReadSubmitButton = (props: {formElement: HTMLFormElement|null, stateWrapper: FunctionComponentStateWrapper<NoteResponseDto>}) => {
    const submit = (e: React.SyntheticEvent) => {
        e.preventDefault();
        (document.getElementById(Doms.loginName) as HTMLElement).style.display = SystemConstants.none;
        const formData = new FormData(props.formElement!);
        const checkboxesArray = (formData.getAll(Doms.chkButton)).map(a => Number(a) + 1);
        const item = {
            "tagsCheckedRequest": checkboxesArray
        };
        const requestBody = JSON.stringify(item);
        Loader.unusedPromise = Loader.postData(props.stateWrapper, requestBody, Loader.readNotePostUrl);
        (document.getElementById(Doms.header) as HTMLElement).style.backgroundColor = SystemConstants.slategreyStr;
    }

    return (
        <div id={Doms.submitStyle}>
            <input form={Doms.textbox} type={Doms.checkbox} id={Doms.submitButton} className={Doms.regularCheckbox} onClick={submit}/>
            <label htmlFor={Doms.submitButton}>Поиск</label>
        </div>
    );
}

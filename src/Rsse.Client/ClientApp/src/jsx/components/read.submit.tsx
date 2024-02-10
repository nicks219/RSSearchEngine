import * as React from "react";
import {FunctionComponentStateWrapper} from "../common/state.handlers";
import {NoteResponseDto} from "../dto/request.response.dto";
import {Loader} from "../common/loader";

export const ReadSubmitButton = (props: {formElement?: HTMLFormElement, stateWrapper: FunctionComponentStateWrapper<NoteResponseDto>}) => {
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

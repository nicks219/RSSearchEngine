import * as React from "react";
import {useContext} from "react";
import {NoteResponseDto} from "../dto/request.response.dto";
import {FunctionComponentStateWrapper} from "../common/state.handlers";
import {CommonContext, RecoveryContext} from "../common/context.provider";
import {getTitleResponse} from "../common/dto.handlers";
import {Loader} from "../common/loader";

export const UpdateSubmitButton = (props: {formElement?: HTMLFormElement, noteDto: NoteResponseDto, stateWrapper: FunctionComponentStateWrapper<NoteResponseDto>}) => {
    const recoveryContext = useContext(RecoveryContext);
    const commonContext = useContext(CommonContext);

    const submit = (e: React.SyntheticEvent) => {
        e.preventDefault();
        const formData = new FormData(props.formElement);
        const checkboxesArray =
            (formData.getAll("chkButton"))
                .map(item => Number(item) + 1);
        const formMessage = formData.get("msg");
        const item = {
            "tagsCheckedRequest": checkboxesArray,
            "textRequest": formMessage,
            "titleRequest": getTitleResponse(props.noteDto),
            "commonNoteID": commonContext.commonNumber
        };
        const requestBody = JSON.stringify(item);
        // используется recovery context
        Loader.unusedPromise = Loader.postData(props.stateWrapper, requestBody, Loader.updateUrl, null, recoveryContext);
    }

    return (
        <div id="submitStyle">
            <input type="checkbox" id="submitButton" className="regular-checkbox" onClick={submit}/>
            <label htmlFor="submitButton">Сохранить</label>
        </div>
    );
}

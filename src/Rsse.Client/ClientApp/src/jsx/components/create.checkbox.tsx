import * as React from "react";
import {Dispatch, SetStateAction, useContext} from "react";
import {NoteResponseDto} from "../dto/request.response.dto";
import {CommonContext} from "../common/context.provider";
import {getStructuredTagsListResponse, getTagsCheckedUncheckedResponse} from "../common/dto.handlers";
import {ComponentMode} from "../common/state.handlers";

export const CreateCheckbox = (props: {noteDto: NoteResponseDto, id: string, onClick: Dispatch<SetStateAction<NoteResponseDto|null>>}) => {
    const commonContext = useContext(CommonContext);
    const checked = getTagsCheckedUncheckedResponse(props) === "checked";

    const getTagName = (i: number) => {
        return getStructuredTagsListResponse(props.noteDto)[i];
    };

    const getTagId = (i: number) => {
        if (props.noteDto.tagIdsInternal && props.noteDto.tagIdsInternal.length > 0) {
            return props.noteDto.tagIdsInternal[i];
        } else if (props.noteDto.structuredTagsListResponse) {
            return props.noteDto.structuredTagsListResponse[i];
        } else {
            return "";
        }
    };

    const loadNoteOnClick = (e: React.SyntheticEvent) => {
        if (commonContext.componentMode == ComponentMode.ExtendedMode) {
            let title = e.currentTarget.innerHTML.valueOf();
            // item(1) это аттрибут about, в нём должен храниться id заметки, на который указывает данный чекбокс:
            let id = e.currentTarget.attributes.item(1)?.nodeValue;
            console.log(`Submitted & redirected: state: ${commonContext.componentMode} title: ${title} id: ${id}`);

            const noteResponseDto = new NoteResponseDto();
            // установка commonNoteID приведет к вызову редиректа после перерисовки CreateView:
            // commonNoteID также выставляется при сохранении новой заметки:
            noteResponseDto.commonNoteID = Number(id);
            props.onClick(noteResponseDto);
        }
    }

    return (
        <div id="checkboxStyle">
            <input name="chkButton" value={props.id} type="checkbox" id={props.id}
                   className="regular-checkbox"
                   defaultChecked={checked}/>
            <label htmlFor={props.id}
                   onClick={loadNoteOnClick}
                   about={getTagId(Number(props.id))}>{getTagName(Number(props.id))}</label>
        </div>
    );
}

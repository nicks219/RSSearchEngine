import * as React from "react";
import {Dispatch, SetStateAction, useContext} from "react";
import {NoteResponseDto} from "../dto/request.response.dto";
import {CommonContext} from "../common/context.provider";
import {getStructuredTagsListResponse, getTagCheckedUncheckedResponse} from "../common/dto.handlers";
import {ComponentMode} from "../common/state.handlers";

export const CreateCheckbox = (props: {noteDto: NoteResponseDto, id: string, onClick: Dispatch<SetStateAction<NoteResponseDto|null>>}) => {
    const commonContext = useContext(CommonContext);
    const isChecked = getTagCheckedUncheckedResponse(props) === "checked";

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
        // extended mode:
        if (commonContext.componentMode == ComponentMode.Extended) {
            const title = e.currentTarget.innerHTML.valueOf();
            const noteResponseDto = new NoteResponseDto();
            // item(1) это аттрибут about, в нём должен храниться id заметки, на который указывает данный чекбокс:
            const id = e.currentTarget.attributes.item(1)?.nodeValue;
            console.log(`Submitted & redirected: state: ${commonContext.componentMode} title: ${title} id: ${id}`);
            // установка commonNoteID приведет к вызову редиректа после перерисовки CreateView:
            // commonNoteID также выставляется при сохранении новой заметки:
            noteResponseDto.commonNoteID = Number(id);
            // const isClassicMode = isNaN(Number(id)); // косвенный признак состояния компонента
            props.onClick(noteResponseDto);
        }
    }

    return (
        <div id="checkboxStyle">
            <input name="chkButton" value={props.id} type="checkbox" id={props.id}
                   className="regular-checkbox"
                   defaultChecked={isChecked}/>
            <label htmlFor={props.id}
                   onClick={loadNoteOnClick}
                   about={getTagId(Number(props.id))}>{getTagName(Number(props.id))}</label>
        </div>
    );
}

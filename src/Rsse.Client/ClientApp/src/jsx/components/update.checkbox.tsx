import {NoteResponseDto} from "../dto/request.response.dto";
import {getStructuredTagsListResponse, getTagCheckedUncheckedResponse} from "../common/dto.handlers";

export const UpdateCheckbox = (props: {noteDto: NoteResponseDto, id: string}) => {
    const checked = getTagCheckedUncheckedResponse(props) === "checked";
    const getTagName = (i: number) => {
        return getStructuredTagsListResponse(props.noteDto)[i];
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

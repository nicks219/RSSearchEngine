import {NoteResponseDto} from "../dto/request.response.dto";
import {getStructuredTagsListResponse, getTagsCheckedUncheckedResponse} from "../common/dto.handlers";

export const UpdateCheckbox = (props: {noteDto: NoteResponseDto, id: string}) => {
    const checked = getTagsCheckedUncheckedResponse(props) === "checked";
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

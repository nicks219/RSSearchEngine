import {NoteResponseDto} from "../dto/request.response.dto";
import {getStructuredTagsListResponse} from "../common/dto.handlers";

export const ReadCheckbox = (props: {id: string, noteDto: NoteResponseDto}) => {
    const getTagName = (i: number) => getStructuredTagsListResponse(props.noteDto)[i];
    return (
        <div id="checkboxStyle">
            <input name="chkButton" value={props.id} type="checkbox" id={props.id}
                   className="regular-checkbox"
                   defaultChecked={false}/>
            <label htmlFor={props.id}>{getTagName(Number(props.id))}</label>
        </div>
    );
}

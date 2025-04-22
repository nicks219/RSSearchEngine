import {NoteResponseDto} from "../dto/request.response.dto";
import {getStructuredTagsListResponse, getTagCheckedUncheckedResponse} from "../common/dto.handlers";
import {Doms, SystemConstants} from "../dto/doms.tsx";

export const UpdateCheckbox = (props: {noteDto: NoteResponseDto, id: string}) => {
    const checked = getTagCheckedUncheckedResponse(props) === SystemConstants.checked;
    const getTagName = (i: number) => {
        return getStructuredTagsListResponse(props.noteDto)[i];
    };
    return (
        <div id={Doms.checkboxStyle}>
            <input name={Doms.chkButton} value={props.id} type={Doms.checkbox} id={props.id}
                   className={Doms.regularCheckbox}
                   defaultChecked={checked}/>
            <label htmlFor={props.id}>{getTagName(Number(props.id))}</label>
        </div>
    );
}

import {NoteResponseDto} from "../dto/request.response.dto";
import {getStructuredTagsListResponse} from "../common/dto.handlers";
import {Doms} from "../dto/doms.tsx";

export const ReadCheckbox = (props: {id: string, noteDto: NoteResponseDto}) => {
    const getTagName = (i: number) => getStructuredTagsListResponse(props.noteDto)[i];
    return (
        <div id={Doms.checkboxStyle}>
            <input name={Doms.chkButton} value={props.id} type={Doms.checkbox} id={props.id}
                   className={Doms.regularCheckbox}
                   defaultChecked={false}/>
            <label htmlFor={props.id}>{getTagName(Number(props.id))}</label>
        </div>
    );
}

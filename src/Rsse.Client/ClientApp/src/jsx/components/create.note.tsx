import {useReducer} from "react";
import {NoteResponseDto} from "../dto/note.response.dto.tsx";
import {getTextResponse, getTitleResponse, setTextResponse, setTitleResponse} from "../common/dto.handlers";
import {Doms} from "../dto/doms.tsx";

export const CreateNote = (props: {noteDto: NoteResponseDto}) => {
    const textAreaCols: number = 73;
    const textAreaRows: number = 30;
    const textAreaRow: number = 1;

    const [, forceUpdate] = useReducer(x => x + 1, 0);

    // TODO: напиши новый вариант ввода, без апдейта на каждый символ:
    const textHandler = (e: string) => {
        setTextResponse(props.noteDto, e);
        forceUpdate();
    }

    // TODO: напиши новый вариант ввода, без апдейта на каждый символ:
    const titleHandler = (e: string) => {
        setTitleResponse(props.noteDto, e);
        forceUpdate();
    }

    return (
        <div>
            <p/>
            {props.noteDto ?
                <div>
                    <h5>
                        <textarea name={Doms.ttl} cols={textAreaCols} rows={textAreaRow} form={Doms.textbox}
                                  value={getTitleResponse(props.noteDto)}
                                  onChange={e => titleHandler(e.target.value)}/>
                    </h5>
                    <h5>
                        <textarea name={Doms.msg} cols={textAreaCols} rows={textAreaRows} form={Doms.textbox}
                                  value={getTextResponse(props.noteDto)}
                                  onChange={e => textHandler(e.target.value)}/>
                    </h5>
                </div>
                : "loading.."}
        </div>
    );
}
